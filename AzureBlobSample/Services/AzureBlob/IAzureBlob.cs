using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureBlobSample.Services.AzureBlob
{
    public interface IAzureBlob
    {
        Task<(bool wasSuccesful, Uri uploadUrl, string blobContainerReference)> UploadToBlob(IFormFile formfile, string fileName = "");
        Task<(bool wasSuccesful, Uri uploadUrl, string blobContainerReference)> UploadToBlob(Stream stream, string fileName);
        Task<bool> DeleteBlob(string blobContainerReference);
    }
}
