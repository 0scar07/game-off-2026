"""
Música y sonido de ambiente de los capítulos 2 y 3, y ambientes de los tres capítulos.
Igual que musica.py: cada canción tiene versión de día y de noche, con el mismo tempo, acordes
y duración, para mezclarlas en el juego según la línea. Los ambientes hacen bucle sin cortes.

    Capítulo 2 · Bosque Otoñal : guitarra pulsada y cuerdas (día) / kalimba y violonchelo (noche)
    Capítulo 3 · Cumbre Helada : celesta y coro (día) / campanas de cristal y pad helado (noche)

Uso:  python Tools/musica_capitulos.py
"""
import numpy as np
from scipy.signal import butter, lfilter

import musica as m

SR = m.SR


class Song:
    def __init__(self, bpm, bars):
        self.beat = 60.0 / bpm
        self.length = int(round(bars * 4 * self.beat * SR))
        self.buf = np.zeros((self.length + SR * 8, 2))

    def at(self, bar, beat):
        return int(round((bar * 4 + beat) * self.beat * SR))

    def add(self, start, sound, pan=0.0, gain=1.0):
        left = np.cos((pan + 1) * np.pi / 4) * gain
        right = np.sin((pan + 1) * np.pi / 4) * gain
        self.buf[start:start + len(sound), 0] += sound * left
        self.buf[start:start + len(sound), 1] += sound * right

    def looped(self):
        out = self.buf[:self.length].copy()
        tail = self.buf[self.length:]
        for i in range(0, len(tail), self.length):
            chunk = tail[i:i + self.length]
            out[:len(chunk)] += chunk
        return out


# ───────────────────────── Instrumentos ─────────────────────────

def pluck(f, dur):
    """Cuerda pulsada: muchos armónicos que se apagan antes cuanto más agudos."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    out = np.zeros(n)
    for k in range(1, 10):
        out += np.sin(2 * np.pi * f * k * t) / k ** 1.15 * np.exp(-t * (1.2 + k * 0.9))
    return out * np.minimum(t / 0.003, 1)


def strings(f, dur, attack=0.6):
    """Cuerdas: sierra suave (armónicos 1/k) con entrada lenta y vibrato."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    vib = 1 + 0.004 * np.sin(2 * np.pi * 5.2 * t)
    out = sum(np.sin(2 * np.pi * f * k * t * vib) / k for k in range(1, 9))
    envelope = np.minimum(t / attack, 1) * np.minimum(1, (dur - t) / 0.6)
    return out * np.clip(envelope, 0, 1)


