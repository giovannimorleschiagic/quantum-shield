# Quantum Shield — Analisi Funzionale

**Documento Funzionale — Piattaforma di Assessment Zero Trust per Configurazioni Identity Microsoft**

| Campo | Valore |
|---|---|
| Versione | 1.0 |
| Data | Giugno 2026 |
| Stato | In revisione |
| Classificazione | Riservato |

---

## 1. Premessa

Il presente documento descrive le funzionalità previste per la realizzazione di una piattaforma destinata alla verifica del livello di allineamento di un'organizzazione ai principi Zero Trust, con particolare riferimento alle configurazioni Identity dei tenant Microsoft.

Il prodotto ha l'obiettivo di analizzare, in modo strutturato e ripetibile, la postura di sicurezza del tenant Microsoft dell'azienda, evidenziando eventuali configurazioni deboli, incomplete, incoerenti o non conformi alle best practice di sicurezza relative a Microsoft Entra ID, Conditional Access, gestione degli utenti privilegiati, autenticazione multifattore, applicazioni registrate, consensi, ruoli amministrativi, accessi guest, dispositivi e workload identity.

La piattaforma non si limita a fornire una fotografia tecnica dello stato del tenant, ma produce una valutazione funzionale del rischio, indicando il grado di disallineamento rispetto ai principi Zero Trust e suggerendo interventi correttivi ordinati per priorità, impatto e urgenza.

---

## 2. Obiettivo del prodotto

L'obiettivo principale del prodotto è fornire alle aziende uno strumento in grado di valutare quanto le configurazioni Identity del proprio ambiente Microsoft siano coerenti con un modello Zero Trust.

Il sistema, previa concessione degli accessi necessari da parte dell'azienda cliente, interroga le configurazioni del tenant Microsoft e costruisce un assessment basato su controlli predefiniti. Tali controlli verificano, ad esempio:

- Se l'accesso degli utenti è protetto da criteri di autenticazione forte
- Se gli account privilegiati sono gestiti secondo logiche di minimo privilegio
- Se le applicazioni dispongono di permessi eccessivi
- Se sono presenti utenti guest non governati
- Se esistono eccezioni rischiose nelle policy di accesso condizionale
- Se il tenant espone superfici di attacco non presidiate

Il risultato dell'analisi viene presentato tramite dashboard, report esportabile e schede di dettaglio, in modo che l'azienda possa comprendere non solo quali configurazioni risultano critiche, ma anche perché lo sono, quale principio Zero Trust viene compromesso e quale azione correttiva è consigliata.

---

## 3. Principi Zero Trust di riferimento

### 3.1 Verifica esplicita

Ogni richiesta di accesso deve essere autenticata e autorizzata sulla base del maggior numero possibile di segnali disponibili: identità dell'utente, posizione, rischio, stato del dispositivo, applicazione utilizzata e contesto della richiesta.

Il prodotto verifica la presenza e la qualità delle configurazioni di autenticazione multifattore, Conditional Access, Identity Protection, blocco dell'autenticazione legacy e controllo degli accessi anomali.

### 3.2 Minimo privilegio

Ogni utente, servizio, applicazione o workload deve disporre solo dei permessi strettamente necessari e per il tempo strettamente necessario. La piattaforma analizza ruoli amministrativi, assegnazioni permanenti, utilizzo di Privileged Identity Management, permessi applicativi, service principal, consensi amministrativi, gruppi privilegiati e account con privilegi elevati.

### 3.3 Assunzione della compromissione

Il sistema parte dal presupposto che una violazione possa già essere in corso o possa verificarsi in qualsiasi momento. Il prodotto verifica la capacità del tenant di limitare il raggio d'azione di un eventuale attaccante, rilevare configurazioni rischiose, ridurre le eccezioni, controllare gli accessi guest, monitorare gli eventi sensibili e garantire tracciabilità delle modifiche.

---

## 4. Perimetro funzionale

### Microsoft Entra ID
Analisi di utenti, gruppi, ruoli, account amministrativi, accessi guest, metodi di autenticazione, criteri MFA e configurazioni generali del tenant.

### Conditional Access
Verifica delle policy esistenti, degli utenti o gruppi inclusi ed esclusi, delle applicazioni protette, delle condizioni applicate e delle eventuali eccezioni presenti.

### Privileged Identity Management
Qualora disponibile nel tenant, analisi delle assegnazioni permanenti, eleggibili, attive, scadute o prive di approvazione.

