# Guia da POC

## O que e

Aplicacao Blazor WebAssembly, local-first, para cadastro territorial municipal. Roda sem backend de dados: a persistencia e o `localStorage` do navegador. Publicada no GitHub Pages.

## Requisitos

- .NET SDK 10.0.4xx (`global.json`).
- PowerShell para o script do Playwright.
- Navegador Chromium para os testes E2E.

## Execucao local

```bash
dotnet restore GISORA.slnx --locked-mode
dotnet run --project src/presentations/GISORA.Presentation
```

A aplicacao abre no endereco informado pelo `launchSettings.json`.

## Build, testes e formatacao

```bash
dotnet build GISORA.slnx -c Release
dotnet format GISORA.slnx --verify-no-changes
dotnet test tests/Unit/GISORA.UnitTests -c Release
dotnet test tests/Architecture/GISORA.ArchitectureTests -c Release
pwsh tests/E2E/GISORA.Web.E2ETests/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test tests/E2E/GISORA.Web.E2ETests -c Release
```

O CI executa exatamente esse conjunto em pull requests e na `main`.

## Publicacao no GitHub Pages

O workflow `.github/workflows/deploy-web.yml` publica a cada push na `main`.

- Publica `src/presentations/GISORA.Presentation`.
- Ajusta o `<base href>` para `/GISORA/`.
- Copia `index.html` para `404.html` (fallback de rotas do Blazor).
- Envia o artefato de `wwwroot` para o Pages.

Nao ha servidor de tiles, API ou banco: o Pages serve apenas arquivos estaticos.

## Checklist pos-deploy

1. Abrir https://celinhodaltro.github.io/GISORA/ e confirmar carregamento sem erro no console.
2. Navegar para cada rota direta (`/properties`, `/map`, `/territory`, `/cemetery`, `/rural`, `/planning`, `/cib`, `/data`) e recarregar a pagina.
3. Confirmar que CSS, imagens da marca, Leaflet e `map.js` carregam (sem 404).
4. Carregar a base demonstrativa e conferir os indicadores.
5. Desenhar uma geometria e confirmar area e persistencia apos recarregar.
6. Exportar um backup em `/data`, limpar o `localStorage`, restaurar e conferir os dados.
7. Verificar se o rodape exibe atribuicao do mapa e se os controles respondem no mobile.

## Roteiro de demonstracao

1. **Visao geral**: carregar dados de demonstracao e conferir indicadores.
2. **Territorio**: criar municipio/distrito/bairro/setor.
3. **Imoveis**: abrir geometria, desenhar lote, salvar e ver no mapa.
4. **Mapa**: alternar camadas, selecionar feicao e ver atributos.
5. **Equipamentos**: criar equipamento com ponto e simbologia por tipo.
6. **Cemiterio**: criar cemiterio, setor, jazigo e sepultamento.
7. **Rural**: criar/importar propriedade e APP.
8. **Zoneamento**: criar zona, desenhar poligono e consultar incidencia.
9. **Consulta CIB**: usar os codigos demo (`DEMO1A2B`, `DEMO3C4D`, `DEMO5E6F`).
10. **Dados e backup**: exportar e restaurar.

## Fluxo de contribuicao

1. Toda mudanca nasce de uma issue.
2. Uma branch por issue, por exemplo `feature/94-desenho-geometrias`.
3. PR referenciando a issue com `Closes #NUMERO`.
4. Build, format e testes verdes antes do merge.
5. Validacao no navegador real para mudancas de UI ou mapa.
6. Nao misturar funcionalidades independentes no mesmo PR.

## Limitacoes da POC

- Sem autenticacao/permissoes reais; modo demonstrativo (ADR 0007).
- Sem API, banco, sincronizacao entre dispositivos ou multiusuario.
- `localStorage` nao e seguro, e local ao navegador e limitado (normalmente poucos MB).
- Sem binarios grandes: panoramas 360 nao usam `localStorage`.
- Dados demonstrativos nao sao dados oficiais.
- Mapa-base e tiles dependem de servico externo; o Pages nao hospeda tiles.
- Integracao SINTER/CIB roda em modo demo; producao exige backend e credenciais.

## Host MAUI (roadmap)

O host `GISORA.Presentation.Hybrid` compartilha paginas e componentes com o Web. Nao faz parte do CI principal.

```bash
dotnet workload install maui-android maui-windows
dotnet build src/presentations/GISORA.Presentation.Hybrid/GISORA.Presentation.Hybrid.csproj -f net10.0-windows10.0.19041.0
```
