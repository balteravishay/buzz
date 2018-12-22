using System.Dynamic;
using Newtonsoft.Json;

namespace Buzz.Model
{
    internal class Parameters
    {
        private Parameters(string deploymentName, string addressPrefix,
            string scriptFileUri, string commandToExecute,
            string userName, string password, int count, string omsId, string omsKey)
        {
            AddressPrefix = addressPrefix;
            DeploymentName = deploymentName;
            ScriptFileUri = scriptFileUri;
            CommandToExecute = commandToExecute;
            AdminUserName = userName;
            AdminPassword = password;
            Count = count;
            OmsId = omsId;
            OmsKey = omsKey;
        }
        
        public string AddressPrefix { get;  }
        
        public string DeploymentName { get; }

        public string ScriptFileUri { get; }

        public string CommandToExecute { get; }

        public string AdminUserName { get; }

        public string AdminPassword { get; }

        public int Count { get; }

        public string OmsKey {get;}

        public string OmsId {get;}

        internal string CreateJson()
        {
            dynamic parameters = new ExpandoObject();

            parameters.deploymentName = GetValueObject(DeploymentName);
            parameters.scriptFileUri = GetValueObject(ScriptFileUri);
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

        internal static Parameters Make(string deploymentName,
            string addressPrefix,string scriptFileUri, string commandToExecute,
            string adminUserName, string adminPassword, int count, string omsId, string omsKey)
        {
            return new Parameters(deploymentName, addressPrefix, 
                scriptFileUri, commandToExecute, adminUserName, adminPassword, count, omsId, omsKey);
        }

        public override string ToString()
        {
            return
                $"Name: {DeploymentName} Address: {AddressPrefix} Count: {Count} UserName: {AdminUserName} Password: {AdminPassword} ScriptFile: {ScriptFileUri} Commnad: {CommandToExecute} ";
        }
    }
}