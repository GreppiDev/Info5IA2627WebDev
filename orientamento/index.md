# Orientamento

Questa sezione raccoglie i materiali di orientamento dedicati alle classi quinte dell'indirizzo Informatica per collegare in modo concreto gli argomenti del [programma del corso](../README.md) — basi di dati SQL, API REST, container Docker, sicurezza web, cloud e Intelligenza Artificiale — con le architetture, le tecnologie e i profili professionali richiesti nel mondo aziendale *enterprise* e nei percorsi universitari o ITS post-diploma.

## IBM — Data Evolution: Architetture Dati Enterprise

Il modulo **Data Evolution** nasce in raccordo con il percorso di orientamento IBM e accompagna lo studente dal classico stack applicativo scolastico (`App Web/Mobile + API REST + RDBMS`) fino alle moderne piattaforme aziendali per la gestione dei dati su larga scala, lo streaming in tempo reale e l'Intelligenza Artificiale generativa e agentica.

### Materiali disponibili

- **Guida completa**: [Oltre l'App: Guida Completa alle Architetture Dati Enterprise](./IBM/data-evolution/index.md) — trattazione graduale ed esaustiva con spiegazioni degli acronimi, diagrammi, esempi SQL/JSON/C#, casi d'uso reali, mappa di raccordo con il programma della 5ª, orientamento professionale, domande di autoverifica, glossario e fonti.
- **Presentazione visiva (30 slide)**: [Architetture_Dati_Enterprise.pdf](./IBM/data-evolution/Architetture_Dati_Enterprise.pdf) — supporto grafico per l'esposizione in aula e il ripasso rapido.

### Indice generale del percorso IBM (`./IBM/data-evolution/`)

1. **[1. Dal Nostro Stack Scolastico al Punto di Rottura Enterprise](./IBM/data-evolution/index.md#1-dal-nostro-stack-scolastico-al-punto-di-rottura-enterprise)**
   - Il punto di partenza del perito informatico (API REST, ASP.NET Core, Docker, RDBMS e garanzie ACID).
   - I limiti su scala aziendale: colli di bottiglia sincroni, varietà dei dati e conflitto **OLTP vs OLAP**.
   - I tre pilastri delle architetture moderne (*Storage Analitico*, *Data in Motion*, *Data Fabric & Integrazione*).
2. **[2. L'Evoluzione dello Storage Analitico: da RDBMS a Data Lakehouse](./IBM/data-evolution/index.md#2-levoluzione-dello-storage-analitico-da-rdbms-a-data-lakehouse)**
   - **Data Warehouse** (architettura distribuita MPP, storage colonnare, *Schema-on-Write*, Data Mart).
   - **Data Lake** su Object Storage (AWS S3, IBM COS), *Schema-on-Read*, rischio di *Data Swamp* e *Dark Data*.
   - Il **Data Lakehouse** aperto: calcolo disaccoppiato, accesso *zero-copy*, formati file aperti (**Parquet**, **ORC**, **Avro**), **Open Table Formats** (**Apache Iceberg**, **Delta Lake**, **Apache Hudi**) e caso reale **IBM watsonx.data** (motori **Presto**, **Spark**, **OpenSearch**, **Cassandra**).
3. **[3. Data Gravity, Data Fabric, Governance e Integrazione dei Dati](./IBM/data-evolution/index.md#3-data-gravity-data-fabric-governance-e-integrazione-dei-dati)**
   - Silos informativi e *Data Gravity*.
   - Il paradigma **Data Fabric**: virtualizzazione dei dati, metadati attivi federati (*Knowledge Graph*), automazione ML, architettura a 6 livelli e **Data Mesh**.
   - Governance, *Data Lineage*, *Data Quality*, mascheramento dei dati sensibili e raccordo con il **GDPR**.
   - Pattern di integrazione: **ETL**, **ELT**, **CDC** (*Change Data Capture*) e virtualizzazione.
4. **[4. Data in Motion: Architetture Event-Driven e Apache Kafka](./IBM/data-evolution/index.md#4-data-in-motion-architetture-event-driven-e-apache-kafka)**
   - Limiti del REST sincrono (*polling*, *tight coupling*, "Spaghetti REST") e modello asincrono **Publish/Subscribe**.
   - **Apache Kafka** (4.4.x): Broker, Topic, Partizioni, log *append-only*, Offset, Consumer Group, **KRaft**, **ELR**, *Tiered Storage*, *MirrorMaker 2* e sicurezza TLS/SASL/ACL.
   - Ecosistema **Kafka Connect**, **Kafka Streams**, **Schema Registry** e piattaforme enterprise gestite (**Confluent** e **IBM Event Streams**).
5. **[5. Integrazione Applicativa Ibrida (iPaaS), Paradigma Streamhouse e AI in Tempo Reale](./IBM/data-evolution/index.md#5-integrazione-applicativa-ibrida-ipaas-paradigma-streamhouse-e-ai-in-tempo-reale)**
   - Integrazione ibrida con **IBM App Connect** (*Designer* no-code, *Toolkit* pro-code, *Flow Pilot*, server **MCP**).
   - Dalla doppia infrastruttura Lambda alla convergenza **Streamhouse** (*Tableflow*, *Lightning Tables*).
   - **OpenRAG**, *Context Engineering* e agenti AI governati, fino alla visione d'insieme end-to-end (*The Enterprise Blueprint*).
6. **[6. Casi d'Uso Reali, Raccordo con il Programma, Orientamento Professionale e Glossario](./IBM/data-evolution/index.md#6-casi-duso-reali-raccordo-con-il-programma-orientamento-professionale-e-glossario)**
   - Casi di studio reali nei settori finanza, logistica, sport, sanità, legale e pubblica amministrazione.
   - Mappa completa di raccordo con le dispense del programma di 5ª.
   - Orientamento sui profili professionali (*Data Engineer*, *Platform/DevOps Engineer*, *Data Scientist/AI Engineer*, *Backend & Integration Engineer*), laboratorio locale in Docker Compose, domande di autoverifica e glossario ragionato degli acronimi.
   - [Riferimenti bibliografici e fonti](./IBM/data-evolution/index.md#riferimenti-bibliografici-e-fonti)
