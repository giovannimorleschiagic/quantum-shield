# Quantum Shield — Proposta Tecnico-Commerciale

**Piattaforma SaaS di Assessment Zero Trust per Configurazioni Identity Microsoft**

| Campo | Valore |
|---|---|
| Versione | 1.0 |
| Data | Giugno 2026 |
| Validità offerta | 60 giorni dalla data di emissione |
| Classificazione | Confidenziale — riservato al destinatario |

---

## 1. Contesto e comprensione delle esigenze

Le organizzazioni che adottano Microsoft 365 ed Entra ID si trovano ad affrontare una sfida strutturale: la configurazione dell'identità digitale è la prima linea di difesa in qualsiasi architettura Zero Trust, ma la sua complessità rende difficile misurare oggettivamente il livello di maturità raggiunto.

Esigenze di mercato identificate:

- Assenza di strumenti strutturati per valutare in modo automatico e ripetibile la postura Identity di un tenant Microsoft
- Assessment manuali costosi, non scalabili e soggetti a interpretazioni divergenti
- Mancanza di raccordo sistematico tra configurazioni tecniche e principi Zero Trust
- Necessità di reportistica comprensibile sia per il CISO che per il management non tecnico
- Crescente pressione normativa (NIS2, DORA, ISO 27001) che richiede evidenza documentata delle verifiche di sicurezza

> **Opportunità di mercato:** Il mercato della cybersecurity identity governance è in crescita a doppia cifra. Soluzioni verticali, automatizzate e integrate con l'ecosistema Microsoft rappresentano un'opportunità differenziante rispetto agli strumenti generici di GRC o ai penetration test tradizionali.

---

## 2. Soluzione proposta

Quantum Shield è una piattaforma SaaS multi-tenant che automatizza il processo di assessment Zero Trust per le configurazioni Microsoft Identity. Il sistema acquisisce le configurazioni di un tenant Azure AD in modalità read-only tramite Microsoft Graph API, le confronta con un set di regole configurabili e produce un assessment strutturato con scoring, evidenze, raccomandazioni e reportistica esportabile.

### 2.1 Differenziatori chiave

| Differenziatore | Descrizione |
|---|---|
| **Zero-touch sui dati cliente** | La piattaforma non scrive mai nel tenant. Accesso read-only tramite permessi minimi espliciti e revocabili in qualsiasi momento. |
| **Rule engine esternalizzato** | Le regole di controllo sono template JSON su Blob Storage. Nuove regole = aggiornamento file, zero re-deploy. |
| **Multi-tenant nativo** | Architettura progettata dalla fondamenta per segregazione completa dei dati tra clienti diversi. |
| **Azure-native** | Key Vault per i secret, Application Insights per l'osservabilità, Managed Identity per l'autenticazione service-to-service. |
| **Output dual-audience** | Report tecnico analitico per il team di sicurezza e sintesi manageriale per CISO/board. |
| **Tracciabilità storica** | Ogni assessment è immutabile e consultabile. Trend di miglioramento/peggioramento nel tempo. |

### 2.2 Copertura funzionale

| Area di controllo | Perimetro di analisi |
|---|---|
| **Autenticazione & MFA** | Copertura MFA, metodi deboli, autenticazione legacy, esclusioni rischiose |
| **Conditional Access** | Copertura policy, eccezioni, protezione amministratori, accessi da reti/paesi a rischio |
| **Ruoli e privilegi** | Account privilegiati, assegnazioni permanenti, PIM, break-glass, concentrazione di ruoli |
| **App & Consensi** | Permessi applicativi, consensi admin, secret scaduti, service principal privilegiati |
| **Guest & External** | Utenti B2B, guest inattivi, guest privilegiati, impostazioni di collaborazione esterna |
| **Audit & Logging** | Log di accesso, audit log, eventi anomali, pattern non conformi |

---

## 3. Perimetro di fornitura per fase

### Fase 1 — Foundation & Core Assessment (MVP)

> **Durata:** 10 settimane · **Output:** Piattaforma operativa con assessment base end-to-end

