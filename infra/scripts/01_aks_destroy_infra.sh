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

# Platform
projectResourceGroupName="rg${project}${locationShort}${platform}${stageShort}${instance}"

projectAksName="aks${project}${locationShort}${platform}${stageShort}${instance}"
projectAksNodepoolResourceGroupName="aksrg${project}${locationShort}${platform}${stageShort}${instance}"

### Terraform destroy

terraform -chdir=../terraform/01_platform_aks destroy \
  -var project=$project \
  -var location_long=$locationLong \
  -var location_short=$locationShort \
  -var stage_short=$stageShort \
  -var stage_long=$stageLong \
  -var instance=$instance \
  -var project_resource_group_name=$projectResourceGroupName \
  -var project_kubernetes_cluster_name=$projectAksName \
  -var project_kubernetes_cluster_nodepool_name=$projectAksNodepoolResourceGroupName
