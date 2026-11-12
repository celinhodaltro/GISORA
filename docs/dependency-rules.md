# Regras de dependencia

Garantidas por teste em `tests/Architecture`. Uma regra violada falha o build.

| Regra | Motivo |
| --- | --- |
| `*.Domain` nao depende de `GISORA.Infrastructure`, `GISORA.Web`, `GISORA.Shared.DesignSystem` | Dominio precisa sobreviver a troca de UI, de armazenamento e de host |
| `*.Domain` nao depende de `Microsoft.JSInterop`, `Microsoft.AspNetCore`, `System.Text.Json`, `Radzen` | Regra de negocio nao pode depender de tecnologia |
| `*.Application` nao depende de `GISORA.Infrastructure` nem de host | Caso de uso depende de contrato, nao de implementacao |
| `*.Application` nao depende de `Microsoft.JSInterop`, `Microsoft.AspNetCore`, `System.Text.Json`, `Radzen` | Um caso de uso deve rodar no servidor e no cliente |
| `GISORA.Infrastructure.*` nao depende de `GISORA.Web` nem de `GISORA.Shared.DesignSystem` | Adaptador nunca conhece quem o consome |
| Tipos de `GISORA.Web`, exceto `Program`, nao dependem de `GISORA.Infrastructure` | Host e composition root; componente consome Application |
| `GISORA.Territory` nao depende de `GISORA.Properties` | Bounded contexts isolados |
| `GISORA.Properties` nao depende de `GISORA.Territory` | Bounded contexts isolados |
| `GISORA.Geospatial` nao depende de `GISORA.Territory` nem de `GISORA.Properties` | Kernel nao conhece regra de contexto |
| Modulos nao dependem de `GISORA.Infrastructure`, `GISORA.Shared` ou `GISORA.Web` | Camadas internas nao apontam para fora |
| Host nao define `AggregateRoot` nem `ValueObject` | Regra de negocio no dominio, nunca no host |

## Como a verificacao funciona

`tests/Architecture/GISORA.ArchitectureTests` carrega todos os assemblies da solucao com ArchUnitNET e inspeciona as dependencias reais de cada tipo, nao o `csproj`. Referencia adicionada por engano, sem uso, nao dispara a regra; uso concreto, sim.

## Dependencias permitidas

```
BuildingBlocks.Domain      <- todos
BuildingBlocks.Application <- Application, Infrastructure, Hosts
Geospatial.Domain          <- Territory, Properties, Infrastructure, Hosts
Geospatial.Application     <- Territory, Properties, Infrastructure, Hosts
<Territory|Properties>.Domain      <- o proprio modulo, Infrastructure, Hosts
<Territory|Properties>.Application <- o proprio modulo, Infrastructure, Hosts
Infrastructure.LocalStorage        <- Hosts
```

## Excecoes registradas

- `GISORA.Web.Program` pode depender de `GISORA.Infrastructure`: composicao de dependencias e a unica funcao do arquivo.
- Testes tem acesso a `internal` de seus modulos via `InternalsVisibleTo`, para reconstrucao de agregado e verificacao de invariantes.
