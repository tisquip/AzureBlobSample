using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobSample.Services.AzureStorage
{
    //Derived from https://github.com/shahedc/SimpleUpload
    public class AzureBlob : IAzureBlob
    {
        private readonly string azureBlobConnectionString;

        public AzureBlob(IConfiguration configuration)
        {
            //I am Deleting the Azure Resource on Tuesday hence the connection string is saved in appsettings rather than a keyvault
            azureBlobConnectionString = configuration["azureBlobConnectionString"]; 
        }

        public async Task<(bool wasSuccesful, Uri uploadUrl, string blobContainerReference)> UploadToBlob(IFormFile formFile, string fileName = "")
        {
            bool wasSuccessful = false;
            Uri uploadUri = null;
            string blobContainerReference = "";

            if (formFile != null)
            {
                using (var stream = formFile.OpenReadStream())
                {
                    fileName = String.IsNullOrWhiteSpace(fileName) ? formFile.FileName : fileName;
                    (wasSuccessful, uploadUri, blobContainerReference) = await UploadToBlob(stream, fileName);
                }
            }
            return (wasSuccessful, uploadUri, blobContainerReference);
        }

        public async Task<(bool wasSuccesful, Uri uploadUrl, string blobContainerReference)> UploadToBlob(Stream stream, string fileName)
        {
            bool wasSuccessful = false;
            Uri uploadUri = null;
            string blobContainerReference = "";

            CloudBlobContainer cloudBlobContainer = null;

            // Check whether the connection string can be parsed.
            if (stream != null
                && !String.IsNullOrWhiteSpace(fileName)
                && CloudStorageAccount.TryParse(azureBlobConnectionString, out var storageAccount))
            {
                try
                {

                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    blobContainerReference = "uploadblob" + Guid.NewGuid().ToString();
                    cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainerReference);
                    await cloudBlobContainer.CreateAsync();

                    // Set the permissions so the blobs are public. 
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

                    await cloudBlockBlob.UploadFromStreamAsync(stream);

                    uploadUri = cloudBlockBlob.Uri;
                    wasSuccessful = true;
                }
                catch (StorageException ex)
                {
                    //TODO: Log exception
                    if (cloudBlobContainer != null)
                    {
                        await cloudBlobContainer.DeleteIfExistsAsync();
                    }
                }
            }
            return (wasSuccessful, uploadUri, blobContainerReference);
        }

        public async Task<bool> DeleteBlob(string blobContainerReference)
        {
            bool wasSuccessful = false;
            try
            {
                if (CloudStorageAccount.TryParse(azureBlobConnectionString, out var storageAccount))
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainerReference);
                    if (cloudBlobContainer != null)
                    {
                        await cloudBlobContainer.DeleteIfExistsAsync();
                        wasSuccessful = true;
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Log exception
            }

            return wasSuccessful;
        }
    }
}
