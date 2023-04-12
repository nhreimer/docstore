# A configuration-driven project for parsing, scraping, and a deduplicated data store.

## Modules
```
DocStore.Engine   = Raw Storage Engine for Documents
XClaw             = JSON Logic
Trident.Core      = Parsing Helper (specific ORM utils primarily for Slavic dept)
DocStore.Driver   = The main utility that glues everything together
```

## Configurations

```
Configurations:
  MainConfig      = pointer to other configs
    SourceConfig  = all the sources and their info, e.g., SourceId, LandingPage, Name
      Schema      = specific parser information for a source
      Requester   = specific requesters to use for collecting new content
    BackendConfig = db queries
    ErrorConfig   = content dumping for query-related errors
```
