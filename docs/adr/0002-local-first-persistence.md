# 0002 - Persistencia local-first

## Status

Aceito

## Contexto

A primeira versao precisa operar sem backend proprio, com dados no dispositivo, e migrar depois para API ASP.NET Core e PostgreSQL/PostGIS sem reconstruir dominio e casos de uso. O armazenamento disponivel no navegador e `localStorage`: sincrono, com quota proxima de 5 MB, sem transacao, sem indice e sem isolamento entre abas.

## Decisao

- Toda persistencia passa por `ILocalStore` em `GISORA.Infrastructure.Persistence`.
- Dominio e Application nao conhecem `localStorage`, JSON, JavaScript ou navegador.
- Um documento por agregado, com filhos embutidos.
- Indice denormalizado por colecao para listagem, mantido junto do documento.
- Versionamento de schema em `gisora:meta:schema`, com migracoes ordenadas e idempotentes.
- Binarios (raster, panorama 360, GeoPackage) ficam fora do `localStorage`, com metadados separados.
- Nenhuma consulta espacial complexa e implementada sobre `localStorage`.

## Alternativas consideradas

- IndexedDB desde o inicio: melhor capacidade e assincronia, mas mais complexo e ainda sem consulta espacial. Fica como evolucao de `Infrastructure.Files`.
- EF Core com provider local (SQLite/WASM): adiciona dependencia pesada e nao resolve consulta espacial.
- Estado apenas em memoria: descartado por perder dado ao recarregar.

## Consequencias

- Leitura de listagem e O(n) sobre o indice, nao sobre documentos; aceitavel em base demonstrativa.
- Escrita de documento e indice ocorre em duas operacoes; `RebuildIndexAsync` corrige inconsistencia.
- Sem concorrencia entre abas: duas abas podem sobrescrever a mesma chave. Aceito e documentado ate a fase de sincronizacao/backend.
- `localStorage` nao e armazenamento seguro: nenhum segredo e nenhuma promessa de seguranca para dado sensivel.
- Migracao para API troca apenas as implementacoes de Infrastructure.
