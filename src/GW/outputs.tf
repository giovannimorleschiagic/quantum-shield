output "resource_group_name" {
  description = "Name of the resource group created for the gateway stack."
  value       = azurerm_resource_group.this.name
}

output "application_gateway_public_ip" {
  description = "Public IP address assigned to the Application Gateway."
  value       = azurerm_public_ip.app_gateway.ip_address
}

output "application_gateway_name" {
  description = "Name of the Azure Application Gateway resource."
  value       = azurerm_application_gateway.this.name
}

output "backend_web_app_name" {
  description = "Name of the sample backend App Service."
  value       = azurerm_linux_web_app.backend.name
}

output "backend_web_app_hostname" {
  description = "Default hostname of the App Service backend."
  value       = azurerm_linux_web_app.backend.default_hostname
}