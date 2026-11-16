# Continuar el desarrollo de FASE sin Claude

Guía para seguir manteniendo y publicando el juego a mano. Complementa a [docs/PULIDO.md](docs/PULIDO.md) (sección «Para seguir sin Claude»), sin repetirla entera.

Documentos hermanos: [ARCHITECTURE.md](ARCHITECTURE.md) · [ROADMAP.md](ROADMAP.md) · [BUILDING.md](BUILDING.md) · [CHANGELOG.md](CHANGELOG.md) · [CONTRIBUTING.md](CONTRIBUTING.md).

## Estado general

- **Juego:** FASE, puzle-plataformas 2.5D de día y noche. Unity 6000.6.7f1 con URP. Hecho para la GitHub Game Off 2026 (entrega el **1 de diciembre**; congelación de contenido el 27 de noviembre).
- **Contenido:** 3 capítulos (19 niveles), 3 niveles secretos, niveles extra en cinco biomas, 14 desafíos diarios, Taller, Sin fin, 4 idiomas.
- **Pulido:** todas las escenas cargan sin errores; la mayoría de niveles y mecánicas **faltan por jugar de principio a fin**. Ver la lista en [docs/PULIDO.md](docs/PULIDO.md).
- **Publicado:** la web está en itch.io (release `web-20261110-1`). Windows y Android se compilan con los menús de Unity, pero no se han subido.
- **Rama de trabajo:** `fase` del repositorio público `0scar07/game-off-2026`.

## Arquitectura

Resumen en una línea: la línea que parte el mundo (`ShiftLine`) recorta dibujo (shaders) y colisión (`WorldPiece`); los niveles son texto y `LevelBuilder` genera las escenas; la interfaz se crea por código (`UiKit`); se guarda en `PlayerPrefs` con claves `fase_*`. Todo el detalle, con diagramas y la tabla archivo → responsabilidad, está en [ARCHITECTURE.md](ARCHITECTURE.md).

## Terminado y pendiente

Ver [ROADMAP.md](ROADMAP.md). Lo esencial:

- **Terminado:** núcleo, tandas A-G, accesibilidad, 4 idiomas, nivel de la semana, Twitch, récords en línea, analítica.
- **Falta probar jugando:** niveles, mecánicas, recompensas, sonido, final secreto, móvil real.
- **No hecho:** cooperativo **en línea** (idea 84), Google Play, Steam.
- **Lo último antes de enviar:** créditos, página de itch, release v1.0, tráiler nuevo y campaña (ideas 92 y 142).

## Decisiones técnicas

| Decisión | Por qué |
|---|---|
| Niveles en texto (`Assets/Levels/*.txt`) y generador | Se editan y revisan en Git; `comprobar_niveles.py` los valida en GitHub |
| Interfaz por código (`UiKit`) | Un solo estilo; se puede montar en ejecución (Taller, Sin fin, resultados) |
| Física 2D con arte 3D | Controles y colisiones sencillos; aspecto con profundidad |
| Recorte día/noche en shader + colisión recortada | La línea se puede mover continuamente sin cambiar de escena |
| Eventos estáticos (`GameEvents`) | Sonido, efectos y logros reaccionan sin depender unos de otros |
| `Shapes` en vez de `GameObject.CreatePrimitive` | En la web, las primitivas añaden colisionadores 3D que el juego no usa |
| Brotli con *decompression fallback* en la web | Funciona en itch y en cualquier servidor sin configurar |
| IL2CPP `OptimizeSize` en la web | La exportación se quedaba sin memoria |
| Sin filtros de audio | La web no los admite; el eco bajo techo se hace repitiendo el sonido más bajo (`Enclosure`) |
| Twitch por IRC anónimo | Sin cuentas, ni OAuth, ni servidor propio |
| Récords y analítica con Supabase | Base de datos gratuita con REST; las reglas (RLS) deciden qué puede hacer el cliente |

## Supabase

- **Proyecto:** «fase», en la organización 0scar07, plan gratis.
- **URL:** `https://nxbhkkyesztocwsowvei.supabase.co`
- **Pausa:** el plan gratis **pausa el proyecto tras una semana sin uso**. Si pasa, se reactiva en el panel de Supabase; mientras tanto, el juego funciona igual sin lo en línea.
- **Clave en el juego:** la clave *anon* es **pública por diseño** y está en `Assets/Scripts/Online.cs` (`Url` y `Key`). Lo que puede hacer lo deciden las reglas de `Tools/supabase.sql`: leer récords y añadir filas, nunca cambiar ni borrar. **Nunca pongas la clave `service_role` en el cliente** (ni en el repositorio): da acceso total.
- Con `Url` o `Key` vacíos, todo lo en línea se apaga solo.
- **Crear las tablas desde cero:** pegar `Tools/supabase.sql` en Supabase → SQL Editor → Run.
- **Consultas útiles** (SQL Editor):
  - Dónde se muere la gente en cada nivel:
    `select level, round(x) as x, count(*) from events where kind = 'death' group by 1, 2 order by 1, 3 desc;`
  - Borrar un récord falso: Table Editor → `records` → borrar la fila.
- La tabla `records` rechaza tiempos fuera de 3 s a 1 h y nombres de más de 12 letras; `events` no se puede leer con la clave pública.
- Se pueden desactivar los eventos en el juego: Opciones → Controles (clave `fase_analitica`).