def choir(f, dur):
    """Coro "aaa": armónicos pesados según los formantes de la vocal."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    vib = 1 + 0.006 * np.sin(2 * np.pi * 4.8 * t)
    out = np.zeros(n)
    for k in range(1, 16):
        h = f * k
        weight = np.exp(-((h - 800) / 220) ** 2) + 0.7 * np.exp(-((h - 1150) / 260) ** 2) + 0.35 * np.exp(-((h - 2900) / 500) ** 2) + 0.15 / k
        out += weight * np.sin(2 * np.pi * h * t * vib)
    envelope = np.minimum(t / 0.9, 1) * np.clip((dur - t) / 0.8, 0, 1)
    return out * envelope


def bell(f, dur, decay=1.2):
    """Campana o celesta: parciales inarmónicos."""
    return m.tone(f, dur, [(1, 1.0), (2.0, 0.45), (3.01, 0.25), (4.2, 0.18), (5.43, 0.1)], 0.002, decay)


def kalimba(f, dur):
    return m.tone(f, dur, [(1, 1.0), (3.0, 0.3), (5.8, 0.08)], 0.002, 0.45)


def bass(f, dur):
    return m.tone(f, dur, [(1, 1), (2, 0.3), (3, 0.08)], 0.01, 0.6)


# ───────────────────────── Canciones ─────────────────────────

def autumn(night, rng):
    """Re menor, 76 BPM: Dm7 · B♭maj7 · Gm7 · A7, cuatro veces."""
    chords = [[50, 53, 57, 60], [46, 50, 53, 57], [43, 46, 50, 53], [45, 49, 52, 55]] * 4
    phrase = [(0, 0, 69, 1.5), (0, 1.5, 72, .5), (0, 2, 74, 2), (1, 0, 74, 1), (1, 1, 72, 1), (1, 2, 69, 2),
              (2, 0, 67, 1.5), (2, 1.5, 70, .5), (2, 2, 74, 1), (2, 3, 72, 1), (3, 0, 69, 3),
              (4, 0, 77, 1.5), (4, 1.5, 76, .5), (4, 2, 74, 2), (5, 0, 72, 1), (5, 1, 74, 1), (5, 2, 69, 2),
              (6, 0, 70, 1), (6, 1, 72, 1), (6, 2, 74, 1), (6, 3, 76, 1), (7, 0, 73, 2), (7, 2, 69, 2)]
    melody = phrase + [(b + 8, t, n, d) for (b, t, n, d) in phrase if b < 7] + [(15, 0, 74, 4)]
    song = Song(76, 16)
    for bar, chord in enumerate(chords):
        if not night:
            order = [0, 2, 1, 3, 2, 1, 3, 2]
            for step in range(8):
                note = chord[order[step]] + 12
                song.add(song.at(bar, step * .5), pluck(m.freq(note), 1.6), pan=-.3 if step % 2 else .3, gain=.11)
            for note in chord[1:]:
                song.add(song.at(bar, 0), strings(m.freq(note), 4 * song.beat + 1.0), pan=.0, gain=.02)
            song.add(song.at(bar, 0), bass(m.freq(chord[0] - 12), 2.4), gain=.3)
            song.add(song.at(bar, 2), bass(m.freq(chord[0] - 12), 2.0), gain=.22)
        else:
            song.add(song.at(bar, 0), strings(m.freq(chord[0] - 12), 4 * song.beat + 1.2, attack=1.2), gain=.07)
            for note in chord[1:]:
                song.add(song.at(bar, 0), strings(m.freq(note), 4 * song.beat + 1.2, attack=1.4), pan=float(rng.uniform(-.5, .5)), gain=.016)
            if rng.random() < .6:
                note = int(rng.choice(chord)) + 24
                song.add(song.at(bar, float(rng.choice([1.5, 2.5, 3.5]))), bell(m.freq(note), 2.5, 0.9), pan=float(rng.uniform(-.7, .7)), gain=.05)
    for bar, beat, note, dur in melody:
        if night:
            song.add(song.at(bar, beat), kalimba(m.freq(note), 2.0), pan=.1, gain=.2)
        else:
            song.add(song.at(bar, beat), strings(m.freq(note), dur * song.beat + .4, attack=.12), pan=-.1, gain=.05)
    return m.reverb(song.looped(), 3.2 if night else 1.6, .42 if night else .22, 3 if night else 4)


def snow(night, rng):
    """Mi, 70 BPM: Emaj7 · C#m7 · Amaj7 · B6sus, cuatro veces."""
    chords = [[52, 56, 59, 63], [49, 52, 56, 59], [45, 49, 52, 56], [47, 51, 54, 56]] * 4
    phrase = [(0, 0, 76, 2), (0, 2, 75, 1), (0, 3, 71, 1), (1, 0, 73, 3), (1, 3, 71, 1),
              (2, 0, 69, 1), (2, 1, 71, 1), (2, 2, 73, 2), (3, 0, 71, 4),
              (4, 0, 80, 2), (4, 2, 78, 1), (4, 3, 76, 1), (5, 0, 75, 2), (5, 2, 73, 2),
              (6, 0, 73, 1), (6, 1, 75, 1), (6, 2, 76, 2), (7, 0, 75, 4)]
    melody = phrase + [(b + 8, t, n, d) for (b, t, n, d) in phrase if b < 7] + [(15, 0, 76, 4)]
    song = Song(70, 16)
    for bar, chord in enumerate(chords):
        if not night:
            for note in chord:
                song.add(song.at(bar, 0), choir(m.freq(note), 4 * song.beat + 0.9), pan=float(rng.uniform(-.4, .4)), gain=.018)
            order = [0, 1, 2, 3, 2, 1]
            for step in range(6):
                note = chord[order[step]] + 24
                song.add(song.at(bar, step * 4 / 6), bell(m.freq(note), 1.6, 0.7), pan=-.35 if step % 2 else .35, gain=.06)
            song.add(song.at(bar, 0), bass(m.freq(chord[0] - 12), 3.0), gain=.22)
        else:
            for note in chord:
                for detune in (-.003, .003):
                    tone = m.tone(m.freq(note + 12) * (1 + detune), 4 * song.beat + 1.5, [(1, 1), (2, .2)], 1.4, 7.0)
                    fade = np.clip((len(tone) - np.arange(len(tone))) / (1.3 * SR), 0, 1)
                    song.add(song.at(bar, 0), tone * fade, pan=detune * 150, gain=.028)
            song.add(song.at(bar, 0), m.tone(m.freq(chord[0] - 24), 4 * song.beat + .5, [(1, 1)], .5, 3.0), gain=.25)
            for _ in range(2):
                if rng.random() < .7:
                    note = int(rng.choice(chord)) + 36
                    song.add(song.at(bar, float(rng.uniform(0, 3.5))), bell(m.freq(note), 2.0, 0.6), pan=float(rng.uniform(-.8, .8)), gain=.035)
    for bar, beat, note, dur in melody:
        if night:
            song.add(song.at(bar, beat), bell(m.freq(note + 12), 3.0, 1.4), pan=.15, gain=.12)
        else:
            song.add(song.at(bar, beat), bell(m.freq(note), 2.5, 1.1), pan=-.15, gain=.14)
    return m.reverb(song.looped(), 4.0 if night else 2.4, .5 if night else .3, 5 if night else 6)


# ───────────────────────── Ambientes ─────────────────────────

AMBIENCE = 24.0


