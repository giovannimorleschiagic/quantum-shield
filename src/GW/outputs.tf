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

output "key_vault_name" {
  description = "Name of the Azure Key Vault."
  value       = azurerm_key_vault.this.name
}

output "key_vault_uri" {
  description = "URI of the Azure Key Vault."
  value       = azurerm_key_vault.this.vault_uri
}

output "sql_server_fqdn" {
  description = "Fully qualified domain name of the Azure SQL Server."
  value       = azurerm_mssql_server.this.fully_qualified_domain_name
}

output "sql_database_name" {
  description = "Name of the Azure SQL Database."
  value       = azurerm_mssql_database.this.name
}

output "storage_account_name" {
  description = "Name of the Blob Storage account."
  value       = azurerm_storage_account.this.name
}

output "blob_container_name" {
  description = "Name of the blob container."
  value       = azurerm_storage_container.blob.name
}