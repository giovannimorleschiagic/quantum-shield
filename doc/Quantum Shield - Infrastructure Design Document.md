# Quantum Shield — Infrastructure Design Document

**Modulo Terraform — Azure Application Gateway & App Service**

| Campo | Valore |
|---|---|
| Versione | 1.0 |
| Data | Giugno 2026 |
| Stato | In revisione |
| Classificazione | Riservato — uso interno |
| Tipo documento | Infrastructure Design Document (IDD) |
| Modulo Terraform | `src/GW/` |
| Provider | `azurerm ~> 4.34` |
| Terraform | `>= 1.5.0` |

---

## 1. Executive Summary

Il modulo Terraform contenuto in `src/GW/` gestisce il provisioning dell'intera infrastruttura Azure per il layer applicativo di Quantum Shield. In pochi comandi, l'operatore ottiene un ambiente completo e ripetibile composto da: Application Gateway v2 (ingress pubblico con health probe attivo), Linux Web App su App Service Plan (backend API .NET 10), rete virtuale dedicata e Public IP statico.

L'approccio Infrastructure-as-Code garantisce la riproducibilità degli ambienti (dev/staging/prod), la tracciabilità delle modifiche infrastrutturali via git e la capacità di distruggere e ricreare l'intero ambiente in pochi minuti.

---

## 2. Obiettivi e principi di design

- **Immutabilità e riproducibilità:** ogni ambiente è creato identico a partire dal codice Terraform. Nessuna modifica manuale tramite portale Azure.
- **Minimo privilegio:** l'App Service backend comunica con l'Application Gateway tramite HTTPS (porta 443) con SNI. Nessun componente espone porte dirette a Internet.
- **Separazione per prefisso:** tutte le risorse usano il parametro `prefix`, che permette di istanziare ambienti multipli nella stessa subscription senza conflitti di nome.
- **Tag uniformi:** ogni risorsa riceve i tag definiti nella variabile `tags` per cost management e inventory.

---

## 3. Risorse provisioniate

| Risorsa Terraform | Nome Azure | Tipo | Note |
|---|---|---|---|
| `azurerm_resource_group` | `rg-{prefix}` | Resource Group | Contenitore di tutte le risorse |
| `azurerm_virtual_network` | `vnet-{prefix}` | VNet | Address space: `10.20.0.0/16` |
| `azurerm_subnet` | `snet-appgw` | Subnet | Dedicata ad Application Gateway: `10.20.0.0/24` |
| `azurerm_public_ip` | `pip-{prefix}-appgw` | Public IP | SKU: Standard, allocazione: Static |
| `azurerm_service_plan` | `asp-{prefix}` | App Service Plan | OS: Linux, SKU configurabile (default B1) |
| `azurerm_linux_web_app` | `app-{prefix}` | Linux Web App | Runtime .NET Core 10.0, `https_only`, `always_on` |
| `azurerm_application_gateway` | `agw-{prefix}` | Application Gateway v2 | SKU: Standard_v2, capacity: 2, HTTP/2 attivo |

---

## 4. Struttura del modulo Terraform

```
src/GW/
├── versions.tf              # Terraform >= 1.5.0, provider azurerm ~> 4.34
├── variables.tf             # Variabili di input con defaults
├── main.tf                  # Definizione di tutte le risorse Azure
├── outputs.tf               # Output del deployment
└── terraform.tfvars.example # Configurazione di esempio (dev)
```

---

## 5. Variabili di input

| Variabile | Tipo | Default | Descrizione |
|---|---|---|---|
| `prefix` | `string` | `quantumshield` | Prefisso per i nomi di tutte le risorse Azure |
| `location` | `string` | `westeurope` | Azure region |
| `app_service_sku_name` | `string` | `B1` | SKU del Linux App Service Plan |
| `app_service_runtime_stack` | `string` | `DOTNETCORE\|10.0` | Runtime .NET per la Web App |
| `tags` | `map(string)` | `{environment=dev, workload=quantum-shield}` | Tag applicati a tutte le risorse |

> Per ambienti di produzione si raccomanda SKU **P2v3** o superiore per App Service.

---

## 6. Output del deployment

| Output | Descrizione |
|---|---|
| `resource_group_name` | Nome del resource group creato |
| `application_gateway_public_ip` | IP pubblico assegnato all'Application Gateway (da usare nel DNS) |
| `application_gateway_name` | Nome della risorsa Application Gateway |
| `backend_web_app_name` | Nome dell'App Service backend |
| `backend_web_app_hostname` | Hostname default dell'App Service (`*.azurewebsites.net`) |

---

## 7. Architettura Application Gateway

