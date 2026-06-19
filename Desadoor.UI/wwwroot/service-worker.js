const CACHE_NAME = 'desadoor-v17';
const STATIC_ASSETS = [
  '/manifest.json',
  '/favicon.png',
  '/icon-192.png',
  '/css/app.css',
  '/css/sistem/tokens.css',
  '/_content/MudBlazor/MudBlazor.min.css',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(STATIC_ASSETS))
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key)))
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  if (event.request.method !== 'GET') return;

  const url = new URL(event.request.url);
  if (url.pathname.startsWith('/_framework/')) {
    event.respondWith(fetch(event.request, { cache: 'no-store' }));
    return;
  }

  // API isteklerini cache'leme — dinamik veri
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(fetch(event.request));
    return;
  }

  // Buyuk binary asset'leri ve ES modulleri cache'leme — GLB/GLTF/HDR/PDF/MJS
  if (/\.(glb|gltf|hdr|pdf|mjs)$/i.test(url.pathname)) {
    event.respondWith(fetch(event.request));
    return;
  }

  const agdanOnce = url.pathname === '/';

  if (agdanOnce) {
    event.respondWith(
      fetch(event.request)
        .then((response) => {
          if (response.ok && response.type === 'basic') {
            const clone = response.clone();
            caches.open(CACHE_NAME).then((cache) => cache.put(event.request, clone));
          }
          return response;
        })
        .catch(() => caches.match(event.request))
    );
    return;
  }

  event.respondWith(
    caches.match(event.request).then((cached) => {
      if (cached) return cached;

      return fetch(event.request).then((response) => {
        if (response.ok && response.type === 'basic') {
          const clone = response.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(event.request, clone));
        }
        return response;
      });
    })
  );
});
