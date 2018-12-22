using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Buzz.Activity
{
    class DuplicateStorageActivity
    {
        private static ILogger _log;

        [FunctionName("DuplicateStorageActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log)
        {
            _log = log;
            var input = new
            {
                Name = context.GetInput<Tuple<string, int, string>>().Item1,
                Index = context.GetInput<Tuple<string, int, string>>().Item2
            };
            var clientId = ConfigurationManager.AppSettings["ApplicationId"];
            var clientSecret = ConfigurationManager.AppSettings["ApplicationSecret"];
            var tenantId = ConfigurationManager.AppSettings["TenantId"];
            string subscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
            string sourceStorageName = ConfigurationManager.AppSettings["SourceStorageName"];
            string sourceStorageKey = ConfigurationManager.AppSettings["SourceStorageKey"];
            string sourceContainerName = ConfigurationManager.AppSettings["SourceAppsContainerName"];

            try
            {
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out var azure, message => _log.LogError(message))) return;
                var sourceStorageAccountt = new CloudStorageAccount(new StorageCredentials(sourceStorageName, sourceStorageKey),true);
                var accountName = $"{input.Name.ToLower()}{input.Index}data";
                var targetStorage = azure.StorageAccounts.Define(accountName)
                    .WithRegion(Region.EuropeWest)
                    .WithExistingResourceGroup(input.Name)
                    .Create();
                var keys = targetStorage.GetKeys();
                CloudBlobClient targetCloudBlobClient = new CloudBlobClient(new Uri(targetStorage.EndPoints.Primary.Blob),
                    new StorageCredentials(accountName, keys[0].Value));
                var sourceCloudBlobClient = sourceStorageAccountt.CreateCloudBlobClient();
                Copy(sourceCloudBlobClient, targetCloudBlobClient, sourceContainerName).Wait();
            }
            catch (Exception e)
            {
                _log.LogError($"ProvisionActivity error processing function {e.Message}", e);
            }
        }


        private static async Task Copy(CloudBlobClient sourceClient,
            CloudBlobClient targetClient,
            string containerName)
        {
            var sourceContainer = sourceClient.GetContainerReference(containerName);
            var blobs = await sourceContainer.ListBlobsSegmentedAsync(new BlobContinuationToken());
            // Create a policy for reading the blob.

            var policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-15),
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            var targetContainer = targetClient.GetContainerReference(containerName);
            
            await targetContainer.CreateIfNotExistsAsync();
            var permission =
                new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Container };
            await targetContainer.SetPermissionsAsync(permission);
            foreach (var blob in blobs.Results)
            {
                var sourceBlob = new CloudBlockBlob(blob.Uri, sourceClient.Credentials);
                // Get SAS of that policy.
                var sourceBlobToken = sourceBlob.GetSharedAccessSignature(policy);
                // Make a full uri with the sas for the blob.
                var sourceBlobSas = $"{sourceBlob.Uri}{sourceBlobToken}";
                var targetBlob = targetContainer.GetBlockBlobReference(sourceBlob.Name);
                if (!await targetBlob.ExistsAsync())
                {
                    await targetBlob.StartCopyAsync(new Uri(sourceBlobSas));
                    await WaitSuccessCopy(targetContainer, sourceBlob.Name);
                }
            }
        }

        private static async Task WaitSuccessCopy(CloudBlobContainer targetContainer, string name)
        {
            var found = false;
            while (!found)
            {
                if ((await targetContainer.ListBlobsSegmentedAsync(name,
                        new  BlobContinuationToken())).Results.FirstOrDefault() is CloudBlob destBlob && destBlob.CopyState.Status == CopyStatus.Success)
                    found = true;
                else
                    Thread.Sleep(5000);
            }

        }
    }
}
