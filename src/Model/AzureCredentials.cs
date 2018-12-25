namespace Buzz.Model
{
    /// <summary>
    /// Credentials for ARM API
    /// </summary>
    class AzureCredentials
    {
        private AzureCredentials(string tenantId, string applicationId, string applicationSecret, string subscriptionId)
        {
            TenantId = tenantId;
            ApplicationId = applicationId;
            ApplicationSecret = applicationSecret;
            SubscriptionId = subscriptionId;
        }

        /// <summary>
        /// Tenant Id
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// Application\Client ID
        /// </summary>
        public string ApplicationId { get; }

        /// <summary>
        /// Application\Client Secret
        /// </summary>
        public string ApplicationSecret { get; }

        /// <summary>
        /// Subscription Id
        /// </summary>
        public string SubscriptionId { get; }

        /// <summary>
        /// Create ARM API credentials object
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="applicationId">Application Id</param>
        /// <param name="applicationSecret">Application Secret</param>
        /// <param name="subscriptionId">Subscription Id</param>
        /// <returns></returns>
        public static AzureCredentials Make(string tenantId, string applicationId,
            string applicationSecret, string subscriptionId) =>
            new AzureCredentials(tenantId, applicationId, applicationSecret, subscriptionId);
    }
}