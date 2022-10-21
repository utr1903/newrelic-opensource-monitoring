### Resource Group ###

# Resource Group
resource "azurerm_resource_group" "platform" {
  name     = var.project_resource_group_name
  location = var.location_long
}
