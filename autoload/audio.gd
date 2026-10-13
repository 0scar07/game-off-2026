extends Node
## Música con fundido entre pistas y efectos de sonido con variación de tono.
## Uso:
##   Audio.play_music(preload("res://assets/audio/tema.ogg"))
##   Audio.play_sfx(preload("res://assets/audio/salto.wav"))
##   Audio.play("coin")  # sonidos generados por código, ver SfxGenerator.PRESETS

const SFX_PLAYERS := 16

## Sonidos con nombre. Empiezan siendo los generados por código; puedes sustituirlos:
##   Audio.sounds["coin"] = preload("res://assets/audio/moneda.wav")
var sounds: Dictionary[String, AudioStream] = {}

var _music_a := AudioStreamPlayer.new()
var _music_b := AudioStreamPlayer.new()
var _current_music: AudioStreamPlayer
var _sfx_players: Array[AudioStreamPlayer] = []
var _next_sfx := 0


func _ready() -> void:
	# La música y los sonidos siguen sonando con el juego en pausa (menús).
	process_mode = Node.PROCESS_MODE_ALWAYS
	for player in [_music_a, _music_b]:
		player.bus = "Music"
		add_child(player)
	_current_music = _music_a
	for i in SFX_PLAYERS:
		var player := AudioStreamPlayer.new()
		player.bus = "SFX"
		add_child(player)
		_sfx_players.append(player)
	for preset_name in SfxGenerator.PRESETS:
		sounds[preset_name] = SfxGenerator.generate_preset(preset_name)


## Reproduce un sonido de `sounds` por su nombre.
func play(sound_name: String, volume_db := 0.0, pitch_variation := 0.1) -> void:
	if not sounds.has(sound_name):
		push_warning("Audio.play: no existe el sonido '%s'" % sound_name)
		return
	play_sfx(sounds[sound_name], volume_db, pitch_variation)


## Cambia la música con un fundido de `fade` segundos. Si ya suena esa pista, no hace nada.
func play_music(stream: AudioStream, fade := 1.0) -> void:
	if stream and _current_music.stream == stream and _current_music.playing:
		return
	var old := _current_music
	var new := _music_b if old == _music_a else _music_a
	_current_music = new

	if old.playing:
		var fade_out := create_tween()
		fade_out.tween_property(old, "volume_db", -60.0, fade)
		fade_out.tween_callback(old.stop)
	if stream:
		new.stream = stream
		new.volume_db = -60.0
		new.play()
		create_tween().tween_property(new, "volume_db", 0.0, fade)


func stop_music(fade := 1.0) -> void:
	play_music(null, fade)


## Reproduce un efecto. `pitch_variation` cambia un poco el tono cada vez para que no canse.
func play_sfx(stream: AudioStream, volume_db := 0.0, pitch_variation := 0.1) -> void:
	if stream == null:
		return
	var player := _sfx_players[_next_sfx]
	_next_sfx = (_next_sfx + 1) % SFX_PLAYERS
	player.stream = stream
	player.volume_db = volume_db
	player.pitch_scale = 1.0 + randf_range(-pitch_variation, pitch_variation)
	player.play()
