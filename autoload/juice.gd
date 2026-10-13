extends Node
## "Juice": efectos pequeños que hacen que cada acción se sienta bien.
##   Juice.shake(0.4)                    temblor de cámara (0..1, se acumula)
##   Juice.kick(Vector2.LEFT)            empujón de cámara en una dirección
##   Juice.hitstop(0.05)                 congela el juego un instante al golpear
##   Juice.flash($Sprite)                destello blanco
##   Juice.squash($Sprite)               estirar y encoger con rebote
##   Juice.pop_in($Boton)                aparecer creciendo con rebote
##   Juice.burst(posicion, color)        explosión de partículas
##   Juice.floating_text("+1", posicion) texto que sube y se desvanece
##
## El temblor y el empujón mueven el `offset` de la Camera2D activa: no lo uses para otra cosa.

## Desplazamiento máximo del temblor, en píxeles.
@export var max_shake := 22.0
## Cuánto trauma se pierde por segundo.
@export var shake_decay := 1.6

var _trauma := 0.0
var _kick := Vector2.ZERO
var _noise := FastNoiseLite.new()
var _noise_time := 0.0
var _hitstop_until_msec := 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_noise.frequency = 0.5


func shake(amount := 0.4) -> void:
	_trauma = minf(_trauma + amount, 1.0)


func kick(direction: Vector2, strength := 14.0) -> void:
	_kick += direction.normalized() * strength


## Congela el juego `duration` segundos (en tiempo real). Varios golpes seguidos no se acumulan.
func hitstop(duration := 0.05, time_scale := 0.02) -> void:
	_hitstop_until_msec = maxi(_hitstop_until_msec, Time.get_ticks_msec() + int(duration * 1000.0))
	Engine.time_scale = time_scale


func flash(item: CanvasItem, duration := 0.12) -> void:
	if not is_instance_valid(item):
		return
	# Colores por encima de 1 aclaran el dibujo hasta dejarlo casi blanco.
	item.modulate = Color(6, 6, 6, item.modulate.a)
	var tween := item.create_tween().set_ignore_time_scale()
	tween.tween_property(item, "modulate", Color(1, 1, 1, item.modulate.a), duration)


## `node` es un Node2D o un Control.
func squash(node: CanvasItem, stretch := Vector2(1.35, 0.7), duration := 0.35) -> void:
	if not is_instance_valid(node):
		return
	var base := _base_scale(node)
	if node is Control:
		node.pivot_offset = node.size / 2.0
	node.set("scale", base * stretch)
	var tween := node.create_tween()
	tween.tween_property(node, "scale", base, duration).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)


## `node` es un Node2D o un Control.
## Ojo: no lo uses en un nodo con colisión (Area2D, cuerpos...), porque la colisión
## también empezaría con tamaño 0. Aplícalo a su dibujo (Sprite2D, Polygon2D...).
func pop_in(node: CanvasItem, duration := 0.35, delay := 0.0) -> void:
	if not is_instance_valid(node):
		return
	var base := _base_scale(node)
	if node is Control:
		node.pivot_offset = node.size / 2.0
	node.set("scale", Vector2.ZERO)
	var tween := node.create_tween()
	tween.tween_interval(delay)
	tween.tween_property(node, "scale", base, duration).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


## Explosión de partículas en `position` (coordenadas del mundo). Se borra sola al terminar.
func burst(position: Vector2, color := Color.WHITE, amount := 18, speed := 320.0, parent: Node = null) -> void:
	var particles := CPUParticles2D.new()
	particles.position = position
	particles.one_shot = true
	particles.explosiveness = 1.0
	particles.amount = amount
	particles.lifetime = 0.55
	particles.spread = 180.0
	particles.gravity = Vector2.ZERO
	particles.initial_velocity_min = speed * 0.4
	particles.initial_velocity_max = speed
	particles.damping_min = speed * 0.8
	particles.damping_max = speed * 1.2
	particles.scale_amount_min = 4.0
	particles.scale_amount_max = 9.0
	particles.color = color
	var fade := Gradient.new()
	fade.set_color(0, Color.WHITE)
	fade.set_color(1, Color(1, 1, 1, 0))
	particles.color_ramp = fade
	particles.z_index = 50
	_add_to_world(particles, parent)
	particles.emitting = true
	particles.finished.connect(particles.queue_free)


## Texto que sube y se desvanece en `position` (coordenadas del mundo).
func floating_text(text: String, position: Vector2, color := Color.WHITE, size := 36, parent: Node = null) -> void:
	var label := Label.new()
	label.text = text
	var settings := LabelSettings.new()
	settings.font_size = size
	settings.font_color = color
	settings.outline_size = 8
	settings.outline_color = Color(0, 0, 0, 0.8)
	label.label_settings = settings
	label.z_index = 60
	_add_to_world(label, parent)
	label.position = position - label.get_combined_minimum_size() / 2.0
	label.pivot_offset = label.get_combined_minimum_size() / 2.0
	var tween := label.create_tween().set_parallel()
	tween.tween_property(label, "position:y", label.position.y - 70.0, 0.8).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	tween.tween_property(label, "modulate:a", 0.0, 0.35).set_delay(0.45)
	label.scale = Vector2(0.4, 0.4)
	tween.tween_property(label, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.chain().tween_callback(label.queue_free)


func _process(_delta: float) -> void:
	# Tiempo real: el temblor sigue aunque el juego esté congelado por un hitstop.
	var real_delta := get_process_delta_time() / maxf(Engine.time_scale, 0.0001)

	if _hitstop_until_msec > 0 and Time.get_ticks_msec() >= _hitstop_until_msec:
		_hitstop_until_msec = 0
		Engine.time_scale = 1.0

	_trauma = maxf(_trauma - shake_decay * real_delta, 0.0)
	_kick = _kick.lerp(Vector2.ZERO, minf(real_delta * 14.0, 1.0))

	var camera := get_viewport().get_camera_2d()
	if camera == null:
		return
	_noise_time += real_delta * 60.0
	var amount := _trauma * _trauma
	camera.offset = Vector2(
		_noise.get_noise_2d(_noise_time, 0.0),
		_noise.get_noise_2d(0.0, _noise_time)
	) * max_shake * amount + _kick


func _base_scale(node: CanvasItem) -> Vector2:
	if not node.has_meta("juice_base_scale"):
		node.set_meta("juice_base_scale", node.get("scale"))
	return node.get_meta("juice_base_scale")


func _add_to_world(node: Node, parent: Node) -> void:
	var target := parent if parent else get_tree().current_scene
	if target:
		target.add_child(node)
