using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;

namespace ai_functions
{
    public static class ValidateFace
    {
        private const string PersonGroupId = "yazilimcilar";

        [FunctionName("ValidateFace")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var faceEndpoint = config["faceEndpoint"];
            var subscriptionKey = config["faceSubscriptionKey"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string imageUrl = data?.imageUrl;
            string personName = data?.personName;
            string personId = data?.personId;

            FaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey), new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = faceEndpoint
            };

            IList<DetectedFace> foundFaces = await faceClient.Face.DetectWithUrlAsync(imageUrl, true);
            var result = await faceClient.Face.IdentifyAsync(foundFaces.Select(x=> x.FaceId.Value).ToList(), PersonGroupId, maxNumOfCandidatesReturned: 3);
            var foundPersonId = result.FirstOrDefault().Candidates.FirstOrDefault().PersonId;
            Person human = await faceClient.PersonGroupPerson.GetAsync(PersonGroupId, foundPersonId);

            var myObj = new { name = human.Name, personId = human.PersonId };
            var jsonToReturn = JsonConvert.SerializeObject(myObj);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
        }
    }
}
