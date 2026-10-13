class_name SfxGenerator
## Genera efectos de sonido retro por código (estilo sfxr), sin archivos de audio.
## Sirven para tener sonido desde el primer minuto; cámbialos luego por los tuyos.

const MIX_RATE := 22050

## Sonidos incluidos. Cada uno es una receta para `generate()`.
const PRESETS := {
	"click": {"wave": "square", "freq": 880.0, "freq_end": 660.0, "length": 0.05, "volume": 0.35},
	"hover": {"wave": "sine", "freq": 1320.0, "freq_end": 1320.0, "length": 0.03, "volume": 0.2},
	"dash": {"wave": "noise", "freq": 2400.0, "freq_end": 600.0, "length": 0.18, "volume": 0.35},
	"jump": {"wave": "square", "freq": 300.0, "freq_end": 640.0, "length": 0.14, "volume": 0.3},
	"hit": {"wave": "square", "freq": 260.0, "freq_end": 70.0, "length": 0.14, "volume": 0.45},
	"coin": {"wave": "square", "freq": 988.0, "freq_end": 988.0, "jump_to": 1319.0, "jump_at": 0.35, "length": 0.22, "volume": 0.3},
	"powerup": {"wave": "saw", "freq": 300.0, "freq_end": 1200.0, "length": 0.4, "volume": 0.25, "vibrato": 12.0},
	"explosion": {"wave": "noise", "freq": 900.0, "freq_end": 80.0, "length": 0.55, "volume": 0.6},
	"lose": {"wave": "square", "freq": 440.0, "freq_end": 90.0, "length": 0.6, "volume": 0.3, "vibrato": 8.0},
}


static func generate_preset(preset_name: String) -> AudioStreamWAV:
	return generate(PRESETS[preset_name])


## Opciones de `recipe`:
##   wave: "square", "saw", "sine" o "noise"
##   freq / freq_end: tono inicial y final en Hz (desliza entre ellos)
##   length: duración en segundos
##   volume: 0..1
##   jump_to / jump_at: salto de tono a `jump_to` Hz en la fracción `jump_at` (como una moneda)
##   vibrato: oscilaciones por segundo del tono
static func generate(recipe: Dictionary) -> AudioStreamWAV:
	var wave: String = recipe.get("wave", "square")
	var length: float = recipe.get("length", 0.2)
	var volume: float = recipe.get("volume", 0.5)
	var freq_start: float = recipe.get("freq", 440.0)
	var freq_end: float = recipe.get("freq_end", freq_start)
	var jump_to: float = recipe.get("jump_to", 0.0)
	var jump_at: float = recipe.get("jump_at", 1.0)
	var vibrato: float = recipe.get("vibrato", 0.0)

	var samples := int(length * MIX_RATE)
	var data := PackedByteArray()
	data.resize(samples * 2)
	var rng := RandomNumberGenerator.new()
	rng.seed = hash(recipe)
	var phase := 0.0
	var noise_value := 0.0
	var attack := mini(int(0.005 * MIX_RATE), samples)

	for i in samples:
		var t := float(i) / samples
		var freq := lerpf(freq_start, freq_end, t)
		if jump_to > 0.0 and t >= jump_at:
			freq = jump_to
		if vibrato > 0.0:
			freq *= 1.0 + 0.05 * sin(TAU * vibrato * i / MIX_RATE)

		var previous_phase := phase
		phase = fmod(phase + freq / MIX_RATE, 1.0)
		var value := 0.0
		match wave:
			"square":
				value = 1.0 if phase < 0.5 else -1.0
			"saw":
				value = phase * 2.0 - 1.0
			"sine":
				value = sin(TAU * phase)
			"noise":
				# Nuevo valor aleatorio en cada ciclo: más agudo cuanto mayor es freq.
				if phase < previous_phase:
					noise_value = rng.randf_range(-1.0, 1.0)
				value = noise_value

		# Envolvente: subida muy corta (evita clics) y bajada lineal hasta el final.
		var envelope := (1.0 - t) if i >= attack else float(i) / attack
		var sample := clampf(value * envelope * volume, -1.0, 1.0)
		data.encode_s16(i * 2, int(sample * 32767.0))

	var stream := AudioStreamWAV.new()
	stream.format = AudioStreamWAV.FORMAT_16_BITS
	stream.mix_rate = MIX_RATE
	stream.stereo = false
	stream.data = data
	return stream
