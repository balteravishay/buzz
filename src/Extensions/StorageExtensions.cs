using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Buzz.Extensions
{
    static class StorageExtensions
    {
        /// <summary>
        /// CopyToContainer content of an Azure blob container  
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="sourceClient"></param>
        /// <param name="targetClient"></param>
        /// <returns></returns>
        public static async Task CopyContainerByName(this CloudBlobClient sourceClient, string containerName,
            CloudBlobClient targetClient)
        {
            var targetContainer = await targetClient.CreateContainerIfNotExists(containerName);
            await Task.WhenAll((await 
                    sourceClient
                    .GetContainerReference(containerName)
                    .ListBlobsSegmentedAsync(new BlobContinuationToken()))
                    .Results
                    .Select(blob =>
                        new CloudBlockBlob(blob.Uri, sourceClient.Credentials).CopyToContainer(targetContainer)).ToArray());
        }

        /// <summary>
        /// Creates a Storage Container if one does not exist
        /// </summary>
        /// <param name="blobClient"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private static async Task<CloudBlobContainer> CreateContainerIfNotExists(this CloudBlobClient blobClient,
            string containerName)
        {
            var targetContainer = blobClient.GetContainerReference(containerName);
            if (await targetContainer.CreateIfNotExistsAsync())
                await targetContainer.SetPermissionsAsync(
                    new BlobContainerPermissions {PublicAccess = BlobContainerPublicAccessType.Container});
            return targetContainer;
        }

        /// <summary>
        /// Copy a Block blob to a target Storage Container
        /// </summary>
        /// <param name="sourceBlob"></param>
        /// <param name="targetContainer"></param>
        /// <returns></returns>
        private static async Task CopyToContainer(this CloudBlockBlob sourceBlob, CloudBlobContainer targetContainer)
        {
            // Make a full uri with the sas for the blob.
            var targetBlob = targetContainer.GetBlockBlobReference(sourceBlob.Name);
            if (!await targetBlob.ExistsAsync())
            {
                await targetBlob.StartCopyAsync(new Uri($"{sourceBlob.Uri}{sourceBlob.MakeSasToken()}"));
                await WaitSuccessCopy(targetContainer, sourceBlob.Name);
            }
        }

        /// <summary>
        /// Create a read only SAS token for a blob
        /// </summary>
        /// <param name="sourceBlob"></param>
        /// <returns></returns>
        private static string MakeSasToken(this CloudBlockBlob sourceBlob) =>
            sourceBlob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-15),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7)
                });
        

        /// <summary>
        /// Waits until the copy operation is succesful.
        /// </summary>
        /// <param name="targetContainer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static async Task WaitSuccessCopy(CloudBlobContainer targetContainer, string name)
        {
            var found = false;
            while (!found)
            {
                if ((await targetContainer.ListBlobsSegmentedAsync(name, new BlobContinuationToken()))
                    .Results
                    .FirstOrDefault() is CloudBlob destBlob && await  destBlob.IsCopied())
                    found = true;
                else
                    Thread.Sleep(1000);
            }
        }

        private static async Task<bool> IsCopied(this CloudBlob @this)
        {
            await @this.FetchAttributesAsync();
            return @this.CopyState != null &&
                   @this.CopyState.Status != CopyStatus.Pending;
        }
    }
}
