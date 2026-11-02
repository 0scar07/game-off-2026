"""
Genera la música de FASE: la misma canción en dos versiones que suenan a la vez y se mezclan
según dónde esté la línea (ver MusicLayers.cs).

    Día   : marimba en arpegios, bajo pulsado, flauta con la melodía y shaker.
    Noche : colchón de sintetizador, cajita de música con la melodía (una octava arriba),
            bajo profundo y reverberación larga.

Las dos tienen el mismo tempo, acordes y duración, y hacen bucle sin cortes (la cola de la
reverberación y de las notas vuelve al principio).

Uso:  python Tools/musica.py      (escribe Assets/Audio/Music/dia.wav y noche.wav)
Requiere numpy y scipy.
"""
import os
import wave

import numpy as np
from scipy.signal import fftconvolve

SR = 44100
BPM = 84
BEAT = 60.0 / BPM
BARS = 16
LENGTH = int(round(BARS * 4 * BEAT * SR))
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio", "Music")

# Acordes por compás (MIDI). Fmaj7 · G · Em7 · Am, cuatro veces.
CHORDS = [
    [53, 57, 60, 64],  # Fmaj7
    [55, 59, 62, 67],  # G
    [52, 55, 59, 62],  # Em7
    [57, 60, 64, 69],  # Am
] * 4

# Melodía: (compás, tiempo, nota MIDI, duración en tiempos). Ocho compases que se repiten
# y un final que se queda en La para cerrar el bucle.
PHRASE = [
    (0, 0, 69, 1.5), (0, 1.5, 72, .5), (0, 2, 76, 1), (0, 3, 74, 1),
    (1, 0, 71, 1.5), (1, 1.5, 74, .5), (1, 2, 79, 2),
    (2, 0, 76, 1), (2, 1, 74, 1), (2, 2, 71, 1), (2, 3, 67, 1),
    (3, 0, 69, 3),
    (4, 0, 72, 1), (4, 1, 76, 1), (4, 2, 77, 1.5), (4, 3.5, 76, .5),
    (5, 0, 74, 1.5), (5, 1.5, 71, .5), (5, 2, 74, 1), (5, 3, 79, 1),
    (6, 0, 76, 2), (6, 2, 79, 1), (6, 3, 83, 1),
    (7, 0, 81, 2), (7, 2, 76, 2),
]
MELODY = PHRASE + [(b + 8, t, n, d) for (b, t, n, d) in PHRASE if b < 7] + [(15, 0, 69, 4)]


def freq(midi):
    return 440.0 * 2 ** ((midi - 69) / 12.0)


def at(bar, beat):
    return int(round((bar * 4 + beat) * BEAT * SR))


class Track:
    """Buffer estéreo con espacio para colas; al final las colas vuelven al principio."""

    def __init__(self):
        self.buf = np.zeros((LENGTH + SR * 8, 2))

    def add(self, start, sound, pan=0.0, gain=1.0):
        left = np.cos((pan + 1) * np.pi / 4) * gain
        right = np.sin((pan + 1) * np.pi / 4) * gain
        end = start + len(sound)
        self.buf[start:end, 0] += sound * left
        self.buf[start:end, 1] += sound * right

    def looped(self):
        out = self.buf[:LENGTH].copy()
        tail = self.buf[LENGTH:]
        for i in range(0, len(tail), LENGTH):
            chunk = tail[i:i + LENGTH]
            out[:len(chunk)] += chunk
        return out


def env(n, attack, decay):
    t = np.arange(n) / SR
    a = np.minimum(t / max(attack, 1e-4), 1.0)
    return a * np.exp(-t / decay)


def tone(f, dur, partials, attack, decay, vibrato=0.0):
    n = int(dur * SR)
    t = np.arange(n) / SR
    phase = 2 * np.pi * f * t
    if vibrato:
        phase += vibrato * np.sin(2 * np.pi * 5.0 * t) * np.minimum(t / 0.4, 1)
    wave_ = sum(amp * np.sin(phase * ratio) for ratio, amp in partials)
    return wave_ * env(n, attack, decay)


def soft_noise(dur, decay, rng):
    n = int(dur * SR)
    noise = rng.standard_normal(n)
    noise = np.diff(noise, prepend=0)  # agudo, como un shaker
    return noise * env(n, 0.002, decay) * 0.3


