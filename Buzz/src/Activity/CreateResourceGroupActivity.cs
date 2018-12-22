using Buzz.Extensions;
using Buzz.Model;
using Microsoft.Azure.WebJobs;
using System;
using System.Configuration;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;

namespace Buzz.Activity
{
    public class CreateResourceGroupActivity
    {
        private static ILogger _log;

        [FunctionName("CreateResourceGroupActivity")]
        public static void Run([ActivityTrigger] DurableActivityContext context, ILogger log)
        {
            _log = log;
            var input = new { ResourceGroupName = context.GetInput<string>() };

            var clientId = ConfigurationManager.AppSettings["ApplicationId"];
            var clientSecret = ConfigurationManager.AppSettings["ApplicationSecret"];
            var tenantId = ConfigurationManager.AppSettings["TenantId"];
            string subscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
            var region = ConfigurationManager.AppSettings["Region"];

            log.LogInformation($"DeleteResourcesGroupActivity start process delete group {input.ResourceGroupName} ");

            try
            {
                if (!AzureCredentials.Make(tenantId, clientId, clientSecret, subscriptionId)
                    .TryGetAzure(out IAzure azure, message => _log.LogError(message))) return;
                azure.GetOrCreateResourceGroup(input.ResourceGroupName, region);
            }
            catch(Exception e)
            {
                _log.LogError($"GetGroupsActivity error processing function {e.Message}", e);
            }
        }
    }
}