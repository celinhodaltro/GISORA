import * as L from '../lib/leaflet/leaflet-src.esm.js';

const instances = new Map();

function normalizeProperties(properties) {
  const result = {};

  for (const [key, value] of Object.entries(properties ?? {})) {
    result[key] = value === null || value === undefined ? '' : String(value);
  }

  return result;
}

export function create(elementId, options, dotNetRef) {
  const element = document.getElementById(elementId);
  if (element === null) {
    throw new Error(`Elemento de mapa nao encontrado: ${elementId}`);
  }

  const map = L.map(element, {
    center: [options.centerLatitude, options.centerLongitude],
    zoom: options.zoom,
    zoomControl: false,
  });

  if (options.tileUrl) {
    L.tileLayer(options.tileUrl, {
      maxZoom: 19,
      attribution: options.attribution ?? '',
    }).addTo(map);
  }

  const layers = new Map();

  map.on('mousemove', (event) => {
    dotNetRef.invokeMethodAsync('HandleCoordinateChanged', event.latlng.lat, event.latlng.lng);
  });

  instances.set(elementId, { map, layers, dotNetRef });
  window.setTimeout(() => map.invalidateSize(), 0);

  return true;
}

export function setLayer(elementId, layerId, geoJson) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  removeLayer(elementId, layerId);

  const group = L.geoJSON(JSON.parse(geoJson), {
    onEachFeature: (feature, layer) => {
      layer.on('click', () => {
        instance.dotNetRef.invokeMethodAsync('HandleFeatureSelected', {
          id: String(feature.id ?? feature.properties?.id ?? ''),
          properties: normalizeProperties(feature.properties),
        });
      });
    },
  }).addTo(instance.map);

  instance.layers.set(layerId, group);

  return true;
}

export function removeLayer(elementId, layerId) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  const layer = instance.layers.get(layerId);
  if (!layer) {
    return false;
  }

  layer.remove();
  instance.layers.delete(layerId);

  return true;
}

export function fitBounds(elementId) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  let bounds = null;

  for (const layer of instance.layers.values()) {
    const layerBounds = layer.getBounds();
    bounds = bounds === null ? layerBounds : bounds.extend(layerBounds);
  }

  if (bounds === null || bounds.isValid() === false) {
    return false;
  }

  instance.map.fitBounds(bounds, { padding: [20, 20] });

  return true;
}

export function zoom(elementId, delta) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  instance.map.setZoom(instance.map.getZoom() + delta);

  return true;
}

export function invalidateSize(elementId) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  instance.map.invalidateSize();

  return true;
}

export function destroy(elementId) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  instance.map.remove();
  instances.delete(elementId);

  return true;
}
