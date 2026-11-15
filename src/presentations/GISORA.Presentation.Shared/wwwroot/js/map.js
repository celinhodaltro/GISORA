import * as L from '../lib/leaflet/leaflet-src.esm.js';

L.Icon.Default.mergeOptions({
  iconRetinaUrl: new URL('../lib/leaflet/images/marker-icon-2x.png', import.meta.url).href,
  iconUrl: new URL('../lib/leaflet/images/marker-icon.png', import.meta.url).href,
  shadowUrl: new URL('../lib/leaflet/images/marker-shadow.png', import.meta.url).href,
});

const instances = new Map();

function sanitizeToken(value) {
  return String(value ?? '').replace(/[^a-z0-9_-]/gi, '').toLowerCase();
}

function createPin(color, icon) {
  const glyph = sanitizeToken(icon);
  const glyphHtml = glyph.length > 0 ? `<i class="rzi" aria-hidden="true">${glyph}</i>` : '';
  const typeClass = glyph.length > 0 ? ` gisora-map-pin--${glyph}` : '';

  return L.divIcon({
    className: `gisora-map-pin-wrapper${typeClass}`,
    html: `<span class="gisora-map-pin" style="--pin-color:${color}">${glyphHtml}</span>`,
    iconSize: [28, 28],
    iconAnchor: [14, 14],
    popupAnchor: [0, -14],
  });
}

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

  L.control.scale({ position: 'bottomleft', imperial: false, maxWidth: 130 }).addTo(map);

  const layers = new Map();
  const instance = { map, layers, dotNetRef, draw: null, onKeyDown: null };

  map.on('mousemove', (event) => {
    dotNetRef.invokeMethodAsync('HandleCoordinateChanged', event.latlng.lat, event.latlng.lng);
  });

  map.on('dblclick', () => completeFromDoubleClick(elementId));

  instance.onKeyDown = (event) => {
    if (event.key === 'Escape') {
      cancelFromKeyboard(elementId);
    }
  };

  window.addEventListener('keydown', instance.onKeyDown);

  instances.set(elementId, instance);
  window.setTimeout(() => map.invalidateSize(), 0);

  return true;
}

export function setLayer(elementId, layerId, geoJson, color) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  removeLayer(elementId, layerId);

  const stroke = color ?? '#1351b4';

  const group = L.geoJSON(JSON.parse(geoJson), {
    style: { color: stroke, weight: 2, fillColor: stroke, fillOpacity: 0.18 },
    pointToLayer: (feature, latlng) => {
      const properties = feature.properties ?? {};
      const pinColor = properties.cor ? String(properties.cor) : stroke;
      const icon = properties.icone ? String(properties.icone) : null;

      return L.marker(latlng, {
        icon: createPin(pinColor, icon),
        title: properties.nome ? String(properties.nome) : '',
        alt: properties.nome ? String(properties.nome) : '',
      });
    },
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

  instance.map.invalidateSize();

  let bounds = null;

  for (const layer of instance.layers.values()) {
    const layerBounds = layer.getBounds();
    bounds = bounds === null ? layerBounds : bounds.extend(layerBounds);
  }

  if (bounds === null || bounds.isValid() === false) {
    return false;
  }

  instance.map.fitBounds(bounds, { padding: [20, 20], animate: false });

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

function normalizeKind(kind) {
  const value = String(kind ?? 'Polygon').toLowerCase();

  if (value === 'point') {
    return 'Point';
  }

  if (value === 'linestring' || value === 'line') {
    return 'LineString';
  }

  return 'Polygon';
}

function toPosition(latlng) {
  return [latlng.lng, latlng.lat];
}

function toRing(points) {
  const ring = points.map(toPosition);
  ring.push(toPosition(points[0]));

  return ring;
}

function buildGeometry(kind, points) {
  if (kind === 'Point') {
    return { type: 'Point', coordinates: toPosition(points[0]) };
  }

  if (kind === 'LineString') {
    return { type: 'LineString', coordinates: points.map(toPosition) };
  }

  return { type: 'Polygon', coordinates: [toRing(points)] };
}

function minimumPoints(kind) {
  if (kind === 'Point') {
    return 1;
  }

  return kind === 'LineString' ? 2 : 3;
}

function disableDraw(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw) {
    return;
  }

  if (instance.draw.preview) {
    instance.draw.preview.remove();
  }

  if (instance.draw.handles) {
    instance.draw.handles.forEach((handle) => handle.remove());
  }

  if (instance.draw.marker) {
    instance.draw.marker.remove();
  }

  if (instance.draw.onMapClick) {
    instance.map.off('click', instance.draw.onMapClick);
  }

  instance.map.doubleClickZoom.enable();
  instance.draw = null;
}

export function enableDraw(elementId, dotNetRef, kind) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  disableDraw(elementId);

  const geometryKind = normalizeKind(kind);
  const state = { mode: 'draw', kind: geometryKind, points: [], preview: null, marker: null, handles: [], dotNetRef };

  state.render = () => {
    if (state.preview === null) {
      state.preview = geometryKind === 'Polygon'
        ? L.polygon(state.points, { color: '#1f6f4a', weight: 3, fillOpacity: 0.12 }).addTo(instance.map)
        : L.polyline(state.points, { color: '#1f6f4a', weight: 3 }).addTo(instance.map);
    } else {
      state.preview.setLatLngs(state.points);
    }

    state.handles.forEach((handle) => handle.remove());
    state.handles = state.points.map((point) => L.marker(point, { icon: vertexIcon }).addTo(instance.map));
  };

  state.onMapClick = (event) => {
    state.points.push(event.latlng);

    if (event.originalEvent?.detail > 1) {
      removeDuplicateTail(instance);
      completeDraw(elementId);

      return;
    }

    if (geometryKind === 'Point') {
      const geometry = buildGeometry(geometryKind, state.points);
      const notify = state.dotNetRef;

      disableDraw(elementId);
      notify.invokeMethodAsync('HandleGeometryCompleted', JSON.stringify(geometry));

      return;
    }

    state.render();
  };

  instance.draw = state;
  instance.map.doubleClickZoom.disable();
  instance.map.on('click', state.onMapClick);

  return true;
}

