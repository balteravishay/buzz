using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;

namespace Buzz.Activity
{
    public class ProvisionActivity
    {
        private static ILogger _log;

        [FunctionName("ProvisionActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log)
        {

            _log = log;
            var input = new
            {
                Name = context.GetInput<Tuple<string, int, int>>().Item1,
                Index = context.GetInput<Tuple<string, int, int>>().Item2,
                Count = context.GetInput<Tuple<string, int, int>>().Item3
            };
            log.LogInformation($"ProvisionActivity provision {input.Name} {input.Index}");

            var clientId = ConfigurationManager.AppSettings["ApplicationId"];
            var clientSecret = ConfigurationManager.AppSettings["ApplicationSecret"];
            var tenantId = ConfigurationManager.AppSettings["TenantId"];
            string subscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
            string userName = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["UserPassword"];
            var templatePath = ConfigurationManager.AppSettings["TemplateFileUri"];
            var scriptFile = ConfigurationManager.AppSettings["ScriptFileUri"];
            var commandToExecute = ConfigurationManager.AppSettings["CommandToExecute"];
            string sourceContainerName = ConfigurationManager.AppSettings["SourceAppsContainerName"];
            var newStorageLocation = $"https://{input.Name.ToLower()}{input.Index}data.blob.core.windows.net/{sourceContainerName}";
            //commandToExecute = commandToExecute
            //    .Replace("APPLICATIONS", newStorageLocation)
            //    .Replace("URL", url);v
            var fileUris = scriptFile;
            var region = ConfigurationManager.AppSettings["Region"];
            var omsKey = ConfigurationManager.AppSettings["OmsWorkspaceKey"];
            var omsId = ConfigurationManager.AppSettings["OmsWorkspaceId"];

            var parameters =
                Parameters.Make(
                    $"{input.Name}{input.Index}",
                    AddressRange.Make(input.Index).AddressRangeString,
                    scriptFile,
                    commandToExecute,
                    userName,
                    password,
                    input.Count,
                    omsId,
                    omsKey);
            try
            {
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out var azure, message => _log.LogError(message))) return;
                _log.LogInformation($"deploy template with parameters: {parameters}");
                azure.DeployResourcesToGroup(input.Name, parameters, region, templatePath, s => _log.LogWarning(s));
            }
            catch(Exception e)
            {
                _log.LogError($"ProvisionActivity error processing function {e.Message}", e);
            }
        }
    }
}