# Comandi di sviluppo — Quantum Shield

## Frontend (`src/FE/`)

```bash
cd src/FE

npm start                                        # Dev server → http://localhost:3000
npm run build                                    # Build produzione → src/FE/build/
npm test                                         # Watch mode interattivo
npm test -- --watchAll=false                     # Tutti i test (CI)
npm test -- --testPathPattern="App"              # Singolo file di test
npm test -- --coverage --watchAll=false          # Con coverage
npm install                                      # Installa dipendenze
```

## Backend (`src/BE/`)

```bash
cd src/BE

dotnet run --project QuantumShield.Be.Api        # Avvia l'API
dotnet build                                     # Build completa della solution
dotnet test                                      # Tutti i test (xUnit)
dotnet test --filter "FullyQualifiedName~ApiTests"  # Singolo test class
```

### EF Core — Migrations

```bash
cd src/BE

# Applica migrations al DB
dotnet ef database update \
  --project QuantumShield.Be.Infrastructure \
  --startup-project QuantumShield.Be.Api

# Crea una nuova migration
dotnet ef migrations add <NomeMigration> \
  --project QuantumShield.Be.Infrastructure \
  --startup-project QuantumShield.Be.Api
```

## Infrastruttura (`src/GW/`)

```bash
cd src/GW

terraform init                                   # Prima esecuzione o dopo modifica provider
terraform plan -var-file=terraform.tfvars        # Preview modifiche
terraform apply -var-file=terraform.tfvars       # Deploy infrastruttura Azure
terraform destroy -var-file=terraform.tfvars     # Distruggi tutto
```

## Note

- Non eseguire `npm run eject` nel FE — operazione irreversibile.
- ESLint FE è configurato in `package.json` (campo `"eslintConfig"`), non in `.eslintrc`.
- Il BE usa Central Package Management: le versioni NuGet sono in `Directory.Packages.props`, non nei singoli `.csproj`.
- Le opzioni di configurazione BE vengono validate all'avvio (`ValidateOnStart`) — errori di config bloccano il boot.