## Multijugador

- **Solo cooperativo local** (misma pantalla) y carrera a dos en la misma pantalla (Extras → Con amigos), más el fantasma de un amigo por código.
- **No hay multijugador en línea.** El cooperativo en línea (idea 84) necesitaría un servidor de juego en tiempo real; Supabase no lo sustituye. Está marcado como no hecho en [ROADMAP.md](ROADMAP.md).

## Twitch

No necesita configuración ni claves: Extras → Twitch → nombre del canal → Conectar. Entra al chat como invitado anónimo (solo lectura) y cuenta votos `!izquierda` / `!derecha` (o `<` / `>`) cada 3 segundos. Implementación: `TwitchVotes.cs` y `Assets/Plugins/WebGL/Twitch.jslib`. Es independiente de las cuentas de Twitch del autor.

## GitHub

- **Repositorio:** `github.com/0scar07/game-off-2026`, rama `fase`. Antes del 1 de diciembre debe ser público (ver [ROADMAP.md](ROADMAP.md)).
- **Workflows** (`.github/workflows`):
  - `niveles.yml`: en cada cambio de `Assets/Levels/**`, `Tools/niveles.py` o `Tools/comprobar_niveles.py`, ejecuta `Tools/comprobar_niveles.py` (todos los niveles se pueden terminar).
  - `itch-web.yml`: al publicar una release `web-AAAAMMDD-N`, prueba la web en Chrome y la sube a itch.
- **Plantillas de issue** (`.github/ISSUE_TEMPLATE`): fallo, idea, prueba de juego y nivel de la comunidad. «Tu opinión» (final del juego) abre `prueba.yml`.
- **Nivel de la semana:** editar `semana/nivel.txt` en GitHub (3 líneas: `nombre | name`, autor, código `FASE-…`). El juego lo descarga al abrir el panel, sin publicar versión nueva.
- **Secretos y variables de Actions:** `BUTLER_API_KEY`, `ITCH_USER`, `ITCH_GAME`. Nunca van en el código.

## Compilaciones

Resumen: menús **Fase → Exportar web / escritorio (Windows) / Android (APK)**; salidas en `Builds/`. Detalle, memoria necesaria, firma del APK y publicación en [BUILDING.md](BUILDING.md).

## itch.io

- Página: `https://0scar07.itch.io/game-off-2026` (la URL que usa el juego al compartir un tiempo).
- Solo el canal **html5** se sube automáticamente. Windows, Linux y Android (ideas 7 y 14) siguen pendientes; hay que quitar las descargas de octubre, que son de otra versión.
- `butler` no está instalado en este PC: la subida la hace GitHub Actions.
- Pendiente: página cuidada (capturas, GIF, descripción y controles), créditos y release v1.0.

## Comandos importantes

| Para qué | Comando o menú |
|---|---|
| Generar escenas tras tocar niveles | Unity → **Fase → Generar niveles** |
| Comprobar que los niveles se pueden terminar | `PYTHONIOENCODING=utf-8 python Tools/comprobar_niveles.py` |
| Escribir los niveles de nuevo | `python Tools/niveles.py` y luego Generar niveles |
| Probar niveles con el bot | Unity → **Fase → Probar niveles jugando (bot)** |
| Desbloquearlo todo para probar | Unity → **Fase → Desbloquear todo (pruebas)** |
| Exportar | **Fase → Exportar web / escritorio (Windows) / Android (APK)** |
| Servir la web en local | `cd Builds/Web && python -m http.server 8000` |
| Prueba automática de la web | `cd Tools/prueba_web && CHROME=chrome node prueba.mjs http://localhost:8000/ capturas` |
| Publicar la web | `gh release create web-AAAAMMDD-N Builds/FASE-web.zip --target fase --title "FASE web AAAA-MM-DD"` |
| Traducciones que faltan | Jugar en portugués o francés y leer `Logs/sin-traducir.txt`; añadir con `python Tools/anadir_traducciones.py` |
| Nuevo nivel de la semana de ejemplo | `python Tools/nivel_semana.py` |
| Símbolos nuevos en la fuente | `python Tools/fuente_simbolos.py` |

Las herramientas de Python necesitan `numpy`, `scipy`, `Pillow` y `fontTools` (ver [Tools/README.md](Tools/README.md)).

## Próximos pasos

1. Jugar todos los niveles de principio a fin y arreglar lo que falle ([docs/PULIDO.md](docs/PULIDO.md)).
2. Pasar la prueba de juego a un amigo y ajustar tiempos de medalla (idea 6).
3. Probar la versión web en un móvil real (táctil, rendimiento, giro de pantalla).
4. Ver el final secreto y «dejar el mundo partido».
5. Créditos tipo película (idea 13), página de itch (14) y nombres de quienes probaron el juego (127).
6. Exportar web, Windows y Android finales; crear un keystore propio para el APK si se va a actualizar.
7. Release v1.0, repositorio público, imagen social y GitHub Pages.
8. Tráiler nuevo, campaña y lanzamiento (92 y 142).
9. Antes de la entrega: comprobar que Supabase no se ha pausado (abrir el panel o jugar un nivel).

## Registro de esta sesión

<!-- El coordinador irá rellenando esta sección. -->
