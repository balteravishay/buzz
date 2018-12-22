using System;
using System.Threading.Tasks;
using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Buzz.Activity
{
    public class DuplicateStorageActivity
    {
        private static ILogger _log;

        [FunctionName("DuplicateStorageActivity")]
        public static async Task Run([ActivityTrigger] DurableActivityContext context, ILogger log,
            ExecutionContext executionContext)
        {
            _log = log;
            var input = new
            {
                Name = context.GetInput<Tuple<string, int>>().Item1,
                Index = context.GetInput<Tuple<string, int>>().Item2
            };
            var config = executionContext.BuildConfiguration();
            var clientId = config["ApplicationId"];
            var clientSecret = config["ApplicationSecret"];
            var tenantId = config["TenantId"];
            string subscriptionId = config["SubscriptionId"];
            string sourceStorageName = config["SourceStorageName"];
            string sourceStorageKey = config["SourceStorageKey"];
            string sourceContainerName = config["SourceAppsContainerName"];

            try
            {
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out var azure, message => _log.LogError(message))) return;
                var sourceStorageAccountt = new CloudStorageAccount(new StorageCredentials(sourceStorageName, sourceStorageKey),true);
                var targetStorage = azure.StorageAccounts.Define($"{input.Name.ToLower()}{input.Index}")
                    .WithRegion(Region.EuropeWest)
                    .WithExistingResourceGroup(input.Name)
                    .Create();
                var keys = targetStorage.GetKeys();
                CloudBlobClient targetCloudBlobClient = new CloudBlobClient(new Uri(targetStorage.EndPoints.Primary.Blob),
                    new StorageCredentials(targetStorage.Name, keys[0].Value));
                var sourceCloudBlobClient = sourceStorageAccountt.CreateCloudBlobClient();
                await sourceContainerName.CopyContainerByName(sourceCloudBlobClient, targetCloudBlobClient);
            }
            catch (Exception e)
            {
                _log.LogError($"ProvisionActivity error processing function {e.Message}", e);
            }
        }
    }
}
