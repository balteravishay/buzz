using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Buzz.Client
{
    /// <summary>
    /// Client Function - user interaction
    /// provide Post and Delete API to create and delete load clusters
    /// </summary>
    public static class DeleteClusterClient
    {
        [FunctionName("DeleteClusterClient")]
        public static async Task<HttpResponseMessage> DeleteCluster([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter, ILogger log)
        {
            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            var name = (string)data?.name;
            if (string.IsNullOrEmpty(name))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Bad input. please supply name");
            }
            log.LogInformation($"delete cluster name: {name}");
            var response = starter.StartNewAsync("DeleteCluster", name);
            return req.CreateResponse(HttpStatusCode.OK, $"started delete process id:{response.Id}");
        }        
    }
}
