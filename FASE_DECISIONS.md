# FASE · Registro de decisiones

Fecha del registro: 17 nov 2026. Cada decisión recoge contexto, problema, alternativas, decisión, consecuencias y evidencia. Lo que no consta en el repo o en las pruebas está marcado como «sin verificar».

## a) Supabase con clave anon y RLS en vez de backend propio

- **Contexto.** FASE necesita récords en línea (idea 80) y analítica anónima (idea 140). El autor es una sola persona y el juego es gratis, sin cuentas de usuario.
- **Problema.** Guardar y leer datos desde web, Windows y Android sin mantener un servidor.
- **Alternativas.** Backend propio (coste y mantenimiento); no tener récords en línea; otro servicio (sin evaluar en el repo).
- **Decisión.** Proyecto Supabase «fase» (plan gratuito). El cliente usa la clave pública `anon` y las reglas de seguridad por filas (RLS) deciden qué se puede hacer: leer y añadir récords, solo añadir eventos, nunca modificar ni borrar.
- **Consecuencias.**
  - No hay secretos en el cliente.
  - Cualquiera con la clave pública puede enviar un tiempo falso; solo lo limitan los `check` de 3 s a 1 h y de 1 a 12 caracteres.
  - El plan gratuito se pausa tras una semana sin uso.
  - La analítica no envía desde el editor y se puede apagar en Opciones → Controles.
- **Evidencia.** `Assets/Scripts/Online.cs`; `Tools/supabase.sql`; prueba con `curl` el 16 nov (insertar, leer top 10, rechazo de borrado, rechazo de tiempos imposibles, eventos no legibles); commit `3a03f95`.

## b) Cooperativo en línea no implementado (solo cooperativo local)

- **Contexto.** La idea 84 propone que uno mueva a Lumi y el otro la línea desde PCs distintos.
- **Problema.** Requiere sincronización en tiempo real entre dos jugadores. Supabase no sustituye a un servidor de juego en tiempo real, WebGL solo permite WebSockets (no UDP) y un servidor tiene coste y mantenimiento.
- **Alternativas.** Servidor de juego propio o servicio de relé (coste); sincronización por sondeo sobre Supabase (latencia, sin verificar); no hacerlo.
- **Decisión.** No implementarlo. Se mantiene el cooperativo local, en el mismo equipo (`Settings.Coop`, segundo mando o teclas propias).
- **Consecuencias.** La idea 84 queda ⛔. Los jugadores que quieran jugar a distancia no pueden. No hay código de red de cooperativo en el repo.
- **Evidencia.** `FASE_MASTER_PLAN.md`, tarea 8; `Assets/Scripts/Settings.cs` y `ShiftLine.cs` (cooperativo local); búsqueda sin resultados de paquetes de red en `Packages/manifest.json`.

## c) Twitch con IRC anónimo de solo lectura

- **Contexto.** La idea 120 deja que el chat de un canal vote hacia dónde va la línea.
- **Problema.** Leer el chat sin pedir al streamer que cree una aplicación de Twitch ni guardar claves en un juego de cliente público.
- **Alternativas.** OAuth y la API de Twitch (necesita secretos o un servidor intermedio); extensión de Twitch (otro proyecto); no integrar.
- **Decisión.** Conectar al IRC de Twitch por WebSocket (`wss://irc-ws.chat.twitch.tv:443`) como invitado anónimo (`justinfan…`), solo para leer. Cuenta votos `!izquierda/!derecha`, `!left/!right`, `<` y `>` en rondas de 3 s. En web se usa un puente JavaScript (`Twitch.jslib`).
- **Consecuencias.** Sin cuenta, sin OAuth y sin secretos. Solo lectura: el juego no puede escribir en el chat. Depende de que Twitch mantenga el acceso anónimo al IRC (sin verificar a futuro).
- **Evidencia.** `Assets/Scripts/TwitchVotes.cs`, `Assets/Plugins/WebGL/Twitch.jslib`; en el editor, conexión al canal `#twitch` con estado 2 y overlay visible (17 nov). No probado en la web.

## d) Cola de exportación que cambia de plataforma y recarga antes de compilar

