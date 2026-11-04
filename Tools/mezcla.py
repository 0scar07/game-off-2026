"""Mezcla final: deja cada tipo de pista de Assets/Audio/Music al mismo volumen (RMS), para que
al mover la línea entre día y noche, o al cambiar de capítulo, no haya saltos de volumen.
Se puede ejecutar las veces que haga falta (siempre deja el mismo resultado).

Uso: python Tools/mezcla.py
"""
import glob
import os

import numpy as np
from scipy.io import wavfile

FOLDER = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio", "Music")

# Nivel de cada tipo de pista (dB RMS). La música manda; el ambiente y las capas, por debajo.
TARGETS = [
    ("amb_", -21.0),        # pájaros, grillos, viento
    ("linea_", -21.0),      # campanas cuando Lumi está sobre la línea
    ("jefe_", -17.0),       # tambores de jefe
    ("final_", -20.0),      # acorde de la meta
    ("", -18.0),            # canciones de cada capítulo (día y noche)
]
CEILING = 10 ** (-1.0 / 20)  # pico máximo: -1 dB


def level(name):
    for prefix, target in TARGETS:
        if name.startswith(prefix):
            return target
    return TARGETS[-1][1]


def main():
    for path in sorted(glob.glob(os.path.join(FOLDER, "*.wav"))):
        name = os.path.basename(path)
        sr, data = wavfile.read(path)
        x = data.astype(np.float32) / 32768.0
        rms = np.sqrt(np.mean(x ** 2)) + 1e-9
        gain = 10 ** (level(name) / 20) / rms
        y = x * gain
        # Si algún pico se pasa, se redondea suave (sin recortar en seco).
        peak = np.max(np.abs(y))
        if peak > CEILING:
            y = np.tanh(y / CEILING) * CEILING
        wavfile.write(path, sr, (y * 32767).astype(np.int16))
        after = 20 * np.log10(np.sqrt(np.mean(y ** 2)) + 1e-9)
        print(f"{name:28s} {20 * np.log10(rms):6.1f} → {after:6.1f} dB")


if __name__ == "__main__":
    main()
