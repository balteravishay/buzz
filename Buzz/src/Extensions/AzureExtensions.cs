using System;
using Buzz.Model;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Rest.Azure;
using Polly;

[assembly: InternalsVisibleTo("LoadGenerator.Tests")]

namespace Buzz.Extensions
{
    /// <summary>
    /// Extension methods to operate Azure resources
    /// </summary>
    internal static class AzureExtensions
    {
        private static Policy RetryPolicy = Policy
            .Handle<CloudException>()
            .WaitAndRetry(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, span, retryCount, context) =>
                {
                    Action<string> log = (Action<string>) context["log"];
                    log($"Request failed with {exception.Message}. Waiting {span} before next retry. Retry attempt {retryCount}");
                }
            );


        /// <summary>
        /// Tries to authenticate to ARM API using Azure SDK. return true if success.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="azure"></param>
        /// <param name="notifyError"></param>
        /// <returns></returns>
        internal static bool TryGetAzure(this AzureCredentials @this, 
            out IAzure azure, Action<string> notifyError)
        {
            try
            {
                azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(
                        SdkContext.AzureCredentialsFactory
                            .FromServicePrincipal(@this.ApplicationId, 
                                @this.ApplicationSecret, @this.TenantId, AzureEnvironment.AzureGlobalCloud))
                    .WithSubscription(@this.SubscriptionId);
                return true;
            }
            catch (Exception e)
            {
                notifyError($"error while authenticating to azure: {e.Message}");
                azure = null;
                return false;
            }
        }
        
       
        /// <summary>
        /// Creates or retrieves a resource group with the provided name and region.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        internal static IResourceGroup GetOrCreateResourceGroup(this IAzure @this, string name, string region)=>
            @this.ResourceGroupExists(name)
                ? @this.ResourceGroups.GetByName(name)
                : @this.ResourceGroups.Define(name)
                    .WithRegion(region)
                    .Create();
        

        /// <summary>
        /// Checks if resource group exists by name
        /// </summary>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool ResourceGroupExists(this IAzure @this, string name) =>
            @this.ResourceGroups.Contain(name);

        /// <summary>
        /// Deletes a virtual network by name
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="vnetName"></param>
        internal static void DeleteVnet(this IAzure @this,
            string resourceGroupName, string vnetName,
            Action<string> log) =>
            RetryPolicy.Execute((context)=>
                @this.Networks.DeleteByResourceGroupAsync(resourceGroupName, vnetName),
                new Dictionary<string, object>() { { "log", log } });
        
        /// <summary>
        /// creates a deployment of virtual machine scale set with provided parameters json string
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="deploymentName"></param>
        /// <param name="templatePath"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static void DeployTemplate(this IAzure @this,
            string resourceGroupName, string deploymentName,
            string templatePath, string parameters, Action<string> log) =>
            RetryPolicy.Execute((context) => @this.Deployments.Define($"{deploymentName}")
                   .WithExistingResourceGroup(resourceGroupName)
                   .WithTemplateLink(templatePath, "1.0.0.0")
                   .WithParameters(parameters)
                   .WithMode(Microsoft.Azure.Management.ResourceManager.Fluent.Models.DeploymentMode.Incremental)
                   .BeginCreate(),
                new Dictionary<string, object>() { { "log", log } });

        /// <summary>
        /// deletes a virtual network by name
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="vmssName"></param>
        internal static void DeleteScaleSet(this IAzure @this,
            string resourceGroupName, string vmssName, Action<string> log) =>
            RetryPolicy.Execute((context) => 
                @this.VirtualMachineScaleSets.DeleteByResourceGroupAsync(resourceGroupName, vmssName),
                new Dictionary<string, object>() { { "log", log } });

        /// <summary>
        /// retrieve a list of virtual machine scale sets names within a specific resource group.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupNameFilter"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetVmssNames(this IAzure @this,
            string resourceGroupNameFilter) =>
             @this.VirtualMachineScaleSets
                 .ListByResourceGroup(resourceGroupNameFilter)
                 .Select(scaleset=> scaleset.Name);

        /// <summary>
        /// deletes a resource group by name
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupName"></param>
        internal static void DeleteGroup(this IAzure @this,
            string resourceGroupName, Action<string> log) =>
            RetryPolicy.Execute((context) => @this
            .ResourceGroups
            .DeleteByNameAsync(resourceGroupName), new Dictionary<string, object>() { { "log", log } });

        /// <summary>
        /// get a resource group by name. returns true if success.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="resourceGroup"></param>
        /// <returns></returns>
        internal static bool TryGetResourceGroup(this IAzure @this, string resourceGroupName, out IResourceGroup resourceGroup)
        {
            if (@this.ResourceGroupExists(resourceGroupName))
            {
                resourceGroup = @this.ResourceGroups.GetByName(resourceGroupName);
                return true;
            }
            resourceGroup = null;
            return false;
        }

        
        /// <summary>
        /// gets an internal vmss vm nic by its internal ip address. returns true if success
        /// </summary>
        /// <param name="this"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="vmssName"></param>
        /// <param name="ip"></param>
        /// <param name="nic"></param>
        /// <returns></returns>
        internal static bool TryGetVmssNicByIp(this IAzure @this,
            string resourceGroupName, string vmssName, string ip, out IVirtualMachineScaleSetNetworkInterface nic)
        {
            var scaleSet = @this.VirtualMachineScaleSets.GetByResourceGroup(resourceGroupName, vmssName);
            if (scaleSet == null)
            {
                nic = null;
                return false;
            }
            nic = scaleSet.ListNetworkInterfaces().FirstOrDefault(n => n.PrimaryPrivateIP.Equals(ip));
            return nic != null;
        }

        /// <summary>
        /// creates a vnet and deploys a template to a resource group with given parameters
        /// </summary>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <param name="region"></param>
        /// <param name="templatePath"></param>
        internal static void DeployResourcesToGroup(this IAzure @this, string resourceGroupName,
            Parameters parameters,
            string region, string templatePath, Action<string> log)
        {
            if (!@this.TryGetResourceGroup(resourceGroupName, out var resourceGroup)) return;
            @this.DeployTemplate(resourceGroup.Name, parameters.DeploymentName, templatePath,
                parameters.CreateJson(), log);
        }

        internal static void DeleteResourcesInGroup(this IAzure @this, string resourceGroupName,
            string resourcesName, Action<string> log)
        {
            @this.DeleteScaleSet(resourceGroupName, resourcesName, log);
            @this.DeleteVnet(resourceGroupName, resourcesName, log);
        }
    }
}
