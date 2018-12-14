using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ai_functions
{
    public static class AddFace
    {
        private const string PersonGroupId = "yazilimcilar";

        [FunctionName("AddFace")]
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

            var faceEndpoint = config["faceEndpoint"];
            var subscriptionKey = config["faceSubscriptionKey"];
            #endregion

            #region Read Body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string imageUrl = data?.imageUrl;
            string personName = data?.personName;
            string personId = data?.personId;
            #endregion

            #region Cognitive Services Calls
            FaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey), new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = faceEndpoint
            };

            //Sample Person Group is created at first run for demo purposes.
            //await faceClient.PersonGroup.CreateAsync(PersonGroupId, PersonGroupId); 
            PersonGroup humanGroup = await faceClient.PersonGroup.GetAsync(PersonGroupId);
            await faceClient.PersonGroup.TrainAsync(PersonGroupId);

            Person human = null;
            if (string.IsNullOrEmpty(personId))
            {
                human = await faceClient.PersonGroupPerson.CreateAsync(humanGroup.PersonGroupId, personName);
            }
            else
            {
                human = await faceClient.PersonGroupPerson.GetAsync(humanGroup.PersonGroupId, new System.Guid(personId));
            }
          
            PersistedFace face = await faceClient.PersonGroupPerson.AddFaceFromUrlAsync(humanGroup.PersonGroupId, human.PersonId, imageUrl);
            #endregion

            #region Return JSON
            var myObj = new { faceId = face.PersistedFaceId, personId = human.PersonId };
            var jsonToReturn = JsonConvert.SerializeObject(myObj);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            };
            #endregion
        }
    }
}
