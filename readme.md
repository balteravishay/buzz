[![Build Status](https://abalterteam.visualstudio.com/LoadGenerator/_apis/build/status/LoadGenerator-ASP.NET-CI%20(1)?branchName=master)](https://abalterteam.visualstudio.com/LoadGenerator/_build/latest?definitionId=50?branchName=master)

# Introduction

Buzz is a scaling platform which allows Azure Virtual Machine Scale Sets (VMSS) to scale beyond the limits of a single set and enables hyper-scale stress tests, DDoS simulators and HPC use cases.

# Information

Buzz orchestrates a number of Azure components to manage high scale clusters of VMs running and performing the same actions, such as generating load on an endpoint, as in the implemented scenario.  
Buzz uses Durable Functions runtime exposing the following APIs:

## Create Cluster: 

**POST:** [FUNCTION_URL]/PostClusterClient?code=[YOUR FUNCTION KEY]

body:
{
    "name": "LOAD",
    "scale": 1000
}

## Delete Cluster: 

**DELETE:** [FUNCTION_URL]/DeleteClusterClient?code=[YOUR FUNCTION KEY]

body:
{
    "name": "LOAD"
}

# Deploy to Azure

**1. create a storage**

resource_group=[resource group name]
location=westeurope
source_storage_name=[source storage name]
source_application_folder=applications
source_client_application_uri=https://eternallybored.org/misc/wget/1.20/64/wget.exe
destination_blob_name=wget.exe

az group create --name $resource_group --location $location

az storage account create --name $source_storage_name --resource-group $resource_group --location $location --sku Standard_LRS

az storage container create --name $source_application_folder --account-name $source_storage_name --public-access blob

az storage blob copy start --destination-blob $destination_blob_name --destination-container $source_application_folder --account-name $source_storage_name --source-uri $source_client_application_uri

source_storage_key=[storage account key]

**2. register application for ARM RBAC**
subscription_id=[subscription id]
rbac_name=[rbac application name]
az ad sp create-for-rbac --name $rbac_name --role="Contributor" --scopes=/subscriptions/$subscription_id

sp_app_id = [client id]
sp_app_secret=[client password]
tenant_id=[tenant id]

**3. create oms workspace**

https://docs.microsoft.com/en-us/azure/azure-monitor/learn/quick-create-workspace

oms_workspace_id=[oms workspace id]
oms_workspace_key=[oms workspace key]

**4. create function**
function_name=[function app name]
function_storage_name=[function storage name]
az storage account create --name $function_storage_name --location $location --resource-group $resource_group --sku Standard_LRS

az functionapp create --name $function_name --storage-account $function_storage_name --consumption-plan-location $location --resource-group $resource_group 

az functionapp config appsettings set --name $function_name  --resource-group $resource_group --settings FUNCTIONS_EXTENSION_VERSION=~2

**5. [OPTIONAL]: deploy test function**
test_function_name=[function app name]
test_function_storage_name=[function storage name]
az storage account create --name $test_function_storage_name --location $location --resource-group $resource_group --sku Standard_LRS

az functionapp create --name $test_function_name --storage-account $test_function_storage_name --consumption-plan-location $location --resource-group $resource_group 

url=[function url with code]

**6. deploy and configure**

script_file_uri=https://raw.githubusercontent.com/balteravishay/buzz/master/scripts/install.ps1
deployment_template_uri=https://raw.githubusercontent.com/balteravishay/buzz/master/scripts/deploy.json
command_to_execute="powershell.exe set-executionpolicy remotesigned & powershell.exe -file install.ps1 -url $url"
node_admin_name=azureuser
node_admin_password=Qwerty12#456
az functionapp config appsettings set --resource-group $resource_group --name $function_name --settings ApplicationId=$sp_app_id ApplicationSecret=$sp_app_secret TenantId=$tenant_id SubscriptionId=$subscription_id SourceStorageName=$source_storage_name SourceStorageKey=$source_storage_key SourceAppsContainerName=$source_application_folder ScriptFileUri=$script_file_uri TemplateFileUri=$deployment_template_uri CommandToExecute="$command_to_execute" Region=$location NodeUserName=$node_admin_name NodeUserPassword=$node_admin_password MaxVmsInScaleSet=600 WaitTime=6 OmsWorkspaceId=$oms_workspace_id OmsWorkspaceKey=$oms_workspace_key

az functionapp deployment source config --resource-group $resource_group --name $function_name --repo-url https://github.com/balteravishay/buzz.git

# Deploy Locally

clone the repo to your local drive and configure the provided settings.json according to the previous steps.

# Modifications and Customizations

The platform can run any client application with a few minor changes.

* Any item in "applications" container of the data storage account will be downloaded to each VM node.
* To change node behavior and client installations process reference your own customized powershell script from the function configuration.
* To Change VM type or OS, reference your own customized ARM template from the function configuration.

# More Resources

Full code story and design considerations here: http://airheads.io/index.php/2018/12/22/to-infinity-and-beyond-or-the-definitive-guide-to-scaling-10k-vms-on-azure/
