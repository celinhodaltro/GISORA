# Fontes de dados e qualidade geografica

## Regras gerais

- Dado oficial tem precedencia sobre dado derivado ou sintetico.
- Todo conjunto tem metadados: origem, responsavel, data, CRS, escala, precisao e situacao de validacao.
- Dado sintetico e sempre identificado como demonstrativo, no nome da camada e nos metadados.
- Precisao cadastral nunca e inventada: se a fonte nao declara precisao, o campo fica nulo e a limitacao e registrada.
- Dados de origens diferentes nao sao tratados como equivalentes nem misturados sem registro.
- As coordenadas originais sao preservadas; transformacao de CRS e explicita e produz nova geometria.

## Municipio de referencia

Passira, Pernambuco. Codigo IBGE `2610509`.

| Camada | Fonte esperada | Situacao |
| --- | --- | --- |
| Limite municipal | IBGE / malha municipal oficial | A obter |
| Distritos e localidades | IBGE / base municipal | A obter |
| Bairros e setores fiscais | Base municipal | A obter |
| Quadras e faces | Cadastro municipal | A obter |
| Lotes e edificacoes | Cadastro imobiliario municipal | A obter |
| Logradouros | Base municipal | A obter |
| Equipamentos publicos | Base municipal | A obter |
| Cemiterios | Base municipal | A obter |
| Zoneamento | Plano diretor municipal | A obter |
| Rural (CAR/SIGEF/SNCI) | SICAR / INCRA | A obter |

Enquanto nao houver dado oficial, a aplicacao opera com base vazia. Nenhum dado ficticio e exibido como se fosse real.

## CRS

- Armazenamento e visualizacao padrao: `EPSG:4326` (WGS 84).
- Cadastro tecnico municipal tende a usar projecao local (SIRGAS 2000 / UTM zona 25S, `EPSG:31985`). A escolha da projecao oficial sera registrada em ADR quando a base oficial chegar.
- CRS suportados na selecao de entrada, trabalho e saida: `EPSG:4326` e `EPSG:31978` a `EPSG:31985` (SIRGAS 2000 / UTM zonas 18S a 25S).
- Transformacao sempre explicita via `ICoordinateTransformer` (Proj4js); CRS desconhecido falha com mensagem e nao ha conversao silenciosa.
- O CRS de origem fica no objeto de geometria. Importacoes e exportacoes registram origem, CRS de origem, CRS de destino e data.
- Detalhes de formato, limites e avaliacao de KML, GeoPackage e Shapefile em [geometry-formats.md](geometry-formats.md).

## Validacao

- Kernel valida estrutura: anel fechado, minimo de coordenadas, CRS unico.
- `GeometryDiagnostics` aponta anel aberto, vertices insuficientes ou duplicados e auto-interseccao, com localizacao quando disponivel.
- `IPropertyGeometryValidator` combina validacao estrutural (Turf.js no cliente) com sobreposicao entre imoveis, ignorando o proprio imovel.
- Sobreposicao relevante e rejeitada antes da persistencia com o codigo `properties.geometry.overlap`; nada e corrigido automaticamente.
- Divergencia entre area declarada e area calculada e registrada, nunca corrigida silenciosamente.

## Dados pessoais

- CPF/CNPJ sao mascarados na saida (`***.***.***-XX`).
- `localStorage` nao e apresentado como armazenamento seguro.
- Dado demonstrativo nao deve conter documento real de pessoa.

## Basemap

Desenvolvimento e demonstracao usam tiles do OpenStreetMap, com atribuicao e parametro configuravel em `MapView`.

Producao municipal deve apontar para servico oficial de tiles (municipal ou estadual), com termo de uso e disponibilidade verificados. Nenhum dado de imovel ou contribuinte e enviado ao provedor de tiles alem da extensao visualizada.
