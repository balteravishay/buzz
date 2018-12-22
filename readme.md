# Introduction

Scaling a cluster of load-test clients (VMs) to order of 10K+ instances using Azure Virtual Machine Scale Sets (VMSS) and Durable Functions.

# Information

This example shows how to scale load test scenarios to order of 10K+ instances using a number of VMSS instances.
It works by deploying ARM templates containing VNET, Load Balancer and VMSS for every 800 VMs (configurable). 


# Deployment

1. create a storage

az group create --name buzz-rg --location westeurope

az storage account create --name buzzdata --resource-group buzz-rg --location westeurope --sku Standard_LRS

az storage container create --name applications --account-name buzzdata public-access blob

az storage blob copy start --destination-blob wget.exe --destination-container applications --account-name buzzdata --source-uri https://eternallybored.org/misc/wget/releases/wget-1.20-win64.zip	

note name and storage key

2. register application for ARM RBAC

az ad sp create-for-rbac --name "buzz-app-rbac" --role="Contribor" --scopes="/subscriptions/12931070-7cce-44a7-bd9a-36159c20f8d0"

note app id, password and tenant id

3. create oms workspace

https://docs.microsoft.com/en-us/azure/azure-monitor/learn/quick-create-workspace

note workspace id and key

4. create function

az storage account create --name buzzfunctionstorage --location westeurope --resource-group buzz-rg --sku Standard_LRS

az functionapp create --name buzzfunctionapp --storage-account buzzfunctionstorage --consumption-plan-location westeurope --resource-group buzz-rg 

az functionapp config appsettings set --name buzzfunctionapp  --resource-group buzz-rg --settings FUNCTIONS_EXTENSION_VERSION=~2
 
5. deploy and configure

config!!!

az functionapp deployment source config --resource-group buzz-rg --name buzzfunctionapp --repo-url https://github.com/balteravishay/buzz.git

6. run