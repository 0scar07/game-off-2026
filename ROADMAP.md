# Hoja de ruta de FASE

Resumen de lo hecho, lo que está en curso y lo pendiente. No repite las 142 ideas: el detalle está en [docs/PENDIENTES.md](docs/PENDIENTES.md) (ideas numeradas), [docs/TANDAS.md](docs/TANDAS.md) (en qué tanda se programó cada una), [docs/PULIDO.md](docs/PULIDO.md) (qué está probado) y [docs/RECORTES.md](docs/RECORTES.md) (plazos de la jam).

**Estados:** ✅ hecho · 🔧 programado, falta probarlo jugando · ⬜ pendiente · 🙋 lo tiene que hacer el autor

**Fecha de entrega:** la GitHub Game Off 2026 acaba el **1 de diciembre**. Congelación de contenido nuevo: **27 de noviembre**.

> Aviso: la tabla de `docs/PENDIENTES.md` no está al día. Muchas ideas de las tandas A-G aparecen allí con ⬜ aunque ya están programadas (según `docs/TANDAS.md` y el historial de commits). Esta hoja de ruta sigue esos dos últimos.

## Hecho ✅

| Área | Qué | Ideas |
|---|---|---|
| Núcleo | Línea día/noche, 3 capítulos (19 niveles de historia + 3 secretos), 3 jefes, medallas, logros, desafío diario, Taller, Sin fin | |
| Sensación | Pausas de impacto, Lumi expresiva, línea con vida, cámara cinematográfica, tutorial sin texto | 9.1-9.4, 5 |
| Rendimiento y guardado | Web comprimida (Brotli), guardado a prueba de fallos | 9, 10 |
| Sonido | Mezcla final, acorde de capítulo, música interactiva, banda sonora en el menú | 11, 19, 40, 78 |
| Presentación | Inicio con logo partido, celebración de capítulo, mapa con vista previa, curiosidades | 12, 20, 22, 23 |
| Accesibilidad | Texto grande, menos destellos, modo relajado, un toque, iconos sol/luna, brillo, velocidad al 80 %, controles táctiles movibles | 21, 41, 42, 57, 71, 130, 137 |
| Idiomas | Español, inglés, portugués y francés completos (~400 textos añadidos el 10 de nov.) | 88 |
| Comunidad | Plantillas de issue (fallo, idea, prueba, nivel), Discussions, kit de prensa, CONTRIBUTING | 4, 5, 6 |
| Tandas A-F | Pulido rápido, mundo y Lumi, menú y extras, ayudas, mecánicas nuevas con niveles extra (Extras → Niveles extra), modos y proyectos grandes (retos, carrera a dos, habilidades, Chispa, decisión y final secreto, biomas ciudad y mar) | ver TANDAS.md |
| Tanda G | Contraste, noche por bioma, HUD nuevo, Taller con paleta, barra del menú, chispas de la línea, agua con olas y hielo | G1-G9 |
| En línea (16 nov.) | Nivel de la semana, encuesta «Tu opinión», Twitch (chat anónimo vota la línea), récords en línea y analítica anónima con Supabase | 43, 62, 120, 80, 140 |
| Web | Exportación estable (con el PC libre), prueba automática en GitHub, subida a itch (`web-20261110-1`) | |

Detalle día a día en [CHANGELOG.md](CHANGELOG.md).

## En curso 🔧

Todo lo programado que aún necesita verse funcionando (ver el inventario de [docs/PULIDO.md](docs/PULIDO.md)):

