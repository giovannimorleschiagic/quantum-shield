variable "prefix" {
  description = "Prefix used for all Azure resource names."
  type        = string
  default     = "quantumshield"
}

variable "resource_group_name" {
  description = "Name of the Azure Resource Group to create."
  type        = string
  default     = "rg-teamorange"
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "westeurope"
}

variable "azure_subscription_id" {
  description = "Azure subscription ID used by the service principal."
  type        = string
}

variable "azure_tenant_id" {
  description = "Azure tenant ID used by the service principal."
  type        = string
}

variable "azure_client_id" {
  description = "Azure client ID (application ID) of the service principal."
  type        = string
}

variable "azure_client_secret" {
  description = "Azure client secret of the service principal."
  type        = string
  sensitive   = true
}

variable "app_service_sku_name" {
  description = "SKU for the Linux App Service Plan."
  type        = string
  default     = "B1"
}

variable "app_service_runtime_stack" {
  description = ".NET runtime stack for the sample Linux Web App backend."
  type        = string
  default     = "DOTNETCORE|10.0"
}

variable "key_vault_purge_protection_enabled" {
  description = "Enable purge protection on the Key Vault."
  type        = bool
  default     = false
}

variable "sql_administrator_login" {
  description = "Administrator login name for Azure SQL Server."
  type        = string
  default     = "sqladminuser"
}

variable "sql_administrator_password" {
  description = "Administrator password for Azure SQL Server."
  type        = string
  sensitive   = true
}

variable "sql_database_name" {
  description = "Name of the Azure SQL Database."
  type        = string
  default     = "quantumshielddb"
}

variable "sql_database_sku_name" {
  description = "SKU name for the Azure SQL Database."
  type        = string
  default     = "Basic"
}

variable "sql_database_max_size_gb" {
  description = "Maximum size of the Azure SQL Database in GB."
  type        = number
  default     = 2
}

variable "sql_allow_azure_services" {
  description = "Allow Azure services to access the Azure SQL Server (0.0.0.0 firewall rule)."
  type        = bool
  default     = true
}

variable "storage_replication_type" {
  description = "Replication type for Blob Storage account."
  type        = string
  default     = "LRS"
}

variable "blob_container_name" {
  description = "Name of the private blob container."
  type        = string
  default     = "app-data"
}

variable "tags" {
  description = "Tags applied to all supported resources."
  type        = map(string)
  default = {
    environment = "dev"
    workload    = "quantum-shield"
  }
}