### App Registration ed Enterprise Application
Verifica dei permessi delegati e applicativi, dei consensi amministrativi, dei secret e certificati in scadenza, delle applicazioni inutilizzate o potenzialmente rischiose.

### Guest e collaborazione esterna
Analisi degli utenti B2B, delle impostazioni di external collaboration, della presenza di guest inattivi, guest privilegiati o guest non soggetti a policy di sicurezza adeguate.

### Audit e sign-in log
Nei limiti consentiti dai permessi e dalle licenze, individuazione di eventi anomali, accessi legacy, accessi rischiosi, applicazioni non protette o pattern di utilizzo non coerenti con le policy attese.

---

## 5. Accesso al tenant e autorizzazioni

Per poter eseguire l'analisi, il prodotto ottiene dall'azienda cliente un'autorizzazione esplicita all'accesso ai dati di configurazione del tenant. L'accesso è progettato secondo il principio del minimo privilegio, richiedendo esclusivamente i permessi necessari alla lettura delle configurazioni da analizzare.

La soluzione privilegia un modello **read-only**, evitando qualsiasi capacità di modifica del tenant in fase di assessment standard.

Il processo di onboarding prevede:

- Una fase di consenso amministrativo
- Una verifica dei permessi concessi
- La registrazione del tenant cliente
- La generazione di un primo assessment
- La possibilità di revocare l'accesso in qualsiasi momento

---

## 6. Moduli funzionali previsti

### 6.1 Onboarding tenant

Il modulo consente all'azienda cliente di registrare il proprio tenant nella piattaforma. Il sistema guida l'utente nella concessione dei permessi necessari, mostrando in modo trasparente quali dati verranno letti, per quale finalità e con quale livello di autorizzazione.

### 6.2 Raccolta configurazioni

Il modulo interroga le API Microsoft disponibili, recupera le informazioni rilevanti e le normalizza in un modello dati interno. La raccolta è tracciata, versionata e associata a una specifica esecuzione di assessment.

Il sistema distingue tra:
- Dati effettivamente recuperati
- Dati non disponibili per mancanza di permessi
- Dati non disponibili per mancanza di licenza
- Dati non applicabili allo specifico tenant

In caso di errore, il sistema distingue tra errori transitori (timeout, API temporaneamente non disponibili) ed errori permanenti (permessi revocati, tenant non raggiungibile).

### 6.3 Rule engine

Il rule engine è il cuore del prodotto. Ogni regola verifica una specifica configurazione del tenant e produce un esito tra: **conforme**, **parzialmente conforme**, **non conforme**, **non verificabile**, **non applicabile**.

Ogni regola è associata a:
- Un principio Zero Trust
- Una categoria di controllo
- Un livello di severità
- Una descrizione del rischio
- Una raccomandazione di remediation
- Eventuali riferimenti tecnici

**Ciclo di vita delle regole:** bozza → in test → attiva → deprecata. Solo le regole attive contribuiscono agli esiti e allo score.

### 6.4 Scoring

Il modulo produce una valutazione sintetica della postura Identity del tenant, espressa in forma percentuale per area funzionale e per principio Zero Trust. Lo score tiene conto di: severità delle criticità, numero di oggetti coinvolti, esposizione potenziale, presenza di eccezioni e impatto operativo della remediation.

### 6.5 Dashboard

La dashboard fornisce una vista immediata dello stato del tenant analizzato: punteggio complessivo, numero di criticità per severità, aree più esposte, trend rispetto agli assessment precedenti e azioni prioritarie consigliate.

Supporta filtri per: categoria, severità, principio Zero Trust, stato della remediation, data di rilevazione e area tecnica.

### 6.6 Report

Il modulo consente la generazione di un documento esportabile destinato sia a interlocutori tecnici sia a stakeholder non tecnici. Include: sintesi manageriale, dettaglio delle evidenze, livello di rischio, principio Zero Trust impattato, motivazione funzionale della criticità e azioni consigliate.

Formati: **PDF** (Fase 1), Word/Excel (evoluzione futura).

### 6.7 Remediation tracking

Il sistema consente di tracciare lo stato di avanzamento delle azioni correttive. Ogni evidenza può essere marcata come: **aperta**, **in analisi**, **accettata come rischio**, **pianificata**, **risolta**, **non applicabile**.

La piattaforma mantiene lo storico delle evidenze per confrontare assessment successivi.

### 6.8 Offboarding tenant

Il processo prevede:
- Revoca dei permessi concessi alla piattaforma sul tenant Microsoft
- Cancellazione di tutti i dati raccolti e degli assessment associati
- Notifica all'utente della corretta esecuzione
- Registrazione tracciabile dell'avvenuta cancellazione

