# Backup e restauracao

A POC armazena tudo no `localStorage` do navegador. A pagina `/data` permite exportar, validar e restaurar esse estado.

## Formato do arquivo

```json
{
  "schemaVersion": 1,
  "exportedAt": "2026-11-14T12:00:00+00:00",
  "records": {
    "gisora:meta:schema": "1",
    "gisora:properties:property-<id>": "{...}"
  }
}
```

- `schemaVersion` deve ser igual a versao atual do armazenamento local.
- `records` contem pares chave/valor crus, sempre no prefixo `gisora:`.
- Binarios, imagens e arquivos grandes nao entram no backup.

## Restauracao

| Modo | Comportamento |
| --- | --- |
| Mesclar | Adiciona registros ausentes; preserva os existentes. |
| Substituir | Remove os registros `gisora:` atuais e grava o backup. |

Antes de restaurar, o arquivo e validado (JSON, versao, namespace e tipos). A restauracao e atomica: se a gravacao falhar, o estado anterior e recarregado. Falhas de quota sao reportadas sem perda silenciosa.

Apos restaurar, recarregue a pagina para que as telas leiam os dados atualizados.

## Limitacoes

- O `localStorage` e local ao navegador e ao perfil; nao ha sincronizacao.
- Nao e um armazenamento seguro; pode ser apagado pelo usuario ou pelo navegador.
- O limite varia por navegador (normalmente poucos megabytes).
- O backup nao substitui uma politica institucional de backup e restauracao.
- A versao do schema precisa coincidir; migracoes futuras devem prever compatibilidade.
