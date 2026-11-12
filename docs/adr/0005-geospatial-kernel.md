# 0005 - Kernel geografico

## Status

Aceito

## Contexto

Territorio, imoveis, cemiterios, rural e planejamento compartilham coordenadas, geometrias, CRS, areas e distancias. Sem um kernel comum, cada contexto reinventa geometria com regras divergentes; com um kernel grande demais, ele vira deposito de regra de negocio de todos.

## Decisao

`Geospatial` contem apenas conceitos neutros:

- `SpatialReference`, `Coordinate`, `CoordinateSequence`;
- `Geometry` com `Point`, `LineString`, `Polygon`, `MultiPolygon`;
- `BoundingBox`, `Area`, `Distance`;
- ports `IGeometryValidator`, `ICoordinateTransformer`, `ISpatialOperations`.

Nenhum conceito de contexto especifico entra no kernel. Regra de lote fica em `Properties`; regra de jazigo fica em `Cemetery`.

Implementacao dos ports nesta fase: nenhuma. Turf.js/Proj4js entram na Fase 2; PostGIS entra com o backend.

## Alternativas consideradas

- Usar NetTopologySuite no dominio: poderosa, mas amarra o modelo a uma biblioteca e a um modelo de geometria que nao controlamos.
- Guardar apenas GeoJSON em texto: perde validacao estrutural e tipo.
- Deixar geometria como `string` no dominio: primitiva sem significado, exatamente o que objetos de valor evitam.

## Consequencias

- Geometria no dominio e imutavel e sempre carrega o CRS.
- Validacao estrutural e garantida no construtor; topologia robusta depende de port.
- Persistencia converte entre modelo de dominio e documento explicito; nenhum atributo de serializacao no dominio.
- Kernel pequeno o suficiente para ser extraido se necessario.