- **Jugar los niveles de principio a fin** y revisar cámara, pistas y lunas: capítulos I-III, secretos, extra (Pradera, Bosque, Cumbre, Ciudad, Mar) y los 14 desafíos diarios.
- **Probar mecánicas y personajes** en movimiento (líneas especiales, piezas, Sombras, abejas, jefes, fauna).
- **Recompensas**: lunas, medallas de tiempo (¿son alcanzables?), medallas de estilo, los 30 logros, plumas, estrellas de Chispa, cosméticos, habilidades.
- **Sonido**: escuchar el paso de día a noche, que la música no se reinicie al morir, efectos y la voz de la intro.
- **Final secreto** y «dejar el mundo partido»: comprobados en código; falta verlos jugando.
- **Táctil y móvil real**: controles, giro de pantalla, rendimiento (idea 8).
- **Bot de pruebas** (G10): mueve la línea y salta, pero no resuelve niveles enteros; las rutas las comprueba `Tools/comprobar_niveles.py`.

## Pendiente ⬜

### Antes de enviar el juego

| Qué | Nota |
|---|---|
| 6 · Curva de dificultad y tiempos de medalla 🙋 | Con partidas del autor y de un amigo ([prueba de juego](.github/ISSUE_TEMPLATE/prueba.yml)) |
| 13 · Créditos tipo película y 127 · nombres de quienes probaron el juego | Para el final; faltan los nombres |
| 14 · Página de itch cuidada | Capturas, GIF, descripción y controles |
| Exportar web, Windows y Android y subirlos; quitar las descargas antiguas de itch | Ver [BUILDING.md](BUILDING.md). El APK sigue firmado con la clave de depuración |
| Release v1.0 con notas y tráiler (1) | Falta publicar la primera versión |
| Repositorio público (antes del 1 de dic.), imagen para compartir (3) y GitHub Pages (2) 🙋 | La imagen está lista en `docs/prensa/redes-1280x640.jpg`; se sube a mano |
| 7 · Subida automática de Windows y Linux a itch | Con las exportaciones finales |

### Lo último: tráiler, campaña y lanzamiento

| # | Idea | Estado |
|---|---|---|
| 92 | Tráiler de lanzamiento nuevo con todo lo añadido y campaña en redes | ⬜ lo último antes de enviar |
| 142 | Lanzamiento completo: página de Steam, prensa y festival de juegos indie | ⬜ lo último antes de enviar; no es programación |

El tráiler actual (50 s, es/en) está grabado dentro del motor (`TrailerDirector`, `Tools/montar_trailer.py`); el nuevo debe incluir lo añadido después.

### No hecho

| # | Idea | Por qué |
|---|---|---|
| 84 | **Cooperativo en línea** (uno mueve a Lumi y otro la línea desde otro PC) | **No está hecho.** Necesita un servidor de juego en tiempo real; Supabase (base de datos con REST) no lo sustituye. Solo existe cooperativo **local** |
| 8 y 8 (CI) | Rendimiento en móvil a 60 fps · compilación automática en GitHub (GameCI) | 🙋 probar en un móvil · GameCI necesita la licencia de Unity como secreto |
| 87, 90, 119, 121 | Google Play, Steam, Switch, Steam Workshop | Cuentas y procesos de cada tienda; el APK necesitaría un keystore propio |
| 122 | Expansión «FASE: Eclipse» | Es otro juego |
| 141 | Creador de niveles con IA | Necesita una clave de API de pago |

### Después de la jam

Orden de impacto según [docs/RECORTES.md](docs/RECORTES.md): capítulo nuevo (82, 139), compartir niveles con enlace (81, ya hay `#nivel=`), versión de Google Play (87), más idiomas como el japonés (necesita otra fuente) y Steam (90).

## Dónde se decide cada cosa

| Pregunta | Documento |
|---|---|
| ¿Qué idea es la número N? | [docs/PENDIENTES.md](docs/PENDIENTES.md) |
| ¿En qué tanda entró? | [docs/TANDAS.md](docs/TANDAS.md) |
| ¿Está probada? | [docs/PULIDO.md](docs/PULIDO.md) |
| ¿Qué se recorta para la jam? | [docs/RECORTES.md](docs/RECORTES.md) |
| ¿Cómo sigo yo solo? | [CONTINUAR_DESARROLLO.md](CONTINUAR_DESARROLLO.md) |
