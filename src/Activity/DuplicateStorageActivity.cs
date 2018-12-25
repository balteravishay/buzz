using System;
using System.Linq;
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
        public static async Task<string> Run([ActivityTrigger] DurableActivityContext context, ILogger log,
            ExecutionContext executionContext)
        {
            //config and input
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
            log.LogInformation($"duplicate storage activity start {input.Name} {input.Index} ");

            try
            {
                // authenticate
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out var azure, message => _log.LogError(message))) return string.Empty;
                // create target storage account
                var targetStorage = azure
                    .StorageAccounts
                    .Define($"{input.Name.ToLower()}{input.Index}")
                    .WithRegion(Region.EuropeWest)
                    .WithExistingResourceGroup(input.Name)
                    .Create();
                // copy the source blob to the target container
                await new CloudStorageAccount(new StorageCredentials(sourceStorageName, sourceStorageKey), true)
                    .CreateCloudBlobClient()
                    .CopyContainerByName(sourceContainerName, 
                        new CloudBlobClient(new Uri(targetStorage.EndPoints.Primary.Blob),
                            new StorageCredentials(targetStorage.Name, targetStorage.GetKeys()[0].Value)));
                // return account key for the new storage
                return targetStorage.GetKeys().FirstOrDefault()?.Value;
            }
            catch (Exception e)
            {
                _log.LogError($"duplicate storage activity error  {e.Message}", e);
            }
            return string.Empty;
        }
    }
}