export function undoLastVertex(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw || instance.draw.kind === 'Point' || instance.draw.points.length === 0) {
    return false;
  }

  instance.draw.points.pop();
  instance.draw.render();

  return true;
}

function completeDraw(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw || instance.draw.mode !== 'draw') {
    return false;
  }

  const state = instance.draw;
  if (state.points.length < minimumPoints(state.kind)) {
    return false;
  }

  const geometry = buildGeometry(state.kind, state.points);
  const dotNetRef = state.dotNetRef;

  disableDraw(elementId);
  dotNetRef.invokeMethodAsync('HandleGeometryCompleted', JSON.stringify(geometry));

  return true;
}

function removeDuplicateTail(instance) {
  const points = instance.draw.points;

  while (points.length > 1) {
    const last = instance.map.latLngToContainerPoint(points[points.length - 1]);
    const previous = instance.map.latLngToContainerPoint(points[points.length - 2]);

    if (last.distanceTo(previous) > 2) {
      return;
    }

    points.pop();
  }
}

function completeFromDoubleClick(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw || instance.draw.mode !== 'draw') {
    return;
  }

  removeDuplicateTail(instance);
  completeDraw(elementId);
}

function cancelFromKeyboard(elementId) {
  const instance = instances.get(elementId);
  if (!instance || !instance.draw) {
    return;
  }

  const dotNetRef = instance.draw.dotNetRef;
  disableDraw(elementId);
  dotNetRef.invokeMethodAsync('HandleDrawingCanceled');
}

export function finishDraw(elementId) {
  return completeDraw(elementId);
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

  const state = { mode: 'point', points: [], line: null, polygon: null, handles: null, marker: null, dotNetRef };
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
  if (geometry.type === 'Point') {
    return false;
  }

  const isLine = geometry.type === 'LineString';
  const coordinates = isLine ? geometry.coordinates : geometry.coordinates[0];
  const positions = isLine ? coordinates : coordinates.slice(0, coordinates.length - 1);
  const latlngs = positions.map(([lng, lat]) => L.latLng(lat, lng));
  const shape = isLine
    ? L.polyline(latlngs, { color: '#2f7fb5', weight: 3 }).addTo(instance.map)
    : L.polygon(latlngs, { color: '#2f7fb5', weight: 3 }).addTo(instance.map);

  const handles = latlngs.map((latlng) => L.marker(latlng, { draggable: true, icon: vertexIcon }).addTo(instance.map));
  const notify = () => {
    const points = handles.map((handle) => handle.getLatLng());
    shape.setLatLngs(points);

    const edited = isLine
      ? { type: 'LineString', coordinates: points.map(toPosition) }
      : { type: 'Polygon', coordinates: [toRing(points)] };
    dotNetRef.invokeMethodAsync('HandleGeometryEdited', JSON.stringify(edited));
  };

  handles.forEach((handle) => handle.on('drag', notify));

  instance.draw = { mode: 'edit', kind: geometry.type, points: [], preview: shape, marker: null, handles, dotNetRef };
  instance.map.doubleClickZoom.disable();
  instance.map.fitBounds(shape.getBounds(), { padding: [24, 24], animate: false });

  return true;
}

export function destroy(elementId) {
  const instance = instances.get(elementId);
  if (!instance) {
    return false;
  }

  disableDraw(elementId);

  if (instance.onKeyDown) {
    window.removeEventListener('keydown', instance.onKeyDown);
  }

  instance.map.remove();
  instances.delete(elementId);

  return true;
}
