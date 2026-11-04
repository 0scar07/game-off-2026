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

## Ideas nuevas (24-42)

Guardadas para más adelante. Como siempre: primero maqueta, luego Unity.

| # | Idea | Estado |
|---|---|---|
| 24 | Piezas de atardecer: solo existen en una franja estrecha justo encima de la línea | ⬜ |
| 25 | Espejos de luz: un rayo de sol rebota y crea una franja de día lejos de la línea | ⬜ |
| 26 | Línea con peso: en algunos niveles cae despacio como un péndulo y hay que sujetarla | ⬜ |
| 27 | Niveles de persecución: la noche avanza sola por detrás | ⬜ |
| 28 | Interruptores de mundo: palancas que cambian qué lado es día y cuál noche unos segundos | ⬜ |
| 29 | Animales que cambian: pájaros de día que de noche son búhos o luciérnagas | ⬜ |
| 30 | Lumi bosteza: si estás quieto un rato se sienta, bosteza y se duerme con el farol apagado | ⬜ |
| 31 | Huellas de Lumi en la nieve que se borran o se congelan según el lado de la línea | ⬜ |
| 32 | Personaje compañero con frases cortas y pequeñas misiones secundarias | ⬜ |
| 33 | Contrarreloj por capítulo: todos sus niveles seguidos con un cronómetro y ranking local | ⬜ |
| 34 | Desafíos con condiciones ("sin mover la línea a la izquierda", "solo 5 agarres"...) | ⬜ |
| 35 | Fantasma de tu mejor partida desde el mapa de viaje | ⬜ |
| 36 | Mutadores tras terminar el juego: gravedad baja, línea invertida, Lumi gigante... | ⬜ |
| 37 | Transición entre niveles con la línea barriendo la pantalla | ⬜ |
| 38 | Narrador de cuento: frases cortas al empezar cada capítulo | ⬜ |
| 39 | Álbum de fotos del modo foto dentro del menú | ⬜ |
| 40 | Música interactiva: cada fragmento recogido añade un instrumento | ⬜ |
| 41 | Modo daltónico con formas: las piezas de noche llevan un icono además del color | ⬜ |
| 42 | Móvil: pausa con un botón y controles táctiles que se pueden mover y cambiar de tamaño | ⬜ |

## Más ideas (43-62)

| # | Idea | Estado |
|---|---|---|
| 43 | Niveles de la semana: un nivel del Taller elegido, cargado desde el repositorio de GitHub | ⬜ |
| 44 | Plataformas que crecen: de día brotan como plantas y de noche se recogen poco a poco | ⬜ |
| 45 | Agua que sube de noche (marea) y baja de día: la línea controla el nivel del agua | ⬜ |
| 46 | Viento solo de día que empuja a Lumi; de noche, calma | ⬜ |
| 47 | Sombras sólidas: de noche la sombra de un objeto se vuelve una plataforma | ⬜ |
| 48 | Llaves y puertas de dos colores: la llave de día solo abre puertas de día | ⬜ |
| 49 | Nivel a oscuras: solo se ve lo que ilumina el farol de Lumi | ⬜ |
| 50 | Lumi ríe al caer de muy alto en agua, tiembla en la nieve y se sacude la lluvia | ⬜ |
| 51 | Estaciones dentro de un nivel: la línea cambia también de otoño a invierno | ⬜ |
| 52 | Pequeños jefes por capítulo antes de la Gran Sombra | ⬜ |
| 53 | Medallas de estilo: terminar sin tocar el suelo de noche, cruzar la línea 50 veces... | ⬜ |
| 54 | Repetición del nivel al terminar (cámara rápida de tu partida) para compartir | ⬜ |
| 55 | Pantalla de estadísticas: tiempo total, caídas, kilómetros recorridos, veces que moviste la línea | ⬜ |
| 56 | Pistas visuales opcionales: una línea fantasma muestra dónde conviene ponerla | ⬜ |
| 57 | Modo historia relajado: sin caídas al vacío, Lumi flota de vuelta a la plataforma | ⬜ |
| 58 | Tutorial de mando: iconos de botones según el mando conectado (Xbox, PlayStation) | ⬜ |
| 59 | Vibración del mando distinta para día y noche | ⬜ |
| 60 | Título del nivel escrito a mano que aparece trazado por la línea | ⬜ |
| 61 | Final secreto si se reúnen todas las lunas y todas las páginas del diario | ⬜ |
| 62 | Encuesta corta al terminar el juego (qué te gustó, qué cambiarías) que abre un issue en GitHub | ⬜ |

## Ideas por impacto (63-92), de menor a mayor

