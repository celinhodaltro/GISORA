# Marca

## Ativo de origem

| Item | Valor |
| --- | --- |
| Arquivo | `Emblema de Mapa em Azul e Verde.png` |
| Formato | PNG com fundo transparente |
| Dimensao original | 1254 x 1254 px |
| Fornecido por | Proprietario do projeto |
| Titularidade | Confirmar antes de uso publico |

O ativo original nao e versionado. Apenas os derivados otimizados entram no repositorio.

## Derivados versionados

| Arquivo | Dimensao | Uso |
| --- | --- | --- |
| `wwwroot/img/brand/gisora-mark-128.png` | 128 x 128 | Cabecalho (`2.25rem`) e splash (`4.5rem`) |
| `wwwroot/img/brand/gisora-mark-64.png` | 64 x 64 | Reserva para telas menores e tiles internos |
| `wwwroot/favicon.png` | 32 x 32 | Favicon do navegador |
| `wwwroot/icon-192.png` | 192 x 192 | Icone de aplicacao |
| `wwwroot/icon-512.png` | 512 x 512 | Icone de aplicacao e futura PWA |

Os arquivos foram gerados por redimensionamento com interpolacao bicubica de alta qualidade a partir do PNG original. Nao houve vetorizacao automatica.

## Regras de aplicacao

- A marca no cabecalho e decorativa: o nome `GISORA` ja aparece em texto ao lado. Usar `alt` vazio e `aria-hidden="true"`.
- Na splash, a marca e a unica representacao textual da aplicacao: usar `alt="GISORA"`.
- Sempre definir altura e largura pelo CSS do container para evitar deslocamento de layout.
- Nao substituir o PNG por SVG tracado automaticamente.
- Manter contraste sobre superficies claras. A marca nao deve receber sombra, contorno ou recoloracao.

## Onde e usada

- `Layout/AppHeader.razor`: marca do cabecalho.
- `wwwroot/index.html`: splash de carregamento e favicon.
- `wwwroot/css/app.css`: dimensoes e enquadramento (`object-fit: contain`).
