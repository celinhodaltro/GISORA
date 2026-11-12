# 0004 - Separacao entre repositorio e read model

## Status

Aceito

## Contexto

A especificacao inicial sugere `IPropertyRepository` com "consultar imoveis". Se a listagem usar o repositorio, cada linha vira um carregamento de agregado completo. No cliente isso custa leitura e parse de documento inteiro; quando existir API, vira N+1 requisicoes HTTP.

## Decisao

Dois contratos distintos por agregado:

- `IPropertyRepository`: escrita e leitura por identidade.
- `IPropertyQueries`: projecao paginada para tela, com filtro e ordenacao.

O repositorio nunca expoe agregado para listagem; a query nunca devolve agregado.

## Alternativas consideradas

- Repositorio unico com metodo `Search`: simples, mas acopla caso de uso de leitura a agregado e impede otimizacao por projecao.
- OData/GraphQL desde o inicio: poder de consulta sem necessidade comprovada.
- CQRS com banco separado: overengineering na fase local-first.

## Consequencias

- Duas interfaces por agregado com listagem relevante.
- Listagem local le apenas indice; medida por teste com 1.000 imoveis e uma unica leitura.
- Na API, `IPropertyQueries` vira endpoint paginado e `IPropertyRepository` vira recurso por identidade.
- Projecao precisa ser atualizada quando um campo de tela mudar; o compilador aponta os pontos.
