extends Control
## Menú de opciones. Se usa desde el menú principal y desde la pausa.
## Llama a `open()` para mostrarlo; emite `closed` al volver.

signal closed

@onready var _music: HSlider = %MusicSlider
@onready var _sfx: HSlider = %SfxSlider
@onready var _fullscreen: CheckButton = %FullscreenCheck
@onready var _back: Button = %BackButton


func _ready() -> void:
	hide()
	# En móvil no hay ventana: la opción de pantalla completa sobra.
	_fullscreen.visible = not Settings.is_mobile()
	_music.value_changed.connect(func(value: float) -> void: Settings.music_volume = value)
	_sfx.value_changed.connect(func(value: float) -> void: Settings.sfx_volume = value)
	_fullscreen.toggled.connect(func(on: bool) -> void: Settings.fullscreen = on)
	_back.pressed.connect(close)


func open() -> void:
	_music.set_value_no_signal(Settings.music_volume)
	_sfx.set_value_no_signal(Settings.sfx_volume)
	_fullscreen.set_pressed_no_signal(Settings.fullscreen)
	show()
	_music.grab_focus()


func close() -> void:
	Settings.save_settings()
	hide()
	closed.emit()


func _unhandled_input(event: InputEvent) -> void:
	if visible and (event.is_action_pressed("ui_cancel") or event.is_action_pressed("pause")):
		get_viewport().set_input_as_handled()
		close()
