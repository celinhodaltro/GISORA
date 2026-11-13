# Integracao SINTER / CIB

Consulta de Unidade Imobiliaria (UI) pelo Cadastro Imobiliario Brasileiro, conforme a documentacao oficial da Receita Federal.

## Fonte oficial

- Guia: https://docs.receitafederal.gov.br/sinter/literal/guide/ui/exemplos-requisicao.html
- Configuracoes de seguranca e ambientes: https://docs.receitafederal.gov.br/sinter/literal/guide/ui/apendice-1-configuracoes-seguranca.html

Nenhum endpoint ou propriedade abaixo foi inferido: todos constam na documentacao publicada.

## Endpoints utilizados

### Consultar uma unica UI por CIB

```http
GET /api/v1/ui/{cib}
Authorization: Bearer <token>
Accept: application/json
```

### Obter token (client credentials)

```http
POST /v1/keycloak/oidc/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
client_id=<CLIENT_ID>
client_secret=<CLIENT_SECRET>
```

Resposta: `access_token`, `expires_in`, `token_type` (a documentacao informa expiracao de 60 segundos).

## Ambientes oficiais

| Ambiente | URL base | Token |
| --- | --- | --- |
| TESTE | `https://api.receitafederal.gov.br/prr-sinter/` | `/v1/keycloak/oidc/token` (POST) |
| PRODUCAO | `https://api.sinter.receitafederal.gov.br` | `/v1/keycloak/oidc/token` (POST) |

## Contrato de resposta

O modelo tipado fica em `src/GISORA.Application/Integration/CibContracts.cs` e mapeia os campos documentados:

| Objeto | Campos |
| --- | --- |
| `InfoIbge` | `nomeMunicipio`, `siglaUf`, `codigoIbge` |
| `Cib` | `valor`, `situacao` |
| `DadosGeraisImovel` | `inscricaoImobiliaria`, `tipoImovel`, `tpArquitetonico`, `destinacaoImovel`, `areaTerreno`, `areaConstruida`, `bice`, `anoConstrutivo`, `valorVenal`, `dtUltimoValorVenal`, `padraoConstrutivo`, `qtdGaragem`, `temPiscina`, `valorRefMercado`, `temBairro`, `dataUltVlrMercado` |
| `AreaConstruidaCompl` | `areaPrivativa`, `areaComum`, `fraIdeal` |
| `EnderecoImovel` | `tipoLogradouro`, `nomeLogradouro`, `bairro`, `cep`, `numeroImovel`, `complNroImovel`, `complEndereco` |
| `Titular` | `niTitular`, `nomeTitular`, `percTitularidade`, `dtAquisicaoTitular`, `docTitularidade`, `tipoTitularidade`, `nomeValido`, `niTitularPrenchidoCorretamente`, `dvniTitularValido` |
| `ServicoRegistroImovel` | `nomeServentiaRI`, `cnsRI`, `cnmRI`, `numMatriculaRI`, `numUltimoAtoRI`, `lvCartRI`, `flCartRI`, `dtUltAtualizacao` |
| `CartorioNotas` | `nomeServentiaNotas`, `cnsNotas`, `lvCartNotas`, `flCartNotas` |
| `ITBI` | `baseCalculITBI`, `dtTransacaoITBI`, `tpTransacaoITBI`, `percTransacionadoITBI`, `valorRefITBI`, `TransmitenteITBI`, `AdquirenteITBI` |

Os codigos de dominio (`tipoImovel`, `tpArquitetonico`, `destinacaoImovel`, `padraoConstrutivo`, `tipoLogradouro`, `docTitularidade`, `tipoTitularidade`, `tpTransacaoITBI`) sao exibidos como **codigo numerico**, com o rotulo "(codigo)". A traducao exige consultar as tabelas de dominio oficiais; nenhum de-para foi inventado.

## Modos de operacao

Configuracao em `src/Presentations/GISORA.Presentation/wwwroot/appsettings.json`:

```json
{
  "Cib": {
    "Mode": "Demo",
    "ProxyBaseUrl": ""
  }
}
```

### Demo (padrao)

- `Mode: "Demo"`.
- `DemoCibPropertyGateway` responde com registros simulados que seguem exatamente a estrutura oficial.
- CIBs disponiveis: `DEMO1A2B`, `DEMO3C4D`, `DEMO5E6F`.
- A interface exibe o selo **Modo demonstracao** e um aviso de dados simulados.

### Producao

- `Mode: "Production"` e `ProxyBaseUrl` apontando para um **backend de integracao**.
- O gateway faz `GET {ProxyBaseUrl}/api/v1/ui/{cib}` e espera o mesmo JSON documentado.
- O backend deve obter o token no Keycloak (`/v1/keycloak/oidc/token`) e enviar `Authorization: Bearer` ao SINTER. Nenhum token, `client_id` ou `client_secret` e enviado ao navegador.
- Sem `ProxyBaseUrl`, o app sobe com um gateway indisponivel e a consulta retorna erro explicito de configuracao.

### GitHub Pages

O Pages e um host estatico. A publicacao usa **Demo**; o modo producao exige um servico proprio (proxy) hospedado fora do Pages.

## Tratamento de erros

| Situacao | Resultado na interface |
| --- | --- |
| 404 | "Nenhuma unidade imobiliaria encontrada para o CIB informado." |
| 401/403 | "A consulta ao SINTER nao foi autorizada..." |
| 429/5xx | "SINTER indisponivel no momento (HTTP xxx)." |
| Timeout / rede | Mensagem de indisponibilidade ou tempo limite |
| JSON inesperado | "Resposta do SINTER em formato inesperado." |

## Arquitetura

```
Pages/Cib.razor
  -> ICibPropertyGateway            (GISORA.Integration.Application)
       -> DemoCibPropertyGateway    (demo, sem credenciais)
       -> ProxyCibPropertyGateway   (producao, via backend)
       -> UnavailableCibPropertyGateway (producao sem configuracao)
```

A pagina nao conhece HTTP, tokens ou formatos de transporte: apenas o contrato tipado.

## Dados pessoais

- O NI do titular nunca e exibido integralmente; a interface mascara o documento.
- Nomes em modo demo sao ficticios e contem "EXAMPLE".
- O armazenamento local nao e usado para persistir respostas do CIB.

## Limitacoes conhecidas

- A API oficial nao foi projetada para consumo direto por navegador (CORS nao documentado); por isso o modo producao exige proxy.
- A resposta do CIB nao inclui geometria; a associacao com o mapa usa a geometria do imovel municipal selecionado, nao a do SINTER.
- A documentacao de ambientes nao informa limites de taxa para a consulta por CIB; o tratamento de HTTP 429 foi implementado de forma defensiva.
