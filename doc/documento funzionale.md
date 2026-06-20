**Documento funzionale — Piattaforma di assessment Zero Trust per configurazioni Identity Microsoft**

**1. Premessa**

Il presente documento descrive le funzionalità previste per la realizzazione di una piattaforma destinata alla verifica del livello di allineamento di un’organizzazione ai principi Zero Trust, con particolare riferimento alle configurazioni Identity dei tenant Microsoft.

Il prodotto ha l’obiettivo di analizzare, in modo strutturato e ripetibile, la postura di sicurezza del tenant Microsoft dell’azienda, evidenziando eventuali configurazioni deboli, incomplete, incoerenti o non conformi alle best practice di sicurezza relative a Microsoft Entra ID, Conditional Access, gestione degli utenti privilegiati, autenticazione multifattore, applicazioni registrate, consensi, ruoli amministrativi, accessi guest, dispositivi e workload identity.

La piattaforma non si limita a fornire una fotografia tecnica dello stato del tenant, ma produce una valutazione funzionale del rischio, indicando il grado di disallineamento rispetto ai principi Zero Trust e suggerendo interventi correttivi ordinati per priorità, impatto e urgenza.

**2. Obiettivo del prodotto**

L’obiettivo principale del prodotto è fornire alle aziende uno strumento in grado di valutare quanto le configurazioni Identity del proprio ambiente Microsoft siano coerenti con un modello Zero Trust.

Il sistema, previa concessione degli accessi necessari da parte dell’azienda cliente, interroga le configurazioni del tenant Microsoft e costruisce un assessment basato su controlli predefiniti. Tali controlli verificano, ad esempio, se l’accesso degli utenti è protetto da criteri di autenticazione forte, se gli account privilegiati sono gestiti secondo logiche di minimo privilegio, se le applicazioni dispongono di permessi eccessivi, se sono presenti utenti guest non governati, se esistono eccezioni rischiose nelle policy di accesso condizionale e se il tenant espone superfici di attacco non presidiate.

Il risultato dell’analisi deve essere presentato tramite dashboard, report esportabile e schede di dettaglio, in modo che l’azienda possa comprendere non solo quali configurazioni risultano critiche, ma anche perché lo sono, quale principio Zero Trust viene compromesso e quale azione correttiva è consigliata.

**3. Principi Zero Trust di riferimento**

Il modello di valutazione del prodotto si basa sui tre principi fondamentali dello Zero Trust.

Il primo principio è la verifica esplicita. Ogni richiesta di accesso deve essere autenticata e autorizzata sulla base del maggior numero possibile di segnali disponibili, come identità dell’utente, posizione, rischio, stato del dispositivo, applicazione utilizzata e contesto della richiesta. In questa prospettiva, il prodotto dovrà verificare la presenza e la qualità delle configurazioni di autenticazione multifattore, Conditional Access, Identity Protection, blocco dell’autenticazione legacy e controllo degli accessi anomali.

Il secondo principio è il minimo privilegio. Ogni utente, servizio, applicazione o workload deve disporre solo dei permessi strettamente necessari e per il tempo strettamente necessario. La piattaforma dovrà quindi analizzare ruoli amministrativi, assegnazioni permanenti, utilizzo di Privileged Identity Management, permessi applicativi, service principal, consensi amministrativi, gruppi privilegiati e account con privilegi elevati.

Il terzo principio è l’assunzione della compromissione. Il sistema deve partire dal presupposto che una violazione possa già essere in corso o possa verificarsi in qualsiasi momento. Per questo motivo, il prodotto dovrà verificare la capacità del tenant di limitare il raggio d’azione di un eventuale attaccante, rilevare configurazioni rischiose, ridurre le eccezioni, controllare gli accessi guest, monitorare gli eventi sensibili e garantire tracciabilità delle modifiche.

**4. Perimetro funzionale**

Il prodotto dovrà operare principalmente sui tenant Microsoft 365 / Microsoft Entra ID dell’azienda cliente. Il perimetro minimo di analisi comprende:

Microsoft Entra ID, con particolare attenzione a utenti, gruppi, ruoli, account amministrativi, accessi guest, metodi di autenticazione, criteri MFA e configurazioni generali del tenant.

Conditional Access, con verifica delle policy esistenti, degli utenti o gruppi inclusi ed esclusi, delle applicazioni protette, delle condizioni applicate e delle eventuali eccezioni presenti.

Privileged Identity Management, qualora disponibile nel tenant, con analisi delle assegnazioni permanenti, eleggibili, attive, scadute o prive di approvazione.

App Registration ed Enterprise Application, con verifica dei permessi delegati e applicativi, dei consensi amministrativi, dei secret e certificati in scadenza, delle applicazioni inutilizzate o potenzialmente rischiose.

