# 0001 - DDD com bounded contexts

## Status

Aceito

## Contexto

O produto cobre cadastro territorial, imobiliario, cemiterios, rural, planejamento, licenciamento, fiscalizacao e integracao tributaria. Unificar tudo num modelo unico produz um "modelo de tudo" com regras conflitantes: a "unidade" de um imovel nao e a "unidade" de um jazigo, e a "quadra" de cemiterio nao obedece ao parcelamento urbano.

## Decisao

Organizar a solucao em bounded contexts, cada um com `Domain` proprio, agregados proprios e contratos proprios. `Geospatial` e kernel compartilhado; `Identity`, `Reports` e `Integration` sao contextos de suporte. Comunicacao entre contextos ocorre por identificador e contrato publico.

## Alternativas consideradas

- Modelo unico com pastas tecnicas (`Entities`, `Services`, `Repositories`): descartado por produzir entidades anemicas e regras duplicadas.
- Microsservicos desde o inicio: descartado por custo operacional sem beneficio na fase local-first.
- Um contexto por tela: descartado por fragmentar invariantes.

## Consequencias

- Mais projetos e contratos explicitos.
- Extração futura de um contexto para servico proprio sem reescrita.
- Regra de negocio tem um lugar obvio; revisao consegue apontar onde ela pertence.
- Testes arquiteturais passam a ser obrigatorios para manter a fronteira.
