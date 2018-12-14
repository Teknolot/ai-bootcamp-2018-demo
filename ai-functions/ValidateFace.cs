using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ai_functions
{
    public static class ValidateFace
    {
        private const string PersonGroupId = "yazilimcilar";

        [FunctionName("ValidateFace")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            #region Config
            var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var faceEndpoint = config["faceEndpoint"];
            var subscriptionKey = config["faceSubscriptionKey"];
            #endregion

            #region Read Body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string imageUrl = data?.imageUrl;
            #endregion

            #region Cognitive Services Calls
            FaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey), new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = faceEndpoint
            };

            IList<DetectedFace> foundFaces = await faceClient.Face.DetectWithUrlAsync(imageUrl, true);
            var result = await faceClient.Face.IdentifyAsync(foundFaces.Select(x=> x.FaceId.Value).ToList(), PersonGroupId, maxNumOfCandidatesReturned: 3);
            var foundPersonId = result.FirstOrDefault()?.Candidates.FirstOrDefault()?.PersonId;
            Person human = await faceClient.PersonGroupPerson.GetAsync(PersonGroupId, foundPersonId.GetValueOrDefault());
            #endregion

            #region Return JSON
            var myObj = new { name = human.Name, personId = human.PersonId };
            var jsonToReturn = JsonConvert.SerializeObject(myObj);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
            #endregion
        }
    }
}
