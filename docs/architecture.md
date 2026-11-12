# Arquitetura

## Visao

O GISORA e organizado por bounded contexts. Cada contexto tem `Domain`, `Application` (quando ha caso de uso) e, quando existe implementacao, `Infrastructure`. Os hosts compoem a aplicacao.

```
Presentation (GISORA.Web, futuro GISORA.Mobile)
        |
        v
Application (casos de uso, contratos de leitura)
        |
        v
Domain (agregados, objetos de valor, eventos, contratos de repositorio)
        ^
        |
Infrastructure (implementa contratos de repositorio e servicos)
```

A seta de Infrastructure aponta para dentro: ela implementa interfaces definidas por Domain/Application. Nenhuma camada interna referencia implementacao concreta.

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

O componente nunca conhece repositorio, agregado ou armazenamento. O handler orquestra: carrega o agregado, chama o comportamento de dominio, persiste e traduz `DomainException` em `Error`.

## Repositorio e read model

Dois contratos por agregado, por decisao deliberada:

- `IPropertyRepository`: escrita e leitura por identidade, com o agregado completo.
- `IPropertyQueries`: projecao para telas, com filtro e paginacao.

Motivo: um unico contrato de repositorio para listagem produz N+1 quando existir API. Com read model separado, a implementacao local le um indice e a futura implementacao HTTP chama um endpoint paginado.

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
3. Troca na composicao de dependencias do host.
4. Geometrias passam a trafegar GeoJSON/WKT e viram `geometry(Geometry, SRID)` no PostGIS.
5. Consulta espacial deixa de ser calculada no cliente.

Domain, Application e os contratos publicos dos modulos permanecem.

## Composicao

`GISORA.Web/Program.cs` e o unico lugar que registra implementacoes concretas. Testes arquiteturais garantem que nenhum outro tipo do host depende de Infrastructure.

## O que ainda nao existe

- API ASP.NET Core, EF Core, PostGIS.
- Autenticacao real: sem backend nao ha identidade confiavel; o modelo sera substituido.
- Sincronizacao entre abas e entre dispositivos.
- Motor geografico (Fase 2).
