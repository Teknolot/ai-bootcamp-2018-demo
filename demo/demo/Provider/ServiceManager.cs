using demo.Helpers;
using Newtonsoft.Json;
using Plugin.Media.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace demo.Provider
{
    public class ServiceManager : IServiceManager
    {
        public async Task<T> Post<T, K>(K req, string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var request = await client.PostAsync(url,
                    new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json"));

                if (request.IsSuccessStatusCode && request.StatusCode == HttpStatusCode.OK)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    if (!String.IsNullOrEmpty(response))
                        return JsonConvert.DeserializeObject<T>(response);
                }
                return default(T);
            }
        }

        public async Task<string> Upload(MediaFile media)
        {
            var imageStream = new ByteArrayContent(ByteConverter.ReadFully(media.GetStream()));
            imageStream.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"{Guid.NewGuid()}-.png"
            };
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                imageStream
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
                client.BaseAddress = new Uri("https://face-validation-functions.azurewebsites.net/api/");
                var request = await client.
                    PostAsync("UploadFile?code=dnkL554a8cNwa67UpWl4LOogqAGCPVxD6G5CkI5NUSieiTTtlkJQyw==",
                    form);

                if (request.IsSuccessStatusCode && request.StatusCode == HttpStatusCode.OK)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    if (!String.IsNullOrEmpty(response))
                    {
                        var url = response.Replace("[\"", "").Replace("\"]", "");
                        return url;
                    }
                }
                return "";
            }
        }
    }
}