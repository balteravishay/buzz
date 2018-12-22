using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Buzz.Orchestrator
{
    /// <summary>
    /// Orchestrator Function - describe the way and order actions are executed
    /// Operate create cluster tasks.
    /// after each partition is created the orchestration sleeps for configurable number of minutes (waitTime).
    /// </summary>
    public static class CreateClusterOrchestrator
    {
        [FunctionName("CreateCluster")]
        public static async Task Run([OrchestrationTrigger]DurableOrchestrationContext context, 
            ILogger log, ExecutionContext executionContext)
        {
            var config = executionContext.BuildConfiguration();

            var vmsinScaleSet = int.Parse(config["MaxVmsInScaleSet"]);
            var waitTime = int.Parse(config["WaitTime"]);

            var input = new {
                Name = context.GetInput<Tuple<string, int>>().Item1,
                Scale = context.GetInput<Tuple<string, int>>().Item2
            }; 
            log.LogInformation($"orchestrate create cluster name {input.Name} with scale {input.Scale}");
            await context.CreateResourceGroupActivity(input.Name);
            foreach (var task in input.Scale
                .PartitionSum(NonZeroInt.Make(vmsinScaleSet))
                .MakeInterleavedCalls(
                    t =>
                    {
                        context.CopyStoargeTask(input.Name, t.Item1);
                        return context.ProvisionTask(input.Name, t.Item1, t.Item2);
                    },
                    t => context.WaitTask(waitTime)))

                await task();
        }
    }
}
