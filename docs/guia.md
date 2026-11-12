# Guia da aplicacao

## O que e

Aplicacao web Blazor WebAssembly, local-first, para cadastro territorial. Roda sem backend: os dados ficam no `localStorage` do navegador.

## Como abrir

- Publicado em https://celinhodaltro.github.io/GISORA/
- Local: `dotnet run --project src/Hosts/GISORA.Web`

## Roteiro de demonstracao (3 minutos)

1. Abrir a visao geral e clicar em **Carregar dados de demonstracao**.
2. Conferir os indicadores: 3 imoveis, 2 logradouros, 2 equipamentos.
3. **Imoveis**: listar, buscar por inscricao e abrir **Geometria** de um imovel.
4. Na geometria: **Desenhar**, clicar 3 ou 4 pontos, **Concluir desenho**, ver a area calculada e **Salvar**.
5. **Mapa**: os imoveis aparecem como poligonos; clicar em um para ver os atributos.
6. **Equipamentos**: criar um equipamento marcando o ponto no mapa.
7. **Territorio**: criar municipio, distrito, bairro e setor.
8. **Logradouros**: cadastrar uma rua.

## O que funciona de ponta a ponta

- Cadastro de municipio, distrito, bairro e setor fiscal.
- Cadastro de logradouros com tipo e bairro.
- Cadastro de imovel com inscricao, endereco, area e titularidade no dominio.
- Desenho e edicao de geometria do lote, com validacao topologica (Turf.js) e area calculada.
- Mapa com camadas, selecao de feicao, inspector e coordenada do cursor.
- Equipamentos publicos com localizacao por ponto.
- Persistencia local com versionamento de schema e indices.
- Base demonstrativa em um clique, marcada como demonstrativa.
- CI com testes unitarios, arquiteturais e E2E em navegador real; deploy automatico.

## O que nao funciona / nao existe

- **Autenticacao e permissoes reais**: modo demonstracao, sem protecao de dado (ADR 0007).
- **API e PostGIS**: toda consulta roda no cliente; nao ha sincronizacao entre dispositivos.
- **Cemiterios, rural, zoneamento, licenciamento, imagens 360**: nao implementados (fases 4+).
- **App mobile e portal publico**: nao implementados.
- **Integracao SINTER/CIB**: bloqueada por leiaute oficial.
- **Dado oficial de Passira**: nao incorporado; a base demonstrativa nao e base real.
- **Limite de armazenamento**: cerca de 5 MB, sem binarios.
- **Edicao/exclusao** de imovel, equipamento, logradouro e territorio.

## Limites de uso

Esta aplicacao nao deve receber dado pessoal real. O armazenamento local nao e seguro e nao ha controle de acesso.

## Como validar

```bash
dotnet restore --locked-mode
dotnet build -c Release
dotnet test tests/Unit -c Release
dotnet test tests/Architecture -c Release
pwsh tests/E2E/GISORA.Web.E2ETests/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test tests/E2E/GISORA.Web.E2ETests -c Release
```
