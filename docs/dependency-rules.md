# Regras de dependencia

Garantidas por teste em `tests/Architecture`. Uma regra violada falha o build.

| Regra | Motivo |
| --- | --- |
| `GISORA.Domain` nao depende de `GISORA.Application`, `GISORA.Infrastructure` ou `GISORA.Web` | Dominio precisa sobreviver a troca de UI, de armazenamento e de host |
| `GISORA.Domain` nao depende de `Microsoft.JSInterop`, `Microsoft.AspNetCore`, `System.Text.Json`, `Radzen` | Regra de negocio nao pode depender de tecnologia |
| `GISORA.Application` nao depende de `GISORA.Infrastructure` nem de host | Caso de uso depende de contrato, nao de implementacao |
| `GISORA.Application` nao depende de `Microsoft.JSInterop`, `Microsoft.AspNetCore`, `System.Text.Json`, `Radzen` | Um caso de uso deve rodar no servidor e no cliente |
| `GISORA.Infrastructure` nao depende de `GISORA.Web` | Adaptador nunca conhece quem o consome |
| Tipos de `GISORA.Web`, exceto `Program`, nao dependem de `GISORA.Infrastructure` | Host e composition root; componente consome Application |
| Contextos de negocio nao dependem uns dos outros | Bounded contexts isolados |
| Kernel `Geospatial` nao depende de contextos de negocio | Kernel nao conhece regra de contexto |
| Host nao define `AggregateRoot` nem `ValueObject` | Regra de negocio no dominio, nunca no host |

## Como a verificacao funciona

`tests/Architecture/GISORA.ArchitectureTests` carrega os assemblies `GISORA.Domain`, `GISORA.Application`, `GISORA.Infrastructure` e `GISORA.Web` com ArchUnitNET e inspeciona as dependencias reais de cada tipo, nao o `csproj`. Referencia adicionada por engano, sem uso, nao dispara a regra; uso concreto, sim.

Como os modulos compartilham assembly, a fronteira entre contextos e verificada por namespace (`GISORA.Domain.<Modulo>` e `GISORA.Application.<Modulo>`).

## Dependencias permitidas

```
GISORA.Domain          <- GISORA.Application, GISORA.Infrastructure, GISORA.Web
GISORA.Application     <- GISORA.Infrastructure, GISORA.Web
GISORA.Infrastructure  <- GISORA.Web (somente composicao)
```

## Excecoes registradas

- `GISORA.Web.Program` pode depender de `GISORA.Infrastructure`: composicao de dependencias e a unica funcao do arquivo.
- Testes tem acesso a `internal` via `InternalsVisibleTo` para reconstrucao de agregado e verificacao de invariantes.
