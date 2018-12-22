namespace Buzz.Model
{
    class IpConfiguration
    {
        public string IpConfigurationId { get; }
        public string PublicIpAddress { get; }

        private IpConfiguration(string ipConfigurationId, string publicIpAddress)
        {
            IpConfigurationId = ipConfigurationId;
            PublicIpAddress = publicIpAddress;
        }

        public static IpConfiguration Make(string ipConfigurationId, string publicIpAddress)
        {
            return new IpConfiguration(ipConfigurationId, publicIpAddress);
        }
    }
}