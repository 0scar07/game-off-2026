extends Node
## Prueba rápida de la plantilla, sin ventana:
##   godot --headless --path . res://tests/smoke_test.tscn
## Recorre menú → juego → pausa → menú y comprueba que todo responde. Sale con código 1 si algo falla.

var _failures := 0


func _ready() -> void:
	await get_tree().process_frame
	# Que este nodo deje de ser la escena actual, para que cambiar de escena no lo borre.
	get_tree().current_scene = null
	await _run()
	print("SMOKE TEST: %s" % ("OK" if _failures == 0 else "%d FALLOS" % _failures))
	get_tree().quit(1 if _failures > 0 else 0)


func _check(condition: bool, message: String) -> void:
	if condition:
		print("  ok   ", message)
	else:
		_failures += 1
		push_error("FALLO: " + message)


func _run() -> void:
	var tree := get_tree()

	# Autoloads y buses de audio.
	_check(AudioServer.get_bus_index("Music") != -1, "existe el bus Music")
	_check(AudioServer.get_bus_index("SFX") != -1, "existe el bus SFX")
	for action in ["move_left", "move_right", "move_up", "move_down", "action", "action_2", "pause"]:
		_check(InputMap.has_action(action) and not InputMap.action_get_events(action).is_empty(),
			"la acción '%s' tiene controles" % action)

	# Ajustes: cambiar el volumen y guardarlo.
	Settings.music_volume = 0.3
	Settings.save_settings()
	Settings.music_volume = 1.0
	Settings.load_settings()
	_check(is_equal_approx(Settings.music_volume, 0.3), "el volumen de la música se guarda y se carga")
	_check(is_equal_approx(db_to_linear(AudioServer.get_bus_volume_db(AudioServer.get_bus_index("Music"))), 0.3),
		"el volumen se aplica al bus Music")
	Settings.music_volume = 0.8
	Settings.save_settings()

	# Menú principal.
	tree.change_scene_to_file("res://scenes/menus/main_menu.tscn")
	await _frames(2)
	var menu := tree.current_scene
	_check(menu.name == "MainMenu", "se abre el menú principal")
	var options: Control = menu.get_node("%OptionsMenu")
	menu.get_node("%OptionsButton").pressed.emit()
	_check(options.visible, "el botón Opciones abre las opciones")
	options.get_node("%BackButton").pressed.emit()
	_check(not options.visible, "Volver cierra las opciones")

	# Jugar → escena de juego con fundido.
	menu.get_node("%PlayButton").pressed.emit()
	await _wait_transition()
	_check(tree.current_scene.name == "Game", "Jugar lleva a la escena de juego")

	# El jugador se mueve con las acciones.
	var player: CharacterBody2D = tree.current_scene.get_node("Player")
	var start := player.position
	Input.action_press("move_right")
	await _physics_frames(20)
	Input.action_release("move_right")
	_check(player.position.x > start.x + 20.0, "el jugador se mueve a la derecha")

	# El joystick virtual mueve al jugador igual que el teclado.
	var joystick := tree.current_scene.get_node("TouchControls/Root/Joystick")
	joystick._set_vector(Vector2(0, -1))
	Input.flush_buffered_events()
	_check(Input.get_vector("move_left", "move_right", "move_up", "move_down").y < -0.9,
		"el joystick virtual pulsa move_up")
	joystick._set_vector(Vector2.ZERO)
	Input.flush_buffered_events()
	_check(Input.get_vector("move_left", "move_right", "move_up", "move_down") == Vector2.ZERO,
		"al soltar el joystick se para")

	# Sonidos generados por código.
	_check(Audio.sounds.size() == SfxGenerator.PRESETS.size(), "se generan todos los sonidos de ejemplo")
	for sound_name in Audio.sounds:
		var stream: AudioStreamWAV = Audio.sounds[sound_name]
		_check(stream.data.size() > 1000, "el sonido '%s' tiene datos" % sound_name)

	# Recoger un orbe suma puntos y crea uno nuevo.
	var game := tree.current_scene
	var orbs := game.get_node("Orbs")
	_check(orbs.get_child_count() == 5, "hay 5 orbes al empezar")
	# (Al moverse antes puede haber recogido alguno: se compara con la puntuación actual.)
	var score_before: int = game.score
	player.global_position = orbs.get_child(0).global_position
	await _physics_frames(3)
	await _frames(2)
	_check(game.score > score_before, "tocar un orbe suma puntos")
	_check(orbs.get_child_count() == 5, "aparece un orbe nuevo al recoger otro")

	# Hitstop: congela y luego vuelve a la velocidad normal.
	Juice.hitstop(0.05)
	_check(Engine.time_scale < 0.1, "hitstop congela el juego")
	await get_tree().create_timer(0.15, true, false, true).timeout
	await _frames(1)
	_check(is_equal_approx(Engine.time_scale, 1.0), "tras el hitstop el juego sigue a velocidad normal")

	# Temblor de cámara.
	var camera: Camera2D = game.get_node("Camera")
	Juice.shake(1.0)
	await _frames(3)
	_check(camera.offset.length() > 0.5, "shake mueve la cámara")
	await get_tree().create_timer(1.0, true, false, true).timeout
	_check(camera.offset.length() < 0.5, "el temblor se calma solo")

	# Pausa con la acción "pause".
	_press_action("pause")
	await _frames(2)
	_check(tree.paused, "la acción pause para el juego")
	var paused_at := player.position
	Input.action_press("move_left")
	await _physics_frames(10)
	Input.action_release("move_left")
	_check(player.position == paused_at, "en pausa el jugador no se mueve")
	_press_action("pause")
	await _frames(2)
	_check(not tree.paused, "pause otra vez reanuda")

	# Pausa → Menú principal.
	_press_action("pause")
	await _frames(2)
	tree.current_scene.get_node("PauseMenu").get_node("%MenuButton").pressed.emit()
	await _wait_transition()
	_check(tree.current_scene.name == "MainMenu", "Menú principal vuelve al menú")
	_check(not tree.paused, "al salir de la pausa el juego no se queda pausado")


func _press_action(action: String) -> void:
	for pressed in [true, false]:
		var event := InputEventAction.new()
		event.action = action
		event.pressed = pressed
		Input.parse_input_event(event)
	Input.flush_buffered_events()


func _frames(count: int) -> void:
	for i in count:
		await get_tree().process_frame


func _physics_frames(count: int) -> void:
	for i in count:
		await get_tree().physics_frame


func _wait_transition() -> void:
	await _frames(1)
	var waited := 0
	while Transition.is_changing and waited < 300:
		await _frames(1)
		waited += 1
	await _frames(2)
