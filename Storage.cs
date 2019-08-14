using MCSC.Classes;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MCSC
{
    public class StorageHelper
    {
        static CloudStorageAccount _storageAccount;
        static CloudBlobContainer _blobContainer;
        static string _connString;
        static string _containerName;
        static string _blobName;

        /// <Summary>
        /// Initializes the StorageAccount configuration based on keys found in .env
        /// </Summary>
        internal void InitializeStorageAccount()
        {
            _connString = Utils.GetEnvVariable("BlobStorageConnectionString");
            _containerName = Utils.GetEnvVariable("BlobStorageContainerName");
            _blobName = Utils.GetEnvVariable("BlobStorageBlobName");
            
            if (CloudStorageAccount.TryParse(_connString, out _storageAccount))
            {
                var blobClient = _storageAccount.CreateCloudBlobClient();
                _blobContainer = blobClient.GetContainerReference(_containerName);
            }
        }

        /// <Summary>
        /// Gets a tweets list from the tweets.json blob in the Tweets container
        /// </Summary>
        internal async Task<List<Tweet>> GetListOfTweetsAsync()
        {
            var latestBlockBlob =  _blobContainer.GetBlockBlobReference(_blobName);
            if (await latestBlockBlob.ExistsAsync())
            {
                string jsonString = await latestBlockBlob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<List<Tweet>>(jsonString);
            }
            return new List<Tweet>();
        }

        /// <Summary>
        /// Uploads an updated tweets.json blob in the Tweets container
        /// </Summary>
        internal async Task UpdateBlobAsync(string jsonFile)
        {
            var cloudBlockBlob = _blobContainer.GetBlockBlobReference(_blobName);
            await cloudBlockBlob.UploadTextAsync(jsonFile);
        }
    }
}