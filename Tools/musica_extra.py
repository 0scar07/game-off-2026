"""Capas extra de música: el brillo de la línea (una por capítulo, en su tonalidad y con el
mismo largo que su canción para que vaya al compás) y los tambores de la Gran Sombra.

Uso: python Tools/musica_extra.py
"""
import os
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, lfilter, fftconvolve

SR = 44100
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio", "Music")
rng = np.random.default_rng(9)


def bell(freq, dur):
    x = np.arange(int(SR * dur)) / SR
    return sum(a * np.sin(2 * np.pi * freq * m * x) * np.exp(-x * d)
               for m, a, d in ((1, 1, 1.8), (2.0, 0.4, 3), (3.0, 0.15, 5), (4.2, 0.08, 8)))


def save(name, x, gain=0.7):
    # Reverb simple para que el brillo flote.
    ir = rng.uniform(-1, 1, int(SR * 2.5)) * np.exp(-np.arange(int(SR * 2.5)) / SR * 2.2)
    wet = fftconvolve(x, ir)[:len(x)] * 0.02
    # La cola que sobra vuelve al principio: el bucle no tiene corte.
    tail = fftconvolve(x, ir)[len(x):] * 0.02
    wet[:len(tail)] += tail[:len(x)]
    y = x * 0.6 + wet
    y = y / (np.max(np.abs(y)) + 1e-9) * gain
    wavfile.write(os.path.join(OUT, name + ".wav"), SR, (y * 32767).astype(np.int16))
    print(name, f"{len(y) / SR:.2f} s")


def shimmer(name, notes, bpm, bars):
    """Campanitas agudas en corcheas al azar sobre las notas dadas (Hz)."""
    beat = 60 / bpm
    total = int(SR * beat * 4 * bars)
    out = np.zeros(total + SR * 3)
    step = beat / 2
    for i in range(int(4 * bars * 2)):
        if rng.random() < 0.55:
            f = notes[rng.integers(len(notes))] * (2 if rng.random() < 0.3 else 1)
            n = bell(f, 2.5) * rng.uniform(0.4, 1.0)
            s = int(i * step * SR)
            out[s:s + len(n)] += n
    # Lo que se sale del final se suma al principio (bucle perfecto).
    out[:SR * 3] += out[total:total + SR * 3]
    save(name, out[:total], 0.6)


def drums(name, bpm, bars):
    """Tambores graves tipo taiko para el jefe."""
    beat = 60 / bpm
    total = int(SR * beat * 4 * bars)
    out = np.zeros(total + SR)
    x = np.arange(int(SR * 0.8)) / SR

    def hit(freq, amp):
        f = freq * (1 + 1.5 * np.exp(-x * 30))
        body = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-x * 7)
        b, a = butter(2, 900 / (SR / 2))
        click = lfilter(b, a, rng.uniform(-1, 1, len(x))) * np.exp(-x * 60) * 0.5
        return (body + click) * amp

    pattern = [(0, 1.0, 60), (1.5, 0.6, 75), (2, 0.9, 60), (3, 0.5, 90), (3.5, 0.6, 90)]
    for bar in range(bars):
        for pos, amp, freq in pattern:
            s = int((bar * 4 + pos) * beat * SR)
            h = hit(freq, amp)
            out[s:s + len(h)] += h
    out[:SR] += out[total:total + SR]
    save(name, out[:total], 0.8)


if __name__ == "__main__":
    D, Fs, A, E, B, Cs = 1174.7, 1480.0, 1760.0, 1318.5, 987.8 * 2, 1108.7 * 2
    # Mismo tempo y largo que dia.wav (84 BPM), otono (76) y nieve (70): 16 compases.
    shimmer("linea_pradera", [D, A, Fs], 84, 16)
    shimmer("linea_bosque", [D, A, 1396.9], 76, 16)
    shimmer("linea_cumbre", [E, B, 1661.2], 70, 16)
    drums("jefe_tambores", 70, 16)
    drums("jefe_tambores_pradera", 84, 16)
    drums("jefe_tambores_bosque", 76, 16)