Guest e collaborazione esterna, con analisi degli utenti B2B, delle impostazioni di external collaboration, della presenza di guest inattivi, guest privilegiati o guest non soggetti a policy di sicurezza adeguate.

Audit e sign-in log, nei limiti consentiti dai permessi concessi e dalle licenze disponibili, per individuare eventi anomali, accessi legacy, accessi rischiosi, applicazioni non protette o pattern di utilizzo non coerenti con le policy attese.

**5. Accesso al tenant e autorizzazioni**

Per poter eseguire l’analisi, il prodotto dovrà ottenere dall’azienda cliente un’autorizzazione esplicita all’accesso ai dati di configurazione del tenant. L’accesso dovrà essere progettato secondo il principio del minimo privilegio, richiedendo esclusivamente i permessi necessari alla lettura delle configurazioni da analizzare.

La soluzione dovrà privilegiare un modello read-only, evitando qualsiasi capacità di modifica del tenant in fase di assessment standard. Eventuali funzionalità future di remediation automatica dovranno essere considerate come modulo separato, soggetto a autorizzazioni distinte, approvazione esplicita e tracciamento puntuale delle operazioni.

Il processo di onboarding dovrà prevedere una fase di consenso amministrativo, una verifica dei permessi concessi, la registrazione del tenant cliente, la generazione di un primo assessment e la possibilità di revocare l’accesso in qualsiasi momento.

**6. Moduli funzionali previsti**

**6.1 Modulo di onboarding tenant**

Il modulo di onboarding consente all’azienda cliente di registrare il proprio tenant all’interno della piattaforma. Durante questa fase vengono raccolte le informazioni minime necessarie all’identificazione del cliente, del tenant e del referente autorizzato.

Il sistema dovrà guidare l’utente nella concessione dei permessi necessari, mostrare in modo trasparente quali dati verranno letti, per quale finalità e con quale livello di autorizzazione. Al termine dell’onboarding, il tenant viene censito nella piattaforma e reso disponibile per l’esecuzione dell’assessment.

**6.2 Modulo di raccolta configurazioni**

Il modulo di raccolta configurazioni ha il compito di interrogare le API Microsoft disponibili, recuperare le informazioni rilevanti e normalizzarle in un modello dati interno. La raccolta dovrà essere tracciata, versionata e associata a una specifica esecuzione di assessment.

Il sistema dovrà distinguere tra dati effettivamente recuperati, dati non disponibili per mancanza di permessi, dati non disponibili per mancanza di licenza e dati non applicabili allo specifico tenant. Questa distinzione è essenziale per evitare valutazioni errate o falsi positivi.

In caso di errore durante la raccolta, il sistema dovrà gestire le eccezioni in modo controllato, registrando il dettaglio dell'errore, notificando l'utente attraverso la pagina di reportistica e consentendo la riesecuzione dell'assessment senza perdere i dati già raccolti. Il sistema dovrà distinguere tra errori transitori, quali timeout o indisponibilità temporanea delle API Microsoft, ed errori permanenti, quali permessi revocati o tenant non più raggiungibile. In presenza di errori parziali, dovrà essere possibile completare l'assessment sui dati disponibili, indicando chiaramente quali aree non sono state analizzate e per quale motivo.

**6.3 Modulo di rule engine**

Il rule engine rappresenta il cuore del prodotto. Ogni regola verifica una specifica configurazione del tenant e produce un esito funzionale. Gli esiti possibili possono essere: conforme, parzialmente conforme, non conforme, non verificabile, non applicabile.

Ogni regola dovrà essere associata a un principio Zero Trust, a una categoria di controllo, a un livello di severità, a una descrizione del rischio, a una raccomandazione di remediation e a eventuali riferimenti tecnici.

Il rule engine dovrà supportare un ciclo di vita strutturato per le regole. Ogni regola potrà trovarsi in uno dei seguenti stati: bozza, in test, attiva, deprecata. Le regole in stato di bozza non vengono eseguite negli assessment produttivi. Le regole in test possono essere eseguite in ambienti di verifica o su tenant selezionati. Solo le regole attive contribuiscono agli esiti e allo score. Le regole deprecate vengono mantenute per garantire la consultazione dello storico degli assessment in cui erano attive. L'aggiunta o la modifica di regole dovrà essere tracciata con versione, data e autore della modifica.

Esempio: se nel tenant sono presenti account amministrativi permanenti senza MFA obbligatoria, la regola dovrà segnalare una non conformità critica, collegata sia al principio di verifica esplicita sia al principio di minimo privilegio.

**6.4 Modulo di scoring**

Il modulo di scoring produce una valutazione sintetica della postura Identity del tenant. Lo score non dovrà essere interpretato come certificazione assoluta di sicurezza, ma come indicatore di maturità e disallineamento rispetto al modello Zero Trust.

