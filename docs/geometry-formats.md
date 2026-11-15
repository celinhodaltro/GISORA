# Formatos geograficos: suporte atual e avaliacao

## Escopo

O GISORA e uma aplicacao Blazor WebAssembly, local-first, sem backend de dados. Toda leitura e escrita de arquivo acontece no navegador. Este documento registra o que esta implementado e a avaliacao dos formatos KML, GeoPackage e Shapefile, sem prometer suporte que nao existe.

## Implementado: GeoJSON

- Importacao de arquivos `GeoJSON` (`FeatureCollection`, `Feature` ou geometria isolada) para atualizar a geometria de imoveis existentes.
- Exportacao de um imovel, de uma camada do mapa ou de um conjunto filtrado de imoveis.
- CRS de origem e de saida explicitamente selecionados: `EPSG:4326` e `EPSG:31978` a `EPSG:31985` (SIRGAS 2000 / UTM zonas 18S a 25S).
- Transformacao de coordenadas via `ICoordinateTransformer` (Proj4js), executada apenas quando o CRS de origem difere do destino.
- Metadados de origem, CRS de origem, CRS de destino, data de exportacao e quantidade de feicoes gravados no membro `gisora` do `FeatureCollection`.
- Cada feicao carrega `id`, `properties` (tipo, inscricao e areas quando disponiveis) e `geometry`.
- Limites: 10 MB e 5000 feicoes por arquivo. Arquivo acima do limite, JSON invalido, tipo nao suportado ou CRS desconhecido falham com mensagem explicita.

Contrato de importacao:

- A feicao e associada ao imovel pelo `id` (GUID do imovel) ou, na falta dele, pela propriedade `inscricao`.
- Feicoes sem imovel correspondente sao informadas e descartadas; nenhum imovel novo e criado e nenhum dado e inventado.
- Geometrias `Polygon` e `MultiPolygon` sao persistidas no cadastro do imovel. `MultiPolygon` representa partes separadas do mesmo imovel (por exemplo, edificacao principal e anexo). `Point` e `LineString` continuam disponiveis para desenho e visualizacao, mas nao substituem a geometria cadastral do lote.
- A geometria importada passa pela mesma validacao do desenho assistido: anel fechado, minimo de vertices, auto-intersecao e sobreposicao com outros imoveis.
- Cada importacao bem-sucedida grava um registro com origem, CRS de origem, CRS de destino, data e quantidade de feicoes.

