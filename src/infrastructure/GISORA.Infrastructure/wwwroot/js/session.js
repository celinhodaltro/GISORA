export function read(name) {
  const prefix = `${name}=`;
  const parts = document.cookie ? document.cookie.split('; ') : [];

  for (const part of parts) {
    if (part.startsWith(prefix)) {
      return decodeURIComponent(part.slice(prefix.length));
    }
  }

  return null;
}

export function write(name, value, maxAgeSeconds) {
  const expires = new Date(Date.now() + (maxAgeSeconds * 1000)).toUTCString();
  document.cookie = `${name}=${encodeURIComponent(value)}; Path=/; Expires=${expires}; SameSite=Lax`;
}

export function clear(name) {
  document.cookie = `${name}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Lax`;
}
