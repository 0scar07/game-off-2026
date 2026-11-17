# FASE · Matriz de pruebas

Pruebas realizadas el 16 y 17 nov 2026. Solo se recogen pruebas con evidencia real; lo demás figura como NO PROBADA o BLOQUEADA.

Resultados: **SUPERADA** · **FALLIDA** · **NO PROBADA** · **BLOQUEADA** (hay un impedimento externo).

## Matriz

| Funcionalidad | Plataforma | Método | Resultado | Evidencia | Fecha |
|---|---|---|---|---|---|
| Carga de la build, menú, entrada al nivel 1 y respuesta a teclas | Web | `Tools/prueba_web/prueba.mjs` en Chrome con servidor HTTP local | SUPERADA | Carga en 6,6 s; sin errores de consola; capturas en `Tools/prueba_web/capturas-1117b` | 16-17 nov 2026 |
| Misma prueba web antes de subir a itch | Web (CI) | GitHub Actions, job `probar` de `.github/workflows/itch-web.yml` | SUPERADA | Runs de las releases `web-20261117-1` y `web-20261117-2`: success | 17 nov 2026 |
| Abrir la build, menú y ausencia de excepciones | Windows | Build abierta a mano; revisión de `Player.log` | SUPERADA | `Player.log` con 0 excepciones; captura del menú renderizado | 16-17 nov 2026 |
| Pantalla negra en el arranque (SSAO) | Windows | Build abierta tras recompilar con la cola de exportación | SUPERADA | Antes salía en negro por recursos de SSAO quitados al compilar; corregido con la cola de exportación (commit `5b1f7fa`) | 16-17 nov 2026 |
| Jugar con teclado dentro de la build | Windows | — | NO PROBADA | Solo se jugó en el editor | — |
| Generación del APK e identidad del paquete | Android | Exportación Unity + `apksigner` | SUPERADA | `com.oscar07.fase`, versionName 0.1.0, versionCode 1, minSdk 26, targetSdk 36, arm64-v8a, firma de depuración | 16-17 nov 2026 |
| Instalar y jugar el APK | Android | — | BLOQUEADA | No hay móvil conectado por ADB | — |
| Rendimiento a 60 fps | Android | — | BLOQUEADA | Sin dispositivo | — |
| Controles táctiles y modo de un toque | Android / web móvil | — | BLOQUEADA | Sin dispositivo | — |
| Cortinilla de nivel sin solapes (1920×1080, 4:3, 21:9) | Editor | Capturas en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Un solo título por nivel (sin repetir tras la cortinilla) | Editor | Inspección en Unity | SUPERADA | Commit `6786ad5` | 16-17 nov 2026 |
| Panel Gráficos en español y francés | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Efecto real de las opciones de Gráficos (resolución, MSAA, sombras) | Windows / web / Android | — | NO PROBADA | El código solo actúa fuera del editor; no se midió en ninguna build | — |
| Opciones y Controles sin solapes | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Pausa sin solapes | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Aviso de logro en español y francés | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Contador de Sin fin alineado | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Pista del jefe en 3 líneas | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Letrero de la flor dentro de pantalla | Editor | Inspección en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Meta → pantalla de resultados correcta | Editor | Partida en Unity | SUPERADA | Capturas del editor | 16-17 nov 2026 |
| Cronómetro quieto bajo la cortinilla | Editor | Lecturas del cronómetro | SUPERADA | 0,04 → 0,71 → 2,24 s | 16-17 nov 2026 |
| Pantallas de resultados a 1366×768, 1280×720 y 2560×1440 | Editor | — | NO PROBADA | Solo se probó 1920×1080, 4:3 y 21:9 | — |
| Carga de las 60 escenas | Editor | Gira de escenas en Unity | SUPERADA | Todas cargan sin errores (fotos) | 16-17 nov 2026 |
| Los 54 niveles se pueden terminar | Python (CI) | `Tools/comprobar_niveles.py` (análisis, no partida real) | SUPERADA | 54/54; se ejecuta en `.github/workflows/niveles.yml` | 16-17 nov 2026 |
| Niveles terminables jugando de verdad | Editor | Bot de pruebas (`PlayBot.cs`) | FALLIDA | El bot solo llega al 10-21 % del camino: no sirve como prueba de partida completa | 16-17 nov 2026 |
| Récords en línea: insertar, leer top 10 | Supabase | `curl` contra la API REST | SUPERADA | Probado el 16 nov | 16 nov 2026 |
| Récords en línea: rechazo de borrado | Supabase | `curl` | SUPERADA | Probado el 16 nov | 16 nov 2026 |
| Récords en línea: rechazo de tiempos imposibles | Supabase | `curl` | SUPERADA | Rechaza fuera de 3 s-1 h (regla `check` de `Tools/supabase.sql`) | 16 nov 2026 |
| Analítica: eventos no legibles con la clave pública | Supabase | `curl` | SUPERADA | Probado el 16 nov | 16 nov 2026 |
| Tabla de récords y envío de analítica dentro del juego | Todas | — | NO PROBADA | Solo se probó el backend; la analítica no envía desde el editor (`Online.cs`) | — |
| Twitch: conexión al canal `#twitch` y overlay | Editor | Prueba manual | SUPERADA | Estado 2 (en el canal) y overlay visible | 16-17 nov 2026 |
| Twitch: votos reales aplicados a la línea | Editor | — | NO PROBADA | No hay registro | — |
| Twitch en la web | Web | — | NO PROBADA | Solo se probó en el editor | — |
| Nivel de la semana: descarga y juego | Editor | Prueba manual | SUPERADA | Descarga de `semana/nivel.txt` y partida | 16-17 nov 2026 |
| Nivel de la semana en web, Windows y Android | Web / PC / Android | — | NO PROBADA | — | — |
| Cooperativo en línea | — | — | BLOQUEADA | No implementado (necesita servidor en tiempo real) | — |
| itch: canales `html5`, `win64` y `android` actualizados | itch | `butler status` | SUPERADA | ✓ en los tres: `web-20261117-2` (html5) y `build-20261117-2` (win64, android), build final | 17 nov 2026 |
| Página de itch pública | itch | — | BLOQUEADA | La página sigue RESTRICTED; la hace el autor | — |
| Rendimiento en Windows (medición) | Windows | `FASE.exe -medir` | SUPERADA | Auto 41–50 FPS, Máxima 20–24 FPS en gráfica integrada; 0 excepciones (tabla abajo) | 17 nov 2026 |
| Revisión de seguridad independiente | — | — | SUPERADA | Sin secretos en el historial; cliente endurecido en `9a09faa`; SQL del servidor pendiente de permiso | 17 nov 2026 |
| Build web final en Chrome | Web | `Tools/prueba_web/prueba.mjs` | SUPERADA | Carga 6,5 s, menú, nivel 1 y teclas sin errores; un solo título por nivel | 17 nov 2026 |
| APK final revisado | Android | `aapt2` + `apksigner` | SUPERADA | `com.oscar07.fase` 0.1.0 (1), target 36, arm64-v8a, firma de depuración válida | 17 nov 2026 |

