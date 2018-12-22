using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Buzz.Client
{
    /// <summary>
    /// Client Function - user interaction
    /// provide Post and Delete API to create and delete load clusters
    /// </summary>
    public static class PostClusterClient
    {
        [FunctionName("PostClusterClient")]
        public static async Task<HttpResponseMessage> PostCluster([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter, ILogger log)
        {
            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            var scale = (int)data?.scale;
            var name = (string)data?.name;
            if (scale == 0 || string.IsNullOrEmpty(name))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Bad input. please supply name and scale > 0");
            }
            log.LogInformation($"create cluster name: {name} scale: {scale}");
            var response = starter.StartNewAsync("CreateCluster", new Tuple<string, int>(name, scale));
            return req.CreateResponse(HttpStatusCode.OK, $"started create process id:{response.Id}");
        }        
    }
}
