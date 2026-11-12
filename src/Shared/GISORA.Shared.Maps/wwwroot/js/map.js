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

const vertexIcon = L.divIcon({ className: 'gisora-map-vertex', iconSize: [12, 12] });

function disableDraw(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw) {
    return;
  }

  if (instance.draw.line) {
    instance.draw.line.remove();
  }

  if (instance.draw.handles) {
    instance.draw.handles.forEach((handle) => handle.remove());
  }

  if (instance.draw.marker) {
    instance.draw.marker.remove();
  }

  if (instance.draw.polygon) {
    instance.draw.polygon.remove();
  }

  if (instance.draw.onMapClick) {
    instance.map.off('click', instance.draw.onMapClick);
  }

  instance.draw = null;
}

export function enableDraw(elementId, dotNetRef) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  disableDraw(elementId);

  const state = { points: [], line: null, polygon: null, handles: null, dotNetRef };
  state.onMapClick = (event) => {
    state.points.push(event.latlng);

    if (state.line === null) {
      state.line = L.polyline(state.points, { color: '#1f6f4a', weight: 3 }).addTo(instance.map);
    } else {
      state.line.setLatLngs(state.points);
    }
  };

  instance.draw = state;
  instance.map.on('click', state.onMapClick);

  return true;
}

export function finishDraw(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw || instance.draw.points.length < 3) {
    return false;
  }

  const points = instance.draw.points;
  const dotNetRef = instance.draw.dotNetRef;
  const ring = points.map((point) => [point.lng, point.lat]);
  ring.push([points[0].lng, points[0].lat]);

  disableDraw(elementId);
  dotNetRef.invokeMethodAsync('HandleGeometryCompleted', JSON.stringify({ type: 'Polygon', coordinates: [ring] }));

  return true;
}

export function cancelDraw(elementId) {
  disableDraw(elementId);

  return true;
}

export function enablePointPick(elementId, dotNetRef) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  disableDraw(elementId);

  const state = { points: [], line: null, polygon: null, handles: null, marker: null, dotNetRef };
  state.onMapClick = (event) => {
    if (state.marker === null) {
      state.marker = L.marker(event.latlng).addTo(instance.map);
    } else {
      state.marker.setLatLng(event.latlng);
    }

    dotNetRef.invokeMethodAsync('HandlePointPicked', event.latlng.lat, event.latlng.lng);
  };

  instance.draw = state;
  instance.map.on('click', state.onMapClick);

  return true;
}

export function enableEdit(elementId, geoJson, dotNetRef) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  disableDraw(elementId);

  const geometry = JSON.parse(geoJson);
  const outer = geometry.coordinates[0];
  const latlngs = outer.slice(0, outer.length - 1).map(([lng, lat]) => L.latLng(lat, lng));
  const polygon = L.polygon(latlngs, { color: '#2f7fb5', weight: 3 }).addTo(instance.map);

  const handles = latlngs.map((latlng) => L.marker(latlng, { draggable: true, icon: vertexIcon }).addTo(instance.map));
  const notify = () => {
    const points = handles.map((handle) => handle.getLatLng());
    polygon.setLatLngs(points);

    const ring = points.map((point) => [point.lng, point.lat]);
    ring.push([points[0].lng, points[0].lat]);
    dotNetRef.invokeMethodAsync('HandleGeometryEdited', JSON.stringify({ type: 'Polygon', coordinates: [ring] }));
  };

  handles.forEach((handle) => handle.on('drag', notify));

  instance.draw = { points: [], line: null, polygon, handles, onMapClick: null, dotNetRef };
  instance.map.fitBounds(polygon.getBounds(), { padding: [24, 24] });

  return true;
}

export function destroy(elementId) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  disableDraw(elementId);
  instance.map.remove();
  instances.delete(elementId);

  return true;
}
