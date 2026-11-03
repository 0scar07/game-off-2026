# Herramientas de FASE

Scripts de Python que generan contenido del juego. Necesitan Python 3.11+ con `numpy`,
`scipy`, `Pillow` y `fontTools` (`pip install numpy scipy pillow fonttools`); el montaje del
tráiler necesita además `ffmpeg`.

| Script | Qué hace | Salida |
|---|---|---|
| `niveles.py` | Dibuja todos los niveles encadenando tramos (puentes, lagos, muros, relojes de sol...), los secretos y los 14 desafíos diarios | `Assets/Levels/*.txt` |
| `comprobar_niveles.py` | Comprueba que cada nivel se puede terminar y que sus fragmentos, cofres y controles tienen camino | informe (falla si hay problemas) |
| `musica.py` · `musica_capitulos.py` | Música de cada capítulo en versión de día y de noche, y los ambientes | `Assets/Audio/Music` |
| `musica_extra.py` | Capas que reaccionan al juego: campanas de la línea y tambores de jefe | `Assets/Audio/Music` |
| `efectos.py` | Efectos de sonido y la vocecita de Lumi, en la tonalidad de la música | `Tools/efectos_prueba` (copiar a `Assets/Audio/Sfx/Fase`) |
| `fuente_simbolos.py` | *Fredoka Fase*: Fredoka con los símbolos de la interfaz (★ ☾ ▶ ✔ ...) | `Assets/Art/Fonts`, `Assets/Resources` |
| `musica_trailer.py` | Música del tráiler, con los tiempos de cada golpe | `Trailer/musica.wav` |
| `montar_trailer.py` | Monta el tráiler con los planos grabados, títulos y música (`es` o `en`) | `Trailer/FASE-trailer-*.mp4` |

Después de cambiar niveles o audio, en Unity: **Fase → Generar niveles**.
