# Arquitetura

## Visao

O GISORA e organizado por bounded contexts. Cada contexto tem `Domain`, `Application` (quando ha caso de uso) e, quando existe implementacao, `Infrastructure`. Os hosts compoem a aplicacao.

```
Presentation (GISORA.Web)
        |
        v
Application (casos de uso, read models)
        |
        v
Domain (agregados, objetos de valor, eventos, contratos de repositorio)
        ^
        |
Infrastructure (LocalStorage, Geospatial, adaptadores)
```

A seta de Infrastructure aponta para dentro: ela implementa interfaces definidas por Domain/Application.

## Direcao de dependencias

| De | Para |
| --- | --- |
| Domain | BuildingBlocks.Domain, kernel Geospatial.Domain |
| Application | Domain do proprio modulo, BuildingBlocks.Application, Geospatial |
| Infrastructure | Domain/Application do modulo, BuildingBlocks, outros adaptadores tecnicos |
| Host | tudo, apenas para compor |

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
Componente MapView -> map.js -> Leaflet
```

O componente nunca conhece repositorio, agregado ou armazenamento. O handler orquestra: carrega o agregado, chama o comportamento de dominio, persiste e traduz `DomainException` em `Error`.

## Repositorio e read model

Dois contratos por agregado, por decisao deliberada:

- `IPropertyRepository`: escrita e leitura por identidade, com o agregado completo.
- `IPropertyQueries`: projecao para telas, com filtro e paginacao.
- `IPropertyMapQueries`: projecao com geometria, separada da listagem para nao pagar parse de geometria em toda tela.

## Núcleo geografico

- `GISORA.Geospatial.Domain`: `SpatialReference`, `Coordinate`, `Geometry` e derivados, `BoundingBox`, `Area`, `Distance`.
- `GISORA.Geospatial.Application`: `IGeometryValidator`, `ICoordinateTransformer`, `ISpatialOperations`, `IGeoJsonSerializer`.
- `GISORA.Infrastructure.Geospatial`: implementacao via Turf.js 7.4 e Proj4js 2.2, vendorizados e carregados por modulo ES.
- `GISORA.Shared.Maps`: componentes Leaflet e `map.js`; nao conhece contexto de negocio.

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

## O que ainda nao existe

- API ASP.NET Core, EF Core e PostGIS.
- Autenticacao real: modo demonstracao ate a issue #51 (ver ADR 0007).
- Desenho e edicao de geometria no mapa (#36).
- Modulos municipais das fases 4 a 7.
- Host mobile, portal publico e integracoes.
- Sincronizacao entre abas e entre dispositivos.
