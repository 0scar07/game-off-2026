extends Control
## Joystick virtual. Pulsa las acciones move_* con la fuerza del desplazamiento,
## así el código del juego usa Input.get_vector() igual con teclado, mando o táctil.

@export var radius := 90.0
@export var knob_radius := 40.0
@export var deadzone := 0.2

var _finger := -2 # -2 = nadie; -1 = ratón (para probar en el PC)
var _vector := Vector2.ZERO
var _sent := {"move_left": 0.0, "move_right": 0.0, "move_up": 0.0, "move_down": 0.0}


func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE


func _input(event: InputEvent) -> void:
	var index := -2
	var pressed := false
	var position := Vector2.ZERO
	if event is InputEventScreenTouch:
		index = event.index
		pressed = event.pressed
		position = event.position
	elif event is InputEventScreenDrag:
		if event.index != _finger:
			return
		_update(event.position)
		get_viewport().set_input_as_handled()
		return
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and not Settings.is_mobile():
		index = -1
		pressed = event.pressed
		position = event.position
	elif event is InputEventMouseMotion and _finger == -1:
		_update(event.position)
		return
	else:
		return

	if pressed and _finger == -2 and get_global_rect().has_point(position):
		_finger = index
		_update(position)
		get_viewport().set_input_as_handled()
	elif not pressed and index == _finger:
		_finger = -2
		_set_vector(Vector2.ZERO)


func _update(position: Vector2) -> void:
	var offset := (position - get_global_rect().get_center()) / radius
	_set_vector(offset.limit_length(1.0))


func _set_vector(value: Vector2) -> void:
	_vector = value if value.length() > deadzone else Vector2.ZERO
	_send("move_left", maxf(-_vector.x, 0.0))
	_send("move_right", maxf(_vector.x, 0.0))
	_send("move_up", maxf(-_vector.y, 0.0))
	_send("move_down", maxf(_vector.y, 0.0))
	queue_redraw()


func _send(action: String, strength: float) -> void:
	if is_equal_approx(_sent[action], strength):
		return
	_sent[action] = strength
	var event := InputEventAction.new()
	event.action = action
	event.pressed = strength > 0.0
	event.strength = strength
	Input.parse_input_event(event)


func _draw() -> void:
	var center := size / 2.0
	draw_circle(center, radius, Color(1, 1, 1, 0.12))
	draw_arc(center, radius, 0.0, TAU, 48, Color(1, 1, 1, 0.35), 3.0, true)
	draw_circle(center + _vector * radius, knob_radius, Color(1, 1, 1, 0.5))


func _notification(what: int) -> void:
	# Al pausar se dejan de recibir eventos: soltar el joystick para que no se quede pulsado.
	if what == NOTIFICATION_PAUSED:
		_finger = -2
		_set_vector(Vector2.ZERO)


func _exit_tree() -> void:
	_set_vector(Vector2.ZERO)
