# FASE · Tablero de tareas de cierre

Fecha: 17 nov 2026 · Plazo de la jam: **1 dic 2026** (congelación de novedades: 27 nov, según `docs/RECORTES.md`).

Estados: BACKLOG · READY · IN PROGRESS · BLOCKED · VERIFIED · COMPLETED. Prioridad: P0 (imprescindible para entregar), P1 (importante), P2 (deseable).

| ID | Tarea | Prioridad | Dependencias | Estado | Evidencia |
|---|---|---|---|---|---|
| T-01 | Revisión de código independiente | P1 | — | IN PROGRESS | Pendiente de informe; los arreglos de revisiones anteriores están en los commits `bb5457b` y `f557830` |
| T-02 | Revisión de seguridad (clave `anon`, RLS de Supabase, workflows, secretos del repo) | P0 | — | IN PROGRESS | Pendiente de informe; `SECURITY.md` y `.github/dependabot.yml` creados el 17 nov |
| T-03 | Medición de rendimiento y prueba con teclado en Windows (`Builds/Windows/FASE.exe`) | P1 | Build de Windows (hecha) | READY | La build abre, el menú se ve y `Player.log` tiene 0 excepciones; falta medir FPS y jugar. `PerfLog.cs` (sin versionar) puede servir |
| T-04 | Pantallas de resultados a 1366×768, 1280×720 y 2560×1440 | P1 | Editor de Unity abierto | READY | Ya vistas a 1920×1080, 4:3 y 21:9 (`FASE_TEST_MATRIX.md`) |
| T-05 | Prueba en móvil Android (instalar APK, controles táctiles, 60 fps) | P1 | Móvil con depuración USB | BLOCKED | Sin dispositivo conectado por ADB. APK en canal `android` de itch (`build-20261117-1`) |
| T-06 | Página de itch pública | P0 | Decisión del autor; T-07 no depende | BLOCKED | Página RESTRICTED; la hace el autor. Pendiente también borrar el `GameOff2026.exe` viejo del canal `windows` (KI-07) |
| T-07 | Envío a la GitHub Game Off 2026 (plazo 1 dic 2026) | P0 | T-06 | BACKLOG | Pasos en `docs/lanzamiento/CAMPANA.md` (itch → Game Off 2026 → Submit) |
| T-08 | Tráiler nuevo y campaña en redes | P2 | T-06, aviso del autor | BACKLOG | El autor avisará. Campaña escrita en `docs/lanzamiento/CAMPANA.md`; tráiler actual del 2 nov (`Trailer/`) |
| T-09 | Actualizar `docs/PENDIENTES.md` con `FASE_REQUIREMENTS_MATRIX.md` | P2 | — | BACKLOG | Discrepancias listadas en la matriz de requisitos |
| T-10 | Reactivar el proyecto de Supabase antes del envío y de la campaña (se pausa tras 1 semana sin uso) | P1 | T-07 | BACKLOG | KI-05 |
| T-11 | Probar Twitch y el nivel de la semana en la build web | P2 | — | BACKLOG | Solo probados en el editor |
| T-12 | Exportaciones y subida a itch de las tres versiones | P0 | — | COMPLETED | `butler status` con ✓ en `html5` (`web-20261117-1`), `win64` y `android` (`build-20261117-1`); run de CI de la web en success |
| T-13 | Prueba de la build web en Chrome (local y CI) | P0 | T-12 | VERIFIED | `Tools/prueba_web/prueba.mjs`: carga 6,6 s, sin errores de consola; capturas en `Tools/prueba_web/capturas-1117b` |
