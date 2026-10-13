extends Node2D
## Escena de juego de ejemplo: recoge orbes. Muestra cómo usar Juice y Audio.

const ORB_SCENE := preload("res://scenes/game/orb.tscn")
const ORB_COUNT := 5
const ARENA := Rect2(80, 120, 1120, 540)

var score := 0

@onready var _score_label: Label = %ScoreLabel
@onready var _orbs: Node2D = $Orbs


func _ready() -> void:
	# En el móvil el texto de ayuda queda debajo de los botones táctiles.
	$Hud/Hint.visible = not Settings.uses_touch()
	for i in ORB_COUNT:
		_spawn_orb()


func _spawn_orb() -> void:
	var orb := ORB_SCENE.instantiate()
	orb.position = Vector2(
		randf_range(ARENA.position.x, ARENA.end.x),
		randf_range(ARENA.position.y, ARENA.end.y)
	)
	orb.collected.connect(_on_orb_collected)
	_orbs.add_child(orb)


func _on_orb_collected(orb: Area2D) -> void:
	score += 1
	_score_label.text = "Puntos: %d" % score
	Juice.squash(_score_label, Vector2(1.3, 1.3))

	Audio.play("coin")
	Juice.burst(orb.position, orb.color)
	Juice.floating_text("+1", orb.position, orb.color)
	Juice.shake(0.25)
	Juice.hitstop(0.04)
	_spawn_orb.call_deferred()
