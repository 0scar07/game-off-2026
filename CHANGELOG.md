# Historial de cambios

Lo más importante de cada día. El detalle está en el historial de commits.

## 16 de noviembre de 2026 · Todo en línea y documentación

- **Récords en línea y analítica anónima** encendidos: proyecto Supabase «fase» con las tablas
  creadas y probadas (top 10 por nivel; nadie puede borrar ni leer los eventos).
- **Nivel de la semana:** el juego lo descarga del repositorio (`semana/nivel.txt`) y lo juega en el
  Taller sin pisar el nivel propio; botón para enviar el tuyo.
- **Encuesta «Tu opinión»** al terminar el juego (abre la plantilla de prueba de juego en GitHub).
- **Twitch:** el chat de un canal vota hacia dónde va la línea (anónimo, sin cuenta; web y escritorio).
- **Bocadillos** de Lumi y Chispa que ya no se salen de la pantalla; en resultados ya no asoma la
  pista de la luciérnaga.
- **README al máximo** (30 segundos, streamers, comunidad, privacidad, preguntas), `CONTRIBUTING.md`
  nuevo y «Para seguir sin Claude» en `docs/PULIDO.md`.
- Guías nuevas: `BUILDING.md`, `ARCHITECTURE.md`, `ROADMAP.md` y `CONTINUAR_DESARROLLO.md`.

## 10 de noviembre de 2026 · Pulido y tanda G

- **Tanda G:** más contraste en la Cumbre, noche propia en cada bioma (setas, árboles oscuros,
  edificios, corales y burbujas), niveles extra repartidos entre los cinco biomas, HUD nuevo
  (número en círculo, lunas como iconos, pista en tarjeta), Taller con paleta dibujada, barra del
  menú más grande, chispas al mover la línea, agua con olas, hielo con brillo y sombra bajo Lumi.
- **Primer bot de pruebas** (Fase → Probar niveles jugando); ya salta los pinchos.
- **Pulido, día 1:** inventario en `docs/PULIDO.md`, música y voz medidas (sin cortes); los niveles
  secretos y extra llevan ★ en vez de un número de la historia.
- **Menús sin textos pisados** en ningún panel ni idioma; botones y fondo de Opciones redondeados.
- **Portugués y francés completos** (~400 textos), registro de lo que falte por traducir en el
  editor, «¡Un copo!» y tienda que cambia de idioma.
- **Interfaz que cabe en 4:3**, tabletas y móvil, y aviso de girar el móvil en vertical.
- **Resultados limpios** y **final** con luna redonda en el cielo.
- **Web:** exportación con menos código C++ (`OptimizeSize`) para que compile con menos memoria;
  exportada y probada (25,7 MB, sin errores) y **subida a itch** (`web-20261110-1`).
- Revisión general: textos del nivel que se pisaban, halo de ocaso más suave y cinco fallos
  conocidos arreglados.

## 9 de noviembre de 2026 · Todas las ideas, por tandas

- **Tanda A:** botones que suenan y se inclinan, cursor dorado, Lumi saluda y habla en el menú, tu
  nombre, racha del desafío, pantalla de carga con récord, pausa en gris, farol que tintinea,
  estornudo en la nieve y dibujo propio para cada logro.
- **Tanda B:** búhos de noche, pétalos, línea de color por capítulo, Lumi tiembla y se ríe, rastro
  de luz, anillo al saltar, cámara lenta «justo a tiempo», eco bajo techo y mensajes grabados.
- **Tanda C:** Extras (estadísticas, medallas de estilo, banda sonora, galería, álbum, mapa de
  capítulos, retos de la semana), plumas por nivel, 6 logros de exploración y tarjeta para compartir.
- **Tanda D:** portugués y francés, pista de la línea, modo relajado, velocidad al 80 %, un toque,
  botones táctiles movibles, iconos de mando, vibración por mundo, narrador y nivel de trucos.
