using Buzz.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
            ILogger log, ExecutionContext executionContext)
        {
            //config and input
            var config = executionContext.BuildConfiguration();
            var waitTime = int.Parse(config["WaitTime"]);
            var input = new { Name = context.GetInput<string>() };
            log.LogInformation($"orchestrate delete cluster name {input.Name}");

            // get names of VMSS components in resource group
            var groupNames = await context.GetResourcesActivity(input.Name);
            // partition to tasks of delete\wait interleaved calls
            foreach (var task in groupNames
                .MakeInterleavedCalls(
                t=> context.DeleteActivity(input.Name, t),
                t=> context.WaitTask(waitTime)))

                await task();
            // delete resource group with rest of components.
            await context.DeleteResourceGroupActivity(input.Name);
        }
    }
}
