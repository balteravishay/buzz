namespace Buzz.Model
{
    class AzureCredentials
    {
        private AzureCredentials(string tenantId, string applicationId, string applicationSecret, string subscriptionId)
        {
            TenantId = tenantId;
            ApplicationId = applicationId;
            ApplicationSecret = applicationSecret;
            SubscriptionId = subscriptionId;
        }

        public string TenantId { get; }

        public string ApplicationId { get; }

        public string ApplicationSecret { get; }

        public string SubscriptionId { get; }

        public static AzureCredentials Make(string tenantId, string applicationId,
            string applicationSecret, string subscriptionId) =>
            new AzureCredentials(tenantId, applicationId, applicationSecret, subscriptionId);
    }
}