# Acessibilidade

A POC segue praticas de acessibilidade compativeis com WCAG 2.1 AA nos fluxos principais, sem declarar conformidade formal.

## Medidas implementadas

- Link "Ir para o conteudo principal" no inicio da pagina, com foco programatico no `<main>`.
- Foco visivel global (`:focus-visible`) em links, botoes, campos e controles do mapa.
- Navegacao por teclado no menu, formularios, camadas e controles do mapa.
- Menu lateral em mobile com `aria-expanded`, `aria-controls` e remocao do fluxo de foco quando fechado (`visibility: hidden`).
- Backdrop do menu como botao com nome acessivel.
- Rotulos `aria-label` em botoes de icone e controles do mapa.
- Mapa com nome acessivel; Leaflet mantem navegacao por teclado.
- Imagens decorativas com `alt` vazio e `aria-hidden`.
- Estados de carregamento, erro e vazio com `role="status"`/`role="alert"`.
- Respeito a `prefers-reduced-motion`.
- Layout responsivo em 360, 768 e 1280 px (menu off-canvas, grids em coluna, cabecalho compacto).

## Verificacao automatizada

`tests/E2E/GISORA.Web.E2ETests/AccessibilityTests.cs` cobre:

- link de salto e foco no conteudo;
- ausencia de controles interativos sem nome acessivel nas paginas principais;
- abertura/fechamento do menu em viewport movel.

## Limitacoes

- Nao ha auditoria formal com leitor de tela nem laudo WCAG.
- Testes automatizados nao substituem validacao manual com teclado, zoom 200% e leitores de tela.
- Contraste deve ser revalidado a cada mudanca de paleta.
