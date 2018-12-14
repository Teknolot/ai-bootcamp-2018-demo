using ai_functions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ai_functions
{
    public static class UploadFile
    {
        [FunctionName("UploadFile")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            #region Config
            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            string storageConnectionString = config["AzureWebJobsStorage"];
            #endregion

            #region Setting Up Cloud Storage
            CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount);
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("tempphotos");
            await cloudBlobContainer.CreateIfNotExistsAsync();
            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await cloudBlobContainer.SetPermissionsAsync(permissions);
            #endregion

            #region Multipart Receive and Upload
            List<string> filesUploaded = new List<string>();
            var boundary = MultipartRequestHelper.GetBoundary(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(req.ContentType), int.MaxValue);
            var reader = new MultipartReader(boundary, req.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                Microsoft.Net.Http.Headers.ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(contentDisposition.FileName.Value);
                    await cloudBlockBlob.UploadFromStreamAsync(section.Body);
                    filesUploaded.Add(cloudBlockBlob.Uri.ToString());
                }
                section = await reader.ReadNextSectionAsync();
            }
            #endregion

            #region Return JSON
            var jsonToReturn = JsonConvert.SerializeObject(filesUploaded.ToArray());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
            #endregion
        }
    }
}
