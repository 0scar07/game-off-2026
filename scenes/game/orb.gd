extends Area2D
## Orbe de ejemplo: flota y se recoge al tocarlo.

signal collected(orb: Area2D)

@export var radius := 18.0
@export var color := Color(0.4, 0.9, 1.0)

var _time := randf() * TAU
## Tamaño del dibujo al aparecer (0 → 1). Solo afecta al dibujo: la colisión
## tiene su tamaño completo desde el principio.
var _appear := 0.0


func _ready() -> void:
	body_entered.connect(_on_body_entered)
	create_tween().tween_property(self, "_appear", 1.0, 0.35) 		.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func _process(delta: float) -> void:
	_time += delta
	queue_redraw()


func _draw() -> void:
	if _appear <= 0.0:
		return
	var bob := Vector2(0, sin(_time * 3.0) * 4.0)
	var glow := 1.0 + sin(_time * 6.0) * 0.15
	var r := radius * _appear
	draw_circle(bob, r * 1.8 * glow, Color(color, 0.15))
	draw_circle(bob, r, color)
	draw_circle(bob + Vector2(-r * 0.3, -r * 0.3), r * 0.3, Color(1, 1, 1, 0.7))


func _on_body_entered(body: Node2D) -> void:
	if body.name != "Player":
		return
	collected.emit(self)
	queue_free()
