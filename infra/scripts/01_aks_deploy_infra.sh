#!/bin/bash

###################
### Infra Setup ###
###################

### Set parameters
project="oss"
locationLong="westeurope"
locationShort="euw"
stageLong="dev"
stageShort="d"
instance="001"

shared="shared"
platform="platform"

### Set variables

# Shared
sharedResourceGroupName="rg${project}${locationShort}${shared}x000"
sharedStorageAccountName="st${project}${locationShort}${shared}x000"

# Platform
projectResourceGroupName="rg${project}${locationShort}${platform}${stageShort}${instance}"
projectAksName="aks${project}${locationShort}${platform}${stageShort}${instance}"
projectAksNodepoolResourceGroupName="aksrg${project}${locationShort}${platform}${stageShort}${instance}"

##############
### Shared ###
##############

### Shared Terraform storage account

# Resource group
echo "Checking shared resource group [${sharedResourceGroupName}]..."
sharedResourceGroup=$(az group show \
  --name $sharedResourceGroupName \
  2> /dev/null)

if [[ $sharedResourceGroup == "" ]]; then
  echo " -> Shared resource group does not exist. Creating..."

  sharedResourceGroup=$(az group create \
    --name $sharedResourceGroupName \
    --location $locationLong)

  echo -e " -> Shared resource group is created successfully.\n"
else
  echo -e " -> Shared resource group already exists.\n"
fi

# Storage account
echo "Checking shared storage account [${sharedStorageAccountName}]..."
sharedStorageAccount=$(az storage account show \
    --resource-group $sharedResourceGroupName \
    --name $sharedStorageAccountName \
  2> /dev/null)

if [[ $sharedStorageAccount == "" ]]; then
  echo " -> Shared storage account does not exist. Creating..."

  sharedStorageAccount=$(az storage account create \
    --resource-group $sharedResourceGroupName \
    --name $sharedStorageAccountName \
    --sku "Standard_LRS" \
    --encryption-services "blob")

  echo -e " -> Shared storage account is created successfully.\n"
else
  echo -e " -> Shared storage account already exists.\n"
fi

# Terraform blob container
echo "Checking Terraform blob container [${project}]..."
terraformBlobContainer=$(az storage container show \
  --account-name $sharedStorageAccountName \
  --name $project \
  2> /dev/null)

if [[ $terraformBlobContainer == "" ]]; then
  echo " -> Terraform blob container does not exist. Creating..."

  terraformBlobContainer=$(az storage container create \
    --account-name $sharedStorageAccountName \
    --name $project \
    2> /dev/null)

  echo -e " -> Terraform blob container is created successfully.\n"
else
  echo -e " -> Terraform blob container already exists.\n"
fi
#########

################
### Platform ###
################

# Set variables
azureAccount=$(az account show)
tenantId=$(echo $azureAccount | jq .tenantId)
subscriptionId=$(echo $azureAccount | jq .id)

# Create backend config
echo -e 'use_microsoft_graph=false
tenant_id='"${tenantId}"'
subscription_id='"${subscriptionId}"'
resource_group_name=''"'${sharedResourceGroupName}'"''
storage_account_name=''"'${sharedStorageAccountName}'"''
container_name=''"'${project}'"''
key=''"'${stageShort}${instance}.tfstate'"''' \
> ../terraform/01_platform_aks/backend.config

# Initialise Terraform
terraform -chdir=../terraform/01_platform_aks init \
  --backend-config="./backend.config"

# Plan Terraform
terraform -chdir=../terraform/01_platform_aks plan \
  -var project=$project \
  -var location_long=$locationLong \
  -var location_short=$locationShort \
  -var stage_short=$stageShort \
  -var stage_long=$stageLong \
  -var instance=$instance \
  -var project_resource_group_name=$projectResourceGroupName \
  -var project_kubernetes_cluster_name=$projectAksName \
  -var project_kubernetes_cluster_nodepool_name=$projectAksNodepoolResourceGroupName \
  -out "./tfplan"

# Apply Terraform
terraform -chdir=../terraform/01_platform_aks apply tfplan

# Get AKS credentials
az aks get-credentials \
  --resource-group $projectResourceGroupName \
  --name $projectAksName \
  --overwrite-existing
#########