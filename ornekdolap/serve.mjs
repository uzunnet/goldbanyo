import { createReadStream, existsSync } from "node:fs";
import { createServer } from "node:http";
import { extname, join, normalize } from "node:path";
import { fileURLToPath } from "node:url";

const kok = fileURLToPath(new URL(".", import.meta.url));
const port = Number.parseInt(process.env.PORT || "5127", 10);

const mime = {
  ".html": "text/html; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".js": "application/javascript; charset=utf-8",
  ".glb": "model/gltf-binary",
  ".gltf": "model/gltf+json"
};

createServer((istek, yanit) => {
  const yol = decodeURIComponent(new URL(istek.url, `http://localhost:${port}`).pathname);
  const istenen = yol === "/" ? "index.html" : yol.slice(1);
  const dosya = normalize(join(kok, istenen));

  if (!dosya.startsWith(kok) || !existsSync(dosya)) {
    yanit.writeHead(404);
    yanit.end("Bulunamadi");
    return;
  }

  yanit.writeHead(200, { "Content-Type": mime[extname(dosya)] || "application/octet-stream" });
  createReadStream(dosya).pipe(yanit);
}).listen(port, "127.0.0.1", () => {
  console.log(`ornekdolap http://127.0.0.1:${port}`);
});
