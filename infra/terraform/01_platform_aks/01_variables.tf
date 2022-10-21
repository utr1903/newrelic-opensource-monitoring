### Variables ###

## General

# project
variable "project" {
  type    = string
  default = "nr1"
}

# location_long
variable "location_long" {
  type    = string
  default = "westeurope"
}

# location_short
variable "location_short" {
  type    = string
  default = "euw"
}

# stage_long
variable "stage_long" {
  type    = string
  default = "dev"
}

# stage_short
variable "stage_short" {
  type    = string
  default = "d"
}

# instance
variable "instance" {
  type    = string
  default = "001"
}

# platform
variable "platform" {
  type    = string
  default = "platform"
}

# k8s version
variable "kubernetes_version" {
  type    = string
  default = "1.24.6"
}

## Resource Names

# Resource Group
variable "project_resource_group_name" {
  type = string
}

# Kubernetes Cluster
variable "project_kubernetes_cluster_name" {
  type = string
}

variable "project_kubernetes_cluster_nodepool_name" {
  type = string
}
