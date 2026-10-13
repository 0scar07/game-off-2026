extends Control
## Botón táctil que pulsa una acción del Input Map (por ejemplo "action" o "pause").
## Admite varios dedos a la vez, a diferencia de los Button normales.

@export var action := "action"
@export var label := "A"
@export var color := Color(1, 1, 1, 0.35)

var _finger := -2 # -2 = nadie; -1 = ratón (para probar en el PC)


func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE


func _input(event: InputEvent) -> void:
	var index := -2
	if event is InputEventScreenTouch:
		index = event.index
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and not Settings.is_mobile():
		index = -1
	else:
		return

	if event.pressed and _finger == -2 and _contains(event.position):
		_finger = index
		_send(true)
		get_viewport().set_input_as_handled()
	elif not event.pressed and index == _finger:
		_finger = -2
		_send(false)


func _contains(point: Vector2) -> bool:
	var rect := get_global_rect()
	return point.distance_to(rect.get_center()) <= minf(rect.size.x, rect.size.y) / 2.0


func _send(pressed: bool) -> void:
	var event := InputEventAction.new()
	event.action = action
	event.pressed = pressed
	event.strength = 1.0 if pressed else 0.0
	Input.parse_input_event(event)
	queue_redraw()


func _draw() -> void:
	var center := size / 2.0
	var r := minf(size.x, size.y) / 2.0
	var fill := color if _finger == -2 else Color(color, minf(color.a * 2.0, 1.0))
	draw_circle(center, r, fill)
	draw_arc(center, r, 0.0, TAU, 48, Color(1, 1, 1, 0.5), 3.0, true)
	var font := get_theme_default_font()
	var font_size := int(r * 0.8)
	var text_size := font.get_string_size(label, HORIZONTAL_ALIGNMENT_CENTER, -1, font_size)
	draw_string(font, center + Vector2(-text_size.x / 2.0, text_size.y / 3.0), label, HORIZONTAL_ALIGNMENT_CENTER, -1, font_size)


func _notification(what: int) -> void:
	# Al pausar se dejan de recibir eventos: soltar el botón para que no se quede pulsado.
	if what == NOTIFICATION_PAUSED and _finger != -2:
		_finger = -2
		_send(false)


func _exit_tree() -> void:
	if _finger != -2:
		_send(false)
