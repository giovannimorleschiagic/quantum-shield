# Azure Application Gateway Terraform

This folder contains a self-contained Terraform stack that provisions:

- an Azure Resource Group
- a Virtual Network and dedicated Application Gateway subnet
- a Standard Public IP for the gateway frontend
- a Linux App Service Plan
- a Linux Web App used as the backend target
- a static-content Linux Web App hosted in the same App Service Plan
- an Azure Key Vault
- an Azure SQL Server and SQL Database
- an Azure Storage Account with a private Blob container
- an Azure Application Gateway v2 configured to route HTTP traffic to the App Service backend over HTTPS

## Architecture

The Application Gateway is deployed into its own subnet and exposes a public frontend IP.
The backend pool uses the App Service default hostname, which is the supported pattern when the backend is an Azure Web App.

## Files

- `versions.tf`: Terraform and provider requirements
- `variables.tf`: input variables
- `main.tf`: Azure resources
- `outputs.tf`: useful deployment outputs
- `terraform.tfvars.example`: sample variable values

## Prerequisites

- Terraform 1.5+
- Azure service principal credentials with permissions on target subscription
- A strong value for `sql_administrator_password` in `terraform.tfvars`

## Usage

```bash
cd src/GW
copy terraform.tfvars.example terraform.tfvars
# set azure_subscription_id, azure_tenant_id, azure_client_id, azure_client_secret
terraform init
terraform plan
terraform apply
```

## Notes

- The current listener is HTTP on port 80 to keep the example minimal.
- The backend hop from Application Gateway to App Service uses HTTPS and preserves the backend host name automatically.
- If you need end-to-end TLS on the frontend, add an SSL certificate, an HTTPS listener, and a redirect rule.