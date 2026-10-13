extends Node
## Da vida a todos los botones del juego sin tocar cada escena:
## crecen un poco al pasar por encima o al seleccionarlos y suenan al pulsarlos.

const HOVER_SCALE := Vector2(1.05, 1.05)

## Al entrar en una escena, el primer botón recibe el foco: no debe sonar.
var _quiet_until_msec := 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	get_tree().node_added.connect(_on_node_added)


func _on_node_added(node: Node) -> void:
	if node is BaseButton:
		_quiet_until_msec = Time.get_ticks_msec() + 200
		node.mouse_entered.connect(_highlight.bind(node, true))
		node.mouse_exited.connect(_on_unhover.bind(node))
		node.focus_entered.connect(_highlight.bind(node, true))
		node.focus_exited.connect(_highlight.bind(node, false))
		node.pressed.connect(_on_pressed.bind(node))
	elif node is Slider:
		node.value_changed.connect(_on_slider_changed.bind(node))


func _highlight(button: BaseButton, on: bool) -> void:
	if button.disabled or not button.is_inside_tree():
		return
	if on and Time.get_ticks_msec() > _quiet_until_msec:
		Audio.play("hover", -6.0)
	button.pivot_offset = button.size / 2.0
	var tween := button.create_tween()
	tween.tween_property(button, "scale", HOVER_SCALE if on else Vector2.ONE, 0.12) \
		.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func _on_unhover(button: BaseButton) -> void:
	# Si sigue teniendo el foco (teclado o mando), que se quede grande.
	if not button.has_focus():
		_highlight(button, false)


func _on_pressed(button: BaseButton) -> void:
	Audio.play("click")
	button.pivot_offset = button.size / 2.0
	button.scale = Vector2(0.92, 0.92)
	var tween := button.create_tween()
	tween.tween_property(button, "scale", HOVER_SCALE, 0.25).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)


func _on_slider_changed(_value: float, slider: Slider) -> void:
	# Un clic al mover el volumen, para oír cómo suena.
	if slider.is_visible_in_tree():
		Audio.play("click", -4.0, 0.0)
