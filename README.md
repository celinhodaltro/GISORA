# GISORA

Plataforma de gestão territorial, cadastro técnico multifinalitário e inteligência geográfica para administração municipal.

## Escopo

- Aplicação web (Blazor WebAssembly) e aplicativo móvel (.NET MAUI Blazor Hybrid).
- Cadastro territorial e imobiliário, logradouros, equipamentos públicos, cemitérios, rural/ambiental, planejamento urbano, licenciamento, fiscalização e imagens 360°.
- Núcleo GIS com Leaflet, Turf.js e Proj4js.

## Estratégia arquitetural

Domain-Driven Design com bounded contexts isolados. A primeira versão é **local-first**: persistência no navegador através de adaptadores em `Infrastructure`, sem backend próprio. Nenhuma regra de negócio depende de `localStorage`, de Blazor ou de JavaScript.

A evolução para API ASP.NET Core + PostgreSQL/PostGIS ocorre pela substituição das implementações de `Infrastructure`, preservando `Domain`, `Application` e os contratos de repositório.

```
Domain  ←  Application  ←  Presentation (Hosts)
                ↑
          Infrastructure (implementa contratos)
```

## Stack

| Camada | Tecnologia |
| --- | --- |
| Runtime | .NET 10 (LTS) |
| Web | Blazor WebAssembly |
| Mobile | .NET MAUI Blazor Hybrid |
| UI | Radzen Blazor |
| Mapas | Leaflet, Turf.js, Proj4js |
| Persistência (fase 1) | LocalStorage via JavaScript Interop |
| Persistência (futuro) | ASP.NET Core, EF Core, PostgreSQL + PostGIS |
| CI/CD | GitHub Actions |

## Estrutura

```
src/
  Hosts/            Aplicações executáveis (Web, Mobile, Api)
  Modules/          Bounded contexts (Domain / Application / Infrastructure)
  Shared/           UI, Design System, Mapas e Contratos entre módulos
  BuildingBlocks/   Primitivas de DDD e de Application
  Infrastructure/   Adaptadores técnicos (LocalStorage, Files, Geospatial)
tests/
  Unit/ Integration/ Architecture/ E2E/
docs/               ADRs, contexto de domínio, matriz de rastreabilidade
```

Cada contexto só ganha camadas quando possui regra de negócio real. A fronteira entre módulos é garantida por testes arquiteturais, não apenas por pastas.

## Desenvolvimento

Requisitos: .NET SDK 10.0.4xx.

```bash
dotnet restore
dotnet build
dotnet test
```

Aplicacao web publicada em https://celinhodaltro.github.io/GISORA/ a cada merge na `main`.

## Documentacao

- [Arquitetura](docs/architecture.md)
- [Bounded contexts](docs/bounded-contexts.md)
- [Regras de dependencia](docs/dependency-rules.md)
- [Convencoes de codigo](docs/conventions.md)
- [Fontes de dados](docs/data-sources.md)
- [ADRs](docs/adr/README.md)

## Processo

Toda alteração nasce de uma issue, em uma branch exclusiva, integrada por pull request com verificações obrigatórias. Gestão via GitHub CLI.

- Floresta de branches: `feature/<issue>-<slug>`, `bugfix/...`, `refactor/...`, `infra/...`
- Commits em Conventional Commits, referenciando a issue.
