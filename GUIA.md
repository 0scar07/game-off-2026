# Guía para el Game Off 2026

Todo lo necesario para hacer el juego en noviembre sin depender de nadie. Léela entera una vez y después úsala como consulta.

**Fechas clave** (hora de España peninsular):

| Cuándo | Qué |
|---|---|
| **Domingo 1 nov, 22:37** | Se anuncia el tema y se abren las entregas |
| **Domingo 29 nov** | **Tu fecha de entrega.** Dos días de margen por si algo falla |
| **Martes 1 dic, 22:37** | Cierre de entregas (13:37 PST). El repo tiene que estar subido y ser público |
| **Hasta el 1 ene 2027** | Votación entre participantes |

Página del jam: <https://itch.io/jam/game-off-2026>

---

## Índice

1. [Primeros pasos con la plantilla](#1-primeros-pasos-con-la-plantilla)
2. [Aprender Godot viniendo de JavaScript](#2-aprender-godot-viniendo-de-javascript)
3. [Cómo usar cada pieza de la plantilla](#3-cómo-usar-cada-pieza-de-la-plantilla)
4. [El día del tema: de cero a idea elegida](#4-el-día-del-tema-de-cero-a-idea-elegida)
5. [Plan de noviembre, semana a semana](#5-plan-de-noviembre-semana-a-semana)
6. [Lista de calidad: lo que hace destacar un juego](#6-lista-de-calidad-lo-que-hace-destacar-un-juego)
7. [Arte, música y sonido: herramientas y assets con licencia](#7-arte-música-y-sonido-herramientas-y-assets-con-licencia)
8. [La página de itch.io](#8-la-página-de-itchio)
9. [Checklist de entrega](#9-checklist-de-entrega)
10. [La votación](#10-la-votación)
11. [Si algo se rompe](#11-si-algo-se-rompe)

---

## 1. Primeros pasos con la plantilla

### Abrir el proyecto

1. Abre **Godot 4.7.2** desde el menú Inicio.
2. Pulsa **Importar**, elige `project.godot` de esta carpeta y **Importar y editar**.
3. Pulsa **▶** (F5) para jugar. **F6** juega solo la escena que tienes abierta.

### Qué hay en cada carpeta

```
autoload/          Scripts globales, accesibles desde cualquier sitio por su nombre
  settings.gd        Settings: volumen, pantalla completa, ¿es móvil?
  audio.gd           Audio: música y efectos
  juice.gd           Juice: temblor, hitstop, partículas...
  sfx_generator.gd   Genera los sonidos de ejemplo por código
  ui_feedback.gd     Sonido y animación automáticos en todos los botones
  transition.tscn    Transition: cambios de escena con fundido
scenes/
  menus/             Menú principal, opciones, pausa
  game/              La demo (jugador, orbes). Aquí irá tu juego
  ui/                Controles táctiles
assets/
  theme.tres         Aspecto de botones, paneles y deslizadores
tests/               Prueba automática
.github/workflows/   Exportación automática a web, Windows y Android
```

### Flujo de trabajo diario

1. Trabaja en Godot.
2. Cuando algo funcione, haz un commit y un push (con GitHub Desktop, VS Code o la terminal):
   ```bash
   git add -A
   git commit -m "Qué he hecho"
   git push
   ```
3. En unos minutos tienes la versión nueva en la pestaña **Actions** del repo y, si configuraste itch, también en itch.
4. **Prueba la versión web en el móvil al menos una vez por semana.** Hay cosas que solo fallan allí.

### La prueba automática

```bash
godot --headless --path . res://tests/smoke_test.tscn
```

GitHub la ejecuta en cada push. **Cuando cambies la demo por tu juego, esta prueba fallará**, porque busca el jugador y los orbes de la demo. Tienes dos opciones:
- Adaptarla: cambia la parte de "Recoger un orbe" por algo de tu juego.
- Quitar las comprobaciones de la demo y dejar solo las de menús, ajustes y pausa.

---

## 2. Aprender Godot viniendo de JavaScript

### Ruta recomendada, de unas 10 horas en total

1. **[Tu primer juego 2D](https://docs.godotengine.org/es/stable/getting_started/first_2d_game/index.html)**, el tutorial oficial. Unas 3-4 horas. Hazlo entero: enseña nodos, escenas, señales y colisiones.
2. **[Learn GDScript From Zero](https://gdquest.github.io/learn-gdscript/)**, de GDQuest. Interactivo y en el navegador. Hazlo deprisa, porque ya sabes programar.
3. **Un minijuego tuyo en un fin de semana**, usando esta plantilla: un Breakout, un Flappy Bird o un "esquiva lo que cae". Ponle todo el juice que puedas. **Es el mejor entrenamiento para noviembre.**
4. Lee el código de la plantilla. Es corto y está comentado.

### Godot en 5 ideas

- **Todo es un árbol de nodos.** Un personaje es un `CharacterBody2D` con hijos `Sprite2D` y `CollisionShape2D`.
- **Una escena es un árbol de nodos guardado** (`.tscn`) que puedes instanciar dentro de otras, como un componente de React.
- **Las señales son eventos**, como `addEventListener`: `boton.pressed.connect(mi_funcion)`.
- **`_process(delta)`** se ejecuta en cada fotograma y **`_physics_process(delta)`** en cada paso de la física (60 por segundo). Movimiento y colisiones van en la de física.
- **Los autoloads son singletons globales**: `Audio`, `Juice`, `Settings`, `Transition`.

### GDScript para alguien de JavaScript

| JavaScript | GDScript |
|---|---|
| `let x = 5` | `var x := 5` (el `:=` deduce el tipo) |
| `const MAX = 10` | `const MAX := 10` |
| `function f(a, b) { }` | `func f(a: int, b: int) -> void:` |
| `{ ... }` | Sangría con tabuladores, como en Python |
| `if (a && !b) {} else if {}` | `if a and not b:` / `elif:` / `else:` |
| `for (let i = 0; i < 5; i++)` | `for i in 5:` |
| `for (const x of lista)` | `for x in lista:` |
| `[1, 2, 3]` / `{a: 1}` | `[1, 2, 3]` / `{"a": 1}` |
| `console.log(x)` | `print(x)` |
| `` `Hola ${n}` `` | `"Hola %s" % n` |
| `null` | `null` |
| `this.x` | `x` o `self.x` |
| `document.querySelector("#jugador")` | `$Jugador` o `get_node("Jugador")` |
| `async/await` | `await get_tree().create_timer(1.0).timeout` |
| `setTimeout(f, 1000)` | `get_tree().create_timer(1.0).timeout.connect(f)` |
| Animar con CSS transitions | `create_tween().tween_property(nodo, "position", destino, 0.3)` |

### Atajos útiles del editor

- **Ctrl+S**: guardar. **F5**: jugar. **F6**: jugar la escena actual. **F8**: parar.
- **Ctrl+A** en el árbol de la escena: añadir un nodo.
- **Ctrl+clic** sobre una función del código: ir a su definición.
- **F1**: buscar en la ayuda integrada. Toda la documentación de Godot está dentro del editor.
- Mientras juegas, la pestaña **Remoto** del árbol muestra los nodos en vivo y deja cambiar valores.

---

## 3. Cómo usar cada pieza de la plantilla

### Empezar tu juego

1. Crea `scenes/game/mi_juego.tscn` o modifica `game.tscn`.
2. Mantén dentro de la escena estos dos nodos de `game.tscn`:
   - **`PauseMenu`**: arrastra `scenes/menus/pause_menu.tscn`.
   - **`TouchControls`**: arrastra `scenes/ui/touch_controls.tscn`.
3. Añade una **`Camera2D`**. Sin ella no funcionan el temblor ni el empujón.
4. Si cambiaste el nombre de la escena, actualiza `GAME_SCENE` en `scenes/menus/main_menu.gd`.
5. Cambia el título del menú en `scenes/menus/main_menu.tscn` y el nombre en **Proyecto → Configuración → Aplicación → Nombre**.

### Controles

Usa siempre las acciones, nunca teclas concretas. Así funciona igual con teclado, mando y pantalla táctil:

```gdscript
var direccion := Input.get_vector("move_left", "move_right", "move_up", "move_down")
if Input.is_action_just_pressed("action"):    # Espacio, J, botón A del mando, botón A táctil
	saltar()
if Input.is_action_just_pressed("action_2"):  # K, X, botón X del mando, botón B táctil
	disparar()
```

- Puedes cambiar o añadir teclas en **Proyecto → Configuración → Mapa de entrada**.
- **Juego de plataformas** (solo izquierda y derecha): `Input.get_axis("move_left", "move_right")`. El joystick táctil sirve igual.
- **Juego de solo tocar la pantalla** (puzles, cartas): borra `TouchControls` de la escena y usa clics. En Godot, un toque en la pantalla también llega como clic.
- Para **probar los controles táctiles en el PC**, selecciona `TouchControls` en la escena y activa **Always Visible**. Se manejan con el ratón.

### Sonido

```gdscript
Audio.play("coin")      # click, hover, dash, jump, hit, coin, powerup, explosion, lose
Audio.play("hit", -6.0) # más bajo (en decibelios)
Audio.play_sfx(preload("res://assets/audio/mi_sonido.wav"))
Audio.play_music(preload("res://assets/audio/tema.ogg"))  # con fundido de 1 segundo
```

- **Para cambiar un sonido generado por uno tuyo**, en un `_ready()` (por ejemplo el del menú principal):
  ```gdscript
  Audio.sounds["coin"] = preload("res://assets/audio/moneda.wav")
  ```
- **Para crear sonidos nuevos por código**, añade una receta en `SfxGenerator.PRESETS`. Lo que significa cada opción está en `autoload/sfx_generator.gd`.
- **Música en bucle**: selecciona el `.ogg` en el panel Sistema de archivos, ve a la pestaña **Importar**, activa **Loop** y pulsa **Reimportar**.
- **En la web el sonido no suena hasta que el jugador hace clic o toca la pantalla.** Es una norma de los navegadores. Como el juego empieza en el menú, el primer clic lo desbloquea.

### Juice

La regla: **cada acción importante debe sentirse con al menos 2 o 3 efectos a la vez.**

```gdscript
# Golpear a un enemigo
Audio.play("hit")
Juice.flash($Enemigo/Sprite2D)
Juice.hitstop(0.05)
Juice.shake(0.3)
Juice.burst($Enemigo.global_position, Color.ORANGE)
Juice.floating_text("-10", $Enemigo.global_position, Color.ORANGE)

# Saltar
Audio.play("jump")
Juice.squash($Sprite2D, Vector2(0.7, 1.3))  # se estira hacia arriba

# Aterrizar
Juice.squash($Sprite2D, Vector2(1.3, 0.7))  # se aplasta

# Que aparezca algo (un botón, un mensaje)
Juice.pop_in($Mensaje)
```

- **No te pases**: un temblor de 1.0 en cada disparo marea. Para cosas pequeñas usa 0.1–0.2 y deja 0.5–1.0 para explosiones y muertes.
- **`pop_in` no se usa en nodos con colisión** (Area2D, cuerpos), porque la colisión también empezaría con tamaño 0. Aplícalo a su `Sprite2D`.
- Hay también un **empujón de cámara**: `Juice.kick(direccion)`. Úsalo al recibir un golpe o al disparar algo potente.

### Cambiar de escena

```gdscript
Transition.change_scene("res://scenes/game/nivel_2.tscn")
Transition.reload_scene()  # reiniciar el nivel (por ejemplo al morir)
```

### Guardar datos (récord, nivel desbloqueado)

Sigue el modelo de `autoload/settings.gd`, con `ConfigFile` y `user://`:

```gdscript
var config := ConfigFile.new()
config.load("user://partida.cfg")
var record: int = config.get_value("partida", "record", 0)
config.set_value("partida", "record", maxi(record, puntos))
config.save("user://partida.cfg")
```

En la web se guarda en el navegador y funciona igual.

### Cambiar el aspecto

- **Colores de botones y paneles**: abre `assets/theme.tres`. Cada estilo (`button_normal`, `button_hover`...) tiene su color.
- **Fuente**: descarga una de [Google Fonts](https://fonts.google.com) (licencia OFL, libre), cópiala a `assets/fonts/`, y en `theme.tres` arrástrala a **Default Font**.
- **Para pixel art**:
  1. En **Proyecto → Configuración → Pantalla → Ventana**, pon la resolución a **640×360** o **320×180**.
  2. En **Stretch**, deja Mode `canvas_items` y Aspect `expand`.
  3. En **Renderizado → Texturas → Filtro de textura por defecto**, elige **Nearest**, para que los píxeles no salgan borrosos.
  4. Sube los tamaños de letra de los menús, que se verán pequeños.

---

## 4. El día del tema: de cero a idea elegida

**Domingo 1 de noviembre, 22:37.** Tómate esa noche y el lunes enteros para esto. Elegir bien la idea vale más que cualquier otra cosa.

### Paso 1: 20 ideas en 30 minutos (esa misma noche)

1. Escribe el tema en el centro de un papel.
2. Durante 10 minutos, apunta **palabras** que te sugiera: significados literales, metáforas, dobles sentidos, objetos, emociones, verbos. Mínimo 30.
3. Durante 20 minutos, convierte esas palabras en **ideas de juego de una frase**: "un juego de X donde Y". **Mínimo 20.** No juzgues ninguna, cantidad antes que calidad.

### Paso 2: descartar lo obvio

**Tacha las 5 primeras ideas.** Son las que tendrán cientos de participantes. Lo que puntúa en Innovación y en Tema es la interpretación que nadie más ha pensado.

Trucos para encontrar giros:
- **Darle la vuelta**: si el tema es "luz", ¿y si juegas como la oscuridad?
- **Tomarlo literal** cuando todos lo tomarán como metáfora, o al revés.
- **Que el tema sea la mecánica, no la decoración.** Si el tema es "eco", que tus acciones se repitan con retraso, no un juego normal con murciélagos.
- **Cambiar de género**: el tema pide acción, pues haz un puzle o un juego de gestión.

### Paso 3: puntuar las que quedan

Para cada idea, puntúa del 1 al 5:

| Criterio | Pregunta |
|---|---|
| **Diversión** | ¿Ya suena divertido explicado en una frase? |
| **Tema** | ¿El tema es imprescindible, o la idea serviría con cualquier tema? |
| **Originalidad** | ¿Has visto esto antes? |
| **Alcance** | ¿Puedo tener una versión jugable en **3 días**? |
| **Juice** | ¿Tiene momentos que piden temblor, partículas y sonido? |

Quédate con las **3** que más sumen.

### Paso 4: tres prototipos (lunes 2 y martes 3)

- **Unas 3 horas por idea, como máximo.** Solo cuadrados, círculos y los sonidos generados. Nada de arte.
- Cada prototipo tiene que responder a una pregunta: **¿es divertido jugarlo 2 minutos?**
- Quédate con el que ya divierte sin gráficos. **Si ninguno divierte, el más simple.** Con juice y arte mejora mucho, pero solo si la base funciona.

### Paso 5: escribir el documento de una página

Antes de programar en serio, escribe en un README o un papel:

- **Frase del juego**: "Un juego de ___ donde ___".
- **Mecánica principal**: el verbo que el jugador repite todo el rato (saltar, disparar, empujar, girar...).
- **Cómo se gana y cómo se pierde.**
- **Lo mínimo para entregar**: lo que tiene que estar sí o sí.
- **Si sobra tiempo**: en orden de prioridad. **Lo más probable es que nada de esto entre, y no pasa nada.**
- **Estilo visual en 3 palabras** y **2 colores principales**.

---

## 5. Plan de noviembre, semana a semana

Con 5 o más horas al día. **Pon todas las fechas en tu calendario el primer día.**

### Semana 1 (1–9 nov): la mecánica

- [ ] Idea elegida y documento de una página (1–3 nov).
- [ ] Mecánica principal **divertida**, aunque sea con cuadrados.
- [ ] **Juego completo de principio a fin**: empezar, jugar, ganar o perder, volver a empezar.
- [ ] Controles de teclado **y táctiles** funcionando.
- [ ] Probado en el móvil con la versión web de Actions.

**Al final de la semana** tiene que existir un juego feo pero completo. Si no, recorta la idea, no amplíes el plazo.

### Semana 2 (10–16 nov): contenido y arte

- [ ] Niveles, enemigos u oleadas: lo que tenga tu juego. **Calidad antes que cantidad**: 5 niveles buenos ganan a 20 regulares.
- [ ] Arte definitivo del personaje, los enemigos y el fondo.
- [ ] **Curva de dificultad**: los primeros 30 segundos tienen que ser fáciles y enseñar jugando.
- [ ] Prueba de estilo: haz una captura y pregúntate si llama la atención en una miniatura.

### Semana 3 (17–23 nov): sonido, juice y que lo prueben otros

- [ ] Música, una o dos pistas en bucle. Una buena pista hace mucho.
- [ ] Sonido para **cada** acción.
- [ ] Juice en todo (ver la [lista de calidad](#6-lista-de-calidad-lo-que-hace-destacar-un-juego)).
- [ ] Menú, pausa y pantalla de fin con tu estilo.
- [ ] **Que lo prueben 3 personas** sin que les expliques nada. Míralas jugar y **calla**. Apunta dónde se atascan, dónde se aburren y qué no entienden. Arregla eso.

### Semana 4 (24–28 nov): pulir

- [ ] **Prohibido añadir funciones nuevas.** Solo arreglar y pulir.
- [ ] Repasar la lista de calidad entera.
- [ ] Página de itch: portada, capturas, GIF y descripción (ver [sección 8](#8-la-página-de-itchio)).
- [ ] README del repo con créditos y licencias.
- [ ] Probar en el móvil, en el PC y en otro navegador.

### Domingo 29 nov: entregar

Ver el [checklist de entrega](#9-checklist-de-entrega). **No lo dejes para el 1 de diciembre**: ese día itch y GitHub van lentos, y si algo falla no hay margen.

### Si vas con retraso

Recorta en este orden:
1. Funciones de la lista "si sobra tiempo".
2. Cantidad de contenido (menos niveles).
3. Variedad (menos tipos de enemigos).

**Nunca recortes el juice, el sonido ni los primeros 30 segundos.** Es lo que más pesa en los votos.

---

## 6. Lista de calidad: lo que hace destacar un juego

Cada persona que vota prueba cientos de juegos durante unos minutos. Esto es lo que marca la diferencia.

### Los primeros 30 segundos

- [ ] El juego se puede jugar **en el navegador**, sin descargar nada.
- [ ] Desde que pulsas Jugar hasta que estás jugando hay **menos de 5 segundos**.
- [ ] **Sin muros de texto.** Las instrucciones se enseñan jugando, o con una frase en pantalla que desaparece.
- [ ] En 10 segundos se entiende qué hay que hacer.

### Sensaciones (juice)

- [ ] Cada acción del jugador tiene **sonido**.
- [ ] Golpes y muertes tienen **hitstop + temblor + partículas**.
- [ ] Saltar y aterrizar tienen **squash**.
- [ ] Las cosas importantes **aparecen** con animación (`pop_in`), no de golpe.
- [ ] El marcador **reacciona** cuando cambia.
- [ ] Cambios de escena con **fundido**. Esto ya viene con la plantilla.
- [ ] Morir y reintentar es **instantáneo**: un toque y vuelves a jugar.

### Aspecto

- [ ] **Paleta limitada**: de 4 a 8 colores. Puedes sacarla de [Lospec](https://lospec.com/palette-list).
- [ ] El personaje **destaca** sobre el fondo, con contraste.
- [ ] Estilo **coherente**: no mezcles pixel art con dibujos vectoriales ni assets de estilos distintos.
- [ ] Fuente con personalidad, en lugar de la de por defecto.

### Sonido

- [ ] Música que no canse en bucle.
- [ ] Música más baja que los efectos.
- [ ] Opciones de volumen. Esto ya viene con la plantilla.

### Jugabilidad

- [ ] Dura **entre 5 y 15 minutos**, o es rejugable en partidas cortas.
- [ ] Tiene un **final** o un objetivo claro (récord, último nivel).
- [ ] La dificultad sube poco a poco.
- [ ] Funciona en **móvil** con los controles táctiles.

### Rendimiento en la web

- [ ] Va fluido en un móvil normal. Si va lento: menos partículas, menos objetos a la vez e imágenes más pequeñas.
- [ ] La versión web pesa **menos de 50 MB**. Comprime la música en `.ogg`.

---

## 7. Arte, música y sonido: herramientas y assets con licencia

### Herramientas gratuitas

| Para | Herramienta | Nota |
|---|---|---|
| Pixel art | [Pixelorama](https://orama-interactive.itch.io/pixelorama) | Gratis y hecho con Godot |
| Pixel art | [LibreSprite](https://libresprite.github.io/) | Gratis, como Aseprite |
| Pixel art | [Aseprite](https://www.aseprite.org/) | De pago (unos 20 €), el estándar |
| Dibujo vectorial | [Inkscape](https://inkscape.org/) | Exporta a PNG o SVG, y Godot importa SVG |
| Paletas | [Lospec](https://lospec.com/palette-list) | Miles de paletas listas |
| Efectos retro | [jsfxr](https://sfxr.me/) | En el navegador; exporta WAV |
| Efectos retro | [ChipTone](https://sfbgames.itch.io/chiptone) | Más completo que jsfxr |
| Música | [BeepBox](https://www.beepbox.co/) | En el navegador, muy fácil |
| Música | [Bosca Ceoil Blue](https://yurisizov.itch.io/boscaceoil-blue) | Muy fácil, ideal para jams |
| Editar audio | [Audacity](https://www.audacityteam.org/) | Recortar, normalizar, exportar a OGG |
| Grabar GIF | [ScreenToGif](https://www.screentogif.com/) | Para la página de itch |

### Assets ya hechos (revisa siempre la licencia)

| Fuente | Licencia | Qué hay |
|---|---|---|
| [Kenney](https://kenney.nl/assets) | **CC0**: úsalo sin pedir permiso | Sprites, interfaz, sonidos, fuentes. Calidad muy alta y coherente |
| [OpenGameArt](https://opengameart.org/) | Varía: filtra por CC0 o CC-BY | De todo, calidad variable |
| [Freesound](https://freesound.org/) | Varía: filtra por CC0 | Efectos de sonido reales |
| [itch.io, assets gratis](https://itch.io/game-assets/free) | Lee cada página | Muchos packs de estilo concreto |
| [Google Fonts](https://fonts.google.com/) | OFL | Fuentes |

### Qué significa cada licencia

- **CC0**: puedes hacer lo que quieras. Mencionarlo es de buena educación, pero no obligatorio.
- **CC-BY**: puedes usarlo, pero **tienes que dar crédito** al autor en los créditos o el README.
- **CC-BY-SA**: como CC-BY, y además lo que hagas con ese asset debe compartirse con la misma licencia.
- **CC-BY-NC**: no se puede usar con fines comerciales. En un jam gratuito suele valer, pero evítalo si algún día quieres vender el juego.
- **Sin licencia clara**: **no lo uses.**

### Créditos

Lleva una lista desde el **primer día** en `CREDITOS.md` o en el README, con el asset, el autor, la licencia y el enlace. El jam pide que solo uses assets que tengas derecho a compartir, y al final no te acordarás de dónde salió cada cosa.

### IA

El formulario de entrega pregunta **cuánto se usó la IA**: nada, ayuda menor o la mayor parte. **Responde con honestidad.** La plantilla y parte del código se hicieron con ayuda de IA (Claude), así que como mínimo marca "ayuda menor", y súbelo si la usas para más. Lo que más valoran los que votan es que el arte, la música y el diseño sean humanos. Si son tuyos o de assets con licencia, dilo en la descripción.

---

## 8. La página de itch.io

### Configuración del juego (Edit game)

- **Kind of project**: HTML.
- **Uploads**: la versión web (`html5`), la de Windows y la de Android las sube GitHub Actions solo.
  - En la versión web marca **This file will be played in the browser**.
  - Marca la de Windows como *Windows* y la de Android como *Android*.
- **Embed options**:
  - **Viewport dimensions**: 1280 × 720, o 960 × 540 si queda grande.
  - **Mobile friendly**: activado; **orientation**: landscape.
  - **Fullscreen button**: activado.
  - **SharedArrayBuffer support**: **desactivado**. La plantilla no lo necesita, y activarlo puede dar problemas en algunos navegadores.
- **Visibility**: *Draft* hasta la entrega; después, *Public*.

### Lo que ve el que vota

1. **Portada** (*Cover image*): **630 × 500**. Es lo que se ve en la lista de juegos del jam: tiene que llamar la atención en pequeño. Pon el personaje y el título grandes, con colores llamativos.
2. **GIF o vídeo corto** al principio de la descripción. De 5 a 10 segundos de lo más espectacular del juego. Grábalo con ScreenToGif.
3. **De 3 a 5 capturas** de momentos distintos.
4. **Descripción corta**, siguiendo esta plantilla:

```markdown
# Nombre del juego

Una frase que vende el juego: qué haces y por qué es divertido.

[GIF]

## Cómo se juega
- Moverse: WASD / flechas / joystick táctil
- Acción: Espacio / botón A
- Pausa: Esc

## Cómo encaja con el tema
Dos o tres frases sobre tu interpretación del tema "___".

## Créditos
- Programación, diseño y arte: 0scar07
- Música: ___ (licencia, enlace)
- Sonidos: ___ (licencia, enlace)
- Hecho con Godot 4.7 para el GitHub Game Off 2026
- Código fuente: https://github.com/0scar07/game-off-2026

## Uso de IA
(Explica con honestidad para qué la usaste.)
```

5. **Colores de la página** (*Edit theme*): ponlos a juego con el juego. Es un detalle que se nota.
6. **Tags**: el género, `godot`, `game-off-2026`, `mobile` y `short`.

---

## 9. Checklist de entrega

**El domingo 29 de noviembre**, en este orden:

### El repo

- [ ] Todo subido: `git status` no muestra nada pendiente.
- [ ] La última ejecución de **Actions está en verde**.
- [ ] **El repo es público**: en GitHub, *Settings → General → Danger Zone → Change visibility → Public*. **Obligatorio.**
- [ ] README con descripción, cómo se juega, capturas y **créditos de todos los assets**.
- [ ] Licencia para tu código, por ejemplo MIT: *Add file → Create new file → `LICENSE`*, y GitHub te ofrece plantillas.

### El juego

- [ ] Probado de principio a fin en la **versión de itch** (no en el editor), en el **PC** y en el **móvil**.
- [ ] Nada de errores en la consola del navegador (F12 → Console).
- [ ] Se puede ganar o terminar, y se puede reintentar.

### itch.io

- [ ] Página completa: portada, GIF, capturas, descripción, créditos y uso de IA.
- [ ] Página en **Public**.
- [ ] **Entregar en el jam**: en la página del jam, *Submit your project* → elige tu juego → rellena el formulario. **Pega el enlace del repo** y **responde a la pregunta de la IA con honestidad**.
- [ ] Abre la entrega y comprueba que el juego carga desde ahí.

---

## 10. La votación

**Del 1 de diciembre al 1 de enero.** Las categorías son General, Jugabilidad, Gráficos, Audio, Innovación e Interpretación del tema.

- **Vota unos 10 juegos al día.** itch muestra antes los juegos de quienes más votan, así que votar mucho = que te voten más.
- **Deja comentarios útiles**: una cosa que te gustó y una concreta que mejorarías. La gente suele devolver la visita.
- **Responde a todos los comentarios** de tu juego.
- **Puedes arreglar bugs graves** durante la votación, pero **no añadir contenido**. Si actualizas, dilo en la página ("v1.1: arreglado X").
- Comparte el juego en redes y en el Discord del jam, con un GIF.

---

## 11. Si algo se rompe

### Actions está en rojo

1. Ve a **Actions** → la ejecución en rojo → el paso con la ✗ → lee el error.
2. Errores típicos:
   - **"Test automático"**: algo de tu juego no pasa la prueba. Ejecútala en tu PC (ver la [sección 1](#la-prueba-automática)) y lee qué comprobación falla. Si cambiaste la demo, adapta la prueba.
   - **"Exportar..." con *configuration errors***: algo cambió en `export_presets.cfg` o en la configuración del proyecto. Deshaz el último cambio de configuración y vuelve a probar.
   - **"Subir a itch.io"**: revisa que `BUTLER_API_KEY`, `ITCH_USER` e `ITCH_GAME` estén bien escritos y que la página de itch exista.

### La web se queda en negro o no carga

- Abre la consola del navegador (F12 → Console) y mira el primer error en rojo.
- Prueba en Chrome. Safari es el que más problemas da.
- Comprueba en itch que *SharedArrayBuffer support* esté **desactivado**.
- En local, la web **no funciona abriendo el `index.html` con doble clic**. Hay que servirla:
  ```bash
  python -m http.server 8000 --directory build/web
  ```

### No suena nada en la web

Normal hasta el primer clic o toque. Es una norma de los navegadores. El menú ya lo resuelve.

### El APK no se instala en Android

- Activa **Instalar apps de origen desconocido** para tu navegador o explorador de archivos.
- Si ya tenías instalada una versión anterior, **desinstálala antes**. Cada versión se firma con una clave distinta mientras no configures una propia (ver el README).

### Godot dice que un script tiene errores

- El error sale abajo, en **Salida** o **Depurador**, con el archivo y la línea. Haz doble clic para ir a ella.
- Lo más habitual: **sangría con espacios en lugar de tabuladores**, o un nombre de nodo que no coincide con el de `$Nodo`.

### Otras preguntas

- Ayuda integrada del editor: **F1**.
- [Documentación de Godot en español](https://docs.godotengine.org/es/stable/).
- [Foro oficial de Godot](https://forum.godotengine.org/).
- [Comunidad del jam en itch](https://itch.io/jam/game-off-2026/community).

**Mucha suerte. Recuerda: pequeño, completo y pulido gana a grande y a medias.**
