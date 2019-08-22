# Twitter Finder

Watches twitter feed for missing persons cases. Performs content scraping of linked websites to feed Language Understanding algorithm to extract relevant entities.

## Local Development

### Tools

You'll need a few things installed to run this project locally.

* Azure Storage Emulator
* Visual Studio Code
* Setup an account with LUIS.ai
* An Azure Account (There is a Free Trial account)
* The Azure Account VS Code Extension
* The Azure Functions VS Code Extension

### Settings

To run this project on your local machine you will need to create a local.settings.json file and add configuration values such as:

``` json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "AzureWebJobsSecretStorageType": "files",
    "BlobStorageConnectionString": "UseDevelopmentStorage=true",
    "BlobStorageContainerName": "tweets",
    "BlobStorageBlobName": "tweets.json",
    "LUISappID": [APP_ID_FROM_LUIS_PORTAL],
    "LUISsubscriptionKey": [SUBSCRIPTION_KEY_FROM_LUIS_PORTAL],
    "LUISendpoint": "https://wxyz.api.cognitive.microsoft.com/luis/v2.0/apps/"
  },
  "ConnectionStrings": {}
}
```
