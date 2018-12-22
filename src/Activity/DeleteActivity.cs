using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;

namespace Buzz.Activity
{
    public class DeleteActivity
    {
        private static ILogger _log;

        [FunctionName("DeleteActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log,
            ExecutionContext executionContext)
        {
            _log = log;
            var input = new
            {
                ResourceGroupName = context.GetInput<Tuple<string, string>>().Item1,
                ResourcesName = context.GetInput<Tuple<string, string>>().Item2
            };
            var config = executionContext.BuildConfiguration();
            var clientId = config["ApplicationId"];
            var clientSecret = config["ApplicationSecret"];
            var tenantId = config["TenantId"];
            string subscriptionId = config["SubscriptionId"];
            log.LogInformation($"DeleteResourcesActivity start process delete group {input.ResourcesName} ");

            try
            {
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out IAzure azure, message => _log.LogError(message))) return;
                azure.DeleteResourcesInGroup(input.ResourceGroupName, input.ResourcesName, s => _log.LogWarning(s));
            }
            catch(Exception e)
            {
                _log.LogError($"GetGroupsActivity error processing function {e.Message}", e);
            }
        }
    }
}