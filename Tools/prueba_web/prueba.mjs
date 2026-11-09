// Prueba de humo de la versión web: abre el juego en un Chrome sin ventana, espera a que cargue,
// pasa la pantalla de inicio, entra a jugar, pulsa unas teclas y comprueba que no hay errores y
// que la imagen se mueve. Guarda capturas en la carpeta indicada.
//
// Uso: node prueba.mjs http://localhost:8000/ capturas
//      (con CHROME=chrome usa el Chrome instalado en vez del de Playwright)
import { chromium } from "playwright";
import fs from "node:fs";
import path from "node:path";

const url = process.argv[2] || "http://localhost:8000/";
const out = process.argv[3] || "capturas";
fs.mkdirSync(out, { recursive: true });

const browser = await chromium.launch({
  channel: process.env.CHROME || undefined,
  headless: true,
  // WebGL por software: los servidores de GitHub no tienen tarjeta gráfica.
  args: ["--use-angle=swiftshader", "--enable-unsafe-swiftshader", "--ignore-gpu-blocklist", "--autoplay-policy=no-user-gesture-required"],
});
const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });

const errors = [];
// Avisos normales de Unity o del navegador que no son fallos del juego.
const harmless = [/AudioContext/i, /favicon/i, /WebGL warning/i, /GPU stall/i, /powerPreference/i];
page.on("pageerror", (e) => errors.push("Excepción: " + e.message));
page.on("console", (m) => {
  if (m.type() !== "error") return;
  const text = m.text();
  if (!harmless.some((r) => r.test(text))) errors.push("Consola: " + text);
});

const shot = async (name) => {
  const file = path.join(out, name + ".png");
  await page.screenshot({ path: file });
  console.log("captura", file);
  return fs.readFileSync(file);
};
const wait = (s) => page.waitForTimeout(s * 1000);
let ok = true;
const fail = (why) => { ok = false; console.error("FALLO:", why); };

const start = Date.now();
await page.goto(url, { waitUntil: "load" });
try {
  // La plantilla quita #loading cuando Unity ha arrancado.
  await page.waitForSelector("#loading", { state: "detached", timeout: 240000 });
  console.log(`Cargado en ${((Date.now() - start) / 1000).toFixed(1)} s`);
} catch {
  const status = await page.locator("#loading span").textContent().catch(() => "");
  fail("no terminó de cargar en 4 minutos " + (status || ""));
}

if (ok) {
  await wait(2);
  await shot("1-inicio");
  await page.mouse.click(640, 360);         // foco al lienzo
  await page.keyboard.press("Space");       // salta la pantalla de inicio
  await wait(8);
  const menu = await shot("2-menu");
  // "Jugar": la primera vez lleva a la historia; se salta con teclas.
  await page.mouse.click(1280 * 0.166, 720 * 0.52);
  await wait(8);
  for (let i = 0; i < 6; i++) { await page.keyboard.press("Space"); await wait(1.5); }
  await wait(6);
  await shot("3-nivel");
  await page.keyboard.down("KeyD"); await wait(1.5);
  await page.keyboard.press("Space"); await wait(1);
  await page.keyboard.up("KeyD");
  await page.keyboard.down("KeyE"); await wait(1.2); await page.keyboard.up("KeyE");
  await wait(1);
  const game = await shot("4-jugando");
  if (menu.equals(game)) fail("la imagen no cambió entre el menú y la partida (¿congelado?)");
}

if (errors.length) { fail(`${errors.length} errores`); for (const e of errors.slice(0, 20)) console.error("  " + e); }
await browser.close();
console.log(ok ? "La web abre y responde sin errores." : "La prueba de la web falló.");
process.exit(ok ? 0 : 1);