L'operazione richiede conferma esplicita ed è irreversibile.

---

## 7. Categorie di controllo

| Categoria | Perimetro di analisi |
|---|---|
| **Autenticazione & MFA** | Copertura MFA, metodi deboli, autenticazione legacy, esclusioni rischiose |
| **Conditional Access** | Copertura policy, eccezioni, protezione amministratori, accessi a rischio |
| **Ruoli e privilegi** | Account privilegiati, assegnazioni permanenti, PIM, break-glass |
| **Applicazioni e consensi** | Permessi applicativi, consensi admin, secret scaduti, service principal |
| **Guest e collaborazione** | Utenti B2B, guest inattivi, guest privilegiati, external collaboration |
| **Dispositivi e compliance** | Stato dispositivo, hybrid join, conformità nelle policy di accesso |
| **Audit e logging** | Log di accesso, audit log, eventi privilegiati, segnali di rischio |

---

## 8. Esempio di evidenza prodotta dal sistema

| Campo | Valore |
|---|---|
| **Titolo** | Account amministrativi esclusi da MFA |
| **Principio Zero Trust** | Verifica esplicita; Minimo privilegio |
| **Severità** | Critica |
| **Stato** | Aperta |

**Descrizione:** Sono stati rilevati uno o più account con ruoli amministrativi che non risultano coperti da criteri di autenticazione multifattore o che risultano esclusi da policy di Conditional Access applicabili agli amministratori.

**Rischio:** Un account amministrativo non protetto da MFA rappresenta un punto di compromissione ad alto impatto. In caso di furto credenziali, un attaccante potrebbe ottenere accesso privilegiato al tenant.

**Raccomandazione:** Applicare MFA obbligatoria a tutti gli account amministrativi, ridurre le esclusioni, verificare la presenza di account break-glass adeguatamente protetti, valutare l'utilizzo di PIM per l'attivazione temporanea dei privilegi.

---

## 9. Requisiti di sicurezza della piattaforma

- Tutti gli accessi sono tracciati
- I dati raccolti sono cifrati a riposo e in transito
- Credenziali, token, secret e certificati gestiti tramite Azure Key Vault
- Modello multi-tenant sicuro con completa segregazione dei dati tra clienti diversi
- Gestione dei ruoli interni: amministratore piattaforma, amministratore cliente, auditor, viewer, operatore tecnico
- Funzione di revoca accesso, cancellazione dati e audit delle consultazioni

---

## 10. Requisiti non funzionali

- La soluzione è scalabile, sicura, modulare e facilmente estendibile
- Il rule engine consente l'aggiunta di nuove regole senza modifiche strutturali
- Le esecuzioni degli assessment sono asincrone, tracciabili e ripetibili
- La piattaforma garantisce tempi di risposta adeguati anche in presenza di tenant enterprise (>10.000 oggetti)

---

## 11. Output previsti

- Dashboard interattiva di sintesi
- Report di assessment esportabile (PDF)
- Elenco dettagliato delle evidenze
- Score complessivo e score per area
- Mappatura delle criticità sui principi Zero Trust
- Piano di remediation prioritizzato
- Storico degli assessment
- Trend di miglioramento o peggioramento nel tempo

---

## 12. Criteri di accettazione

Il sistema soddisfa i criteri di accettazione quando è in grado di:

- Consentire a un cliente autorizzato di collegare un tenant Microsoft tramite consenso amministrativo
- Recuperare le configurazioni Identity previste dal perimetro minimo di analisi
- Eseguire un set iniziale di regole di controllo e produrre un esito per ciascuna regola
- Associare ogni evidenza a: severità, descrizione del rischio, principio Zero Trust e raccomandazione
- Generare una dashboard consultabile dall'utente
- Generare un report esportabile
- Mantenere lo storico degli assessment eseguiti
- Garantire segregazione dei dati tra clienti diversi
- Operare in modalità read-only nella fase standard di assessment

---

## 13. Possibili evoluzioni future

- Funzionalità di remediation assistita
- Confronto con baseline personalizzate (NIS2, DORA, ISO 27001)
- Integrazione con sistemi SIEM
- Apertura automatica di ticket
- Classificazione del rischio tramite AI
- Suggerimenti contestuali basati sul settore aziendale
- Simulazione dell'impatto delle modifiche prima della loro applicazione
- **Continuous monitoring**: alerting su variazioni critiche tra un assessment e l'altro (nuovo admin globale, CA disabilitata, consenso ad alto privilegio aggiunto, ecc.)
