# Infrastruttura Terraform — Quantum Shield (`src/GW/`)

## Panoramica

Il modulo `src/GW/` contiene un'infrastruttura Terraform self-contained che esegue il provisioning completo dell'ambiente Azure per Quantum Shield: Application Gateway, App Service (backend + static), Key Vault, SQL Server, SQL Database, Storage Account, Application Insights e rete.

**Tool:** Terraform >= 1.5.0 · Provider `azurerm ~> 4.34`

---

## Risorse provisioniate

| Risorsa Terraform | Nome Azure | Descrizione |
|---|---|---|
| `azurerm_resource_group` | `var.resource_group_name` | Resource group (nome diretto, non prefissato) |
| `azurerm_virtual_network` | `vnet-{prefix}` | VNet `10.20.0.0/16` |
| `azurerm_subnet` | `snet-appgw` | Subnet AppGW `10.20.0.0/24` (service endpoint: Microsoft.Web) |
| `azurerm_public_ip` | `pip-{prefix}-appgw` | Public IP Static Standard |
| `azurerm_service_plan` | `asp-{prefix}` | App Service Plan Linux |
| `azurerm_linux_web_app` (backend) | `app-{prefix}` | API BE .NET Core 10, `https_only`, `always_on`, Managed Identity |
| `azurerm_linux_web_app` (static) | `static-{prefix}` | FE statico Node.js 22-lts |
| `azurerm_application_insights` | `appi-{prefix}` | Application Insights (type: web) |
| `azurerm_key_vault` | `kv-{prefix}-{suffix}` | Key Vault Standard, RBAC, soft-delete 7 gg |
| `azurerm_mssql_server` | `sql-{prefix}-{suffix}` | SQL Server v12, TLS 1.2 |
| `azurerm_mssql_database` | `var.sql_database_name` | SQL Database (SKU/size configurabili) |
| `azurerm_storage_account` | `st{prefix}{suffix}` | Blob Storage Standard, LRS, TLS 1.2 |
| `azurerm_storage_container` | `var.blob_container_name` | Container privato per artefatti |
| `random_string` | — | Suffisso 6 char per nomi globalmente unici |
| Role Assignments (×3) | — | Key Vault Secrets Officer per backend MI + deployer |
| Key Vault Secrets (×2) | — | SQL connection string + Blob connection string in KV |

> Il backend App Service ha **IP restriction**: accetta traffico solo dalla subnet AppGW (`allow-appgw-subnet`).

---

## Struttura dei file

```
src/GW/
├── versions.tf              # Terraform >= 1.5.0, provider azurerm ~> 4.34
├── variables.tf             # 18 variabili di input
├── main.tf                  # Definizione di tutte le risorse Azure
├── outputs.tf               # 14 output del deployment
├── INFRA_README.md          # Note operative interne del team infra
└── terraform.tfvars.example # Configurazione di esempio (dev)
```

---

## Variabili di input

| Variabile | Default | Descrizione |
|---|---|---|
| `prefix` | `quantumshield` | Prefisso nomi risorse |
| `resource_group_name` | `rg-teamorange` | Nome diretto del Resource Group |
| `location` | `westeurope` | Azure region |
| `azure_subscription_id` | — | Subscription ID (Service Principal) |
| `azure_tenant_id` | — | Tenant ID (Service Principal) |
| `azure_client_id` | — | Client ID (Service Principal) |
| `azure_client_secret` | — | ⚠️ sensitive — Client Secret (SP) |
| `app_service_sku_name` | `B1` | SKU App Service Plan |
| `app_service_runtime_stack` | `DOTNETCORE\|10.0` | Runtime .NET backend |
| `key_vault_purge_protection_enabled` | `false` | Purge protection Key Vault |
| `sql_administrator_login` | `sqladminuser` | Login admin SQL |
| `sql_administrator_password` | — | ⚠️ sensitive — Password admin SQL |
| `sql_database_name` | `quantumshielddb` | Nome database |
| `sql_database_sku_name` | `Basic` | SKU SQL Database |
| `sql_database_max_size_gb` | `2` | Dimensione massima DB |
| `sql_allow_azure_services` | `true` | Firewall rule per Azure services |
| `storage_replication_type` | `LRS` | Replica Storage Account |
| `blob_container_name` | `app-data` | Nome container blob |

