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
- Cadastro tecnico municipal tende a usar projecao local (SIRGAS 2000 / UTM zona 25S, `EPSG:31985`). A escolha sera registrada em ADR quando a base oficial chegar.
- Transformacao sempre explicita; o CRS de origem fica no objeto de geometria.

## Validacao

- Kernel valida estrutura: anel fechado, minimo de coordenadas, CRS unico.
- Validacao topologica (auto-interseccao, sobreposicao) depende de `IGeometryValidator`, implementado com Turf.js no cliente e PostGIS no servidor.
- Divergencia entre area declarada e area calculada e registrada, nunca corrigida silenciosamente.

## Dados pessoais

- CPF/CNPJ sao mascarados na saida (`***.***.***-XX`).
- `localStorage` nao e apresentado como armazenamento seguro.
- Dado demonstrativo nao deve conter documento real de pessoa.

## Basemap

Desenvolvimento e demonstracao usam tiles do OpenStreetMap, com atribuicao e parametro configuravel em `MapView`.

Producao municipal deve apontar para servico oficial de tiles (municipal ou estadual), com termo de uso e disponibilidade verificados. Nenhum dado de imovel ou contribuinte e enviado ao provedor de tiles alem da extensao visualizada.