Lo scoring potrà essere espresso in forma percentuale, per area funzionale e per principio Zero Trust. Ad esempio, il sistema potrà mostrare un punteggio complessivo, un punteggio relativo a Conditional Access, uno relativo agli account privilegiati, uno relativo alle applicazioni e uno relativo agli utenti guest.

Lo score dovrà tenere conto della severità delle criticità, del numero di oggetti coinvolti, dell’esposizione potenziale, della presenza di eccezioni e dell’impatto operativo della remediation.

**6.5 Modulo dashboard**

La dashboard dovrà fornire una vista immediata dello stato del tenant analizzato. La schermata principale dovrà mostrare il punteggio complessivo, il numero di criticità per severità, le aree più esposte, il trend rispetto agli assessment precedenti e le azioni prioritarie consigliate.

La dashboard dovrà permettere di filtrare le evidenze per categoria, severità, principio Zero Trust, stato della remediation, data di rilevazione e area tecnica.

**6.6 Modulo report**

Il modulo report dovrà consentire la generazione di un documento esportabile, destinato sia a interlocutori tecnici sia a stakeholder non tecnici. Il report dovrà includere una sintesi manageriale, il dettaglio delle evidenze, il livello di rischio, il principio Zero Trust impattato, la motivazione funzionale della criticità e le azioni consigliate.

Il report dovrà essere esportabile almeno in formato PDF e, in una fase successiva, eventualmente in formato Word o Excel.

**6.7 Modulo remediation tracking**

Il sistema dovrà consentire di tracciare lo stato di avanzamento delle azioni correttive. Ogni evidenza potrà essere marcata come aperta, in analisi, accettata come rischio, pianificata, risolta o non applicabile.

La piattaforma dovrà mantenere lo storico delle evidenze, in modo da confrontare assessment successivi e verificare se le criticità siano state effettivamente risolte, peggiorate o rimaste invariate.

**6.8 Modulo offboarding tenant**

Il modulo di offboarding consente all'azienda cliente o all'amministratore della piattaforma di terminare il rapporto con un tenant precedentemente registrato.

Il processo di offboarding dovrà prevedere la revoca dei permessi concessi alla piattaforma sul tenant Microsoft, la cancellazione di tutti i dati raccolti e degli assessment associati al tenant, la notifica all'utente della corretta esecuzione dell'operazione e la produzione di una registrazione tracciabile dell'avvenuta cancellazione.

Il sistema dovrà consentire una cancellazione selettiva, ad esempio la sola revoca dell'accesso con conservazione dello storico per un periodo definito, e una cancellazione completa e immediata di tutti i dati. L'operazione di offboarding dovrà richiedere conferma esplicita e dovrà essere irreversibile una volta completata.

**7. Categorie di controllo**

Le principali categorie di controllo previste sono le seguenti.

**Autenticazione e MFA**

Il sistema verifica se gli utenti, in particolare quelli privilegiati, sono protetti da autenticazione multifattore, se sono ancora consentiti metodi deboli, se sono presenti esclusioni rischiose e se l’autenticazione legacy è ancora utilizzabile.

**Conditional Access**

Il sistema analizza la presenza, la copertura e la coerenza delle policy di Conditional Access. Devono essere verificate le policy su utenti amministrativi, utenti standard, applicazioni critiche, accessi da paesi o reti non attendibili, dispositivi non conformi, rischio utente e rischio di accesso.

**Ruoli amministrativi e privilegi**

Il sistema verifica il numero di account privilegiati, la presenza di assegnazioni permanenti, l’utilizzo di PIM, gli account break-glass, gli amministratori inattivi, gli utenti guest con ruoli elevati e l’eventuale eccessiva concentrazione di privilegi.

**Applicazioni e consensi**

Il sistema analizza le App Registration e le Enterprise Application per individuare permessi eccessivi, consensi amministrativi rischiosi, secret obsoleti o in scadenza, applicazioni non utilizzate, service principal privilegiati e applicazioni con accesso a dati sensibili.

**Utenti guest e collaborazione esterna**

Il sistema verifica la governance degli utenti guest, la presenza di guest inattivi, guest privilegiati, guest esclusi da MFA o Conditional Access, impostazioni troppo permissive di collaborazione esterna e assenza di processi di revisione periodica.

**Dispositivi e compliance**

Il sistema valuta, ove possibile, se le policy di accesso tengono conto dello stato del dispositivo, della conformità, dell’hybrid join o di segnali equivalenti. L’obiettivo è verificare se l’accesso alle risorse aziendali sia condizionato anche all’affidabilità del dispositivo.

**Audit, logging e monitoraggio**

Il sistema verifica la disponibilità e l’utilizzo dei log di accesso, audit log, eventi privilegiati, attività applicative e segnali di rischio. L’analisi dovrà evidenziare eventuali lacune nella capacità di rilevazione, investigazione e risposta.

