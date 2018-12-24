using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Buzz.Activity
{
    public class ProvisionActivity
    {
        private static ILogger _log;

        [FunctionName("ProvisionActivity")]
        public static async Task Run([ActivityTrigger] DurableActivityContext context, ILogger log,
            ExecutionContext executionContext)
        {

            _log = log;
            var input = new
            {
                Name = context.GetInput<Tuple<string, int, int, string>>().Item1,
                Index = context.GetInput<Tuple<string, int, int, string>>().Item2,
                Count = context.GetInput<Tuple<string, int, int, string>>().Item3,
                StorageKey = context.GetInput<Tuple<string, int, int, string>>().Item4
            };
            log.LogInformation($"ProvisionActivity provision {input.Name} {input.Index}");
            var config = executionContext.BuildConfiguration();
            var clientId = config["ApplicationId"];
            var clientSecret = config["ApplicationSecret"];
            var tenantId = config["TenantId"];
            string subscriptionId = config["SubscriptionId"];
            string userName = config["NodeUserName"];
            string password = config["NodeUserPassword"];
            var templatePath = config["TemplateFileUri"];
            var scriptFile = config["ScriptFileUri"];
            var commandToExecute = config["CommandToExecute"];
            string sourceContainerName = config["SourceAppsContainerName"];
            var region = config["Region"];
            var omsKey = config["OmsWorkspaceKey"];
            var omsId = config["OmsWorkspaceId"];
            
           var files = (await (new CloudStorageAccount(new StorageCredentials($"{input.Name.ToLower()}{input.Index}", input.StorageKey), true)
                   .CreateCloudBlobClient()).GetContainerReference(sourceContainerName)
                .ListBlobsSegmentedAsync(new BlobContinuationToken())).Results;
            
            var fileUris = files.Select(t => $"'{t.Uri.ToString()}'").Append($"'{scriptFile}'").ToArray();
            

            var parameters =
                Parameters.Make(
                    $"{input.Name}{input.Index}",
                    AddressRange.Make(input.Index).AddressRangeString,
                    fileUris,
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