## Cobertura por plataforma

| Plataforma | Qué se ha probado | Qué falta |
|---|---|---|
| Web | Carga, menú, nivel 1 y teclas en Chrome (local y CI) | Móvil real; Twitch; nivel de la semana; opciones de Gráficos |
| Windows | Arranca, menú, 0 excepciones, rendimiento medido en Auto, Baja y Máxima | Jugar con teclado; probar el menú de Gráficos en la build |
| Android | El APK se genera con los datos correctos | Todo lo que sea ejecutarlo (sin dispositivo) |
| Editor | Interfaz en 3 proporciones, 60 escenas, flujo de meta y resultados | Resoluciones 1366×768, 1280×720 y 2560×1440 |

## Límites de las pruebas automáticas

- `Tools/comprobar_niveles.py` analiza el diseño del nivel; no ejecuta una partida.
- `Tools/prueba_web/prueba.mjs` comprueba carga, menú, entrada al nivel 1 y que el juego responde a teclas y no da errores; no recorre niveles.
- No hay Unity Test Framework (ver `FASE_DECISIONS.md`, registro f).

## Rendimiento medido (17 nov 2026)

Build de Windows, nivel `05-criaturas`, ventana 1920×1080, VSync activo (pantalla de 60 Hz), con
`FASE.exe -medir -calidad N -nivel 05-criaturas -salir 32` (`Assets/Scripts/PerfLog.cs`).
Equipo: AMD Radeon Vega 3 integrada (gama baja), Direct3D 11. Medias de 5 s, sin excepciones.

| Calidad | FPS (por tramo de 5 s) | Peor fotograma | Memoria total |
|---|---|---|---|
| Auto | 44,7 · 48,0 · 51,2 · 49,2 | 33 ms (100 ms al cargar) | 261 MB |
| Máxima | 27,1 · 36,8 · 35,6 · 45,2 | 33–217 ms | 253 MB |
| Baja | 59,8 · 57,8 · 59,6 · 60,1 | 17–33 ms | 256 MB |

Hallazgo: el ajuste automático de resolución (`QualityGuard.cs`) bajaba la escala también con una
calidad elegida a mano (Máxima pasaba de 1,5 a 1,0). Corregido: ahora solo actúa en «Auto».

### Repetición con la build final (17 nov 2026, 09:55)

Mismo equipo y método, build de Windows exportada con todos los arreglos (`9a09faa`). El primer
tramo de cada medición incluye la carga del nivel y no se cuenta.

| Calidad | Escala | FPS (por tramo de 5 s) | Peor fotograma | Memoria total |
|---|---|---|---|---|
| Máxima | 1,50 (se mantiene) | 24,3 · 20,6 · 19,6 · 23,6 · 20,2 | 67–83 ms | 257 MB |
| Auto | 1,00 → 0,90 | 41,4 · 43,4 · 43,3 · 45,8 · 49,6 | 33 ms | 256 MB |

Conclusión: la calidad elegida ya se respeta (Máxima no baja de 1,5). En una gráfica integrada
Máxima va a ~22 FPS, como avisa su descripción («Para equipos potentes»); Auto queda en 41–50 FPS
bajando la escala sola, y Baja llega a 60. 0 excepciones en las dos ejecuciones.
