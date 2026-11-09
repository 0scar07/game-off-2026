"""Sonidos pequeños que se cargan desde Resources/Sonidos (sin pasar por el generador de niveles):
el "tin" al pasar por un botón, el cascabel del farol al caminar y el estornudo de Lumi.

Uso: python Tools/sonidos_ui.py
"""
import os
import numpy as np
from scipy.io import wavfile

SR = 44100
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Sonidos")
rng = np.random.default_rng(3)


def save(name, x, gain=0.6):
    x = x / (np.max(np.abs(x)) + 1e-9) * gain
    os.makedirs(OUT, exist_ok=True)
    wavfile.write(os.path.join(OUT, name + ".wav"), SR, (x * 32767).astype(np.int16))
    print(name, f"{len(x) / SR:.2f} s")


def bell(freq, dur, decay):
    t = np.arange(int(SR * dur)) / SR
    tone = sum(a * np.sin(2 * np.pi * freq * m * t) for m, a in ((1, 1), (2.76, 0.35), (5.4, 0.12)))
    return tone * np.exp(-t * decay) * np.minimum(1, t / 0.002)


# Botón: un "tin" corto y suave.
save("boton", bell(1760, 0.18, 30), 0.35)

# Farol: dos campanitas muy cerca, como un cascabel.
x = bell(2349, 0.35, 14)
x[int(SR * 0.03):] += bell(2637, 0.35 - 0.03, 16) * 0.7
save("farol", x, 0.3)

# Estornudo: "a..." (ruido filtrado que sube) y "¡chís!" (golpe de ruido agudo).
t1 = np.arange(int(SR * 0.28)) / SR
breath = rng.uniform(-1, 1, len(t1)) * np.sin(np.pi * t1 / t1[-1]) * 0.25
voice = np.sin(2 * np.pi * (380 + 240 * t1 / t1[-1]) * t1) * np.sin(np.pi * t1 / t1[-1]) * 0.35
t2 = np.arange(int(SR * 0.22)) / SR
hiss = rng.uniform(-1, 1, len(t2))
hiss = np.convolve(hiss, [1, -0.95], mode="same") * np.exp(-t2 * 16) * np.minimum(1, t2 / 0.004)
save("estornudo", np.concatenate([breath + voice, np.zeros(int(SR * 0.04)), hiss]), 0.55)

# Nota de campana (Do agudo): cada fragmento la toca subiendo por la escala (MusicLayers).
save("nota", bell(1046.5, 1.2, 4.5), 0.5)

# Pluma, hoja o copo: un brillo corto de dos notas.
x = bell(1568, 0.5, 9)
x[int(SR * 0.07):] += bell(2093, 0.5 - 0.07, 9) * 0.8
save("pluma", x, 0.4)