---

## Output del deployment (14)

| Output | Descrizione |
|---|---|
| `resource_group_name` | Nome RG |
| `application_gateway_public_ip` | IP pubblico AppGW |
| `application_gateway_name` | Nome AppGW |
| `backend_web_app_name` / `_hostname` | App Service backend |
| `static_content_app_name` / `_hostname` | App Service FE statico |
| `application_insights_name` / `_connection_string` | AppInsights |
| `key_vault_name` / `_uri` | Key Vault |
| `key_vault_sql_connection_secret_name` | Nome secret SQL in KV |
| `key_vault_blob_connection_secret_name` | Nome secret Blob in KV |
| `sql_server_fqdn` / `sql_database_name` | SQL Server |
| `storage_account_name` / `blob_container_name` | Storage |

---

## Convenzioni di naming

| Risorsa | Pattern | Unicità |
|---|---|---|
| Resource Group | `var.resource_group_name` (diretto) | Manuale |
| VNet, AppGW, App Service, AppInsights | `{tipo}-{prefix}` | Globale |
| Key Vault | `kv-{prefix}-{random6}` | Globalmente unico (max 24 char) |
| SQL Server | `sql-{prefix}-{random6}` | Globalmente unico (max 63 char) |
| Storage Account | `st{prefix}{random6}` | Globalmente unico (max 24 char, no trattini) |

---

## Sicurezza infrastrutturale

- **Managed Identity**: il backend App Service ha `SystemAssigned` identity con ruolo `Key Vault Secrets Officer`
- **IP Restriction**: il backend accetta solo traffico dalla subnet AppGW (`snet-appgw`) — bloccato il traffico diretto
- **Secrets in Key Vault**: la SQL connection string e la Blob connection string vengono scritte in Key Vault al deploy
- **TLS**: SQL Server TLS 1.2, Storage Account TLS 1.2, App Service `https_only`
- **Blob privato**: container con `container_access_type = "private"`

---

## Comandi

```bash
cd src/GW

# Setup iniziale
copy terraform.tfvars.example terraform.tfvars
# Compilare terraform.tfvars con subscription ID, SP credentials, password SQL

terraform init
terraform plan -var-file=terraform.tfvars
terraform apply -var-file=terraform.tfvars

# Distruggi
terraform destroy -var-file=terraform.tfvars
```

### Prerequisiti

1. Terraform >= 1.5.0
2. Azure CLI: `az login` oppure Service Principal configurato in `terraform.tfvars`
3. Subscription selezionata

---

## Flusso networking

```
Internet (HTTP :80)
    └── Public IP → Application Gateway v2 (Standard_v2)
         ├── Backend: API BE (app-{prefix}) via HTTPS :443
         │         IP restriction: solo da snet-appgw
         └── (routing statico FE da configurare)
```

Il FE statico (`static-{prefix}`) è attualmente su App Service separato ma non ancora connesso all'AppGW tramite una routing rule dedicata.


## Panoramica

Il modulo `src/GW/` contiene un'infrastruttura Terraform self-contained che esegue il provisioning dei componenti Azure dell'application layer: Application Gateway, App Service Plan, Linux Web App e rete.

**Tool:** Terraform >= 1.5.0 · Provider `azurerm ~> 4.34`

---

## Risorse provisioniate

