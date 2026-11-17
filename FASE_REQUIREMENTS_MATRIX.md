# FASE · Matriz de requisitos (trazabilidad)

Fecha: 17 nov 2026 · Versión del juego: 0.1.0 · Fuente: [`docs/PENDIENTES.md`](docs/PENDIENTES.md), contrastada con [`docs/TANDAS.md`](docs/TANDAS.md), [`FASE_MASTER_PLAN.md`](FASE_MASTER_PLAN.md), `git log` y `Assets/Scripts`.

## Leyenda

| Icono | Significado |
|---|---|
| ✅ | Hecho y comprobado (con evidencia concreta) |
| 🔧 | Hecho (existe en el código) sin comprobar en todas las plataformas |
| ⬜ | Pendiente |
| ⛔ | Bloqueado (depende de algo externo al código) |

Plataformas: **W** = web, **PC** = Windows, **A** = Android, **Todas** = las tres.

Reglas que se han seguido:

- El estado sale del **código y del historial**, no de la columna «Estado» de `PENDIENTES.md`, que está desactualizada (ver [Discrepancias](#discrepancias-entre-pendientesmd-tandasmd-y-el-código)).
- Una idea que existe en `Assets/Scripts` pero de la que no hay prueba registrada en las tres plataformas queda en 🔧. Solo se pone ✅ cuando hay evidencia concreta (CI, prueba web, comprobación en documentación del repo).
- «Archivos» son los que he localizado por nombre y por el resumen `///` de la clase. Si no hay archivo claro, pone «sin localizar».
- Los números de las primeras secciones de `PENDIENTES.md` (1-14) **se repiten** con los de las ideas 5-14 de otras secciones. Aquí llevan prefijo de sección: R (Repositorio), S (Sensación), P (Primeros minutos), C (Cualquier sitio), A (Sonido y presentación).

## 1. Repositorio, primeros minutos, rendimiento y presentación

| ID | Descripción (de PENDIENTES.md) | Estado | Archivos / evidencia | Plataforma |
|---|---|---|---|---|
| R1 | Releases con versión y notas (v1.0 para la jam) e insignias | ⬜ | Existen releases `web-*` y `build-*` (última `build-20261117-1`); no hay release `v1.0` (`gh release list`). Insignias y tabla de descargas en `README.md` | — |
| R2 | Jugar en GitHub Pages además de itch | ⬜ | El repo ya es público (`gh repo view`: PUBLIC, rama por defecto `fase`); Pages sin verificar | W |
| R3 | Imagen para compartir 1280×640 | ⬜ | Imagen lista en `docs/prensa/`; subirla en *Settings → Social preview* es manual (autor) | — |
| R4 | Kit de prensa: logo, capturas, descripciones y tráiler | ✅ | `docs/prensa/` (capturas, README). El tráiler no va en la release todavía (ver 92) | — |
| R5 | Niveles de la comunidad (plantilla de issue) | ✅ | `.github/ISSUE_TEMPLATE/nivel.yml` | — |
| R6 | Discussions activadas | 🔧 | Ajuste del repo en GitHub; sin verificar | — |
| R7 | Subida automática a itch de Windows y Linux | 🔧 | `.github/workflows/itch-builds.yml` sube **win64 y android**. Linux no existe | PC, A |
| R8 | Compilación automática en GitHub (GameCI) | ⛔ | Necesita licencia de Unity como secreto. Hoy la exportación es local | — |
| S9.1 | Micro-pausas al coger luna, golpear al jefe o aterrizar | 🔧 | `TimeControl.cs`, `FxManager.cs` | Todas |
| S9.2 | Lumi expresiva (estira, aplasta, polvo, se ladea) | 🔧 | `LumiStyle.cs`, `PlayerController.cs`, `Scarf.cs` | Todas |
| S9.3 | La línea con vida (vibra, chispea, sonido según velocidad) | 🔧 | `LineJuice.cs`, `LineVisual.cs`, `ShiftLine.cs` | Todas |
| S9.4 | Cámara cinematográfica | 🔧 | `CameraFollow.cs` | Todas |
| P5 | Tutorial sin texto en el nivel 1 | 🔧 | `TutorialHint.cs` | Todas |
| P6 | Curva de dificultad y tiempos de medalla | ⛔ | Necesita partidas del autor y de probadores; `Medals.cs` ya existe | — |
| P7 | Ritmo de los niveles (sin tramos vacíos > 15 columnas) | ✅ | `Tools/comprobar_niveles.py` (avisa de huecos) ejecutado en CI (`.github/workflows/niveles.yml`) | — |
| C8 | Rendimiento en móvil a 60 fps | ⛔ | Sin dispositivo Android conectado. Hay `QualityGuard.cs` y `PerfLog.cs` (este último sin versionar); sin medidas | A |
| C9 | Carga más rápida de la web | ✅ | Brotli, `OptimizeSize`, audio comprimido (`Assets/Editor/WebBuild.cs`). Carga medida: 6,6 s con `Tools/prueba_web/prueba.mjs` | W |
| C10 | Guardado a prueba de fallos | 🔧 | `SaveGuard.cs`, `Progress.cs` (cada 10 s, al cambiar de pestaña y al cerrar) | Todas |
| A11 | Mezcla de sonido final | 🔧 | `Tools/mezcla.py`, `GameAudio.cs` | Todas |
| A12 | Pantalla de inicio con el logo partido por la línea | 🔧 | `LogoSplit.cs`, `SplashScreen.cs` | Todas |
| A13 | Créditos tipo película | ⬜ | `Ending.cs` menciona créditos en su resumen; que sean «tipo película» sin verificar | Todas |
| A14 | Página de itch cuidada | ⛔ | La página está RESTRICTED; hacerla pública es del autor | — |

## 2. Ideas 16-142

Estado según el código. «PENDIENTES» indica lo que dice hoy `docs/PENDIENTES.md`.

| Nº | Descripción | Estado | Archivos relacionados | Plataforma |
|---|---|---|---|---|
| 16 | El mundo reacciona a la línea (hierba, hojas, charcos) | 🔧 | `LineReactor.cs`, `Sway.cs`, `WaterWaves.cs` | Todas |
| 17 | Lumi mira hacia lo importante | 🔧 | `LumiGaze.cs` | Todas |
| 18 | Piezas al alcance iluminadas | 🔧 | `PieceHighlight.cs` | Todas |
| 19 | Acorde final en la meta | 🔧 | `GameAudio.cs`, `LevelGoal.cs` | Todas |
| 20 | Celebración de capítulo | 🔧 | `ChapterCelebration.cs` | Todas |
| 21 | Tamaño de texto y menos destellos | 🔧 | `TextScaler.cs`, `Settings.cs` | Todas |
| 22 | Vista previa del nivel en el mapa de viaje | 🔧 | `PreviewPan.cs`, `JourneyWalker.cs` | Todas |
| 23 | «¿Sabías qué?» | 🔧 | `MenuExtras.cs`, `Loc.cs` | Todas |
| 24 | Piezas de atardecer | 🔧 | `SunsetPiece.cs`, `SunsetBand.cs` | Todas |
| 25 | Espejos de luz | 🔧 | `Mirror.cs`, `MirrorTarget.cs` | Todas |
| 26 | Línea con peso (péndulo) | 🔧 | `LineWeight.cs` | Todas |
| 27 | Niveles de persecución | 🔧 | `LineChase.cs` | Todas |
| 28 | Interruptores de mundo | 🔧 | `WorldSwitch.cs` | Todas |
| 29 | Animales que cambian | 🔧 | `Wildlife.cs` | Todas |
| 30 | Lumi bosteza y se duerme | 🔧 | `LumiSleep.cs`, `FloatAway.cs` | Todas |
| 31 | Huellas en la nieve | 🔧 | `Footprints.cs` (existía antes de las tandas) | Todas |
| 32 | Compañero con frases y misiones | 🔧 | `Companion.cs` (Chispa) | Todas |
| 33 | Contrarreloj por capítulo | 🔧 | `ChapterRun.cs`, `MenuExtras.Challenges.cs` | Todas |
| 34 | Desafíos con condiciones | 🔧 | `Conditions.cs`, `ConditionWatcher.cs` | Todas |
| 35 | Fantasma de tu mejor partida desde el mapa | 🔧 | `Ghost.cs` (el botón del mapa: sin verificar) | Todas |
| 36 | Mutadores | 🔧 | `Mutators.cs`, `MutatorApplier.cs` | Todas |
| 37 | Transición con la línea barriendo la pantalla | 🔧 | `SceneFader.cs` | Todas |
| 38 | Narrador de cuento | 🔧 | `StoryPage.cs` | Todas |
| 39 | Álbum de fotos del modo foto | 🔧 | `Album.cs`, `PhotoMode.cs` | Todas |
| 40 | Música interactiva por fragmento | 🔧 | `MusicLayers.cs` | Todas |
| 41 | Piezas de noche con icono además del color | 🔧 | `PieceIcons.cs` | Todas |
| 42 | Móvil: pausa con botón y controles táctiles movibles | 🔧 | `MobileControls.cs`, `MobileButton.cs`, `TouchEditor.cs`, `MobileInput.cs` | A, W (móvil) |
| 43 | Nivel de la semana desde GitHub | 🔧 | Ver [detalle](#43--nivel-de-la-semana) | Todas |
| 44 | Plataformas que brotan de día | 🔧 | `Sprout.cs` | Todas |
| 45 | Marea | 🔧 | `Tide.cs` | Todas |
| 46 | Viento solo de día | 🔧 | `Wind.cs`, `AirCurrent.cs` | Todas |
| 47 | Sombras sólidas de noche | 🔧 | Sin localizar por nombre (TANDAS E dice hecha); sin verificar | Todas |
| 48 | Llaves y puertas de día y de noche | 🔧 | `KeyDoor.cs`, `KeyPickup.cs` | Todas |
| 49 | Nivel a oscuras | 🔧 | `DarkLevel.cs` | Todas |
| 50 | Reacciones de Lumi (ríe, tiembla, se sacude) | 🔧 | `LumiReactions.cs` | Todas |
| 51 | Estaciones dentro de un nivel | 🔧 | `Seasons.cs` | Todas |
| 52 | Pequeños jefes por capítulo | 🔧 | `Boss.cs`, `BossWave.cs` (ya existían) | Todas |
| 53 | Medallas de estilo | 🔧 | `StyleMedals.cs`, `StyleTracker.cs` | Todas |
| 54 | Repetición del nivel al terminar | 🔧 | `Ghost.cs`, `ResultsScreen.cs` | Todas |
| 55 | Pantalla de estadísticas | 🔧 | `Stats.cs` | Todas |
| 56 | Línea fantasma opcional | 🔧 | `LineHint.cs`, `LineAdvisor.cs`, `GlowPath.cs` | Todas |
| 57 | Modo relajado | 🔧 | `FloatAway.cs`, `Settings.cs` | Todas |
| 58 | Iconos de botones según el mando | 🔧 | `InputGlyphs.cs` | PC (mando) |
| 59 | Vibración distinta día/noche | 🔧 | `Rumble.cs` | PC (mando), A |
| 60 | Título trazado por la línea | 🔧 | `LevelTitle.cs` | Todas |
| 61 | Final secreto | 🔧 | `StoryChoice.cs`, `Ending.cs` | Todas |
| 62 | Encuesta al terminar el juego → issue | 🔧 | Ver [detalle](#62--encuesta-al-terminar) | Todas |
| 63 | Cursor de línea dorada en el menú | 🔧 | `GoldCursor.cs` | W, PC |
| 64 | Sonido al pasar sobre cada botón | 🔧 | `UiHover.cs`, `Tools/sonidos_ui.py` | Todas |
| 65 | Frases de Lumi en el menú | 🔧 | `MenuTalk.cs`, `LumiBubble.cs` | Todas |
| 66 | Racha de días del desafío diario | 🔧 | `Profile.cs`, `DailyChallenge.cs` | Todas |
| 67 | Pantalla de carga con datos del nivel | 🔧 | `SceneFader.cs` / `Hud.cs` (archivo exacto sin verificar) | Todas |
| 68 | El menú sigue la hora real | 🔧 | `MenuMood.cs` (ya existía; amanecer y atardecer añadidos) | Todas |
| 69 | Logros con dibujo propio | 🔧 | `AchievementIcons.cs`, `Achievements.cs` | Todas |
| 70 | Tarjeta para compartir al terminar | 🔧 | `ShareCard.cs`, `Share.cs` | Todas |
| 71 | Velocidad al 80 % | 🔧 | `TimeControl.cs`, `Settings.cs` | Todas |
| 72 | Puntos de control en el mapa de pausa | 🔧 | `PauseMap.cs`, `Checkpoint.cs` | Todas |
| 73 | Rastro de luz al correr | 🔧 | `LumiStyle.cs`, `FxManager.cs` | Todas |
| 74 | Cámara lenta «justo a tiempo» | 🔧 | `TimeControl.cs` | Todas |
| 75 | Nivel opcional de trucos avanzados | 🔧 | `LevelCatalog.cs` | Todas |
| 76 | Color de línea por capítulo | 🔧 | `LineVisual.cs` | Todas |
| 77 | Escenas tipo cuento entre capítulos | 🔧 | `StoryPage.cs`, `Intro.cs` | Todas |
| 78 | Reproductor de banda sonora | 🔧 | `SoundtrackBar.cs` | Todas |
| 79 | Speedrun con tramos | 🔧 | `ChapterRun.cs` | Todas |
| 80 | Récords en línea | 🔧 | Ver [detalle](#80--récords-en-línea) | Todas |
| 81 | Editor de niveles con enlace | 🔧 | `LevelCode.cs`, `LinkOpener.cs`, `Workshop.cs`, `RuntimeLevel.cs`. Enlace `?nivel=FASE-…` (TANDAS) / `#nivel=` (ROADMAP): inconsistente, sin verificar | Todas |
| 82 | Capítulo 4 (ciudad de noche, auroras) | 🔧 | Bioma `ciudad` en `Assets/Editor/LevelBuilder*.cs`; está como niveles extra, no como capítulo de la historia | Todas |
| 83 | Atardecer como tercer mundo | 🔧 | `SunsetBand.cs`, `SunsetPiece.cs` | Todas |
| 84 | Cooperativo en línea | ⛔ | Ver [detalle](#84--cooperativo-en-línea). Solo existe cooperativo local | — |
| 85 | Voces y sonidos de Lumi | 🔧 | `Sfx.cs`, `GameAudio.cs` (ya existían) | Todas |
| 86 | Jefe final con varias fases | 🔧 | `Boss.cs`, `BossWave.cs` | Todas |
| 87 | Versión en Google Play | ⛔ | Necesita keystore propio y cuenta de desarrollador. Hoy firma de depuración | A |
| 88 | Más idiomas (pt, fr, ja) | 🔧 | `Loc.cs`, `LocTranslations*.cs`. Hechos portugués y francés; japonés no (otra fuente) | Todas |
| 89 | Historia ampliada a 40 niveles | 🔧 | `LevelCatalog.cs`, `Assets/Levels`. El comprobador cuenta 54 niveles terminables; TANDAS dice 40 | Todas |
| 90 | Versión en Steam | ⛔ | Cuentas y proceso de la tienda | PC |
| 91 | Niveles infinitos adaptativos | 🔧 | `EndlessRunner.cs` | Todas |
| 92 | Tráiler nuevo y campaña en redes | ⬜ | Ver [detalle](#92--tráiler-nuevo-y-campaña) | — |
| 93 | Lumi saluda al volver al menú | 🔧 | `MenuWalker.cs` / `MenuTalk.cs` (sin verificar cuál) | Todas |
| 94 | Estrellas fugaces | 🔧 | `ShootingStars.cs` | Todas |
| 95 | Farol que tintinea | 🔧 | `Spring.cs`, `Scarf.cs` (sin verificar cuál) | Todas |
| 96 | Pausa en blanco y negro salvo la línea | 🔧 | `PauseMood.cs` | Todas |
| 97 | Ánimo tras muchas caídas | 🔧 | `Encouragement.cs` | Todas |
| 98 | Mantener R vuelve al punto de control | 🔧 | `HoldToReturn.cs` | Todas |
| 99 | Notas al coger fragmentos | 🔧 | `MusicLayers.cs` | Todas |
| 100 | Nombre del jugador en el menú | 🔧 | `Profile.cs` | Todas |
| 101 | Pétalos al florecer | 🔧 | `FxManager.cs`, `Sprout.cs` | Todas |
| 102 | Anillo de luz al saltar | 🔧 | `FxManager.cs`, `PlayerController.cs` | Todas |
| 103 | Coleccionable por capítulo | 🔧 | `Feather.cs`, `Feathers.cs` | Todas |
| 104 | Lunas que faltan en el mapa de pausa | 🔧 | `PauseMap.cs` | Todas |
| 105 | Niveles con dos caminos | 🔧 | TANDAS E dice hecha; sin localizar archivo, sin verificar | Todas |
| 106 | Plataformas solo mientras Lumi está sobre la línea | 🔧 | TANDAS E dice hecha; sin localizar archivo, sin verificar | Todas |
| 107 | Clima que cambia con la línea | 🔧 | `LightMood.cs`, `FxManager.cs` (ya existía) | Todas |
| 108 | Logros de exploración | 🔧 | `Achievements.cs` | Todas |
| 109 | Galería de modelos | 🔧 | `Gallery.cs` | Todas |
| 110 | Enemigo que copia la línea | 🔧 | `LineChaser.cs` | Todas |
| 111 | Nivel musical | 🔧 | `BeatPlatform.cs` | Todas |
| 112 | Puzles de luz | 🔧 | `MirrorMode.cs`, `Mirror.cs` | Todas |
| 113 | Línea curva | 🔧 | `CurveLine.cs` | Todas |
| 114 | Llevar objetos de día a noche | 🔧 | `Carryable.cs`, `ChangingBox.cs` | Todas |
| 115 | Mundo abierto pequeño entre capítulos | 🔧 | `JourneyWalker.cs` (Lumi camina por el mapa) | Todas |
| 116 | Carrera a pantalla partida | 🔧 | `RaceMode.cs`, `RaceSetup.cs` | PC (2 jugadores) |
| 117 | Habilidades desbloqueables | 🔧 | `Skills.cs` | Todas |
| 118 | Historia con decisiones | 🔧 | `StoryChoice.cs` | Todas |
| 119 | Versión para consolas | ⛔ | Cuentas y proceso de plataforma | — |
| 120 | Integración con Twitch | 🔧 | Ver [detalle](#120--twitch) | W, PC (no móvil: sin verificar) |
| 121 | Editor completo y Steam Workshop | ⛔ | Steam | PC |
| 122 | Secuela «FASE: Eclipse» | ⬜ | Es otro juego | — |
| 123 | Lumi estornuda en la nieve | 🔧 | `LumiReactions.cs` | Todas |
| 124 | Botones del menú se inclinan hacia el ratón | 🔧 | `UiHover.cs` | W, PC |
| 125 | Reloj de arena en la pausa | 🔧 | `PlayToday.cs`, `PauseMenu.cs` | Todas |
| 126 | Nubes que tapan el sol | 🔧 | `LightMood.cs` | Todas |
| 127 | Créditos con los probadores | ⛔ | Faltan los nombres | — |
| 128 | Sonido ambiente en zonas cerradas | 🔧 | `Enclosure.cs` | Todas |
| 129 | Mensajes escondidos en el decorado | 🔧 | `HiddenMessages.cs` | Todas |
| 130 | Ajuste de brillo | 🔧 | `Brightness.cs`, `Settings.cs` | Todas |
| 131 | Retos semanales con ropa para Lumi | 🔧 | `WeeklyChallenges.cs`, `Wardrobe.cs`, `Shop.cs` | Todas |
| 132 | Flor donde más caes | 🔧 | `FallFlower.cs` | Todas |
| 133 | Mapa del capítulo que se pinta | 🔧 | `ChapterLabel.cs`, `JourneyWalker.cs` (sin verificar) | Todas |
| 134 | Puertas con la línea en un ángulo | 🔧 | `LineDoor.cs` | Todas |
| 135 | Niveles verticales de escalada | 🔧 | `Assets/Editor/LevelBuilder*.cs` (niveles) | Todas |
| 136 | Fantasma de amigos con código | 🔧 | `FriendGhost.cs` | Todas |
| 137 | Modo de un solo toque | 🔧 | `OneTouchMode.cs` | A, W (móvil) |
| 138 | Sombras largas de atardecer | 🔧 | `LightMood.cs` | Todas |
| 139 | Capítulo submarino | 🔧 | Bioma `mar` en `LevelBuilder*.cs`, `WaterWaves.cs` | Todas |
| 140 | Analítica anónima | 🔧 | Ver [detalle](#140--analítica-anónima) | Todas (no en editor) |
| 141 | Creador de niveles con IA | ⛔ | Necesita clave de API de pago | — |
| 142 | Lanzamiento completo (Steam, prensa, festivales) | ⬜ | Ver [detalle](#142--lanzamiento-completo) | — |

## 3. Verificación detallada de las ideas 43, 62, 80, 84, 92, 120, 140 y 142

### 43 · Nivel de la semana

**Estado: 🔧** (PENDIENTES y MASTER_PLAN dicen ✅; solo hay prueba en el editor).

Criterios de aceptación:

1. El juego descarga `semana/nivel.txt` de la rama `fase` del repo (`raw.githubusercontent.com/0scar07/game-off-2026/fase/semana/nivel.txt`), con tiempo máximo de 10 s.
2. Si el archivo tiene menos de 3 líneas o el código `FASE-` no se decodifica, el estado pasa a «fallo» y no se ofrece jugarlo.
3. Extras → Nivel de la semana muestra nombre (según idioma) y autor, y permite jugarlo.
4. Jugarlo abre el Taller con ese código sin pisar el nivel propio del jugador.
5. El botón de envío abre la plantilla de issue `nivel.yml`.

Evidencias:

- Código: `Assets/Scripts/WeeklyLevel.cs` (descarga con `UnityWebRequest`, `Parse`, `Play`), `Assets/Scripts/MenuExtras.Extras.cs` (llamada a `WeeklyLevel.Fetch()` y botón «JugarSemana»), `Assets/Scripts/LinkOpener.cs`, `Assets/Scripts/Workshop.cs`, `Tools/nivel_semana.py`.
- Datos: `semana/nivel.txt` existe («Puentes alternos | Alternating bridges», autor 0scar07).
- Plantilla: `.github/ISSUE_TEMPLATE/nivel.yml`.
- Prueba: descarga y juego en el editor (16-17 nov). **No probado** en web, Windows ni Android.
- La rama `fase` es la predeterminada del repo (`gh repo view`), así que la URL es válida. Si algún día se cambia la rama predeterminada, el código sigue apuntando a `fase`.
- Sin verificar: que el navegador permita la descarga desde `raw.githubusercontent.com` en la build web (CORS).
- Sin verificar: «sin pisar el nivel propio» (lo afirma el mensaje del commit `5c685ea`; no lo he leído en `Workshop.cs`).

### 62 · Encuesta al terminar

**Estado: 🔧**. Lo que existe es un botón, no una encuesta.

Criterios de aceptación:

1. Al final del juego hay un botón «Tu opinión» (4 idiomas).
2. Abre en el navegador la plantilla de issue de prueba de juego del repo.

Evidencias:

- `Assets/Scripts/ShareCard.cs` líneas 45-46: `Application.OpenURL(WeeklyLevel.Repo + "/issues/new?template=prueba.yml")`.
- `.github/ISSUE_TEMPLATE/prueba.yml` existe; el README enlaza a la misma plantilla.
- El jugador necesita cuenta de GitHub para enviar la issue.
- No hay prueba registrada de que el botón abra la pestaña en web, Windows o Android.
- El texto de PENDIENTES («encuesta corta … que abre un issue») es más ambicioso que lo implementado: no hay formulario dentro del juego.

### 80 · Récords en línea

**Estado: 🔧**. El backend está probado; falta verlo dentro del juego (MASTER_PLAN, tarea 5).

Criterios de aceptación:

1. Al terminar un nivel de historia (no diario ni Taller) y con la red activa, se muestra el top 10 del nivel.
2. Si hay nombre, se sube el tiempo solo cuando la partida es «justa» (sin asistencia ni mutadores activos).
3. El nombre se recorta a 12 caracteres.
4. El servidor acepta tiempos entre 3 s y 1 h, rechaza el borrado y no permite modificar filas de otros.
5. Un nombre aparece una vez en el top, con su mejor marca.

Evidencias:

- Código: `Assets/Scripts/Online.cs` (`Online`, `OnlineRecords.SubmitAndFetch`), `Assets/Scripts/ResultsScreen.cs` (líneas ~144-156, condición `fair`).
- SQL: `Tools/supabase.sql` (tabla `records` con `check` de nivel, nombre y tiempo; RLS con `select` e `insert` para `anon`; función `top_records`).
- Prueba con `curl` el 16 nov: insertar récord, leer top 10, rechazo de borrado y rechazo de tiempos imposibles.
- No hay prueba de la interfaz de la tabla dentro de una build.
- Limitación conocida: cualquiera con la clave pública puede enviar un tiempo falso dentro de 3 s-1 h (ver `FASE_KNOWN_ISSUES.md`).

### 84 · Cooperativo en línea

**Estado: ⛔**. No implementado, por decisión (ver `FASE_DECISIONS.md`, registro b).

Criterios de aceptación (para cuando se haga):

1. Dos jugadores en PCs distintos: uno controla a Lumi y otro la línea.
2. Funciona en web (solo WebSockets) y en escritorio.
3. Latencia aceptable para saltar y mover la línea (umbral sin definir).

Evidencias:

- No hay código de red de cooperativo: la única referencia a `WebSocket` en `Assets/Scripts` es `TwitchVotes.cs` (búsqueda de `WebSocket|NetworkManager|Netcode|Photon`), y `Packages/manifest.json` no incluye paquetes de red.
- Existe cooperativo **local**: `Settings.Coop` (`Settings.cs`), teclas propias, segundo mando para la línea (`ShiftLine.cs`) y aviso en `Hud.cs`.
- MASTER_PLAN tarea 8: «necesita un servidor de juego en tiempo real (Supabase no lo sustituye)».

### 92 · Tráiler nuevo y campaña

**Estado: ⬜** (campaña escrita; tráiler nuevo pendiente; nada publicado).

Criterios de aceptación:

1. Tráiler que muestre lo añadido después de las tandas A-G.
2. Textos de campaña listos para publicar.
3. Publicación en redes por el autor.

Evidencias:

- Campaña escrita: `docs/lanzamiento/CAMPANA.md` (calendario, X/Bluesky en inglés y español, Reddit, Discord, devlog, envío a la jam).
- Tráiler existente: `Trailer/FASE-trailer-es.mp4` y `FASE-trailer-en.mp4` con fecha de archivo **2 nov 2026**, anteriores a las tandas A-G (commit `4535e35`). Por tanto **no es** el «tráiler nuevo»; que el viejo siga valiendo, sin verificar.
- Herramientas: `Assets/Scripts/TrailerDirector.cs`, `Tools/montar_trailer.py`, `Tools/musica_trailer.py`.
- Tarea de tablero: el autor avisará cuando toque.
- Los `Trailer/` y `Trailer/tmp` pesan mucho; `Trailer/titulos${c}_*.png` es un nombre de archivo con una variable de shell sin expandir (parece un fallo de script, sin verificar).

### 120 · Twitch

**Estado: 🔧**. Probado en el editor; no en web.

Criterios de aceptación:

1. Extras → Twitch: se escribe un canal y el juego se conecta como invitado anónimo (solo lectura), sin OAuth ni secretos.
2. Cada ronda de 3 s se cuentan los votos `!izquierda`/`!derecha`, `!left`/`!right`, `<`/`>`.
3. La línea se desliza hacia el lado ganador; el jugador sigue pudiendo moverla.
4. Un overlay muestra estado y votos.
5. En web funciona con el puente JavaScript.

Evidencias:

- `Assets/Scripts/TwitchVotes.cs`: `wss://irc-ws.chat.twitch.tv:443`, `PASS SCHMOOPIIE` (convención pública de Twitch para invitados) y `NICK justinfan#####`; constantes `Round = 3f`, `Step = 2.5f`; estados 0-3.
- Web: `Assets/Plugins/WebGL/Twitch.jslib` y `DllImport` bajo `#if UNITY_WEBGL && !UNITY_EDITOR`.
- UI: `MenuExtras.Extras.cs` (línea ~173).
- Prueba: en el editor, conectado al canal `#twitch`, estado 2 (en el canal) y overlay visible. **No probado en la web** ni en la build de Windows.
- El estado 2 demuestra conexión; no hay registro de un voto real contado y aplicado a la línea (sin verificar).

### 140 · Analítica anónima

**Estado: 🔧**. El backend está probado; el código del juego **no envía nada desde el editor**, así que hace falta una build para verlo.

Criterios de aceptación:

1. Se registran `start`, `death`, `finish` y `quit` con nivel, posición x y segundos. Sin nombres ni identificadores.
2. Solo en niveles de historia (no diario ni Taller) y no en el editor.
3. Se puede apagar en Opciones → Controles.
4. El servidor solo permite insertar; leer los eventos solo se puede desde el panel de Supabase.

Evidencias:

- Código: `Assets/Scripts/Online.cs`, clase `Analytics` (`Add` descarta si `Application.isEditor` o `!Allowed`); interruptor en `MenuExtras.cs` línea ~403 (solo si `Online.Enabled`).
- SQL: `Tools/supabase.sql` (tabla `events`, `check` de `kind`, RLS con solo `insert`).
- Prueba con `curl` el 16 nov: «eventos que no se pueden leer».
- Sin verificar: si la prueba web de CI (`prueba.mjs`, que entra al nivel 1) inserta eventos reales en la base de producción.
- Sin verificar: aviso de privacidad visible para el jugador dentro del juego (el README sí habla de privacidad).

### 142 · Lanzamiento completo

**Estado: ⬜**. No es programación.

Criterios de aceptación:

1. Página de Steam, nota de prensa y presencia en festivales de juegos indie.
2. Página de itch pública.
3. Envío a la GitHub Game Off 2026 antes del 1 dic 2026.

Evidencias:

- `ROADMAP.md` línea 61: «⬜ lo último antes de enviar; no es programación».
- Steam (90) está ⛔; no hay carpeta ni documento de página de Steam.
- La página de itch sigue RESTRICTED; el envío a la jam está en el tablero (BACKLOG).
- `docs/lanzamiento/CAMPANA.md` solo cubre itch, redes y la jam, no Steam ni festivales.

## Discrepancias entre PENDIENTES.md, TANDAS.md y el código

1. **PENDIENTES marca ⬜ casi todas las ideas 24-142**, pero TANDAS y el historial (`git log`) muestran que las tandas A-G están programadas (commits `14a2846`, `ed91983`, `b71877f`, `cf05b80`, `aa9e209`/`711a2dd`, `9f882ef`, `3968c81`). Lo programado queda en 🔧 aquí.
2. **TANDAS dice que 43, 62, 80, 120 y 140 «no se pueden programar desde aquí»** (necesitan repo público, servidor o Twitch). Ya están hechas (commits `5c685ea`, `b2bc5e4`, `aa9e209`, `3a03f95`). Solo 84, 87, 90, 92, 119, 121, 122, 127, 141 siguen sin hacer.
3. **Ideas con ✅ en PENDIENTES** (43, 62, 80, 120, 140) quedan aquí en 🔧: solo hay pruebas en el editor o con `curl`, no en las tres plataformas.
4. **Numeración duplicada** en PENDIENTES: las ideas 5-14 de las primeras secciones colisionan con las de las secciones siguientes (por ejemplo, «5 Tutorial» y «5 Niveles de la comunidad»; «9» aparece como 9.1-9.4 y como «Carga más rápida»). Aquí llevan prefijo de sección.
5. **88 Idiomas**: PENDIENTES ⬜; TANDAS D dice portugués y francés hechos (interfaz fija, niveles, capítulos y pistas; el resto sale en inglés). El japonés no.
6. **89 «40 niveles»** (TANDAS F) frente a los **54 niveles** que comprueba `Tools/comprobar_niveles.py`. El primer número es del 9 nov; el segundo es el del 16-17 nov.
7. **G10 / bot de pruebas**: TANDAS dice que no pasa los pinchos del nivel 1; el commit `67f67ea` dice que ya los salta; la medición del 17 nov es que solo llega al 10-21 % del camino. No sirve como prueba de partida completa.
8. **TANDAS «falta probar en Unity»** en todas las tandas; después hay commits de pruebas (`69d8641` y la gira de 60 escenas). No hay lista por idea de qué se probó y qué no.
9. **PENDIENTES «Al final»** pide exportar y subir las tres versiones: hecho el 17 nov (itch: `html5`, `win64`, `android`). Queda «Hacer público el repositorio»: ya es público. Lo que queda es hacer pública la página de itch.
10. **R7**: PENDIENTES dice «Windows y Linux»; solo hay Windows y Android (`itch-builds.yml`).
11. **Rendimiento (C8)**: `PerfLog.cs` aparece sin versionar (`git status`); `Assets/Editor/FaseReviewBridge.cs`, `FaseReviewChecks.cs` y un script de pruebas locales también. MASTER_PLAN dice que los scripts de prueba locales no se suben: coherente, pero `PerfLog.cs` está en `Assets/Scripts` y se compilaría en la build si se llega a añadir a git (sin verificar).
12. **Enlace de nivel (81)**: TANDAS escribe `?nivel=FASE-…` y ROADMAP `#nivel=`; sin verificar cuál implementa `LinkOpener.cs`.
13. **Android `targetSdk`**: `ProjectSettings.asset` tiene `AndroidTargetSdkVersion: 0` (automático); el APK generado tiene `targetSdk 36` (dato de la comprobación con apksigner/aapt del 17 nov).
