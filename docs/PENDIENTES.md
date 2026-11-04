# Hoja de ruta

Ideas aprobadas para FASE y su estado. Las maquetas de referencia están en [`maquetas/`](maquetas)
(se pueden repintar con [`maquetas.html`](maquetas/maquetas.html)`?s=N`).

✅ hecho · 🟡 a medias · ⬜ pendiente · 🙋 necesita al autor

## Repositorio

| # | Idea | Estado |
|---|---|---|
| 1 | Releases con versión y notas (v1.0 para la jam) e insignias de versión y descargas | 🟡 insignias y tabla de descargas listas; falta publicar la primera versión |
| 2 | Jugar en GitHub Pages además de itch | ⬜ cuando el repo sea público |
| 3 | Imagen para compartir 1280×640 | 🙋 lista en [`prensa/redes-1280x640.jpg`](prensa/redes-1280x640.jpg); se sube a mano en *Settings → Social preview* |
| 4 | Kit de prensa: logo, capturas, descripciones y tráiler | ✅ [`prensa/`](prensa/README.md) (el tráiler irá en la release) |
| 5 | Niveles de la comunidad (plantilla de issue con el código del Taller) | ✅ |
| 6 | Discussions activadas | ✅ |
| 7 | Subida automática a itch también de Windows y Linux | ⬜ con las exportaciones finales |
| 8 | Compilación automática en GitHub (GameCI) | ⬜ 🙋 necesita la licencia de Unity como secreto |

## Sensación al jugar

| # | Idea | Estado | Maqueta |
|---|---|---|---|
| 9.1 | Pausas de 2-3 fotogramas al coger luna, golpear al jefe o aterrizar fuerte |✅ | [lumi-expresiva](maquetas/lumi-expresiva.png) |
| 9.2 | Lumi expresiva: se estira, se aplasta, polvo al frenar, se ladea al girar |✅ | [lumi-expresiva](maquetas/lumi-expresiva.png) |
| 9.3 | La línea con vida: vibra al moverla rápido, chispea al cruzar objetos, sonido más grave con la velocidad |✅ | |
| 9.4 | Cámara cinematográfica: mira hacia la acción y se aleja en los momentos grandes |✅ | |

## Primeros minutos

| # | Idea | Estado | Maqueta |
|---|---|---|---|
| 5 | Tutorial sin texto en el nivel 1 (flecha, mano fantasma, puente que aparece) |✅ | [tutorial-nivel-1](maquetas/tutorial-nivel-1.png) |
| 6 | Curva de dificultad y tiempos de medalla | ⬜ 🙋 con tus partidas | |
| 7 | Ritmo de los niveles: recortar tramos vacíos y poner respiros | ✅ revisado (ningún tramo vacío pasa de 15 columnas); el comprobador avisa si aparece uno | |

## Que funcione en cualquier sitio

| # | Idea | Estado |
|---|---|---|
| 8 | Rendimiento en móvil a 60 fps | ⬜ 🙋 probar en un móvil |
| 9 | Carga más rápida de la web (comprimir música y texturas) | ✅ Brotli, código del motor recortado, efectos y voz en mono comprimidos, música a calidad 0,45 (el ahorro se mide en la próxima exportación) |
| 10 | Guardado a prueba de fallos | ✅ se guarda cada 10 s, al cambiar de pestaña y al cerrar; se sigue desde el último punto de control |

## Sonido y presentación

| # | Idea | Estado | Maqueta |
|---|---|---|---|
| 11 | Mezcla de sonido final | ✅ todas las pistas al mismo volumen (`Tools/mezcla.py`), la música baja un poco al coger luna | |
| 12 | Pantalla de inicio con el logo partido por la línea | ✅ | [pantalla-de-inicio](maquetas/pantalla-de-inicio.png) |
| 13 | Créditos tipo película | ⬜ (para el final) | [creditos](maquetas/creditos.png) |
| 14 | Página de itch cuidada | ⬜ (para el final) | |

## Más ideas

| # | Idea | Estado | Maqueta |
|---|---|---|---|
| 16 | El mundo reacciona a la línea: hierba que se dobla, hojas, charcos que se congelan | ✅ plantas que se doblan, hojas al arrastrarla y charcos de agua/hielo | [idea-16](maquetas/idea-16.png) |
| 17 | Lumi mira hacia lo importante (fragmentos cercanos, la línea al moverla) | ✅ | [idea-17](maquetas/idea-17.png) |
| 18 | Al agarrar la línea se iluminan las piezas que van a cambiar | ✅ contorno dorado en las piezas al alcance | [idea-18](maquetas/idea-18.png) |
| 19 | La música resuelve en un acorde final en la meta | ✅ un acorde por capítulo en la tonalidad de su canción | |
| 20 | Celebración de capítulo: Lumi coloca su fragmento en la luna | ✅ (prueba: Fase → Probar celebración de capítulo) | [idea-20](maquetas/idea-20.png) |
| 21 | Accesibilidad: tamaño de texto y reducir destellos y temblores | ✅ tamaño de texto en vivo y "Menos destellos" | [idea-21](maquetas/idea-21.png) |
| 22 | Vista previa del nivel en el mapa de viaje | ✅ plano del nivel recorriéndose, con medallas y récord | [idea-22](maquetas/idea-22.png) |
| 23 | Pantalla "¿Sabías qué?" con curiosidades que se desbloquean | ✅ 12 curiosidades en el menú | [idea-23](maquetas/idea-23.png) |

## Al final

- Exportar web, Windows y Android y subirlos (itch y release v1.0).
- Hacer público el repositorio antes del 1 de diciembre.
