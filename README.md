# Game Off 2026

Plantilla base en **Godot 4.7** para el [Game Off 2026](https://itch.io/jam/game-off-2026), lista para web, Android y escritorio.

📖 **[GUIA.md](GUIA.md)**: cómo usar la plantilla, aprender Godot, el día del tema, el plan de noviembre, assets con licencia, la página de itch y la entrega.

## Qué incluye

- **Menú principal, opciones y pausa**, navegables con ratón, teclado, mando y pantalla táctil.
- **Ajustes guardados**: volumen de música y de efectos, y pantalla completa (`autoload/settings.gd`).
- **Audio**: música con fundido entre pistas y efectos con variación de tono (`autoload/audio.gd`).
- **Transiciones** con fundido a negro entre escenas (`autoload/transition.gd`).
- **Controles táctiles**: joystick virtual y botones A/B/pausa con varios dedos a la vez. Solo aparecen en pantallas táctiles.
- **Controles** en el Input Map: `move_left/right/up/down`, `action`, `action_2` y `pause`, con teclado, flechas y mando.
- **Juice** (`autoload/juice.gd`): temblor y empujón de cámara, hitstop, destellos, estirar y encoger, aparecer con rebote, partículas y texto flotante.
- **Sonidos de ejemplo generados por código** (`autoload/sfx_generator.gd`): click, hover, dash, jump, hit, coin, powerup, explosion y lose. No hacen falta archivos ni licencias.
- **Botones con vida** (`autoload/ui_feedback.gd`): todos los botones crecen al pasar por encima y suenan al pulsarlos, sin tocar cada escena.
- **Tema visual** (`assets/theme.tres`): botones redondeados, paneles y deslizadores con los colores del juego. Cámbialo una vez y cambian todos los menús.
- **Renderizador Compatibility**, el que mejor funciona en web y en móviles.
- **Demo**: recoge orbes con el dash. Usa todo lo anterior; cámbiala por tu juego.

## Uso rápido

```gdscript
# Escenas
Transition.change_scene("res://scenes/game/game.tscn")

# Controles (funciona con teclado, mando y táctil)
var direccion := Input.get_vector("move_left", "move_right", "move_up", "move_down")

# Sonido
Audio.play("coin")                                      # sonido generado
Audio.sounds["coin"] = preload("res://assets/audio/moneda.wav")  # sustituirlo por uno tuyo
Audio.play_music(preload("res://assets/audio/tema.ogg"))

# Juice: combina varios en cada acción importante
Juice.shake(0.3)                        # temblor (0..1)
Juice.hitstop(0.05)                     # congelar un instante al golpear
Juice.flash($Sprite)                    # destello blanco
Juice.squash($Sprite)                   # estirar y encoger
Juice.burst(position, Color.RED)        # partículas
Juice.floating_text("+10", position)    # texto que sube
```

La escena necesita una `Camera2D` para que funcionen el temblor y el empujón. Juice usa el `offset` de la cámara, así que no lo cambies tú.

## Exportación automática (GitHub Actions)

Con cada push a `main`, [`.github/workflows/build.yml`](.github/workflows/build.yml):

1. pasa la prueba automática;
2. exporta la **web**, **Windows** y **Android (APK)**;
3. guarda las tres en la ejecución (pestaña **Actions → la ejecución → Artifacts**);
4. si está configurado, las sube a **itch.io** con butler.

Para subirlas a itch, en GitHub ve a **Settings → Secrets and variables → Actions** y añade:

| Tipo | Nombre | Valor |
|---|---|---|
| Secret | `BUTLER_API_KEY` | tu clave de API de itch ([itch.io/user/settings/api-keys](https://itch.io/user/settings/api-keys)) |
| Variable | `ITCH_USER` | tu usuario de itch |
| Variable | `ITCH_GAME` | la parte final de la URL del juego (`usuario.itch.io/<esto>`) |

**Firma del APK (opcional):** sin keystore propio, cada versión se firma con una clave temporal y para instalar una nueva en el móvil hay que desinstalar antes la anterior. Para evitarlo, añade los secrets `ANDROID_KEYSTORE_BASE64` (el `.keystore` en base64) y `ANDROID_KEYSTORE_PASSWORD`. El alias de la clave debe ser `gameoff`.

## Prueba automática

```bash
godot --headless --path . res://tests/smoke_test.tscn
```

Recorre el menú, el juego y la pausa, y sale con código 1 si algo falla.
