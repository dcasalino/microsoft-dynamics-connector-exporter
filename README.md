# Microsoft Dynamics Connector Exporter

## Italiano
Questa utility nasce per risolvere un problema concreto: **verificare in modo rapido e affidabile che le mappature dei connettori Dynamics siano identiche su tutti gli ambienti deployati**.  
Se, come nel mio caso, devi garantire coerenza tra più connettori/istanze, questo progetto è stato fondamentale per confrontare i flussi e individuare differenze nascoste.

### Perché è utile (e perché fa davvero la differenza)
- **Esporta le mappature dei flussi** in file di testo leggibili, così puoi ispezionare e confrontare facilmente.
- **Evidenzia modifiche** rispetto a esecuzioni precedenti, segnalando in console i flussi cambiati.
- **Archivia versioni con timestamp**, utile per audit e storico delle configurazioni.
- **Genera anche i file `.map`** originali, utili per backup o analisi tecniche.

### Come funziona in breve
1. Si connette ai servizi di configurazione di Microsoft Dynamics Connector.
2. Elenca integrazioni e flussi disponibili.
3. Esporta tutte le mappe del flusso selezionato in:
- file `.txt` con mapping, filtri e hash
- file `.map` con la definizione XML
4. Confronta automaticamente con l’ultima esportazione salvata.

### Configurazione
Le credenziali e il contesto sono letti da `MDCE/App.config`:
- `Username`
- `Password`
- `Domain`
- `Customer`

### Librerie incluse (DLL)
Nel progetto sono incluse alcune librerie di Microsoft Dynamics Connector, necessarie per connettersi ai servizi e leggere/esportare le definizioni dei flussi. Qui sotto trovi lo scopo e i principali tipi usati in questa codebase.

**`MDCE/lib/Microsoft.Dynamics.Integration.Client.Core.dll`**  
Scopo: client e contratti per il servizio di configurazione.  
Tipi usati: `ConfigurationServiceContractClient`, `ApplicationIntegration`, `SiteIntegration`, `MapWithDetail`, `MapDefinition`, `EntryValidationResult`, `FilterDefinition`, `FilterElement`, `FilterCriterion`, `SourceValue`.
Namespace principali: `Microsoft.Dynamics.Integration.Client.Core.ConfigurationService`.
Riferimenti nel codice: `MDCE/Program.cs:1`, `MDCE/Program.cs:31`, `MDCE/Program.cs:36-216`, `MDCE/Program.cs:237-255`, `MDCE/Program.cs:310`.

**`MDCE/lib/Microsoft.Dynamics.Integration.Service.dll`**  
Scopo: contratti e tipi di supporto per i servizi di integrazione richiamati dal client di configurazione.  
Tipi usati: richiamati indirettamente dal client durante l’export e la validazione.
Namespace principali: usati tramite i proxy del client di configurazione.
Riferimenti nel codice: `MDCE/Program.cs:31`, `MDCE/Program.cs:175`, `MDCE/Program.cs:192`, `MDCE/Program.cs:210`, `MDCE/Program.cs:310`.

**`MDCE/lib/Microsoft.Dynamics.Integration.Mapping.Helpers.dll`**  
Scopo: helper per interpretare e manipolare le definizioni di mapping.  
Tipi usati: usati indirettamente nella lettura delle mappature restituite dal servizio.
Namespace principali: usati indirettamente dai tipi di `MapDefinition`.
Riferimenti nel codice: `MDCE/Program.cs:175`, `MDCE/Program.cs:237-265`.

**`MDCE/lib/Microsoft.Dynamics.Integration.Common.dll`**  
Scopo: tipi comuni condivisi tra i moduli di integrazione.  
Tipi usati: contenitori e definizioni comuni consumate dalle API di configurazione.
Namespace principali: usati indirettamente dai proxy di servizio.
Riferimenti nel codice: `MDCE/Program.cs:36-216`.

### Esecuzione
Dopo il build, avvia l’eseguibile:
```console
MDCE/bin/Debug/MicrosoftDynamicsConnectorExporter.exe
```
Puoi anche passare direttamente i GUID (integrazione e flusso) per saltare la selezione interattiva.

Se il tuo obiettivo è **dimostrare che le mappature tra connettori Dynamics sono allineate**, questo tool ti fa risparmiare ore di controlli manuali e riduce il rischio di errori.

---

## English
This utility was created to solve a very practical problem: **quickly and reliably verifying that Dynamics connector mappings are identical across all deployed environments**.  
If, like me, you need consistency across multiple connectors/instances, this project has been essential for comparing flows and spotting hidden differences.

### Why it is useful (and why it makes a real difference)
- **Exports flow mappings** into readable text files so you can inspect and compare them easily.
- **Highlights changes** versus previous runs, reporting modified flows in the console.
- **Archives timestamped versions**, which is great for audits and configuration history.
- **Also generates the original `.map` files**, useful for backup or deep analysis.

### How it works (quick overview)
1. Connects to Microsoft Dynamics Connector configuration services.
2. Lists available integrations and flows.
3. Exports all maps of the selected flow into:
- `.txt` files with mappings, filters, and hashes
- `.map` files with the XML definition
4. Automatically compares against the latest saved export.

### Configuration
Credentials and context are read from `MDCE/App.config`:
- `Username`
- `Password`
- `Domain`
- `Customer`

### Included Libraries (DLLs)
The project ships with Microsoft Dynamics Connector libraries required to connect to configuration services and export flow definitions. Below is the purpose and the main types used by this codebase.

**`MDCE/lib/Microsoft.Dynamics.Integration.Client.Core.dll`**  
Purpose: client and contracts for the configuration service.  
Types used: `ConfigurationServiceContractClient`, `ApplicationIntegration`, `SiteIntegration`, `MapWithDetail`, `MapDefinition`, `EntryValidationResult`, `FilterDefinition`, `FilterElement`, `FilterCriterion`, `SourceValue`.
Main namespace: `Microsoft.Dynamics.Integration.Client.Core.ConfigurationService`.
Code references: `MDCE/Program.cs:1`, `MDCE/Program.cs:31`, `MDCE/Program.cs:36-216`, `MDCE/Program.cs:237-255`, `MDCE/Program.cs:310`.

**`MDCE/lib/Microsoft.Dynamics.Integration.Service.dll`**  
Purpose: integration service contracts and supporting types invoked by the configuration client.  
Types used: indirectly by the client during export and validation.
Main namespace: used via configuration client proxies.
Code references: `MDCE/Program.cs:31`, `MDCE/Program.cs:175`, `MDCE/Program.cs:192`, `MDCE/Program.cs:210`, `MDCE/Program.cs:310`.

**`MDCE/lib/Microsoft.Dynamics.Integration.Mapping.Helpers.dll`**  
Purpose: helpers to interpret and manipulate mapping definitions.  
Types used: indirectly while reading mapping definitions returned by the service.
Main namespace: used indirectly by `MapDefinition` types.
Code references: `MDCE/Program.cs:175`, `MDCE/Program.cs:237-265`.

**`MDCE/lib/Microsoft.Dynamics.Integration.Common.dll`**  
Purpose: common/shared integration types.  
Types used: shared containers and definitions consumed by configuration APIs.
Main namespace: used indirectly by service proxies.
Code references: `MDCE/Program.cs:36-216`.

### Run
After building, launch the executable:
```console
MDCE/bin/Debug/MicrosoftDynamicsConnectorExporter.exe
```
You can also pass the integration and site flow GUIDs directly to skip interactive selection.

If your goal is **to prove that Dynamics connector mappings are aligned across environments**, this tool saves hours of manual checks and reduces the risk of mistakes.
