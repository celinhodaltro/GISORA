export function isAvailable() {
  try {
    const probe = 'gisora:probe';
    window.localStorage.setItem(probe, '1');
    window.localStorage.removeItem(probe);
    return true;
  } catch {
    return false;
  }
}

export function getItem(key) {
  return window.localStorage.getItem(key);
}

export function setItem(key, value) {
  window.localStorage.setItem(key, value);
}

export function removeItem(key) {
  window.localStorage.removeItem(key);
}

export function getKeys(prefix) {
  const keys = [];
  for (let index = 0; index < window.localStorage.length; index++) {
    const key = window.localStorage.key(index);
    if (key !== null && key.startsWith(prefix)) {
      keys.push(key);
    }
  }
  return keys;
}

export function removeKeys(prefix) {
  const keys = getKeys(prefix);
  for (const key of keys) {
    window.localStorage.removeItem(key);
  }
}
