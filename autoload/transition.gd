extends CanvasLayer
## Cambios de escena con fundido a negro.
## Uso: Transition.change_scene("res://scenes/game/game.tscn")

const DURATION := 0.3

var is_changing := false

@onready var _fade: ColorRect = $Fade


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_fade.modulate.a = 0.0
	_fade.mouse_filter = Control.MOUSE_FILTER_IGNORE


func change_scene(path: String) -> void:
	if is_changing:
		return
	is_changing = true
	# Bloquea los clics durante el fundido.
	_fade.mouse_filter = Control.MOUSE_FILTER_STOP
	await _tween_fade(1.0)
	get_tree().paused = false
	get_tree().change_scene_to_file(path)
	await get_tree().process_frame
	await _tween_fade(0.0)
	_fade.mouse_filter = Control.MOUSE_FILTER_IGNORE
	is_changing = false


func reload_scene() -> void:
	change_scene(get_tree().current_scene.scene_file_path)


func _tween_fade(alpha: float) -> void:
	var tween := create_tween()
	tween.tween_property(_fade, "modulate:a", alpha, DURATION)
	await tween.finished
