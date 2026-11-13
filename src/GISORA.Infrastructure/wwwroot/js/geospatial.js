import { turf, proj4 } from '../lib/geospatial.bundle.js';

const utmSouthDefinitions = {
  31978: 18,
  31979: 19,
  31980: 20,
  31981: 21,
  31982: 22,
  31983: 23,
  31984: 24,
  31985: 25,
};

for (const [code, zone] of Object.entries(utmSouthDefinitions)) {
  proj4.defs(
    `EPSG:${code}`,
    `+proj=utm +zone=${zone} +south +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs`,
  );
}

const srid = (code) => `EPSG:${code}`;
const sameSrid = (a, b) => String(a) === String(b);

function mapCoordinates(coordinates, action) {
  if (Array.isArray(coordinates[0])) {
    return coordinates.map((part) => mapCoordinates(part, action));
  }

  return action(coordinates);
}

function ringArea(ring) {
  let sum = 0;

  for (let index = 0; index < ring.length - 1; index++) {
    const [x1, y1] = ring[index];
    const [x2, y2] = ring[index + 1];
    sum += x1 * y2 - x2 * y1;
  }

  return Math.abs(sum) / 2;
}

function polygonArea(polygonCoordinates) {
  const [exterior, ...holes] = polygonCoordinates;
  const holesArea = holes.reduce((total, hole) => total + ringArea(hole), 0);

  return Math.max(ringArea(exterior) - holesArea, 0);
}

function planarArea(geometry) {
  if (geometry.type === 'Polygon') {
    return polygonArea(geometry.coordinates);
  }

  if (geometry.type === 'MultiPolygon') {
    return geometry.coordinates.reduce((total, polygon) => total + polygonArea(polygon), 0);
  }

  return 0;
}

function centroid(geometry) {
  return turf.centroid(turf.feature(geometry)).geometry.coordinates;
}

function toFeature(geometry) {
  return turf.feature(geometry);
}

export function validate(geometryJson) {
  const geometry = JSON.parse(geometryJson);
  const errors = [];

  if (geometry.type === 'Polygon') {
    if (turf.kinks(toFeature(geometry)).features.length > 0) {
      errors.push('A geometria possui auto-intersecao.');
    }
  }

  if (geometry.type === 'MultiPolygon') {
    if (turf.kinks(turf.multiPolygon(geometry.coordinates)).features.length > 0) {
      errors.push('A geometria possui auto-intersecao.');
    }
  }

  if (geometry.type === 'Polygon' || geometry.type === 'MultiPolygon') {
    const polygons = geometry.type === 'Polygon' ? [geometry.coordinates] : geometry.coordinates;

    for (const polygon of polygons) {
      if (polygon[0].length < 4) {
        errors.push('Anel externo com menos de 4 coordenadas.');
      }
    }
  }

  return { valid: errors.length === 0, errors };
}

export function transform(geometryJson, sourceCode, targetCode) {
  const geometry = JSON.parse(geometryJson);

  if (sameSrid(sourceCode, targetCode)) {
    return geometryJson;
  }

  const from = srid(sourceCode);
  const to = srid(targetCode);

  return JSON.stringify({
    ...geometry,
    coordinates: mapCoordinates(geometry.coordinates, (coordinate) => {
      const [x, y] = proj4(from, to, coordinate);

      return coordinate.length > 2 ? [x, y, coordinate[2]] : [x, y];
    }),
  });
}

export function area(geometryJson, sridCode) {
  const geometry = JSON.parse(geometryJson);

  return sameSrid(sridCode, 4326) ? turf.area(geometry) : planarArea(geometry);
}

export function distance(firstJson, secondJson, sridCode) {
  const first = centroid(JSON.parse(firstJson));
  const second = centroid(JSON.parse(secondJson));

  if (sameSrid(sridCode, 4326)) {
    return turf.distance(turf.point(first), turf.point(second), { units: 'meters' });
  }

  return Math.hypot(second[0] - first[0], second[1] - first[1]);
}

export function intersects(firstJson, secondJson) {
  return turf.booleanIntersects(toFeature(JSON.parse(firstJson)), toFeature(JSON.parse(secondJson)));
}

export function contains(containerJson, containedJson) {
  return turf.booleanContains(toFeature(JSON.parse(containerJson)), toFeature(JSON.parse(containedJson)));
}

export function intersection(firstJson, secondJson) {
  const result = turf.intersect(
    toFeature(JSON.parse(firstJson)),
    toFeature(JSON.parse(secondJson)),
  );

  return result === null ? null : JSON.stringify(result.geometry);
}
