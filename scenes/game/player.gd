extends CharacterBody2D
## Jugador de ejemplo con vista cenital: se mueve con move_* y hace un dash con "action".
## Sirve para comprobar que la plantilla funciona; cámbialo por el de tu juego.

signal dashed

@export var speed := 420.0
@export var acceleration := 3000.0
@export var dash_speed := 1300.0
@export var dash_time := 0.15
@export var dash_cooldown := 0.3

var _dash_left := 0.0
var _cooldown_left := 0.0
var _bumped := false

@onready var _body: Polygon2D = $Body
@onready var _trail: CPUParticles2D = $Trail


func is_dashing() -> bool:
	return _dash_left > 0.0


func _physics_process(delta: float) -> void:
	var input := Input.get_vector("move_left", "move_right", "move_up", "move_down")
	_cooldown_left -= delta

	if Input.is_action_just_pressed("action") and _cooldown_left <= 0.0:
		_dash(input)

	if _dash_left > 0.0:
		_dash_left -= delta
	else:
		velocity = velocity.move_toward(input * speed, acceleration * delta)
	_trail.emitting = is_dashing()

	if velocity.length() > 10.0:
		rotation = velocity.angle()
	move_and_slide()

	# Chocar contra una pared durante el dash: empujón de cámara y sonido.
	if is_dashing() and get_slide_collision_count() > 0 and not _bumped:
		_bumped = true
		var normal := get_slide_collision(0).get_normal()
		Juice.kick(-normal, 18.0)
		Juice.shake(0.2)
		Audio.play("hit", -4.0)
		Juice.squash(_body, Vector2(0.6, 1.4))


func _dash(input: Vector2) -> void:
	var direction := input if input != Vector2.ZERO else Vector2.RIGHT.rotated(rotation)
	velocity = direction.normalized() * dash_speed
	_dash_left = dash_time
	_cooldown_left = dash_cooldown
	_bumped = false
	Audio.play("dash")
	Juice.squash(_body, Vector2(1.5, 0.6))
	Juice.shake(0.12)
	dashed.emit()
