using System.Dynamic;
using Newtonsoft.Json;

namespace Buzz.Model
{
    /// <summary>
    /// ARM Template Parameters object
    /// </summary>
    internal class Parameters
    {
        private Parameters(string deploymentName, string addressPrefix,
            string[] fileUris, string commandToExecute,
            string userName, string password, int count, string omsId, string omsKey)
        {
            AddressPrefix = addressPrefix;
            DeploymentName = deploymentName;
            FileUris = fileUris;
            CommandToExecute = commandToExecute;
            AdminUserName = userName;
            AdminPassword = password;
            Count = count;
            OmsId = omsId;
            OmsKey = omsKey;
        }
        
        /// <summary>
        /// VNet address prefix
        /// </summary>
        public string AddressPrefix { get;  }
        
        /// <summary>
        /// ARM Deployment Name
        /// </summary>
        public string DeploymentName { get; }

        /// <summary>
        /// VM Custom Scripts extension download file URIs
        /// </summary>
        public string[] FileUris { get; }

        /// <summary>
        /// VM Custom Scripts extension command line
        /// </summary>
        public string CommandToExecute { get; }

        /// <summary>
        /// Node VM Admin User Name
        /// </summary>
        public string AdminUserName { get; }

        /// <summary>
        /// Node VM Admin Password
        /// </summary>
        public string AdminPassword { get; }

        /// <summary>
        /// Size of Virtual Machine Scale Set
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// OMS Log Analytics Workspace Key
        /// </summary>
        public string OmsKey {get;}

        /// <summary>
        /// OMS Log Analytics Workspace Id
        /// </summary>
        public string OmsId {get;}

        /// <summary>
        /// Create a JSON representation of Parameteres
        /// </summary>
        /// <returns></returns>
        internal string CreateJson()
        {
            dynamic parameters = new ExpandoObject();

            parameters.deploymentName = GetValueObject(DeploymentName);
            parameters.scriptFileUri = GetValueObject(FileUris);
            parameters.commandToExecute = GetValueObject(CommandToExecute);
            parameters.adminUsername = GetValueObject(AdminUserName);
            parameters.adminPassword = GetValueObject(AdminPassword);
            parameters.count = GetValueObject(Count);
            parameters.omsId = GetValueObject(OmsId);
            parameters.omskey = GetValueObject(OmsKey);
            parameters.addressPrefix = GetValueObject(AddressPrefix);
            return JsonConvert.SerializeObject(parameters);
        }

        private static dynamic GetValueObject(object value)
        {
            dynamic objectValue = new ExpandoObject();
            objectValue.value = value;
            return objectValue;
        }

        /// <summary>
        /// Create an ARM Template Properties obejct
        /// </summary>
        /// <param name="deploymentName"> Deployment Name</param>
        /// <param name="addressPrefix">VNet Address prefix</param>
        /// <param name="fileUris">Download File Uris</param>
        /// <param name="commandToExecute">Custom Script Command</param>
        /// <param name="adminUserName">Node VM Admin User Name</param>
        /// <param name="adminPassword">Node VM Admin User Password</param>
        /// <param name="count">VMM Size</param>
        /// <param name="omsId">Log Analytics Workspace ID</param>
        /// <param name="omsKey">Log Analytics Workspace Key</param>
        /// <returns></returns>
        internal static Parameters Make(string deploymentName,
            string addressPrefix,string[] fileUris, string commandToExecute,
            string adminUserName, string adminPassword, int count, string omsId, string omsKey)=>
            new Parameters(deploymentName, addressPrefix, 
                fileUris, commandToExecute, adminUserName, adminPassword, count, omsId, omsKey);
        /// <summary>
        /// 
        /// override ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"Name: {DeploymentName} Address: {AddressPrefix} Count: {Count} UserName: {AdminUserName} Password: {AdminPassword} ScriptFile: {string.Join(",", FileUris)} Commnad: {CommandToExecute} ";
        }
    }
}