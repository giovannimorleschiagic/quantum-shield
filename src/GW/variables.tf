variable "prefix" {
  description = "Prefix used for all Azure resource names."
  type        = string
  default     = "quantumshield"
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "westeurope"
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

variable "tags" {
  description = "Tags applied to all supported resources."
  type        = map(string)
  default = {
    environment = "dev"
    workload    = "quantum-shield"
  }
}