"""Música del tráiler de FASE (50 s, 96 BPM): intro de campanas, golpe en el título, tambores
en el montaje, tensión con la Gran Sombra y final luminoso en Re mayor.
Uso: python Tools/musica_trailer.py  → Trailer/musica.wav (y Trailer/golpes.txt con los tiempos)
"""
import os
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, lfilter, fftconvolve

SR = 44100
BPM = 96
BEAT = 60 / BPM
LENGTH = 50.0
OUT = os.path.join(os.path.dirname(__file__), "..", "Trailer")
rng = np.random.default_rng(4)
L = np.zeros(int(SR * (LENGTH + 4)))
R = np.zeros_like(L)


def t(d):
    return np.arange(int(SR * d)) / SR


def add(at, x, pan=0.0, gain=1.0):
    s = int(at * SR)
    x = x[: max(0, len(L) - s)] * gain
    L[s:s + len(x)] += x * (1 - max(pan, 0))
    R[s:s + len(x)] += x * (1 + min(pan, 0))


def hz(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def bell(f, d=3.0):
    x = t(d)
    return sum(a * np.sin(2 * np.pi * f * m * x) * np.exp(-x * k) for m, a, k in ((1, 1, 1.5), (2.76, .3, 4), (5.4, .1, 8)))


def pad(freqs, d, attack=1.5):
    x = t(d)
    out = sum(np.sin(2 * np.pi * f * x + np.sin(2 * np.pi * 0.3 * x) * 0.3) + 0.3 * np.sin(2 * np.pi * f * 2.003 * x) for f in freqs)
    e = np.minimum(1, x / attack) * np.minimum(1, (d - x) / 1.0)
    return out * e / len(freqs)


def brass(freqs, d):
    """Acorde metálico (sierra filtrada) para los golpes."""
    x = t(d)
    out = np.zeros(len(x))
    for f in freqs:
        for detune in (0.997, 1.0, 1.004):
            ph = (f * detune * x) % 1.0
            out += 2 * ph - 1
    b, a = butter(2, 1800 / (SR / 2))
    out = lfilter(b, a, out) / (len(freqs) * 3)
    return out * np.exp(-x * 1.2) * np.minimum(1, x / 0.01)


def taiko(f=55, amp=1.0):
    x = t(1.2)
    freq = f * (1 + 1.2 * np.exp(-x * 25))
    body = np.sin(2 * np.pi * np.cumsum(freq) / SR) * np.exp(-x * 5)
    b, a = butter(2, 700 / (SR / 2))
    skin = lfilter(b, a, rng.uniform(-1, 1, len(x))) * np.exp(-x * 40) * 0.6
    return (body + skin) * amp


def snare():
    x = t(0.4)
    b, a = butter(2, [1500 / (SR / 2), 8000 / (SR / 2)], btype="band")
    return lfilter(b, a, rng.uniform(-1, 1, len(x))) * np.exp(-x * 14) * 0.5 + np.sin(2 * np.pi * 190 * x) * np.exp(-x * 25) * 0.4


def riser(d):
    x = t(d)
    b, a = butter(2, [800 / (SR / 2), 9000 / (SR / 2)], btype="band")
    n = lfilter(b, a, rng.uniform(-1, 1, len(x)))
    return n * (x / d) ** 2 * 0.6 + np.sin(2 * np.pi * np.cumsum(200 + 1800 * (x / d) ** 2) / SR) * (x / d) ** 3 * 0.3


def sub_boom():
    x = t(3)
    return np.sin(2 * np.pi * np.cumsum(60 * np.exp(-x * 0.8) + 30) / SR) * np.exp(-x * 1.2)


D = 62  # Re4
hits = []

# 0–12 s: intro. Pad grave y campanas Re–La–Fa# que caen como estrellas.
add(0, pad([hz(38), hz(45), hz(50)], 13, 3), gain=0.5)
for i, (beat, n) in enumerate([(0, 74), (2, 81), (3, 78), (5, 76), (6, 74), (8, 81), (10, 85), (11, 81), (12, 78), (14, 76), (16, 74), (17, 78)]):
    add(1.0 + beat * BEAT * 0.5, bell(hz(n)), pan=(-0.4 if i % 2 else 0.4), gain=0.35)

# 11–12.5: aspiración antes del título.
add(10.6, riser(1.9), gain=0.7)

# 12.5 s: GOLPE del título.
T = 12.5
hits.append(T)
add(T, taiko(48, 1.4)); add(T, sub_boom(), gain=0.9); add(T, brass([hz(50), hz(57), hz(62), hz(66)], 3.5), gain=0.9)

# 13–34 s: montaje con tambores (cada compás un golpe fuerte, para cortar a ritmo).
start = T + 2 * BEAT
bar = 4 * BEAT
chords = [[50, 57, 62, 66], [46, 53, 58, 62], [43, 50, 55, 59], [45, 52, 57, 61]]  # Re · Si♭ · Sol · La
b = 0
while start + b * bar < 33.6:
    at = start + b * bar
    hits.append(round(at, 3))
    add(at, taiko(52, 1.0)); add(at + 1.5 * BEAT, taiko(65, 0.6)); add(at + 2 * BEAT, taiko(52, 0.8))
    add(at + 3 * BEAT, taiko(70, 0.5)); add(at + 3.5 * BEAT, taiko(70, 0.5))
    if b >= 2:
        add(at + 1 * BEAT, snare(), gain=0.5); add(at + 3 * BEAT, snare(), gain=0.5)
    add(at, pad([hz(n) for n in chords[b % 4]], bar + 0.3, 0.05), gain=0.45)
    for k in range(8):
        add(at + k * BEAT / 2, bell(hz(chords[b % 4][k % 4] + 24), 1.0), pan=0.3 if k % 2 else -0.3, gain=0.12)
    b += 1

# 34–40 s: la Gran Sombra. Silencio casi total, latido grave y tensión.
BOSS = 34.0
hits.append(BOSS)
add(BOSS, sub_boom(), gain=1.0)
for k in range(8):
    add(BOSS + 0.6 + k * BEAT, taiko(40, 0.9 if k % 2 == 0 else 0.5))
add(BOSS, pad([hz(38), hz(44), hz(50)], 6.5, 2), gain=0.6)
add(38.4, riser(1.6), gain=0.8)

# 40 s: amanecer. Re mayor brillante y final.
END = 40.0
hits.append(END)
add(END, taiko(48, 1.3)); add(END, sub_boom(), gain=0.8)
add(END, brass([hz(50), hz(57), hz(62), hz(66), hz(69)], 5), gain=0.7)
add(END, pad([hz(50), hz(57), hz(62), hz(66), hz(74)], 10, 0.3), gain=0.6)
for i, n in enumerate([74, 78, 81, 86, 90]):
    add(END + 0.4 + i * 0.18, bell(hz(n), 5), pan=(i - 2) * 0.2, gain=0.3)

# Mezcla: reverb, compresión suave, límite.
ir = rng.uniform(-1, 1, int(SR * 2.2)) * np.exp(-np.arange(int(SR * 2.2)) / SR * 2.6) * 0.015
out = []
for ch in (L, R):
    wet = fftconvolve(ch, ir)[: len(ch)]
    y = ch * 0.8 + wet * 0.5
    out.append(y)
y = np.stack(out, 1)[: int(SR * LENGTH)]
y = np.tanh(y / (np.max(np.abs(y)) + 1e-9) * 1.6) / np.tanh(1.6)
fade = int(SR * 2.5)
y[-fade:] *= np.linspace(1, 0, fade)[:, None]
os.makedirs(OUT, exist_ok=True)
wavfile.write(os.path.join(OUT, "musica.wav"), SR, (y * 0.92 * 32767).astype(np.int16))
open(os.path.join(OUT, "golpes.txt"), "w").write("\n".join(str(h) for h in hits))
print("golpes:", hits)
