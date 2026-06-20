# Infrastruttura Terraform — Quantum Shield (`src/GW/`)

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
