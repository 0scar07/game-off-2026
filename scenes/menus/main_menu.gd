extends Control

const GAME_SCENE := "res://scenes/game/game.tscn"

@onready var _title: Label = $Center/Rows/Title
@onready var _play: Button = %PlayButton
@onready var _options: Button = %OptionsButton
@onready var _quit: Button = %QuitButton
@onready var _options_menu: Control = %OptionsMenu


func _ready() -> void:
	_quit.visible = Settings.can_quit()
	_play.pressed.connect(func() -> void: Transition.change_scene(GAME_SCENE))
	_options.pressed.connect(_options_menu.open)
	_quit.pressed.connect(get_tree().quit)
	_options_menu.closed.connect(_options.grab_focus)
	_play.grab_focus()

	# Entrada: el título y los botones aparecen uno tras otro.
	await get_tree().process_frame
	Juice.pop_in(_title, 0.5)
	var delay := 0.15
	for button in [_play, _options, _quit]:
		Juice.pop_in(button, 0.4, delay)
		delay += 0.08
	_float_title()


## El título flota suavemente arriba y abajo.
func _float_title() -> void:
	var tween := create_tween().set_loops()
	var base_y := _title.position.y
	tween.tween_property(_title, "position:y", base_y - 8.0, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(_title, "position:y", base_y, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
