using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Buzz.Extensions
{
    /// <summary>
    /// Calls to Durable Function activities and timers. 
    /// </summary>
    static class DurableContextExtensions
    {
        public static IConfigurationRoot BuildConfiguration(this ExecutionContext context) =>
            new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        public static Task WaitTask(this DurableOrchestrationContext @this, int waitTime) =>
            @this.CreateTimer(
                DateTime.UtcNow.Add(TimeSpan.FromMinutes(waitTime)),
                CancellationToken.None);
        
        public static Task ProvisionTask(this DurableOrchestrationContext @this, string name, int index, int scale, string storageKey) =>
            @this.CallActivityAsync("ProvisionActivity", Tuple.Create(name, index, scale, storageKey));

        public static Task<string[]> GetResourcesActivity(this DurableOrchestrationContext @this, string resourceGroupName) =>
            @this.CallActivityAsync<string[]>("GetResourcesActivity", resourceGroupName);

        public static Task DeleteActivity(this DurableOrchestrationContext @this, string resourceGroupName, string resourcesName) =>
            @this.CallActivityAsync("DeleteActivity", Tuple.Create(resourceGroupName, resourcesName));

        public static Task CreateResourceGroupActivity(this DurableOrchestrationContext @this, string name) =>
            @this.CallActivityAsync("CreateResourceGroupActivity", name);

        public static Task DeleteResourceGroupActivity(this DurableOrchestrationContext @this, string name) =>
            @this.CallActivityAsync("DeleteResourceGroupActivity", name);

        public static Task<string> CopyStoargeTask(this DurableOrchestrationContext @this, string name, int index) =>
            @this.CallActivityAsync<string>("DuplicateStorageActivity", Tuple.Create(name, index));


    }
}