| Modulo | Contenuto | Incluso |
|---|---|---|
| Infrastruttura Azure | App Service, SQL, Blob, Key Vault, App Insights, App Gateway, CI/CD | ✅ |
| Autenticazione | MSAL FE (popup Azure AD), validazione token BE, gestione sessione sicura | ✅ |
| Onboarding tenant | Wizard registrazione, consenso amministrativo, verifica permessi, primo assessment | ✅ |
| Raccolta configurazioni | Microsoft Graph: Entra ID base, gestione errori parziali | ✅ |
| Rule engine v1 | Template JSON su Blob, confronti base, set iniziale 20+ regole | ✅ |
| Assessment pipeline | Flusso Pending→InProgress→Completed/Failed, tracciabilità SQL | ✅ |
| Scoring base | Score per area (MFA, CA, Ruoli, App, Guest), pesatura per severità | ✅ |
| Dashboard v1 | Score, criticità per severità, ultime esecuzioni, azioni prioritarie | ✅ |
| Report PDF | Export PDF con sintesi manageriale + dettaglio evidenze | ✅ |
| Offboarding tenant | Revoca accesso, cancellazione dati, log tracciabile | ✅ |

### Fase 2 — Advanced Assessment & Reporting

> **Durata:** 8 settimane · **Output:** Copertura estesa, scoring avanzato, remediation tracking

| Modulo | Contenuto | Incluso |
|---|---|---|
| Graph avanzato | Conditional Access, PIM, App Registration, Enterprise App, Guest, Audit log | ✅ |
| Rule engine v2 | Ciclo di vita regole (bozza/test/attiva/deprecata), versionamento, 60+ regole | ✅ |
| Scoring avanzato | Score per principio Zero Trust, ponderazione oggetti coinvolti | ✅ |
| Dashboard avanzata | Filtri multi-dimensionali, trend storico, heat-map, comparazione assessment | ✅ |
| Remediation tracking | Stato evidenze (aperta/in analisi/pianificata/risolta/accettata), storico | ✅ |
| Report dual-audience | Executive summary + dettaglio tecnico, mappatura principi ZT | ✅ |
| Gestione ruoli interni | Admin piattaforma, admin cliente, auditor, viewer — RBAC completo | ✅ |

### Fase 3 — Enterprise & Continuous Monitoring

> **Durata:** 6 settimane · **Output:** Monitoraggio continuo, integrazioni, scalabilità enterprise

| Modulo | Contenuto | Incluso |
|---|---|---|
| Assessment asincrono | Coda Azure Service Bus, background worker, notifiche al completamento | ✅ |
| Continuous monitoring | Scheduling automatico, alerting su variazioni critiche | ✅ |
| Baseline personalizzate | Template per settore/normativa (NIS2, DORA, ISO 27001) | ✅ |
| Export avanzato | Report Word/Excel, API REST pubblica per integrazione SIEM/ITSM | ✅ |
| Performance & scale | Redis Cache, ottimizzazione query, supporto tenant >10.000 oggetti | ✅ |

---

## 4. Team proposto e profili

| Ruolo | Responsabilità principali | Fase | Allocazione |
|---|---|---|---|
| Solution Architect | Design architetturale, quality gate, supervisione sicurezza | 1-2-3 | 30% |
| Senior BE Developer ×2 | ASP.NET Core, EF Core, Azure SDK, Microsoft Graph, Domain model | 1-2-3 | 100% |
| Senior FE Developer | React 19, TypeScript strict, MSAL, dashboard, report, UX/UI | 1-2-3 | 100% |
| DevOps / Cloud Engineer | Azure infra, CI/CD, Key Vault, App Gateway, monitoring, IaC | 1-2-3 | 50% |
| QA Engineer | Test plan, test funzionali/integrazione, performance, security testing | 1-2-3 | 50% |
| Project Manager | Pianificazione sprint, risk management, reporting avanzamento | 1-2-3 | 25% |
| Security Reviewer | Security review codice e architettura, threat model, pentest interno | 2-3 | 20% |

> Tutto il team firma NDA specifico. Nessun accesso a dati reali di produzione al di fuori dell'ambiente di test designato.

---

## 5. Stime di effort e piano di consegna

### 5.1 Breakdown effort per fase (giorni/persona)

