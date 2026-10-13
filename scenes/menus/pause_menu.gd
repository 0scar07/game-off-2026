extends CanvasLayer
## Menú de pausa. Ponlo en cualquier escena de juego: se abre con Esc/P/Start
## o con el botón de pausa táctil, y para el juego mientras está abierto.

const MAIN_MENU := "res://scenes/menus/main_menu.tscn"

@onready var _root: Control = $Root
@onready var _resume: Button = %ResumeButton
@onready var _restart: Button = %RestartButton
@onready var _options: Button = %OptionsButton
@onready var _menu: Button = %MenuButton
@onready var _options_menu: Control = %OptionsMenu


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_root.hide()
	_resume.pressed.connect(resume)
	_restart.pressed.connect(Transition.reload_scene)
	_options.pressed.connect(_options_menu.open)
	_menu.pressed.connect(func() -> void: Transition.change_scene(MAIN_MENU))
	_options_menu.closed.connect(_options.grab_focus)


func pause() -> void:
	if Transition.is_changing:
		return
	get_tree().paused = true
	_root.show()
	_resume.grab_focus()


func resume() -> void:
	get_tree().paused = false
	_root.hide()


func _unhandled_input(event: InputEvent) -> void:
	if not event.is_action_pressed("pause"):
		return
	get_viewport().set_input_as_handled()
	if _root.visible:
		resume()
	else:
		pause()


## Si el jugador cambia de pestaña o de app, pausar automáticamente.
func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT and not _root.visible and is_inside_tree():
		pause()