- **Tanda E:** llaves y puertas, caja que se lleva, nivel a oscuras, sombra que persigue la línea,
  plataformas al ritmo, línea con peso y curva, marea, viento, espejos, estaciones y 11 niveles extra.
- **Tanda F:** retos (contrarreloj, condiciones, mutadores), carrera a dos y fantasma de un amigo,
  habilidades, Chispa, decisión y final secreto, jefe por fases, biomas ciudad y mar con 6 niveles
  extra y Lumi en el mapa.
- **Semana 2:** mantener R vuelve al punto de control, ánimo tras caídas, Lumi se duerme, estrellas
  fugaces, lunas que faltan en el mapa de pausa y transición con la línea.
- **Niveles:** muros de hielo en el nivel 2, tablas que se atraviesan desde abajo, pistas cortadas
  y un aviso del generador si algo se sale del mapa.
- **Web:** la prueba automática usa Google Chrome y se ejecuta antes de subir a itch; formas sin
  colisionador (quita errores en la web).
- Maquetas de cada tanda y lista de recortes hasta la entrega (`docs/RECORTES.md`).

## 3 de noviembre de 2026 · Pulido y repositorio

- **Menú reorganizado:** logo y *Continuar* a la izquierda (con el nivel por el que vas y la luna
  reunida), barra de iconos abajo y Lumi paseando a la derecha.
- **Fondo del menú y del final** con suelo continuo; las piezas de día y de noche flotan encima.
- **Fuente *Fredoka Fase*:** añade los símbolos de la interfaz (★ ☾ ▶ ✔ ✖ ⏱ ∞ …), que en la web
  salían como cuadros vacíos.
- **Orden de los niveles** corregido (los nombres con letra, como `04b`, iban antes que `04-`).
- *Relojes de sol* y algunos desafíos diarios subían por encima del mapa: corregido.
- **Comprobador de niveles** (`Tools/comprobar_niveles.py`) que entiende todas las mecánicas;
  se ejecuta en GitHub en cada cambio de los niveles.
- Repositorio: README con capturas, documento de diseño, licencia MIT, plantillas de issues y PR.

## 2 de noviembre de 2026 · Contenido y sistemas

- **Medallas** (Bronce, Plata, Oro, Luna), **22 logros**, desafíos por nivel, estadísticas, confeti.
- **Armario de Lumi** (bufandas y faroles), **Nox**, **tienda** de cosméticos.
- **Accesibilidad:** modo asistido, alto contraste, teclas configurables, velocidad de la línea, vibración.
- **Mecánicas:** segunda línea, línea inclinada y tumbada, relojes de sol, corrientes de aire,
  luciérnagas, energía de la línea, cajas que cambian, burbuja de noche del farol.
- **Jefes:** Garra del Alba, Reina de Escarcha y la Gran Sombra.
- **Modos:** desafío diario (14), Taller de niveles con códigos, sin fin, espejo, cooperativo local.
- **Historia:** diario de Lumi (15 páginas), dos finales con cinemática, narración de la intro.
- Pausa con mapa del nivel, modo foto, fantasma del récord, repeticiones.
- Efectos de sonido y voz de Lumi propios; música que reacciona a la línea y a los jefes.
- Huellas, pájaros que se vuelven murciélagos, clima, rayos de sol, transición de página.
- **Tráiler** de 50 s en español e inglés grabado dentro del motor.

## 1 de noviembre de 2026 · Nace FASE

- Proyecto en Unity 6 (URP): la línea, los shaders de recorte y las piezas de cada mundo.
- Arte de Quaternius y Kenney, tres biomas, cielo, posprocesado y partículas.
- Controles para teclado, mando y móvil; versión web con subida automática a itch.io.
- Música propia de día y de noche que se mezcla con la línea.
- Niveles en texto y generador de escenas; 11 niveles en 3 capítulos.
- Menú, mapa de viaje, pausa, final, intro con viñetas, español e inglés.