```
Internet (HTTP :80)
    │
    ▼
Public IP (Static, Standard SKU) — pip-{prefix}-appgw
    │
    ▼
Application Gateway v2 (Standard_v2, capacity=2, HTTP/2 enabled)
  ├── Frontend IP: publicFrontend → Public IP
  ├── Frontend Port: port-80 (HTTP :80)
  ├── HTTP Listener: public-http-listener
  ├── Routing Rule: default-route (Basic, priority=100)
  ├── Backend Pool: appservice-backend-pool (FQDN: *.azurewebsites.net)
  ├── Backend Settings: HTTPS :443, pick_host_name=true, timeout=30s
  └── Health Probe: HTTPS GET /, interval=30s, threshold=3, codes 200-399
    │
    ▼
Linux Web App — app-{prefix}.azurewebsites.net (HTTPS :443)
    │
    ▼
API Backend .NET Core 10.0
```

> ⚠️ Il listener pubblico è attualmente HTTP sulla porta 80. Per abilitare TLS end-to-end aggiungere: certificato SSL, frontend port 443, listener HTTPS e redirect rule HTTP → HTTPS.

---

## 8. Configurazione App Service

| Impostazione | Valore | Note |
|---|---|---|
| `https_only` | `true` | Reindirizza automaticamente HTTP → HTTPS |
| `always_on` | `true` | Evita il cold start — istanza sempre attiva |
| `dotnet_version` | `10.0` | Runtime .NET estratto da `app_service_runtime_stack` |
| `WEBSITES_PORT` | `80` | Porta su cui l'app ascolta internamente |
| HTTP/2 | abilitato (Gateway) | HTTP/2 attivo sull'Application Gateway |

Il nome dell'App Service viene normalizzato automaticamente: i caratteri `_` vengono sostituiti con `-` e troncato a 60 caratteri per rispettare i vincoli Azure.

---

## 9. Networking

| Componente | CIDR / Config | Note |
|---|---|---|
| VNet | `10.20.0.0/16` | Virtual network dedicata al workload |
| Subnet AppGW | `10.20.0.0/24` (`snet-appgw`) | Richiesta da Application Gateway — non condivisibile |
| Public IP | Static Standard | Assegnato all'Application Gateway frontend |
| App Service | Multi-tenant (`azurewebsites.net`) | Accesso dall'AppGW via FQDN su HTTPS :443 |

> La subnet `snet-appgw` è riservata esclusivamente all'Application Gateway. Azure non consente di aggiungere altre risorse in questa subnet.

---

## 10. Convenzioni di naming

Con `prefix = quantumshield-dev`:

```
rg-quantumshield-dev            # Resource Group
vnet-quantumshield-dev          # Virtual Network
snet-appgw                      # Subnet (nome fisso)
pip-quantumshield-dev-appgw     # Public IP
asp-quantumshield-dev           # App Service Plan
app-quantumshield-dev           # Linux Web App
agw-quantumshield-dev           # Application Gateway
```

---

## 11. Comandi operativi

### Setup iniziale

```bash
cd src/GW
copy terraform.tfvars.example terraform.tfvars
# Modificare terraform.tfvars con i valori specifici dell'ambiente
terraform init
```

### Ciclo di vita standard

```bash
# Preview delle modifiche (sempre prima di apply)
terraform plan -var-file=terraform.tfvars

# Deploy / aggiornamento infrastruttura
terraform apply -var-file=terraform.tfvars

# Verifica stato corrente
terraform show

# Distruggi tutto (ambienti temporanei)
terraform destroy -var-file=terraform.tfvars
```

### Prerequisiti

1. Terraform >= 1.5.0 installato
2. Azure CLI autenticato: `az login`
3. Subscription selezionata: `az account set --subscription <id>`
4. Permessi: Contributor sulla subscription o sul resource group target

---

## 12. Ambienti multipli

| Ambiente | `prefix` | `app_service_sku_name` | Utilizzo |
|---|---|---|---|
| Sviluppo | `quantumshield-dev` | `B1` | Test locali e sviluppo feature |
| Staging | `quantumshield-stg` | `P1v2` | Collaudo pre-produzione |
| Produzione | `quantumshield-prod` | `P2v3` | Workload clienti live |

---

## 13. Evoluzioni previste

| Feature | Descrizione |
|---|---|
| **WAF v2** | Upgrade SKU da Standard_v2 a WAF_v2 per abilitare Web Application Firewall (OWASP Top 10) |
| **HTTPS frontend** | Certificato SSL, frontend port 443, listener HTTPS, redirect HTTP → HTTPS |
| **Autoscaling** | Autoscaling App Service Plan via Azure Monitor per picchi di carico |
| **Private networking** | VNet Integration per eliminare esposizione diretta su azurewebsites.net |
| **Remote state** | Terraform backend su Azure Blob Storage per state condiviso in team e lock |
| **CI/CD** | Pipeline GitHub Actions con `terraform plan/apply` e approval manuale in produzione |
