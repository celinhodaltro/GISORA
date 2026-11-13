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
  GISORA.Domain/          Agregados, value objects e contratos, por contexto
  GISORA.Application/     Casos de uso, consultas e contratos, por contexto
  GISORA.Infrastructure/  Persistencia, geoespacial e integracoes
  GISORA.Web/             Blazor WASM: paginas, layout, mapas e design system
tests/
  Unit/ GISORA.UnitTests
  Architecture/ GISORA.ArchitectureTests
  E2E/ GISORA.Web.E2ETests
docs/                     ADRs, contexto de dominio e regras
```

Cada contexto mantem seu namespace (`GISORA.Domain.<Modulo>` / `GISORA.Application.<Modulo>`). A fronteira entre modulos e garantida por testes arquiteturais.
## Desenvolvimento

Requisitos: .NET SDK 10.0.4xx.

```bash
dotnet restore
dotnet build
dotnet test
```

Aplicacao web publicada em https://celinhodaltro.github.io/GISORA/ a cada merge na `main`.

## Estado atual

| Rota | Funcao |
| --- | --- |
| `/` | Visao geral com contadores reais |
| `/properties` | Listagem de imoveis com busca |
| `/properties/new` | Cadastro de imovel |
| `/map` | Imoveis com geometria no mapa |
| `/streets` | Logradouros |
| `/territory` | Municipio, distrito, bairro e setor |

O motor geografico (Leaflet, Turf.js, Proj4js) esta integrado. O cadastro funciona no dispositivo; nao ha backend. Veja [roadmap](docs/roadmap.md) para o que falta.

Aviso: enquanto nao houver autenticacao e API, a aplicacao opera em modo demonstracao e nao deve receber dado pessoal real (ADR 0007).

## Documentacao

- [Arquitetura](docs/architecture.md)
- [Guia da aplicacao](docs/guia.md)
- [Roadmap](docs/roadmap.md)
- [Bounded contexts](docs/bounded-contexts.md)
- [Regras de dependencia](docs/dependency-rules.md)
- [Convencoes de codigo](docs/conventions.md)
- [Fontes de dados](docs/data-sources.md)
- [ADRs](docs/adr/README.md)

## Processo

Toda alteração nasce de uma issue, em uma branch exclusiva, integrada por pull request com verificações obrigatórias. Gestão via GitHub CLI.

- Floresta de branches: `feature/<issue>-<slug>`, `bugfix/...`, `refactor/...`, `infra/...`
- Commits em Conventional Commits, referenciando a issue.
