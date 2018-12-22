using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;

namespace Buzz.Activity
{
    public class DeleteResourcesGroupActivity
    {
        private static ILogger _log;

        [FunctionName("DeleteResourceGroupActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log,
            ExecutionContext executionContext)
        {
            _log = log;
            var input = new { ResourceGroupName = context.GetInput<string>() };
            var config = executionContext.BuildConfiguration();
            var clientId = config["ApplicationId"];
            var clientSecret = config["ApplicationSecret"];
            var tenantId = config["TenantId"];
            string subscriptionId = config["SubscriptionId"];

            log.LogInformation($"start delete group {input.ResourceGroupName} ");

            try
            {
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out IAzure azure, message => _log.LogError(message))) return;
                azure.DeleteGroup(input.ResourceGroupName, s=> _log.LogWarning(s));
            }
            catch(Exception e)
            {
                _log.LogError($"error deleting groups {e.Message}", e);
            }
        }
    }
}