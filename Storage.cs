namespace MCSC
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using MCSC.Classes;
    
    public class StorageHelper
    {
        static CloudStorageAccount storageAccount;
        static CloudBlobContainer blobContainer;
        static string connString;
        static string containerName;
        static string blobName;

        /// <Summary>
        /// Initializes the StorageAccount configuration based on keys found in .env
        /// </Summary>
        internal void InitializeStorageAccount()
        {
            connString = Utils.GetEnvVariable("BlobStorageConnectionString");
            containerName = Utils.GetEnvVariable("BlobStorageContainerName");
            blobName = Utils.GetEnvVariable("BlobStorageBlobName");
            
            if (CloudStorageAccount.TryParse(connString, out storageAccount))
            {
                var blobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = blobClient.GetContainerReference(containerName);
            }
        }

        /// <Summary>
        /// Gets a tweets list from the tweets.json blob in the Tweets container
        /// </Summary>
        internal async Task<List<Tweet>> GetListOfTweetsAsync()
        {
            var latestBlockBlob =  blobContainer.GetBlockBlobReference(blobName);
            string jsonString = await latestBlockBlob.DownloadTextAsync();
            return JsonConvert.DeserializeObject<List<Tweet>>(jsonString);
        }

        /// <Summary>
        /// Uploads an updated tweets.json blob in the Tweets container
        /// </Summary>
        internal async void UpdateBlobAsync(string jsonFile)
        {
            var cloudBlockBlob = blobContainer.GetBlockBlobReference(blobName);
            await cloudBlockBlob.UploadTextAsync(jsonFile);
        }
    }
}