Formato de exportacao:

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "id": "018f4a4e-....",
      "properties": { "tipo": "imovel", "inscricao": "001.002.003", "areaLoteM2": "500", "crs": "EPSG:4326" },
      "geometry": { "type": "Polygon", "coordinates": [[[...]]] }
    }
  ],
  "gisora": {
    "origin": "imovel",
    "sourceCrs": "EPSG:4326",
    "targetCrs": "EPSG:4326",
    "exportedAt": "2026-11-14T12:00:00.0000000+00:00",
    "featureCount": "1"
  }
}
```

O arquivo gerado e reimportavel pela propria tela de dados. `gisora.sourceCrs` e lido como sugestao quando o usuario escolhe deteccao automatica; o CRS selecionado na tela sempre prevalece.

## Avaliacao: KML

- **Natureza**: XML da OGC, usado por Google Earth. Suporta `Placemark`, `Point`, `LineString`, `Polygon`, estilos e pastas.
- **Leitura no navegador**: viavel. O navegador ja oferece `DOMParser`; a conversao para GeoJSON pode usar `@tmcw/togeojson` (MIT, pequeno, sem dependencias nativas). E aceitavel para importar geometrias simples.
- **Escrita no navegador**: viavel. `tokml` (MIT) converte GeoJSON em KML, ou a geracao pode ser feita a mao para os tres tipos suportados.
- **Limites**: KML e definido em WGS 84 (`EPSG:4326`), sem CRS alternativo; estilos, `NetworkLink`, imagens e GroundOverlay ficam fora do escopo; atributos sao textos sem tipagem; nao ha round-trip fiel das propriedades.
- **Risco**: XML exige cuidado com entidades e tamanho; o `DOMParser` do navegador nao resolve entidades externas, mas o arquivo ainda precisa de limite de tamanho.
- **Decisao**: nao implementado nesta POC. Candidato natural para uma segunda etapa por custo baixo de leitura.

## Avaliacao: GeoPackage

- **Natureza**: contêiner SQLite da OGC com tabelas, `gpkg_spatial_ref_sys`, geometrias WKB e indices espaciais opcionais.
- **Leitura no navegador**: tecnicamente viavel com SQLite compilado para WebAssembly (`sql.js`, `wa-sqlite` ou `@sqlite.org/sqlite-wasm`), carregando o arquivo em um sistema de arquivos virtual.
- **Escrita no navegador**: possivel, mas exige criar esquema OGC, inserir WKB e manter metadados de CRS; o custo e alto para o ganho na POC.
- **Limites**: dependencia binaria grande (da ordem de megabytes), inicializacao assincrona, copia do arquivo para memoria, ausencia de suporte direto a R*Tree sem extensao, dificuldade de depuracao no navegador e variacao entre navegadores.
- **Risco**: o esforco de implementacao e manutencao supera o de GeoJSON e KML somados.
- **Decisao**: nao implementado. Reavaliar apenas se houver requisito de intercambio com bases oficiais em GeoPackage.

## Avaliacao: Shapefile

- **Natureza**: conjunto de arquivos binarios (`.shp`, `.shx`, `.dbf`, `.prj`, opcional `.cpg`). Nao e um arquivo unico.
- **Leitura no navegador**: viavel com `shpjs`/`shapefile-js` (MIT) e `proj4` para o `.prj`. Aceita geometrias e atributos do DBF.
- **Escrita no navegador**: exige gerar `.shp`, `.shx`, `.dbf`, `.prj` e empacotar em ZIP; nao ha caminho simples e o resultado precisa ser validado em ferramenta externa.
- **Limites**: codificacao do DBF (`.cpg`) e causa comum de acentos corrompidos; nomes de campo limitados a 10 caracteres; um unico tipo de geometria por arquivo; ordem de aneis e multipartes exigem cuidado; sem metadados ricos de CRS alem do `.prj`.
- **Risco**: alto para escrita, medio para leitura, com armadilhas de codificacao e de geometria que nao aparecem nos testes automatizados.
- **Decisao**: nao implementado. Se necessario, limitar a importacao de leitura e deixar a exportacao para uma ferramenta desktop.

## Comparativo

| Formato | Leitura no navegador | Escrita no navegador | CRS | Risco | Situacao |
| --- | --- | --- | --- | --- | --- |
| GeoJSON | Sim, nativa com `System.Text.Json` | Sim | Explicito no arquivo e nos metadados | Baixo | Implementado |
| KML | Sim (`DOMParser` + `togeojson`) | Sim (`tokml`) | Fixo em WGS 84 | Baixo | Avaliado |
| GeoPackage | Sim com SQLite WASM | Sim, com custo alto | Tabela `gpkg_spatial_ref_sys` | Alto | Avaliado |
| Shapefile | Sim (`shpjs` + `proj4`) | Complexo (multiarquivo + ZIP) | `.prj` + `proj4` | Medio a alto | Avaliado |

## Regras de seguranca e qualidade

- Nenhum arquivo e enviado a servidor; leitura e escrita ocorrem no navegador.
- Limite de 10 MB e 5000 feicoes por importacao, verificado antes do parse.
- Arquivo invalido nao altera dados existentes: a importacao e aplicada feicao a feicao e cada uma passa por validacao.
- Dado importado preserva a origem, o CRS de origem, o CRS de destino e a data no registro de importacao.
- Nenhuma transformacao silenciosa: o CRS de origem e o de destino aparecem na tela e no arquivo exportado.
