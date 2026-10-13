extends Area2D
## Orbe de ejemplo: flota y se recoge al tocarlo.

signal collected(orb: Area2D)

@export var radius := 18.0
@export var color := Color(0.4, 0.9, 1.0)

var _time := randf() * TAU


func _ready() -> void:
	body_entered.connect(_on_body_entered)
	Juice.pop_in(self)


func _process(delta: float) -> void:
	_time += delta
	queue_redraw()


func _draw() -> void:
	var bob := Vector2(0, sin(_time * 3.0) * 4.0)
	var glow := 1.0 + sin(_time * 6.0) * 0.15
	draw_circle(bob, radius * 1.8 * glow, Color(color, 0.15))
	draw_circle(bob, radius, color)
	draw_circle(bob + Vector2(-radius * 0.3, -radius * 0.3), radius * 0.3, Color(1, 1, 1, 0.7))


func _on_body_entered(body: Node2D) -> void:
	if body.name != "Player":
		return
	collected.emit(self)
	queue_free()
