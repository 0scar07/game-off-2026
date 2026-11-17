# FASE · Problemas conocidos

Fecha: 17 nov 2026. Gravedad: **Alta** (puede impedir jugar o publicar), **Media** (afecta a la calidad o a la confianza), **Baja** (molestia o deuda).

| ID | Problema | Gravedad | Siguiente paso |
|---|---|---|---|
| KI-01 | El APK de Android no se ha probado en ningún dispositivo: no hay móvil conectado por ADB. Se sabe que se genera con los datos correctos (`com.oscar07.fase`, 0.1.0, minSdk 26, arm64-v8a), no que arranque | Alta | Conectar un móvil Android (depuración USB) y probar instalación, arranque, controles táctiles y rendimiento. Tarea del tablero: T-05 |
| KI-02 | El APK va firmado con la clave de depuración; no sirve para Google Play | Media | Cuando el autor decida, crear un keystore propio, guardarlo fuera del repositorio y anotar la contraseña a salvo. Ver `FASE_DECISIONS.md` (g) |
| KI-03 | El cooperativo en línea (idea 84) no está hecho; solo existe cooperativo local | Baja | Decisión tomada: no implementarlo en la jam. Reevaluar con un servidor de tiempo real. Ver `FASE_DECISIONS.md` (b) |
| KI-04 | Los récords en línea se pueden manipular: cualquiera con la clave pública puede insertar un tiempo falso. Los únicos límites son 3 s-1 h, nombre de 1 a 12 caracteres y nivel de 1 a 200. No hay límite de frecuencia | Media | Aceptable para una jam. Si hace falta, validar en una función del servidor (límite por IP o por nombre, tiempo mínimo por nivel) y borrar filas falsas desde el panel de Supabase |
| KI-05 | El plan gratuito de Supabase pausa el proyecto tras una semana sin uso; las llamadas fallarían y el juego muestra la tabla vacía o un error (comportamiento exacto sin verificar) | Media | Reactivar el proyecto desde el panel antes de enviar a la jam y de cualquier campaña; o programar una llamada periódica (sin hacer) |
| KI-06 | La página de itch sigue RESTRICTED (no es pública) | Alta | El autor debe hacerla pública. Tarea del tablero: T-06 |
| KI-07 | El canal `windows` de itch conserva un `GameOff2026.exe` viejo, de otra versión | Media | Borrarlo con `butler` o desde la página de itch (lo hace el autor). La versión nueva está en el canal `win64` |
| KI-08 | El bot de pruebas (`PlayBot.cs`) solo llega al 10-21 % del camino; no sirve como prueba de partida completa. `comprobar_niveles.py` analiza pero no juega | Media | Seguir afinando el bot o hacer una pasada manual de los niveles principales. Ver `FASE_TEST_MATRIX.md` |
| KI-09 | `docs/PENDIENTES.md` está desactualizado respecto a `docs/TANDAS.md` y al historial: marca ⬜ ideas ya programadas y tiene numeración repetida | Baja | Actualizar `PENDIENTES.md` a partir de `FASE_REQUIREMENTS_MATRIX.md` (esa tarea no se ha hecho aquí) |
| KI-10 | No se ha probado jugando con teclado la build de Windows (solo se abrió; menú y 0 excepciones) | Media | Jugar los primeros niveles con la `FASE.exe` del canal `win64`. Tarea del tablero: T-03 |
| KI-11 | No se han medido el rendimiento ni el efecto de las opciones de Gráficos en ninguna build | Media | Medir FPS en Windows (tarea T-03); `PerfLog.cs` existe pero está sin versionar |
| KI-12 | Las pantallas de resultados solo se han visto a 1920×1080, 4:3 y 21:9 | Baja | Probar a 1366×768, 1280×720 y 2560×1440. Tarea del tablero: T-04 |
| KI-13 | Twitch y el nivel de la semana solo se han probado en el editor; la web no | Media | Probarlos en la build web (el nivel de la semana depende de poder descargar desde `raw.githubusercontent.com`; sin verificar) |
| KI-14 | La prueba web de CI entra al nivel 1; si la analítica está activa fuera del editor, podría insertar eventos reales en la base de producción (sin verificar) | Baja | Revisar la tabla `events` en Supabase y, si es el caso, desactivar la analítica en la prueba |
| KI-15 | Hay archivos sin versionar en el árbol de trabajo (`PerfLog.cs` en `Assets/Scripts`, scripts de revisión en `Assets/Editor`, `Assets/_Recovery`, `.utmp/`) | Baja | Decidir qué se versiona y qué se añade a `.gitignore`. `PerfLog.cs` está en `Assets/Scripts` y se incluiría en las builds si se versiona |
| KI-16 | El tráiler de `Trailer/` es del 2 nov, anterior a las tandas A-G; no muestra lo nuevo | Baja | Hacer el tráiler nuevo cuando el autor avise (tarea T-08) |
