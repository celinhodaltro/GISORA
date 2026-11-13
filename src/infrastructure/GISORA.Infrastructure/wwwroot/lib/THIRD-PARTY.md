# Bibliotecas vendorizadas

| Biblioteca | Versao | Licenca | Origem |
| --- | --- | --- | --- |
| @turf/turf | 7.4.0 | MIT | https://github.com/Turfjs/turf |
| proj4 | 2.22.0 | MIT | https://github.com/proj4js/proj4js |

`geospatial.bundle.js` foi gerado com esbuild (`--bundle --format=esm --minify`) a partir de `entry.js`:

```js
import * as turf from '@turf/turf';
import proj4 from 'proj4';
export { turf, proj4 };
```

Nenhum CDN em runtime. Para atualizar, repetir o bundle e registrar aqui a nova versao.
