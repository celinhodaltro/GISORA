# 0006 - Radzen como design system

## Status

Aceito

## Contexto

O produto precisa de componentes de dados (grid com paginacao e filtro), formularios, dialogs e notificacoes em dois hosts (web e mobile hibrido). A especificacao exige .NET 10 e componentes compartilhados.

## Decisao

Usar `Radzen.Blazor` como base de componentes, dentro de `GISORA.Presentation.Shared.DesignSystem`, com tokens proprios em CSS e componentes de composicao (`PageHeader`, `StatCard`, `EmptyState`).

Versao verificada nesta maquina: `Radzen.Blazor 13.0.0`, com alvo `lib/net10.0`, compilando e publicando no host WebAssembly.

## Alternativas consideradas

- MudBlazor: boa base, mas o template disponivel na maquina nao fixa versao e a escolha do produto foi Radzen.
- Componentes proprios do zero: custo alto para grid com paginacao e acessibilidade.
- Bootstrap puro: sem componentes de dados; exigiria reimplementar grid.

## Consequencias

- Dois arquivos de Radzen referenciados no host (`material-base.css` e `Radzen.Blazor.min.js`) e um registro de servicos (`AddRadzenComponents`).
- Componentes de negocio consomem Radzen sem expor Radzen na API do design system, exceto onde o componente e de dados (grid da pagina de imoveis).
- Tema trocavel sem reescrever componentes de negocio.
- O CSS do Radzen nao e duplicado no host: um unico link no `index.html`.
