using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace Buzz.Activity
{
    public class ProvisionActivity
    {
        private static ILogger _log;

        [FunctionName("ProvisionActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log,
            ExecutionContext executionContext)
        {

            _log = log;
            var input = new
            {
                Name = context.GetInput<Tuple<string, int, int>>().Item1,
                Index = context.GetInput<Tuple<string, int, int>>().Item2,
                Count = context.GetInput<Tuple<string, int, int>>().Item3
            };
            log.LogInformation($"ProvisionActivity provision {input.Name} {input.Index}");
            var config = executionContext.BuildConfiguration();
            var clientId = config["ApplicationId"];
            var clientSecret = config["ApplicationSecret"];
            var tenantId = config["TenantId"];
            string subscriptionId = config["SubscriptionId"];
            string userName = config["UserName"];
            string password = config["UserPassword"];
            var templatePath = config["TemplateFileUri"];
            var scriptFile = config["ScriptFileUri"];
            var commandToExecute = config["CommandToExecute"];
            string sourceContainerName = config["SourceAppsContainerName"];
            var newStorageLocation = $"https://{input.Name.ToLower()}{input.Index}.blob.core.windows.net/{sourceContainerName}";
            //commandToExecute = commandToExecute
            //    .Replace("APPLICATIONS", newStorageLocation)
            //    .Replace("URL", url);v
            var fileUris = scriptFile;
            var region = config["Region"];
            var omsKey = config["OmsWorkspaceKey"];
            var omsId = config["OmsWorkspaceId"];

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