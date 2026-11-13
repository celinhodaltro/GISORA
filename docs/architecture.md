# Arquitetura

## Visao

O GISORA usa tres projetos de producao, organizados por camada e, dentro de cada camada, por modulo funcional. O host compoe a aplicacao.

```
src/presentations/       (Shared, Presentation e Hybrid)
        |
        v
src/core/GISORA.Application    (casos de uso, consultas, contratos)
        |
        v
src/core/GISORA.Domain         (agregados, value objects, eventos, contratos de repositorio)
        ^
        |
src/infrastructure/GISORA.Infrastructure (persistencia, geoespacial, integracoes)
```

A dependencia aponta para dentro: Infrastructure implementa interfaces definidas por Domain/Application. O host referencia os tres projetos apenas para composicao.

## Estrutura de pastas

```text
src/
  GISORA.Domain/
    Common/             primitivas de DDD (Entity, AggregateRoot, ValueObject, Guard, DomainException)
    Geospatial/         kernel geografico
    Territory/          municipio, distrito, bairro, setor fiscal, quadra
    Properties/         imoveis, edificacoes, unidades, titularidade
    Roads/              logradouros
    PublicFacilities/   equipamentos publicos
    Cemetery/           cemiterios, jazigos, sepultamentos
    Rural/              propriedades rurais, CAR, SIGEF, APP
    UrbanPlanning/      zonas e restricoes
  GISORA.Application/
    Common/             Result, Error, PagedResult
    Geospatial/         ports de geometria, CRS e operacoes espaciais
    Territory/ ...      Commands, Queries, Handlers e contratos de cada contexto
    Integration/        contratos tipados do SINTER/CIB
  GISORA.Infrastructure/
    Persistence/        ILocalStore, documentos, indices, repositorios e consultas
    Geospatial/         Turf.js, Proj4js, GeoJSON
    Integrations/       IBGE e SINTER/CIB
  presentations/
    GISORA.Presentation.Shared/   Pages/ Layout/ Components/ Maps/ DesignSystem/ wwwroot/
    GISORA.Presentation/          Blazor WASM host
    GISORA.Presentation.Hybrid/   MAUI Blazor Hybrid host
tests/
  Unit/GISORA.UnitTests
  Architecture/GISORA.ArchitectureTests
  E2E/GISORA.Web.E2ETests
```

Cada modulo mantem seu proprio namespace (`GISORA.Domain.<Modulo>` e `GISORA.Application.<Modulo>`), o que preserva a fronteira logica mesmo compartilhando assembly.

## Direcao de dependencias

| De | Para |
| --- | --- |
| Domain | apenas BCL |
| Application | Domain |
| Infrastructure | Domain, Application e bibliotecas tecnicas |
| Web | Domain, Application, Infrastructure (composicao) |

Garantido por `tests/Architecture` (ver `dependency-rules.md`).

## Fluxo de uma operacao

Leitura:

```
Componente Razor -> IPropertyQueries -> LocalStoragePropertyQueries -> ILocalStore -> localStorage
```

Escrita:

```
Componente Razor -> Handler (Application) -> Property (Domain) -> IPropertyRepository -> LocalStoragePropertyRepository -> ILocalStore
```

Mapa:

```
IGeoJsonSerializer (port) -> GeoJsonSerializer -> GeoJsonMapper -> GeoJSON
Componente MapView -> js/map.js -> Leaflet
```

O componente nunca conhece repositorio, agregado ou armazenamento. O handler orquestra: carrega o agregado, chama o comportamento de dominio, persiste e traduz `DomainException` em `Error`.

## Repositorio e read model

Dois contratos por agregado:

- `IPropertyRepository`: escrita e leitura por identidade, com o agregado completo.
- `IPropertyQueries`: projecao para telas, com filtro e paginacao.
- `IPropertyMapQueries`: projecao com geometria, separada da listagem para nao pagar parse de geometria em toda tela.

## Nucleo geografico

- `GISORA.Domain.Geospatial`: `SpatialReference`, `Coordinate`, `Geometry` e derivados, `BoundingBox`, `Area`, `Distance`.
- `GISORA.Application.Geospatial`: `IGeometryValidator`, `ICoordinateTransformer`, `ISpatialOperations`, `IGeoJsonSerializer`.
- `GISORA.Infrastructure.Geospatial`: implementacao via Turf.js 7.4 e Proj4js 2.2, vendorizados e carregados por modulo ES.
- `GISORA.Presentation.Shared.Maps`: componentes Leaflet e `js/map.js`; nao conhece contexto de negocio.

O basemap padrao e OpenStreetMap, configuravel por parametro. Producao municipal deve apontar para servico oficial de tiles.

## Persistencia atual

`localStorage` atras de `ILocalStore`:

- chave `gisora:{namespace}:{key}`;
- um documento por agregado, com filhos embutidos;
- indice denormalizado por colecao para listagem;
- versionamento de schema em `gisora:meta:schema`;
- binarios (raster, panorama, GeoPackage) nao entram aqui.

## Caminho de migracao

1. `LocalStoragePropertyRepository` -> `ApiPropertyRepository` (mesmo contrato, HTTP).
2. `LocalStoragePropertyQueries` -> `ApiPropertyQueries` (endpoint paginado).
3. Consulta espacial deixa de ser calculada no cliente e passa ao PostGIS.
4. Autorizacao e auditoria passam a ser aplicadas no servidor (ADR 0007).
5. Troca na composicao de dependencias do host.

Domain, Application e os contratos publicos dos modulos permanecem.

## Paginas existentes

| Rota | Funcao |
| --- | --- |
| `/` | Visao geral com contadores reais |
| `/properties` | Listagem de imoveis com busca e paginacao |
| `/properties/new` | Cadastro de imovel |
| `/map` | Imoveis com geometria e inspector |
| `/streets` | Logradouros |
| `/territory` | Municipio, distrito, bairro e setor |
| `/facilities` | Equipamentos publicos com simbologia por tipo |
| `/cemetery` | Cemiterios, jazigos e sepultamentos |
| `/rural` | Propriedades rurais, CAR/SIGEF e APP |
| `/planning` | Zoneamento e restricoes incidentes |
| `/cib` | Consulta CIB no SINTER (demo/producao) |

## O que ainda nao existe

- API ASP.NET Core, EF Core e PostGIS.
- Autenticacao real: modo demonstracao ate a issue #51 (ver ADR 0007).
- Host mobile, portal publico e exportacao tributaria.
- Sincronizacao entre abas e entre dispositivos.