| Fase / Area | BE | FE | DevOps | QA | Totale |
|---|---|---|---|---|---|
| **Fase 1 — Foundation & Core** | | | | | |
| Infrastruttura Azure + CI/CD | 4 | — | 10 | 2 | 16 |
| Autenticazione (MSAL FE + BE) | 5 | 7 | — | 3 | 15 |
| Onboarding & offboarding tenant | 10 | 12 | — | 5 | 27 |
| Graph integration base + pipeline | 18 | — | — | 4 | 22 |
| Rule engine v1 + template | 14 | — | — | 4 | 18 |
| Scoring + Dashboard v1 + Report PDF | 8 | 18 | — | 6 | 32 |
| **Subtotale Fase 1** | **59** | **37** | **10** | **24** | **130** |
| **Fase 2 — Advanced Assessment** | | | | | |
| Graph avanzato (CA, PIM, App, Guest) | 18 | — | — | 4 | 22 |
| Rule engine v2 + 60+ regole | 15 | — | — | 4 | 19 |
| Scoring avanzato + dashboard avanzata | 6 | 16 | — | 4 | 26 |
| Remediation tracking + RBAC | 12 | 10 | — | 5 | 27 |
| Report dual-audience | 5 | 8 | — | 3 | 16 |
| **Subtotale Fase 2** | **56** | **34** | **—** | **20** | **110** |
| **Fase 3 — Enterprise & Monitoring** | | | | | |
| Assessment asincrono + Service Bus | 10 | 4 | 3 | 3 | 20 |
| Continuous monitoring + alerting | 12 | 8 | 2 | 4 | 26 |
| Baseline custom + export avanzato | 8 | 6 | — | 3 | 17 |
| Performance, Redis, scale | 6 | 2 | 4 | 3 | 15 |
| **Subtotale Fase 3** | **36** | **20** | **9** | **13** | **78** |
| **TOTALE COMPLESSIVO** | **151** | **91** | **19** | **57** | **318** |

*Le stime includono un buffer del 15%. SA (30 gg) e PM (20 gg) sono valorizzati separatamente.*

### 5.2 Piano di consegna — Macro milestone

| Milestone | Settimana | Deliverable |
|---|---|---|
| Kickoff & Setup | W1 | Ambiente Azure, repo Git, CI/CD operativa |
| Alpha Fase 1 | W5 | Onboarding funzionante, primo assessment (env. dev) |
| **Go-live Fase 1 (MVP)** | **W10** | **Piattaforma in produzione: assessment, dashboard, report PDF** |
| Alpha Fase 2 | W14 | Graph avanzato, rule engine v2 (env. staging) |
| **Go-live Fase 2** | **W18** | **Advanced assessment, RBAC, report dual-audience in produzione** |
| Alpha Fase 3 | W22 | Continuous monitoring, Service Bus (env. staging) |
| **Go-live Fase 3** | **W24** | **Soluzione enterprise completa, API pubblica, export avanzato** |

> Metodologia: **Agile** (sprint bisettimanali). Demo al cliente al termine di ogni sprint. UAT formale prima di ogni go-live.

---

## 6. Opzioni commerciali

*Tutti i prezzi sono espressi IVA esclusa.*

---

### ⚡ Opzione A — MVP

**Prezzo fisso chiavi in mano: € 155.000**
*Fase 1 · 10 settimane · Piattaforma operativa con assessment base end-to-end*

| Voce di costo | Gg/p | Tariffa | Importo |
|---|---|---|---|
| 2× Senior BE Developer | 118 | € 680/gg | € 80.240 |
| 1× Senior FE Developer | 50 | € 630/gg | € 31.500 |
| DevOps / Cloud Engineer | 25 | € 680/gg | € 17.000 |
| QA Engineer | 25 | € 520/gg | € 13.000 |
| Solution Architect (30%) | 15 | € 900/gg | € 13.500 |
| Project Manager (25%) | 13 | € 680/gg | € 8.840 |
| Set iniziale 20+ regole ZT + template | — | incluso | incluso |
| Totale listino | | | € 164.080 |
| Sconto offerta (5,5%) | | | − € 9.024 |
| **PREZZO FINALE** | | | **€ 155.000** |

---

### 🚀 Opzione B — Prodotto Completo

