using Buzz.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Threading.Tasks;

namespace Buzz.Orchestrator
{
    /// <summary>
    /// Orchestrator Function - describe the way and order actions are executed
    /// Operate delete cluster tasks.
    /// after each partition is deleted the orchestration sleeps for configurable number of minutes (waitTime).
    /// </summary>
    public static class DeleteClusterOrchestrator
    {
        [FunctionName("DeleteCluster")]
        public static async Task Run([OrchestrationTrigger]DurableOrchestrationContext context, 
            ILogger log)
        {
            var waitTime = int.Parse(ConfigurationManager.AppSettings["WaitTime"]);
            
            var input = new { Name = context.GetInput<string>() };
            log.LogInformation($"orchestrate delete cluster name {input.Name}");

            var groupNames = await context.GetResourcesActivity(input.Name);

            foreach (var task in groupNames
                .MakeInterleavedCalls(
                t=> context.DeleteActivity(input.Name, t),
                t=> context.WaitTask(waitTime)))

                await task();

            await context.DeleteResourceGroupActivity(input.Name);
        }
    }
}
