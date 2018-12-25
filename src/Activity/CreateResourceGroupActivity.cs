using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;

namespace Buzz.Activity
{
    public class CreateResourceGroupActivity
    {
        private static ILogger _log;

        [FunctionName("CreateResourceGroupActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log, 
            ExecutionContext executionContext)
        {
            // config and input
            _log = log;
            var input = new { ResourceGroupName = context.GetInput<string>() };
            var config = executionContext.BuildConfiguration();
            var clientId = config["ApplicationId"];
            var clientSecret = config["ApplicationSecret"];
            var tenantId = config["TenantId"];
            string subscriptionId = config["SubscriptionId"];
            var region = config["Region"];
            log.LogInformation($"create resource group activity start {input.ResourceGroupName} ");

            try
            {
                // authenticate and create resource group if not exists
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out IAzure azure, message => _log.LogError(message))) return;
                azure.GetOrCreateResourceGroup(input.ResourceGroupName, region);
            }
            catch(Exception e)
            {
                _log.LogError($"create resource group activity error processing function {e.Message}", e);
            }
        }
    }
}