**8. Esempio di evidenza prodotta dal sistema**

Titolo evidenza: Account amministrativi esclusi da MFA.

Descrizione: Sono stati rilevati uno o più account con ruoli amministrativi che non risultano coperti da criteri di autenticazione multifattore o che risultano esclusi da policy di Conditional Access applicabili agli amministratori.

Principio Zero Trust impattato: Verifica esplicita; minimo privilegio.

Severità: Critica.

Rischio: Un account amministrativo non protetto da MFA rappresenta un punto di compromissione ad alto impatto. In caso di furto credenziali, un attaccante potrebbe ottenere accesso privilegiato al tenant e modificare configurazioni, utenti, applicazioni o policy di sicurezza.

Raccomandazione: Applicare MFA obbligatoria a tutti gli account amministrativi, ridurre le esclusioni, verificare la presenza di account break-glass adeguatamente protetti e monitorati, valutare l’utilizzo di Privileged Identity Management per l’attivazione temporanea dei privilegi.

Stato: Aperta.

**9. Requisiti di sicurezza della piattaforma**

La piattaforma dovrà essere progettata con particolare attenzione alla sicurezza, poiché tratta informazioni sensibili relative alla postura Identity dei tenant aziendali.

Tutti gli accessi dovranno essere tracciati. I dati raccolti dovranno essere cifrati a riposo e in transito. Le credenziali, i token, i secret e i certificati utilizzati dalla piattaforma dovranno essere gestiti tramite sistemi sicuri, evitando la memorizzazione in chiaro.

La piattaforma dovrà adottare un modello multi-tenant sicuro, garantendo la completa segregazione dei dati tra clienti diversi. Ogni cliente dovrà poter accedere esclusivamente ai propri tenant e ai propri report.

Dovrà essere prevista una gestione dei ruoli interni alla piattaforma, distinguendo almeno tra amministratore piattaforma, amministratore cliente, auditor, viewer e operatore tecnico.

Dovrà inoltre essere prevista una funzione di revoca dell’accesso, cancellazione dei dati, audit delle consultazioni e registrazione delle attività effettuate dagli utenti della piattaforma.

**10. Requisiti non funzionali**

La soluzione dovrà essere scalabile, sicura, modulare e facilmente estendibile. Il rule engine dovrà permettere l’aggiunta di nuove regole senza richiedere modifiche strutturali all’intera piattaforma.

Le esecuzioni degli assessment dovranno essere asincrone, tracciabili e ripetibili. Ogni assessment dovrà conservare la data di esecuzione, il tenant analizzato, la versione delle regole utilizzate, gli esiti prodotti e gli eventuali limiti dell’analisi.

La piattaforma dovrà garantire tempi di risposta adeguati nella consultazione delle dashboard, anche in presenza di tenant di grandi dimensioni.

**11. Output previsti**

Gli output principali del sistema sono:

Dashboard interattiva di sintesi.

Report di assessment esportabile.

Elenco dettagliato delle evidenze.

Score complessivo e score per area.

Mappatura delle criticità sui principi Zero Trust.

Piano di remediation prioritizzato.

Storico degli assessment.

Trend di miglioramento o peggioramento nel tempo.

**12. Criteri di accettazione**

Il sistema dovrà consentire a un cliente autorizzato di collegare un tenant Microsoft tramite consenso amministrativo.

Il sistema dovrà recuperare le configurazioni Identity previste dal perimetro minimo di analisi.

Il sistema dovrà eseguire un set iniziale di regole di controllo e produrre un esito per ciascuna regola.

Il sistema dovrà associare ogni evidenza a una severità, a una descrizione del rischio, a un principio Zero Trust e a una raccomandazione.

Il sistema dovrà generare una dashboard consultabile dall’utente.

Il sistema dovrà generare un report esportabile.

Il sistema dovrà mantenere lo storico degli assessment eseguiti.

Il sistema dovrà garantire segregazione dei dati tra clienti diversi.

Il sistema dovrà operare in modalità read-only nella fase standard di assessment.

**13. Possibili evoluzioni future**

In una fase successiva il prodotto potrà essere esteso con funzionalità di remediation assistita, confronto con baseline personalizzate, integrazione con sistemi SIEM, apertura automatica di ticket, classificazione del rischio tramite AI, suggerimenti contestuali basati sul settore aziendale e simulazione dell’impatto delle modifiche prima della loro applicazione.

Potrà inoltre essere previsto un modulo di continuous monitoring, che consenta di rilevare variazioni critiche nel tenant tra un assessment e l’altro, ad esempio la creazione di un nuovo amministratore globale, l’aggiunta di un consenso applicativo ad alto privilegio, la disattivazione di una policy di Conditional Access o l’introduzione di una nuova esclusione rischiosa.
