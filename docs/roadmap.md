# Roadmap

Estado real do projeto. Cada linha aponta para a issue que entrega o item.

## Fase 1 - Fundacao arquitetural

Concluida.

- Solucao, convencoes de build e versionamento central de pacotes.
- Primitivas de DDD e `Result`.
- Kernel geografico (value objects e ports).
- Contextos Territory e Properties (dominio e casos de uso).
- Persistencia local com schema versionado e indices.
- Design system, shell web e composicao de dependencias.
- Testes arquiteturais, CI, deploy no GitHub Pages e correcao do fallback SPA.

## Fase 2 - Nucleo geografico

Concluida.

- Interop Turf/Proj4 com bundle vendorizado.
- Componentes Leaflet (`MapView`, `MapToolbar`, `LayerControl`, `FeatureSelector`, `FeatureInspector`, `CoordinateControl`).
- Pagina de mapa de imoveis com selecao.
- E2E de navegador cobrindo boot, persistencia, geospatial e mapa.

## Fase 3 - Cadastro territorial

Concluida.

- Logradouros (#33).
- Telas de territorio (#34).
- Formulario de imovel (#35).
- Desenho e edicao de geometria com validacao topologica e area calculada (#36).

## Fase 4 - Modulos municipais

Nao iniciada.

- Equipamentos publicos (#40).
- Cemiterios (#41).
- Rural e ambiental (#42).
- Zoneamento (#43).
- Licenciamento e fiscalizacao (#44).
- Imagens 360 (#45).

## Fase 5 - Mobile

Nao iniciada. Bloqueio de ambiente: Android SDK nao detectado e JDK fora do PATH.

- Host MAUI Blazor Hybrid, GPS e camera (#46).

## Fase 6 - Portal publico

Nao iniciada.

- Consulta publica e solicitacoes (#47).

## Fase 7 - Integracoes e relatorios

Nao iniciada. Depende de leiaute oficial verificado.

- Exportacao tributaria e SINTER/CIB (#48).
- Indicadores e exportacoes (#49).

## Fase 8 - Estabilizacao

Nao iniciada.

- Autenticacao, autorizacao e auditoria (#51).
- Documentacao continua (#50).
- Performance de listagem e leitura geografica por extensao.
- Matriz de rastreabilidade requisito -> issue -> teste.

## Dividas conhecidas

- Sem autenticacao real: modo demonstracao enquanto nao houver API (ADR 0007).
- Mapa carrega todos os imoveis com geometria; exige consulta por extensao no backend.
- Desenho suporta apenas poligono simples pela interface; buracos exigem importacao.
- `localStorage` limitado a cerca de 5 MB e sem transacao.
- Sem sincronizacao entre abas ou dispositivos.
- Dados oficiais de Passira ainda nao incorporados.
