# 0008 - Interface de cadastro unificada em mapa

## Status

Aceito

## Contexto

Quadras, imoveis, equipamentos e rurais possuem telas separadas, cada uma com formularios fora do mapa e fluxos de desenho proprios. O #119 trouxe os controles de desenho para dentro do mapa, mas a operacao de cadastro continua fragmentada: o usuario alterna entre painel e mapa, nao enxerga todas as feicoes ao mesmo tempo e nao consegue editar uma feicao existente sem sair do contexto cartografico.

Ao mesmo tempo, o mapa nao pode virar uma tela generica de edicao. Existem duas necessidades distintas:

1. **Visualizacao e exportacao** de camadas: consulta, legenda, atributos e GeoJSON.
2. **Cadastro e edicao** de feicoes: criar, selecionar, editar dados e geometria.

Restricoes atuais:

- Blazor WASM com `MainLayout` unico (header, navegacao lateral e `app-content` com padding e scroll).
- Persistencia local-first; nao existe backend para compensar operacoes parciais.
- `LayerControl`, `MapView` e `DrawingTool` sao componentes compartilhados e nao conhecem contexto de negocio.

## Decisao

### Rota e escopo

- Criar a rota unica `/cadastro/{modulo}`, com `modulo` em `quadras`, `imoveis`, `equipamentos` e `rurais`.
- `/cadastro` redireciona para `/cadastro/quadras`.
- O modulo ativo e refletido na rota, preservando deep link, historico do navegador e testes.
- `/map` permanece como ambiente de visualizacao e exportacao; nao se torna editor.
- As rotas antigas de listagem (`/urban-blocks`, `/properties`, `/facilities`, `/rural`) passam a redirecionar para o shell na fase de consolidacao. Rotas profundas de formulario e geometria continuam existindo ate o modal substitui-las.

### Shell

- `CadastroWorkspace` e o unico dono do estado de modulo, visibilidade de camadas, feicao selecionada, operacao atual (ocioso, criando, editando) e modal aberto.
- Modulos sao descritores (`CadastroModule`: identificador, rotulo, icone, camadas e projetor de resumo). Painel e modal de cada modulo sao componentes proprios selecionados por `switch`; nao criar um framework generico de formularios.
- A barra superior e composta apenas por icones dos modulos, com tooltip e `aria-label`.
- Todas as camadas sao carregadas ao abrir o shell e podem ser alternadas individualmente no `LayerControl`.

### Camada ativa

- `LayerControl` ganha o contrato de bloqueio (`LockedLayerIds`): enquanto um modulo esta ativo, as camadas desse modulo ficam visiveis e o checkbox desabilitado, com explicacao acessivel. O modulo ativo e o contexto de cadastro corrente, e a regra continua valendo durante criacao e edicao.
- Ao trocar de modulo, o estado de visibilidade anterior e restaurado.

### Criacao

- O `+` flutuante no canto inferior direito e o unico ponto de entrada de criacao no shell.
- Em Equipamentos, o `+` abre os tipos como icones e o tipo escolhido inicia o posicionamento do ponto.
- Em Quadras, Imoveis e Rurais, o `+` inicia diretamente o desenho do poligono.
- O lapis de inicio de desenho do #119 deixa de ser a entrada de criacao nesse shell. Durante a sessao, o controle do mapa continua exibindo concluir, desfazer e cancelar.
- O `+` fica desabilitado enquanto existir sessao de desenho ou edicao ativa.

### Rascunho e finalizacao

- A feicao criada existe como rascunho em memoria ate o salvamento no modal. Nada e persistido ao concluir o desenho ou posicionamento.
- Ao concluir, o rascunho e selecionado automaticamente e o popup de resumo abre.
- Cancelar o modal descarta o rascunho e restaura a selecao anterior.
- Edicao de feicao existente so persiste no salvamento do modal.

### Selecao, popup e modal

- Clique em ponto ou poligono seleciona a feicao e abre popup compacto com resumo e a acao "Abrir cadastro".
- "Abrir cadastro" abre o modal com todos os campos do modulo.
- Salvar atualiza mapa, camada e popup sem recarregar a pagina.
- Fechar o modal mantem a selecao e o popup.

### Edicao geometrica

- O lapis de edicao geometrica fica no popup da feicao selecionada e aparece somente com selecao ativa.
- Equipamentos permitem mover o ponto; Quadras, Imoveis e Rurais permitem mover vertices do poligono.
- A edicao reutiliza o fluxo assistido existente e as validacoes do #95.

### Tela cheia

- O shell ocupa toda a area de conteudo com um wrapper full-bleed que cancela o padding de `app-content`.
- Nao criar layout alternativo nem duplicar header e navegacao.

## Alternativas consideradas

- **Reutilizar `/map` como shell de cadastro**: rejeitado. Mistura visualizacao/exportacao com edicao, aumenta o risco de regressao e mantem dois publicos com necessidades diferentes na mesma tela.
- **Manter paginas separadas com paineis laterais**: rejeitado. Contraria o objetivo de um padrao unico e nao resolve a alternancia de camadas nem a selecao de feicoes.
- **Criar `CadastroLayout` separado**: rejeitado por duplicar header/navegacao e complicar o roteamento. O full-bleed por CSS resolve a tela cheia sem novo layout.
- **Persistir equipamento ou poligono imediatamente ao concluir**: rejeitado. Criaria registros incompletos no `localStorage` e exigiria compensacao em caso de cancelamento.
- **Lapis de edicao no `MapToolbar`**: rejeitado. O controle e global do mapa; o lapis deve refletir o contexto da feicao selecionada.

## Consequencias

- Nova rota e redirecionamentos; E2E existentes precisam ser atualizados na fase de consolidacao.
- `LayerControl` passa a ter contrato de bloqueio, com teste garantindo que camada bloqueada nao pode ser ocultada.
- O shell concentra estado e os modulos recebem dados e devolvem intencao por evento, mantendo as convencoes de componentizacao.
- O fluxo de cada modulo precisa distinguir explicitamente rascunho de feicao persistida.
- Popup, modal e edicao precisam coexistir com o desenho sem capturar cliques indevidos no mapa.
- `/map` continua existindo como visualizador; a navegacao deve deixar clara a diferenca entre visualizar e cadastrar.
- O `+` como unica entrada de criacao elimina a duplicidade com o lapis do #119, mas exige que o modo de criacao seja escolhido antes do desenho.

## Criterios para revisao desta decisao

- Entrada de backend/PostGIS (#105, #106) que mude a estrategia de rascunho para server-side.
- Inclusao de novos modulos no shell (cemiterio, zoneamento) que exija descritores mais ricos.
- Volume de feicoes que inviabilize carregar todas as camadas simultaneamente.