| # | Idea | Impacto | Estado |
|---|---|---|---|
| 63 | Cursor propio con forma de línea dorada en el menú | Bajo | ⬜ |
| 64 | Sonido distinto al pasar el ratón por cada botón del menú | Bajo | ⬜ |
| 65 | Frase aleatoria de Lumi en el menú ("¿Seguimos?", "Hoy hay luna llena") | Bajo | ⬜ |
| 66 | Contador de días seguidos jugando el desafío diario | Bajo | ⬜ |
| 67 | Pantalla de carga con datos del nivel que viene (récord, lunas que faltan) | Bajo | ⬜ |
| 68 | El menú cambia de hora real: amanecer, tarde o noche según el reloj del PC | Bajo-medio | ⬜ |
| 69 | Logros con dibujo propio en vez de estrella genérica | Bajo-medio | ⬜ |
| 70 | Tarjeta para compartir al terminar el juego (tiempo total, lunas, medallas) | Bajo-medio | ⬜ |
| 71 | Ajuste de velocidad del juego (80 %, 100 %) para quien va con calma | Medio | ⬜ |
| 72 | Puntos de control que se ven en el mapa de pausa | Medio | ⬜ |
| 73 | Lumi deja un rastro de luz al correr rápido | Medio | ⬜ |
| 74 | Cámara lenta automática al cruzar la línea justo a tiempo en un salto | Medio | ⬜ |
| 75 | Nivel tutorial opcional de "trucos avanzados" (dos líneas, burbuja del farol) | Medio | ⬜ |
| 76 | Cada capítulo con un color de línea propio (dorada, naranja, azul hielo) | Medio | ⬜ |
| 77 | Escenas cortas entre capítulos con dibujos y texto (estilo cuento) | Medio-alto | ⬜ |
| 78 | Banda sonora que se puede escuchar en el menú (reproductor de pistas) | Medio-alto | ⬜ |
| 79 | Modo "speedrun" con cronómetro por tramos y comparación con tu récord | Medio-alto | ⬜ |
| 80 | Tabla de récords en línea por nivel (con nombre corto) | Medio-alto | ⬜ |
| 81 | Editor de niveles con compartir por enlace (el código va en la URL) | Alto | ⬜ |
| 82 | Capítulo 4: un mundo nuevo (ciudad de noche, cielo de auroras) | Alto | ⬜ |
| 83 | Mecánica de tercera fase: el atardecer como mundo propio entre día y noche | Alto | ⬜ |
| 84 | Cooperativo en línea: uno mueve a Lumi y el otro la línea desde otro PC | Alto | ⬜ |
| 85 | Voces y sonidos de Lumi (risas, "¡hop!", suspiros) grabados o generados | Alto | ⬜ |
| 86 | Jefe final con varias fases que usa la línea contra ti | Alto | ⬜ |
| 87 | Versión de móvil publicada en Google Play | Muy alto | ⬜ |
| 88 | Traducciones a más idiomas (portugués, francés, japonés) | Muy alto | ⬜ |
| 89 | Modo historia ampliado: 40 niveles, con mapa del mundo explorable | Muy alto | ⬜ |
| 90 | Versión en Steam con logros de Steam y tarjetas | Muy alto | ⬜ |
| 91 | Generador de niveles infinitos con dificultad que se adapta a cómo juegas | Muy alto | ⬜ |
| 92 | Tráiler de lanzamiento nuevo con todo lo añadido y una campaña en redes | Muy alto | ⬜ |

## Más ideas por impacto (93-122), de menor a mayor