**Prezzo fisso chiavi in mano: € 265.000**
*Fase 1 + Fase 2 · 18 settimane · Assessment avanzato, remediation, reportistica completa*

| Voce di costo | Gg/p | Tariffa | Importo |
|---|---|---|---|
| 2× Senior BE Developer | 230 | € 670/gg | € 154.100 |
| 1× Senior FE Developer | 90 | € 620/gg | € 55.800 |
| DevOps / Cloud Engineer | 25 | € 670/gg | € 16.750 |
| QA Engineer | 45 | € 510/gg | € 22.950 |
| Solution Architect (30%) | 27 | € 880/gg | € 23.760 |
| Project Manager (25%) | 23 | € 670/gg | € 15.410 |
| Security Reviewer (20%) | 9 | € 880/gg | € 7.920 |
| Set 60+ regole ZT + template avanzati | — | incluso | incluso |
| Totale listino | | | € 296.690 |
| Sconto offerta (10,7%) | | | − € 31.690 |
| **PREZZO FINALE** | | | **€ 265.000** |

---

### 🏆 Opzione C — Enterprise

**Prezzo fisso chiavi in mano: € 370.000**
*Fase 1 + Fase 2 + Fase 3 · 24 settimane · Soluzione completa con monitoring continuo e integrazioni*

| Voce di costo | Gg/p | Tariffa | Importo |
|---|---|---|---|
| 2× Senior BE Developer | 302 | € 660/gg | € 199.320 |
| 1× Senior FE Developer | 110 | € 610/gg | € 67.100 |
| DevOps / Cloud Engineer | 34 | € 660/gg | € 22.440 |
| QA Engineer | 58 | € 500/gg | € 29.000 |
| Solution Architect (30%) | 36 | € 860/gg | € 30.960 |
| Project Manager (25%) | 30 | € 660/gg | € 19.800 |
| Security Reviewer (20%) | 14 | € 860/gg | € 12.040 |
| Set 80+ regole ZT, template NIS2/DORA/ISO | — | incluso | incluso |
| Totale listino | | | € 380.660 |
| Sconto offerta (2,8%) | | | − € 10.660 |
| **PREZZO FINALE** | | | **€ 370.000** |

---

> **Piano di pagamento suggerito:** 30% alla firma · 30% go-live Fase 1 · 25% go-live Fase 2 · 15% collaudo finale.
> Per Opzione A: 40% firma · 40% UAT · 20% go-live.

---

## 7. Infrastruttura Azure — costi stimati

*Costi mensili per l'ambiente di produzione, IVA esclusa. Non inclusi nel prezzo di sviluppo.*

| Componente | SKU consigliato | Costo/mese (est.) | Note |
|---|---|---|---|
| App Service Plan | P2v3 (2 core, 8 GB) | € 180–240 | Scalabile a P3v3 |
| Azure SQL Database | Standard S3 (100 DTU) | € 120–180 | Scalabile a Business Critical |
| Azure Blob Storage | LRS, Hot tier | € 15–40 | Template + backup |
| Azure Key Vault | Standard | € 10–25 | Per operazioni/secret |
| Application Insights | Pay-per-use | € 30–100 | Dipende da volume log |
| Application Gateway | WAF_v2 | € 250–380 | Voce di costo principale |
| Azure Service Bus | Standard (solo Fase 3) | € 10–30 | Solo da Fase 3 |
| Azure Cache for Redis | Basic C1 (solo Fase 3) | € 40–70 | Solo da Fase 3 |
| **Totale Fase 1-2** | | **€ 605–965/mese** | ~€ 780 avg |
| **Totale Fase 3** | | **€ 655–1.065/mese** | ~€ 860 avg |

---

## 8. Assunzioni, dipendenze ed esclusioni

### 8.1 Assunzioni

- Il cliente dispone di una subscription Azure attiva con permessi di provisioning delle risorse richieste
- Il cliente mette a disposizione un referente tecnico con conoscenza di Azure AD (min. 4 ore/settimana durante Fase 1)
- I tenant utilizzati per i test saranno sandbox dedicati — mai tenant di produzione
- Il cliente accetta il flusso di consenso amministrativo Microsoft standard
- Il set iniziale di regole copre le aree Zero Trust prioritarie; set aggiuntivi sono negoziabili
- Gestione dominio custom e certificati TLS a carico del cliente

