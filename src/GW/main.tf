locals {
  resource_group_name = "rg-${var.prefix}"
  vnet_name           = "vnet-${var.prefix}"
  appgw_subnet_name   = "snet-appgw"
  public_ip_name      = "pip-${var.prefix}-appgw"
  appgw_name          = "agw-${var.prefix}"
  app_service_plan    = "asp-${var.prefix}"
  web_app_name        = substr(replace("app-${var.prefix}", "_", "-"), 0, 60)
  key_vault_name      = substr(replace("kv-${var.prefix}-${random_string.global_suffix.result}", "_", "-"), 0, 24)
  sql_server_name     = substr(replace("sql-${var.prefix}-${random_string.global_suffix.result}", "_", "-"), 0, 63)
  storage_account_name = substr(
    lower(replace(replace("st${var.prefix}${random_string.global_suffix.result}", "-", ""), "_", "")),
    0,
    24
  )
}

data "azurerm_client_config" "current" {}

resource "random_string" "global_suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_resource_group" "this" {
  name     = local.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_virtual_network" "this" {
  name                = local.vnet_name
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  address_space       = ["10.20.0.0/16"]
  tags                = var.tags
}

resource "azurerm_subnet" "app_gateway" {
  name                 = local.appgw_subnet_name
  resource_group_name  = azurerm_resource_group.this.name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = ["10.20.0.0/24"]
  service_endpoints    = ["Microsoft.Web"]
}

resource "azurerm_public_ip" "app_gateway" {
  name                = local.public_ip_name
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  allocation_method   = "Static"
  sku                 = "Standard"
  tags                = var.tags
}

resource "azurerm_service_plan" "this" {
  name                = local.app_service_plan
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  os_type             = "Linux"
  sku_name            = var.app_service_sku_name
  tags                = var.tags
}

resource "azurerm_linux_web_app" "backend" {
  name                = local.web_app_name
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  service_plan_id     = azurerm_service_plan.this.id
  https_only          = true
  tags                = var.tags

  site_config {
    always_on                         = true
    ip_restriction_default_action     = "Deny"
    scm_ip_restriction_default_action = "Deny"

    ip_restriction {
      name                      = "allow-appgw-subnet"
      priority                  = 100
      action                    = "Allow"
      virtual_network_subnet_id = azurerm_subnet.app_gateway.id
    }

    application_stack {
      dotnet_version = replace(var.app_service_runtime_stack, "DOTNETCORE|", "")
    }
  }

  app_settings = {
    WEBSITES_PORT = "80"
  }
}

resource "azurerm_key_vault" "this" {
  name                          = local.key_vault_name
  location                      = azurerm_resource_group.this.location
  resource_group_name           = azurerm_resource_group.this.name
  tenant_id                     = data.azurerm_client_config.current.tenant_id
  sku_name                      = "standard"
  rbac_authorization_enabled    = true
  soft_delete_retention_days    = 7
  purge_protection_enabled      = var.key_vault_purge_protection_enabled
  public_network_access_enabled = true
  tags                          = var.tags
}

resource "azurerm_mssql_server" "this" {
  name                          = local.sql_server_name
  resource_group_name           = azurerm_resource_group.this.name
  location                      = azurerm_resource_group.this.location
  version                       = "12.0"
  administrator_login           = var.sql_administrator_login
  administrator_login_password  = var.sql_administrator_password
  minimum_tls_version           = "1.2"
  public_network_access_enabled = true
  tags                          = var.tags
}

resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  count = var.sql_allow_azure_services ? 1 : 0

  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "this" {
  name           = var.sql_database_name
  server_id      = azurerm_mssql_server.this.id
  sku_name       = var.sql_database_sku_name
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb    = var.sql_database_max_size_gb
  zone_redundant = false
  tags           = var.tags
}

resource "azurerm_storage_account" "this" {
  name                            = local.storage_account_name
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_tier                    = "Standard"
  account_replication_type        = var.storage_replication_type
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  tags                            = var.tags
}

resource "azurerm_storage_container" "blob" {
  name                  = var.blob_container_name
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = "private"
}

resource "azurerm_application_gateway" "this" {
  name                = local.appgw_name
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  http2_enabled       = true
  tags                = var.tags

  sku {
    name     = "Standard_v2"
    tier     = "Standard_v2"
    capacity = 2
  }

  gateway_ip_configuration {
    name      = "appGatewayIpConfig"
    subnet_id = azurerm_subnet.app_gateway.id
  }

  frontend_port {
    name = "port-80"
    port = 80
  }

  frontend_ip_configuration {
    name                 = "publicFrontend"
    public_ip_address_id = azurerm_public_ip.app_gateway.id
  }

  backend_address_pool {
    name  = "appservice-backend-pool"
    fqdns = [azurerm_linux_web_app.backend.default_hostname]
  }

  backend_http_settings {
    name                                = "appservice-http-settings"
    cookie_based_affinity               = "Disabled"
    port                                = 443
    protocol                            = "Https"
    request_timeout                     = 30
    pick_host_name_from_backend_address = true
    probe_name                          = "appservice-probe"
  }

  http_listener {
    name                           = "public-http-listener"
    frontend_ip_configuration_name = "publicFrontend"
    frontend_port_name             = "port-80"
    protocol                       = "Http"
  }

  probe {
    name                                      = "appservice-probe"
    protocol                                  = "Https"
    path                                      = "/"
    interval                                  = 30
    timeout                                   = 30
    unhealthy_threshold                       = 3
    pick_host_name_from_backend_http_settings = true
    minimum_servers                           = 0

    match {
      status_code = ["200-399"]
    }
  }

  request_routing_rule {
    name                       = "default-route"
    rule_type                  = "Basic"
    http_listener_name         = "public-http-listener"
    backend_address_pool_name  = "appservice-backend-pool"
    backend_http_settings_name = "appservice-http-settings"
    priority                   = 100
  }
}