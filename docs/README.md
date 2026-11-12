# Documentacao tecnica

| Documento | Conteudo |
| --- | --- |
| [architecture.md](architecture.md) | Camadas, direcao de dependencias, composicao por host |
| [bounded-contexts.md](bounded-contexts.md) | Contextos delimitados e ondas de implementacao |
| [dependency-rules.md](dependency-rules.md) | Regras de dependencia garantidas por teste |
| [data-sources.md](data-sources.md) | Proveniencia, CRS, precisao e marcação de dado demonstrativo |
| [adr/](adr/) | Decisoes arquiteturais registradas |

## ADRs

Toda decisao estrutural relevante vira um ADR em `adr/NNNN-titulo.md`, com contexto, decisao, alternativas rejeitadas e consequencias. ADR nao e ata: e registro do motivo.

## Convencoes

- Documentacao entra por pull request, como codigo.
- Nenhuma afirmacao de versao ou comportamento sem verificacao na maquina ou no CI.
- Diagramas em texto (Mermaid) para permanecerem versionaveis.
