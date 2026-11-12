# 0003 - Fronteiras de agregado

## Status

Aceito

## Contexto

Cadastro imobiliario tem relacoes que precisam ser consistentes ao mesmo tempo: area do lote, area construida, soma das titularidades e historico de alteracao. Se edificacoes, unidades e titularidade forem agregados independentes, toda operacao exige transacao distribuida, que nao existe em `localStorage` e e cara tambem em API.

## Decisao

- `Property` e raiz de agregado e contem `Building`, `Unit`, `Ownership` e `CadastralHistoryEntry`.
- A soma das titularidades e validada no fechamento (`EnsureOwnershipBalanced`), nao em cada insercao; `AddOwnership` impede passar de 100%.
- `BuiltArea` soma apenas edificacoes; unidades sao subdivisoes e nao entram, para nao contar area duas vezes.
- `UrbanBlock` contem `BlockFace`: a unicidade de logradouro por quarteirao precisa ser atomica.
- `Municipality`, `District`, `Neighborhood`, `FiscalSector` sao agregados independentes, ligados por identificador.

## Alternativas consideradas

- `Building` e `Unit` como agregados proprios desde o inicio: descartado por exigir transacao entre agregados.
- `UrbanPlanning` no agregado de `Property`: descartado; zoneamento muda por lei, nao por imovel.
- Um agregado unico de territorio: descartado por tamanho e contenção artificial.

## Consequencias

- `Property` e `UrbanBlock` ficam grandes; carregar o agregado inteiro na escrita e aceitavel nessa escala.
- Se edificacao passar a ter ciclo de vida proprio (alvara, vistoria, laudo), separa-se em agregado com identificador e evento.
- Historico dentro do agregado serve para rastreabilidade local; versionamento completo com geometria vira responsabilidade do backend.