- **Contexto.** Se exportan tres versiones (web, Windows, Android) desde el mismo proyecto de Unity 6000.6.7f1.
- **Problema.** Si se cambiaba de plataforma y se compilaba en la misma llamada, Unity decidía qué recursos de render quitar con los datos de la plataforma anterior. En Windows faltaban los del SSAO y la build salía con la pantalla en negro.
- **Alternativas.** Compilar con `-buildTarget` desde línea de comandos para cada plataforma (no probado); conservar los recursos a mano (frágil); aceptar el fallo (no).
- **Decisión.** `Assets/Editor/WebBuild.cs` mantiene una cola (`SessionState`, clave `fase_exportar_cola`). Para cada plataforma cambia primero el destino activo, espera a la recompilación y recarga, y solo entonces compila. Menú «Fase → Exportar las tres».
- **Consecuencias.** Las tres exportaciones son más lentas y dependen de que el editor siga abierto. La build de Windows ya no sale en negro.
- **Evidencia.** Commits `5b1f7fa` y `6be1bef`; Windows: menú renderizado y `Player.log` sin excepciones (17 nov).

## e) Opción Gráficos: perfiles Mobile/PC y ajustes en tiempo de ejecución, solo fuera del editor

- **Contexto.** El juego corre en navegador de escritorio, Windows y móvil, con dispositivos muy distintos.
- **Problema.** Un único nivel de calidad no sirve para todos. Cambiar los recursos de URP desde el juego en el editor modificaría los archivos del proyecto.
- **Alternativas.** Una sola calidad fija; solo el `QualityGuard` automático; ajustes que también escriben en el editor.
- **Decisión.** Perfiles de calidad `Mobile` y `PC` (`Assets/Settings`). `Settings.cs` elige el perfil (Auto: ordenador y navegador usan PC, móvil usa Mobile) y sobreescribe `renderScale`, MSAA y resolución de sombras en 5 niveles (Auto/Baja/Media/Alta/Máxima), además de pantalla completa, VSync y FPS. Los ajustes se aplican solo si `!Application.isEditor`. `QualityGuard` baja o sube la resolución interna según los FPS (tampoco actúa en el editor).
- **Consecuencias.** En el editor no se ve el efecto real de las opciones: hay que mirarlo en una build, y eso aún no se ha medido.
- **Evidencia.** `Assets/Scripts/Settings.cs` (líneas ~216-262), `QualityGuard.cs`; panel visto en el editor en español y francés. Efecto real: no probado.

## f) Sin Unity Test Framework: pruebas automáticas en Python y Playwright

- **Contexto.** Los scripts de `Assets/Scripts` están en `Assembly-CSharp` sin archivos `.asmdef`.
- **Problema.** Sin asmdef, un proyecto de pruebas del Unity Test Framework no puede referenciar el código del juego sin reestructurarlo. Además, GitHub Actions no tiene Unity (no hay licencia como secreto).
- **Alternativas.** Reorganizar el código en asmdef y escribir pruebas en modo Edit y Play; GameCI con licencia (ver R8 en la matriz de requisitos); solo pruebas manuales.
- **Decisión.** No introducir el Unity Test Framework. Las pruebas automáticas son: (1) `Tools/comprobar_niveles.py`, que comprueba en CI (`.github/workflows/niveles.yml`) que todos los niveles se pueden terminar; (2) `Tools/prueba_web/prueba.mjs` con Playwright en Chrome, que se ejecuta en CI antes de subir la web a itch (`itch-web.yml`).
- **Consecuencias.** No hay pruebas unitarias del código C#. El comprobador analiza el diseño, no juega; la prueba web solo cubre carga, menú, nivel 1 y teclas. El bot de pruebas (`PlayBot.cs`) llega solo al 10-21 % del camino.
- **Evidencia.** No existe ningún `.asmdef` en `Assets`; los dos workflows; 54/54 niveles terminables (17 nov); run de `web-20261117-1` en success.

## g) APK con firma de depuración hasta que el autor cree su keystore

- **Contexto.** Se publica un APK en itch para instalar a mano (paquete `com.oscar07.fase`, versión 0.1.0, versionCode 1).
- **Problema.** Un keystore propio es una credencial que debe guardar el autor fuera del repositorio; perder la contraseña impide actualizar la app en tiendas.
- **Alternativas.** Crear un keystore ahora (lo decide el autor y no se pone en el repositorio); no publicar Android.
- **Decisión.** Mantener la firma de depuración (`AndroidKeystoreName` vacío en `ProjectSettings`) hasta que el autor cree su keystore.
- **Consecuencias.** Sirve para instalar a mano, no para Google Play (idea 87 ⛔). Al cambiar a un keystore propio, los usuarios tendrían que desinstalar la versión firmada con depuración antes de instalar la nueva (comportamiento general de Android, sin verificar con este APK).
- **Evidencia.** `BUILDING.md` (sección de Android); `apksigner` sobre el APK (17 nov); el APK no se ha probado en dispositivo.
