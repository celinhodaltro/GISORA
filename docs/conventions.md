# Convencoes de codigo

## Sem comentarios

Nao usar comentarios em C#, Razor ou JavaScript. Nem XML doc, nem comentario de linha, nem comentario de bloco.

Se uma regra precisa de explicacao, o nome do tipo, do metodo ou da variavel esta errado. Renomear e a correcao.

Excecoes: nenhuma no codigo. Decisoes vao para `docs/adr/`; regras vao para testes.

## Nomes

- Tipos, metodos, propriedades e conceitos tecnicos em ingles.
- Texto exibido ao usuario em portugues, na camada de apresentacao ou em recursos.
- Sem abreviacao obscura (`qtd`, `vlr`, `dsc`).
- Booleanos afirmativos (`IsActive`, `HasFaces`).

## Componentizacao

- Um componente Razor, uma responsabilidade. Componente que busca dado, filtra, formata e desenha esta errado.
- Regra de negocio nunca no componente: fica em Domain ou Application.
- Componente recebe dado por parametro e devolve intencao por evento.
- Extrair componente quando o bloco tem nome proprio e ciclo de vida proprio, nunca por estetica.
- Componente sem `@inject` de repositorio ou de store.

## Camadas

- `Domain` nao referencia `Infrastructure`, host ou biblioteca de UI.
- `Application` depende de contratos, nunca de implementacao concreta.
- `Infrastructure` implementa contratos e concentra tecnologia (JS, JSON, HTTP, arquivo).
- Host compoe dependencias; nao contem regra.

## Contratos

- Repositorio para escrita e leitura por identidade.
- Query service para projecao, filtro e paginacao.
- DTO de leitura nao expoe entidade de dominio.

## Testes

- Nome do teste descreve o comportamento, sem comentario.
- Teste de dominio nao toca infraestrutura.
- Um comportamento por teste.

## E2E

`tests/E2E/GISORA.Web.E2ETests` usa Playwright contra o host real.

```bash
dotnet build GISORA.slnx -c Release
pwsh tests/E2E/GISORA.Web.E2ETests/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test tests/E2E/GISORA.Web.E2ETests -c Release --no-build
```

O fixture inicia o host em porta dedicada e encerra ao final. E2E valida boot, DI, migracao de schema e navegacao; nao valida aparencia.
