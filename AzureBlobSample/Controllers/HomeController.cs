using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AzureBlobSample.Models;
using AzureBlobSample.Services.AzureStorage;
using Microsoft.AspNetCore.Http;

namespace AzureBlobSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAzureBlob azureBlob;

        public HomeController(IAzureBlob azureBlob)
        {
            this.azureBlob = azureBlob;
        }

        public IActionResult Index()
        {

            HomeViewModel viewModel = new HomeViewModel() {LastSavedImageUrl = TempData["mydata"] as string, BlobContainerReference = TempData["blobecontainer"] as string};
           
            return View(viewModel);
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> Post(HomeViewModel inputModel)
        {
            var result = await azureBlob.UploadToBlob(inputModel.FormFile);

            if (result.wasSuccesful)
            {
                // car.ImageUrl = result.uploadUrl.ToString()
                // car.ImageBlobContainerReference = result.blobContainerReference
                // Save car object to Database
            }

            TempData["mydata"] =  result.uploadUrl.ToString();
            TempData["blobecontainer"] = result.blobContainerReference;
            return RedirectToAction("Index");
           
        }

        [HttpPost("DeleteBlob")]
        public async Task<IActionResult> DeleteBlob(HomeViewModel inputModel)
        {
            var uploadSuccess = await azureBlob.DeleteBlob(inputModel.BlobContainerReference);

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class HomeViewModel
    {
        public IFormFile FormFile { get; set; }
        public string BlobContainerReference { get; set; }
        public string LastSavedImageUrl { get; set; }
    }
}
