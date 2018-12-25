# Introduction

Buzz is a scaling platform which allows Azure Virtual Machine Scale Sets (VMSS) to scale beyond the limits of a single set and enables hyper-scale stress tests, DDoS simulators and HPC use cases.

# Information

Buzz orchestrates a number of Azure components to manage high scale clusters of VMs running and performing the same actions, such as generating load on an endpoint, as in the implemented scenario.
Buzz uses Durable Functions runtime exposing the following APIs:

## Create Cluster: 

**POST:** [FUNCTION_URL]/PostCluster?code=[YOUR FUNCTION KEY]

body:
{
    "name": "LOAD",
    "scale": 1000
}

## Delete Cluster: 

**DELETE:** [FUNCTION_URL]/DeleteCluster?code=[YOUR FUNCTION KEY]

body:
{
    "name": "LOAD"
}

# Deploy to Azure

**1. create a storage**

az group create --name buzz-rg --location westeurope

az storage account create --name buzzdata --resource-group buzz-rg --location westeurope --sku Standard_LRS

az storage container create --name applications --account-name buzzdata public-access blob

az storage blob copy start --destination-blob wget.exe --destination-container applications --account-name buzzdata --source-uri https://eternallybored.org/misc/wget/1.20/64/wget.exe

source_storage_name=[your storage name, buzzdata]
source_storage_key=[storage account key]
source_application_folder=[your container name, applications]

**2. register application for ARM RBAC**

az ad sp create-for-rbac --name "buzz-app-rbac" --role="Contribor" --scopes="/subscriptions/[YOUR SUBSCRIPTION ID]"

sp_app_id = [client id]
sp_app_secret=[client password]
tenant_id=[tenant id]
subscription_id=[subscription id]

**3. create oms workspace**

https://docs.microsoft.com/en-us/azure/azure-monitor/learn/quick-create-workspace

oms_workspace_id=[oms workspace id]
oms_workspace_key=[oms workspace key]

**4. create function**

az storage account create --name buzzfunctionstorage --location westeurope --resource-group buzz-rg --sku Standard_LRS

az functionapp create --name buzzfunctionapp --storage-account buzzfunctionstorage --consumption-plan-location westeurope --resource-group buzz-rg 

az functionapp config appsettings set --name buzzfunctionapp  --resource-group buzz-rg --settings FUNCTIONS_EXTENSION_VERSION=~2

**5. [OPTIONAL]: deploy test function**

az storage account create --name buzzfunctionteststorage --location westeurope --resource-group buzz-rg --sku Standard_LRS

az functionapp create --name buzzfunctiontestapp --storage-account buzzfunctionteststorage --consumption-plan-location westeurope --resource-group buzz-rg 

url=[function url with code]

**6. deploy and configure**

az functionapp config appsettings set --resource-group buzz-rg --name buzzfunctionapp --settings ApplicationId=$sp_app_id ApplicationSecret=$sp_app_secret TenantId=tenant_id SubscriptionId=subscription_id SourceStorageName=source_storage_name SourceStorageKey=source_storage_key SourceAppsContainerName=source_application_folder ScriptFileUri="https://raw.githubusercontent.com/balteravishay/buzz/master/scripts/install.ps1" TemplateFileUri="https://raw.githubusercontent.com/balteravishay/buzz/master/scripts/deploy.json" CommandToExecute="powershell.exe set-executionpolicy remotesigned & powershell.exe -file install.ps1 -url $url" Region=westeurope NodeUserName=azureuser NodeUserPassword=Qwerty!23456 MaxVmsInScaleSet=600 WaitTime=6 OmsWorkspaceId=$oms_workspace_id OmsWorkspaceKey=$oms_workspace_id]

az functionapp deployment source config --resource-group buzz-rg --name buzzfunctionapp --repo-url https://github.com/balteravishay/buzz.git

# More Resources

Full code story and design considerations here: http://airheads.io/index.php/2018/12/22/to-infinity-and-beyond-or-the-definitive-guide-to-scaling-10k-vms-on-azure/