def reverb(track, seconds, mix, seed):
    rng = np.random.default_rng(seed)
    n = int(seconds * SR)
    t = np.arange(n) / SR
    ir = rng.standard_normal((n, 2)) * np.exp(-t / (seconds / 5))[:, None]
    ir[:int(0.01 * SR)] = 0
    wet = np.stack([fftconvolve(track[:, c], ir[:, c])[:len(track)] for c in range(2)], axis=1)
    # La cola que se sale del final vuelve al principio (bucle sin cortes).
    full = np.stack([fftconvolve(track[:, c], ir[:, c]) for c in range(2)], axis=1)
    over = full[len(track):]
    wet[:len(over)] += over[:len(track)]
    wet *= np.abs(track).max() / (np.abs(wet).max() + 1e-9)
    return track * (1 - mix) + wet * mix


def day(rng):
    tr = Track()
    marimba = [(1, 1.0), (4, 0.18), (10, 0.04)]
    for bar, chord in enumerate(CHORDS):
        notes = [n + 12 for n in chord] + [chord[1] + 24, chord[2] + 24]
        order = [0, 1, 2, 3, 4, 5, 4, 2]
        for step in range(8):
            note = notes[order[step]]
            tr.add(at(bar, step * .5), tone(freq(note), 1.0, marimba, .003, .35), pan=-.35 if step % 2 else .35, gain=.16)
        root = chord[0] - 12
        for beat in (0, 2, 2.5):
            tr.add(at(bar, beat), tone(freq(root), .9, [(1, 1), (2, .35), (3, .1)], .005, .3), gain=.32)
        for step in range(8):
            if step % 2:
                tr.add(at(bar, step * .5), soft_noise(.12, .035, rng), pan=.5, gain=.10)
    flute = [(1, 1.0), (2, .22), (3, .06)]
    for bar, beat, note, dur in MELODY:
        sound = tone(freq(note), dur * BEAT + .25, flute, .04, dur * BEAT * 1.2 + .2, vibrato=.012)
        tr.add(at(bar, beat), sound, gain=.20)
    return reverb(tr.looped(), 1.2, .18, 1)


def night(rng):
    tr = Track()
    for bar, chord in enumerate(CHORDS):
        dur = 4 * BEAT + 1.5
        for note in chord:
            for detune in (-.004, 0, .004):
                sound = tone(freq(note) * (1 + detune), dur, [(1, 1), (2, .25), (3, .08)], 1.1, 6.0)
                fade = np.minimum(1, (len(sound) - np.arange(len(sound))) / (1.2 * SR))
                tr.add(at(bar, 0), sound * fade, pan=detune * 120, gain=.035)
        sub = tone(freq(chord[0] - 24), 4 * BEAT + .5, [(1, 1), (2, .15)], .4, 3.0)
        tr.add(at(bar, 0), sub, gain=.30)
        # Estrellas: notas sueltas muy agudas en sitios al azar.
        if rng.random() < .7:
            note = rng.choice(chord) + 36
            tr.add(at(bar, float(rng.choice([1.5, 2.5, 3.5]))), tone(freq(note), 2.0, [(1, 1), (2.76, .3)], .002, .6),
                   pan=float(rng.uniform(-.8, .8)), gain=.05)
    music_box = [(1, 1.0), (2.76, .35), (5.4, .08)]
    for bar, beat, note, dur in MELODY:
        tr.add(at(bar, beat), tone(freq(note + 12), 2.5, music_box, .002, .9), pan=.15, gain=.17)
    return reverb(tr.looped(), 3.5, .45, 2)


def write(name, audio):
    audio = audio / (np.abs(audio).max() + 1e-9) * 10 ** (-3 / 20)
    data = (audio * 32767).astype(np.int16)
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, name)
    with wave.open(path, "wb") as f:
        f.setnchannels(2)
        f.setsampwidth(2)
        f.setframerate(SR)
        f.writeframes(data.tobytes())
    print(f"{os.path.normpath(path)}: {len(data) / SR:.1f} s")


if __name__ == "__main__":
    rng = np.random.default_rng(84)
    write("dia.wav", day(rng))
    write("noche.wav", night(rng))
