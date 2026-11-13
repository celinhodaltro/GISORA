# Bounded contexts

| Contexto | Responsabilidade | Onda |
| --- | --- | --- |
| Identity | Usuarios, perfis, permissoes, autenticacao | 8 (#51, ADR 0007) |
| Geospatial | Kernel: geometrias, CRS, coordenadas, operacoes espaciais | 1 |
| Territory | Municipio, distritos, bairros, setores fiscais, quadras | 1 |
| Properties | Imoveis, lotes, edificacoes, unidades, contribuintes, titularidade | 1 |
| Roads | Logradouros, segmentos viarios | 3 |
| PublicFacilities | Escolas, saude, pracas, parques, predios publicos | 4 |
| Cemetery | Cemiterios, setores, quadras, jazigos, sepultamentos | 4 |
| Rural | Propriedades rurais, CAR, SIGEF, APP, reserva legal | 4 |
| UrbanPlanning | Zoneamento, uso do solo, plano diretor, restricoes | 4 |
| Licensing | Processos, licencas, alvaras, tramitacao | 4 |
| Inspection | Fiscalizacoes, vistorias, ocorrencias, autos | 4 |
| Imagery | Panoramas 360, capturas, associacao geografica | 4 |
| Integration | Tributacao, SINTER, CIB, importacao e exportacao | 7 |
| Reports | Relatorios, indicadores, consultas consolidadas | 7 |

Onda 3 nao significa um contexto por vez obrigatoriamente: significa que cada um nasce quando tem caso de uso real.

## Regras de fronteira

- Um contexto nao referencia o `Domain` nem o `Application` de outro.
- Dados de outro contexto entram por identificador, nunca por entidade.
- Kernel compartilhado hoje: `Geospatial` e `Common`.
- Se um contexto precisar de dado de outro, o caminho e um contrato publico (futuro `GISORA.Shared.Contracts`) ou evento de integracao, nao acesso ao repositorio interno.

## Exemplo aplicado

`Cemetery` precisara de endereco e localizacao. Ele recebe `streetId`/`neighborhoodId` e uma geometria; nao consulta `Properties` nem `Territory`.

`Properties` guarda `StreetId` no `Address` e `BlockFace` guarda `StreetId`; ambos sao `Guid`, sem dependencia de `Roads`.

## Contextos que nao devem absorver regra alheia

`Geospatial` e kernel: geometria, CRS, medida, distancia. Ele nao conhece lote, jazigo ou zona. Regra de cemiterio fica em `Cemetery`; regra de parcelamento fica em `UrbanPlanning`.

## Extração futura

Cada contexto pode virar servico proprio sem reescrita, porque:

- contratos de repositorio ja isolam persistencia;
- dependencias entre contextos sao por identificador;
- os testes arquiteturais impedem acoplamento acidental.