| Risorsa Terraform | Nome Azure | Descrizione |
|---|---|---|
| `azurerm_resource_group` | `rg-{prefix}` | Resource group contenitore |
| `azurerm_virtual_network` | `vnet-{prefix}` | VNet con address space `10.20.0.0/16` |
| `azurerm_subnet` | `snet-appgw` | Subnet dedicata all'Application Gateway (`10.20.0.0/24`) |
| `azurerm_public_ip` | `pip-{prefix}-appgw` | Public IP statico Standard per il frontend del gateway |
| `azurerm_service_plan` | `asp-{prefix}` | App Service Plan Linux |
| `azurerm_linux_web_app` | `app-{prefix}` | Web App Linux (backend API BE) |
| `azurerm_application_gateway` | `agw-{prefix}` | Application Gateway v2 |

---

## Convenzioni di naming

Tutte le risorse usano il prefisso configurabile `var.prefix` (default: `quantumshield`):

```
rg-quantumshield
vnet-quantumshield
agw-quantumshield
asp-quantumshield
app-quantumshield
pip-quantumshield-appgw
```

---

## Struttura dei file

```
src/GW/
├── versions.tf              # Terraform >= 1.5.0, provider azurerm ~> 4.34
├── variables.tf             # Variabili di input
├── main.tf                  # Definizione di tutte le risorse Azure
├── outputs.tf               # Output del deployment
└── terraform.tfvars.example # Esempio di configurazione
```

---

## Variabili di input

| Variabile | Default | Descrizione |
|---|---|---|
| `prefix` | `quantumshield` | Prefisso per i nomi di tutte le risorse Azure |
| `location` | `westeurope` | Azure region |
| `app_service_sku_name` | `B1` | SKU del Linux App Service Plan |
| `app_service_runtime_stack` | `DOTNETCORE\|10.0` | Runtime .NET per la Web App |
| `tags` | `{environment="dev", workload="quantum-shield"}` | Tag applicati a tutte le risorse |

---

## Output

| Output | Descrizione |
|---|---|
| `resource_group_name` | Nome del resource group creato |
| `application_gateway_public_ip` | IP pubblico assegnato all'Application Gateway |
| `application_gateway_name` | Nome della risorsa Application Gateway |
| `backend_web_app_name` | Nome dell'App Service backend |
| `backend_web_app_hostname` | Hostname default dell'App Service |

---

## Architettura dell'Application Gateway

```
Internet (HTTP :80)
    │
    ▼
Public IP (Static, Standard SKU)
    │
    ▼
Application Gateway v2 (Standard_v2, capacity=2)
  ├── Frontend: publicFrontend → port-80 (HTTP)
  ├── Backend pool: App Service default hostname
  ├── Backend settings: HTTPS :443, pick_host_name_from_backend=true
  ├── Health probe: HTTPS /, interval=30s, threshold=3
  └── Routing rule: default-route → appservice-backend-pool
```

**Nota:** Il listener corrente è HTTP sulla porta 80. La comunicazione Application Gateway → App Service avviene in HTTPS. Per TLS end-to-end sul frontend aggiungere certificato SSL, listener HTTPS e redirect rule.

---

## Comandi

```bash
cd src/GW

# Inizializzazione (prima esecuzione o dopo modifica provider)
terraform init

# Preview delle modifiche
terraform plan -var-file=terraform.tfvars

# Deploy
terraform apply -var-file=terraform.tfvars

# Distruggi l'infrastruttura
terraform destroy -var-file=terraform.tfvars
```

### Setup iniziale

```bash
cd src/GW
copy terraform.tfvars.example terraform.tfvars
# Modificare terraform.tfvars con i propri valori
terraform init
terraform plan
terraform apply
```

---

## Prerequisiti

- Terraform >= 1.5.0 installato
- Azure CLI autenticato (`az login`) oppure Service Principal configurato
- Subscription Azure selezionata (`az account set --subscription <id>`)

---

## Note operative

- L'App Service è configurato con `https_only = true` e `always_on = true`
- Il nome dell'App Service viene troncato a 60 caratteri e i `_` vengono sostituiti con `-` per rispettare i vincoli Azure
- La VNet usa lo spazio `10.20.0.0/16`; la subnet AppGW è `10.20.0.0/24`
- HTTP/2 è abilitato sull'Application Gateway