| # | Idea | Impacto | Estado |
|---|---|---|---|
| 93 | Lumi saluda con la mano cuando vuelves al menú tras un rato fuera | Bajo | ⬜ |
| 94 | Estrellas fugaces de vez en cuando en el cielo de noche | Bajo | ⬜ |
| 95 | El farol de Lumi tintinea al caminar | Bajo | ⬜ |
| 96 | Al pausar, el mundo se queda en blanco y negro salvo la línea | Bajo | ⬜ |
| 97 | Mensaje de ánimo tras muchas caídas seguidas ("¡Casi!") | Bajo | ⬜ |
| 98 | Atajo para reiniciar desde el último punto de control (mantener R) | Bajo-medio | ⬜ |
| 99 | Notas musicales al recoger fragmentos que forman una melodía en el nivel | Bajo-medio | ⬜ |
| 100 | Nombre del jugador en el menú ("¡Hola, Oscar!") | Bajo-medio | ⬜ |
| 101 | Partículas de pétalos al florecer las plantas cuando pasa el día | Bajo-medio | ⬜ |
| 102 | Saltos con un pequeño eco visual (anillo de luz en el suelo) | Medio | ⬜ |
| 103 | Coleccionables de capítulo: una pluma, una hoja, un copo de nieve | Medio | ⬜ |
| 104 | Mapa de pausa con las lunas que faltan marcadas | Medio | ⬜ |
| 105 | Niveles con dos caminos (fácil y difícil) que se juntan al final | Medio | ⬜ |
| 106 | Plataformas que solo existen mientras Lumi está sobre la línea | Medio | ⬜ |
| 107 | Clima que cambia con la línea: lluvia de día y estrellas fugaces de noche | Medio | ⬜ |
| 108 | Logros de exploración: tocar todos los bordes del mapa, quedarse quieto un minuto | Medio-alto | ⬜ |
| 109 | Modo galería: modelos de Lumi, Nox y los enemigos que se pueden girar | Medio-alto | ⬜ |
| 110 | Enemigo nuevo que copia la posición de la línea (la sigue o la evita) | Medio-alto | ⬜ |
| 111 | Nivel musical: plataformas y peligros al ritmo de la canción | Medio-alto | ⬜ |
| 112 | Puzles de luz: reflejar la luz del farol para abrir caminos | Alto | ⬜ |
| 113 | Línea curva en niveles especiales (arco en lugar de recta) | Alto | ⬜ |
| 114 | Lumi puede llevar objetos (caja, flor) del día a la noche | Alto | ⬜ |
| 115 | Mundo abierto pequeño entre capítulos que conecta los niveles | Alto | ⬜ |
| 116 | Modo carrera a pantalla partida para dos jugadores | Alto | ⬜ |
| 117 | Sistema de habilidades que se desbloquean (doble salto, planear con el farol) | Alto | ⬜ |
| 118 | Historia con decisiones: ayudar a la noche o al día cambia el final | Muy alto | ⬜ |
| 119 | Versión para consolas (Switch) con controles adaptados | Muy alto | ⬜ |
| 120 | Integración con Twitch: el público mueve la línea por votación | Muy alto | ⬜ |
| 121 | Herramientas para la comunidad: editor de niveles completo y Steam Workshop | Muy alto | ⬜ |
| 122 | Secuela o expansión: "FASE: Eclipse", con una tercera línea | Muy alto | ⬜ |

## Últimas ideas (123-142), de menor a mayor impacto

| # | Idea | Impacto | Estado |
|---|---|---|---|
| 123 | Lumi estornuda al entrar en la nieve | Bajo | ⬜ |
| 124 | Los botones del menú se inclinan un poco hacia el ratón | Bajo | ⬜ |
| 125 | Reloj de arena en la pausa que muestra cuánto llevas jugando hoy | Bajo | ⬜ |
| 126 | Nubes que tapan el sol y oscurecen un poco el lado de día | Bajo | ⬜ |
| 127 | Créditos con los nombres de quienes probaron el juego | Bajo-medio | ⬜ |
| 128 | Sonido ambiente que cambia al entrar en cuevas o zonas cerradas | Bajo-medio | ⬜ |
| 129 | Mensajes escondidos en el decorado (carteles, piedras grabadas) | Bajo-medio | ⬜ |
| 130 | Ajuste de brillo en Opciones | Medio | ⬜ |
| 131 | Retos semanales con premio de ropa para Lumi | Medio | ⬜ |
| 132 | Lumi recuerda dónde cayó más veces y deja una flor en ese sitio | Medio | ⬜ |
| 133 | Mapa del capítulo que se va pintando a medida que avanzas | Medio | ⬜ |
| 134 | Puertas que solo se abren si la línea está en un ángulo concreto | Medio-alto | ⬜ |
| 135 | Niveles verticales de escalada con la línea tumbada | Medio-alto | ⬜ |
| 136 | Ghost de amigos: cargar la partida de otro con un código | Medio-alto | ⬜ |
| 137 | Modo de un solo toque para móvil (la línea sigue a Lumi sola, tú solo saltas) | Alto | ⬜ |
| 138 | Iluminación en tiempo real con sombras largas al atardecer | Alto | ⬜ |
| 139 | Capítulo submarino: de día agua clara, de noche bioluminiscencia | Alto | ⬜ |
| 140 | Analítica anónima: dónde se atasca la gente, para ajustar niveles | Muy alto | ⬜ |
| 141 | Modo creador con IA: describes un nivel y se genera una propuesta | Muy alto | ⬜ |
| 142 | Lanzamiento completo: página de Steam, prensa y festival de juegos indie | Muy alto | ⬜ |

## Al final

- Exportar web, Windows y Android y subirlos (itch y release v1.0).
- Hacer público el repositorio antes del 1 de diciembre.
