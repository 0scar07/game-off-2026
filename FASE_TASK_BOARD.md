# FASE · Tablero de tareas de cierre

Fecha: 17 nov 2026 · Plazo de la jam: **1 dic 2026** (congelación de novedades: 27 nov, según `docs/RECORTES.md`).

Estados: BACKLOG · READY · IN PROGRESS · BLOCKED · VERIFIED · COMPLETED. Prioridad: P0 (imprescindible para entregar), P1 (importante), P2 (deseable).

| ID | Tarea | Prioridad | Dependencias | Estado | Evidencia |
|---|---|---|---|---|---|
| T-01 | Revisión de código independiente | P1 | — | VERIFIED | 8 hallazgos (ninguno crítico ni alto), todos corregidos en `cff8fda` |
| T-02 | Revisión de seguridad (clave `anon`, RLS de Supabase, workflows, secretos del repo) | P0 | — | REVIEW | Sin secretos en el historial. Cliente endurecido en `cff8fda`; SQL de Supabase preparado (T-14) |
| T-03 | Medición de rendimiento y prueba con teclado en Windows (`Builds/Windows/FASE.exe`) | P1 | Build de Windows | IN PROGRESS | FPS medidos (`FASE_TEST_MATRIX.md`); falta repetir Máxima con la build nueva y jugar con teclado |
| T-04 | Pantallas de resultados a 1366×768, 1280×720 y 2560×1440 | P1 | Editor de Unity | VERIFIED | Fotos en las cuatro resoluciones y 20:9; dos solapes corregidos |
| T-05 | Prueba en móvil Android (instalar APK, controles táctiles, 60 fps) | P1 | Móvil con depuración USB | BLOCKED | Sin dispositivo conectado por ADB. APK en canal `android` de itch (`build-20261117-1`) |
| T-06 | Página de itch pública | P0 | Decisión del autor; T-07 no depende | BLOCKED | Página RESTRICTED; la hace el autor. Pendiente también borrar el `GameOff2026.exe` viejo del canal `windows` (KI-07) |
| T-07 | Envío a la GitHub Game Off 2026 (plazo 1 dic 2026) | P0 | T-06 | BACKLOG | Pasos en `docs/lanzamiento/CAMPANA.md` (itch → Game Off 2026 → Submit) |
| T-08 | Tráiler nuevo y campaña en redes | P2 | T-06, aviso del autor | BACKLOG | El autor avisará. Campaña escrita en `docs/lanzamiento/CAMPANA.md`; tráiler actual del 2 nov (`Trailer/`) |
| T-09 | Actualizar `docs/PENDIENTES.md` con `FASE_REQUIREMENTS_MATRIX.md` | P2 | — | BACKLOG | Discrepancias listadas en la matriz de requisitos |
| T-10 | Reactivar el proyecto de Supabase antes del envío y de la campaña (se pausa tras 1 semana sin uso) | P1 | T-07 | BACKLOG | KI-05 |
| T-11 | Probar Twitch y el nivel de la semana en la build web | P2 | — | BACKLOG | Solo probados en el editor |
| T-12 | Exportaciones y subida a itch de las tres versiones | P0 | — | COMPLETED | `butler status` con ✓ en `html5` (`web-20261117-1`), `win64` y `android` (`build-20261117-1`); run de CI de la web en success |
| T-13 | Prueba de la build web en Chrome (local y CI) | P0 | T-12 | VERIFIED | `Tools/prueba_web/prueba.mjs`: carga 6,6 s, sin errores de consola; capturas en `Tools/prueba_web/capturas-1117b` |
| T-14 | Aplicar `Tools/supabase-endurecer.sql` en producción | P1 | Permiso del autor | BLOCKED | SQL listo; probar después que el juego sigue subiendo récords |
| T-15 | Borrar la rama `main` (o su `build.yml`) y proteger `fase` | P1 | Permiso del autor | BLOCKED | KI-17 |
| T-16 | Exportar de nuevo las tres versiones con los arreglos, probar y publicar | P0 | T-01, T-02 | IN PROGRESS | Exportación en cola lanzada el 17 nov |
