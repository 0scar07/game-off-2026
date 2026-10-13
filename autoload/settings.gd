extends Node
## Ajustes del jugador (volumen, pantalla completa...), guardados en user://settings.cfg.
## Accesible desde cualquier script como `Settings`.

signal changed

const PATH := "user://settings.cfg"

var music_volume := 0.8:
	set(value):
		music_volume = clampf(value, 0.0, 1.0)
		_apply_bus_volume("Music", music_volume)
var sfx_volume := 0.8:
	set(value):
		sfx_volume = clampf(value, 0.0, 1.0)
		_apply_bus_volume("SFX", sfx_volume)
var fullscreen := false:
	set(value):
		fullscreen = value
		_apply_fullscreen()


func _ready() -> void:
	load_settings()


func load_settings() -> void:
	var config := ConfigFile.new()
	if config.load(PATH) == OK:
		music_volume = config.get_value("audio", "music_volume", music_volume)
		sfx_volume = config.get_value("audio", "sfx_volume", sfx_volume)
		fullscreen = config.get_value("display", "fullscreen", fullscreen)
	else:
		# Primera vez: aplicar los valores por defecto.
		music_volume = music_volume
		sfx_volume = sfx_volume


func save_settings() -> void:
	var config := ConfigFile.new()
	config.set_value("audio", "music_volume", music_volume)
	config.set_value("audio", "sfx_volume", sfx_volume)
	config.set_value("display", "fullscreen", fullscreen)
	config.save(PATH)
	changed.emit()


## true en móviles y navegadores de móvil, donde no hay ventana que redimensionar.
func is_mobile() -> bool:
	return OS.has_feature("mobile") or OS.has_feature("web_android") or OS.has_feature("web_ios")


## Mostrar los controles táctiles en pantalla.
func uses_touch() -> bool:
	return is_mobile() or DisplayServer.is_touchscreen_available()


## El botón "Salir" no tiene sentido en la web.
func can_quit() -> bool:
	return not OS.has_feature("web")


func _apply_bus_volume(bus_name: String, linear: float) -> void:
	var bus := AudioServer.get_bus_index(bus_name)
	if bus == -1:
		return
	AudioServer.set_bus_volume_db(bus, linear_to_db(linear))
	AudioServer.set_bus_mute(bus, linear <= 0.001)


func _apply_fullscreen() -> void:
	if is_mobile() or not is_inside_tree():
		return
	var mode := DisplayServer.WINDOW_MODE_FULLSCREEN if fullscreen else DisplayServer.WINDOW_MODE_WINDOWED
	if DisplayServer.window_get_mode() != mode:
		DisplayServer.window_set_mode(mode)