def band_noise(dur, low, high, rng, order=2):
    n = int(dur * SR)
    b, a = butter(order, [low / (SR / 2), high / (SR / 2)], btype="band")
    return lfilter(b, a, rng.standard_normal(n))


def lfo(n, speed, rng, depth=0.5):
    t = np.arange(n) / SR
    return 1 - depth + depth * (0.5 + 0.5 * np.sin(2 * np.pi * speed * t + rng.uniform(0, 6)) * np.sin(2 * np.pi * speed * 0.37 * t + rng.uniform(0, 6)))


def wind(rng, low=180, high=900, depth=0.6, gain=0.25):
    noise = band_noise(AMBIENCE + 2, low, high, rng)
    return noise / np.abs(noise).max() * lfo(len(noise), 0.11, rng, depth) * gain


def chirp(f0, f1, dur):
    n = int(dur * SR)
    t = np.arange(n) / SR
    f = np.linspace(f0, f1, n)
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * np.sin(np.pi * t / dur) ** 2


def place(bed, sound, start, gain, pan=0.0):
    start = int(start * SR)
    end = min(start + len(sound), len(bed))
    l = np.cos((pan + 1) * np.pi / 4) * gain
    r = np.sin((pan + 1) * np.pi / 4) * gain
    bed[start:end, 0] += sound[:end - start] * l
    bed[start:end, 1] += sound[:end - start] * r


def stereo(mono):
    return np.stack([mono, mono], axis=1)


def seamless(audio):
    """Funde los últimos 2 s con los primeros para que el bucle no se note."""
    n = int(AMBIENCE * SR)
    fade = int(2 * SR)
    out = audio[:n].copy()
    tail = audio[n:n + fade]
    ramp = np.linspace(0, 1, len(tail))[:, None]
    out[:len(tail)] = out[:len(tail)] * ramp + tail * (1 - ramp)
    return out


def birds(rng):
    bed = stereo(wind(rng, 300, 1500, gain=0.06))
    t = 0.3
    while t < AMBIENCE:
        base = rng.uniform(2600, 4200)
        pan = rng.uniform(-.8, .8)
        for k in range(int(rng.integers(2, 6))):
            place(bed, chirp(base, base * rng.uniform(1.1, 1.4), rng.uniform(.05, .11)), t + k * rng.uniform(.09, .16), .12, pan)
        t += rng.uniform(.8, 2.6)
    return bed


def crickets(rng, gain=.05):
    bed = stereo(wind(rng, 120, 500, gain=0.05))
    for voice in range(3):
        f = rng.uniform(4200, 5200)
        pan = rng.uniform(-.8, .8)
        t = rng.uniform(0, 1)
        while t < AMBIENCE:
            n = int(.018 * SR)
            pulse = np.sin(2 * np.pi * f * np.arange(n) / SR) * np.hanning(n)
            for k in range(3):
                place(bed, pulse, t + k * .03, gain, pan)
            t += rng.uniform(.35, .6)
    return bed


def owls(rng, bed):
    t = rng.uniform(1, 4)
    while t < AMBIENCE - 2:
        pan = rng.uniform(-.9, .9)
        for k, pitch in enumerate((390, 360)):
            n = int(.38 * SR)
            tt = np.arange(n) / SR
            hoot = (np.sin(2 * np.pi * pitch * tt) + .25 * np.sin(4 * np.pi * pitch * tt)) * np.sin(np.pi * tt / .38) ** 1.5
            place(bed, hoot, t + k * .55, .09, pan)
        t += rng.uniform(5, 9)
    return bed


def chimes(rng, bed, notes, gain=.04, gap=(2.0, 5.0)):
    t = rng.uniform(.5, 2)
    while t < AMBIENCE - 1:
        place(bed, bell(m.freq(int(rng.choice(notes))), 2.5, .8), t, gain, rng.uniform(-.8, .8))
        t += rng.uniform(*gap)
    return bed


def leaves(rng):
    bed = stereo(wind(rng, 600, 4000, depth=.8, gain=.12))
    return bed


if __name__ == "__main__":
    rng = np.random.default_rng(76)
    m.write("otono_dia.wav", autumn(False, rng))
    m.write("otono_noche.wav", autumn(True, rng))
    m.write("nieve_dia.wav", snow(False, rng))
    m.write("nieve_noche.wav", snow(True, rng))

    m.write("amb_pradera_dia.wav", seamless(birds(rng)))
    m.write("amb_pradera_noche.wav", seamless(crickets(rng)))
    m.write("amb_bosque_dia.wav", seamless(leaves(rng) + stereo(wind(rng, 200, 700, gain=.12))))
    m.write("amb_bosque_noche.wav", seamless(owls(rng, crickets(rng, .025) + stereo(wind(rng, 150, 600, gain=.08)))))
    m.write("amb_cumbre_dia.wav", seamless(chimes(rng, stereo(wind(rng, 250, 1400, depth=.8, gain=.28)), [88, 90, 92, 95])))
    m.write("amb_cumbre_noche.wav", seamless(chimes(rng, stereo(wind(rng, 100, 500, depth=.7, gain=.3)), [95, 97, 100], .03, (3, 7))))