### 8.2 Dipendenze critiche

| Dipendenza | Responsabile | Impatto se ritardata |
|---|---|---|
| Provisioning subscription Azure | Cliente | Slittamento W1-W2 |
| Creazione app registration Azure AD (test) | Cliente + team | Blocca sviluppo Graph integration |
| Accesso Key Vault in env. dev | Cliente | Blocca sviluppo Fase 1 step 4-5 |
| UAT con tenant reale (staging) | Cliente | Condiziona go-live Fase 1 |
| Definizione set regole prioritarie | Cliente + team | Workshop 1 giorno in W1 |

### 8.3 Esclusioni

- Sviluppo funzionalità di remediation automatica (modifica del tenant cliente)
- Integrazione con SIEM/ITSM di terze parti (Splunk, ServiceNow) — disponibile come estensione Fase 3 Extra
- Training utente finale e onboarding operativo — quotabili separatamente
- Supporto e manutenzione post-go-live — contratto di manutenzione dedicato
- Adeguamenti normativi specifici non coperti dal set di regole standard
- Costi licenza Microsoft 365 / Entra ID P2 eventualmente necessari per PIM e Identity Protection

---

## 9. Perché questa soluzione

### Confronto con le alternative

| Scenario | Approccio alternativo | Quantum Shield |
|---|---|---|
| Verifica postura ZT | Assessment manuale consulenziale | Automatizzato, ripetibile, storico |
| Costo per assessment | € 15.000–40.000 (consulenza) | Marginale a regime |
| Frequenza assessment | 1 volta/anno | On-demand + continuo |
| Obiettività risultati | Dipende dal consulente | Deterministica, basata su regole |
| Confronto storico | Non disponibile | Trend automatico, delta visibile |
| Integrazione normativa | Manuale, framework generici | Template NIS2/DORA/ISO (Fase 3) |
| Time to insight | Settimane | Minuti |

### 9.1 ROI atteso

Un'azienda enterprise con 5 tenant che commissiona assessment manuali annuali spende tipicamente **€ 75.000–200.000/anno** in consulenza. Quantum Shield, a regime, riduce questo costo a pochi k€/anno di infrastruttura con assessment illimitati, on-demand e confrontabili nel tempo.

Per un **vendor/MSSP** che offre il servizio ai propri clienti, il breakeven si raggiunge tipicamente entro **8–12 mesi** dal go-live, assumendo un pricing per cliente di € 5.000–15.000/anno.

---

## 10. Prossimi passi e accettazione

### 10.1 Processo di accettazione

| Step | Azione | Responsabile | Timing |
|---|---|---|---|
| 1 | Revisione proposta e richiesta chiarimenti | Cliente | Entro 10 gg |
| 2 | Workshop tecnico di allineamento (2 ore, remoto) | Cliente + team | Entro 15 gg |
| 3 | Eventuale adeguamento proposta e negoziazione | Entrambi | Entro 20 gg |
| 4 | Firma contratto e NDA | Entrambi | Entro 30 gg |
| 5 | Kickoff ufficiale del progetto | Team | W1 post-firma |

### 10.2 Condizioni dell'offerta

- **Validità:** 60 giorni dalla data di emissione
- I prezzi sono fissi (lump-sum) salvo variazioni di perimetro concordate per iscritto tramite Change Request
- Attività extra-perimetro valorizzate alle tariffe giornaliere indicate nelle tabelle di costo
- La proposta è soggetta a firma di contratto di sviluppo e NDA prima dell'avvio
- I diritti di proprietà intellettuale sul codice sviluppato sono trasferiti al cliente al saldo del corrispettivo contrattuale

### 10.3 Opzione selezionata

- [ ] **Opzione A — MVP** · € 155.000 · 10 settimane
- [ ] **Opzione B — Prodotto Completo** · € 265.000 · 18 settimane
- [ ] **Opzione C — Enterprise** · € 370.000 · 24 settimane

---

*Documento riservato — da non divulgare a terzi senza autorizzazione scritta del fornitore.*
