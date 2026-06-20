# Azure Application Gateway Terraform

This folder contains a self-contained Terraform stack that provisions:

- an Azure Resource Group
- a Virtual Network and dedicated Application Gateway subnet
- a Standard Public IP for the gateway frontend
- a Linux App Service Plan
- a Linux Web App used as the backend target
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
- Azure credentials already available through Azure CLI, managed identity, or service principal
- A target Azure subscription selected before running Terraform

## Usage

```bash
cd src/GW
copy terraform.tfvars.example terraform.tfvars
terraform init
terraform plan
terraform apply
```

## Notes

- The current listener is HTTP on port 80 to keep the example minimal.
- The backend hop from Application Gateway to App Service uses HTTPS and preserves the backend host name automatically.
- If you need end-to-end TLS on the frontend, add an SSL certificate, an HTTPS listener, and a redirect rule.