"""Efectos de sonido propios de FASE, sintetizados (sin samples).

Uso: python Tools/efectos.py [carpeta]   (por defecto Tools/efectos_prueba)
Cada efecto suena en la tonalidad de la música (Re mayor) para que encaje.
"""
import os, sys
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, lfilter

SR = 44100
rng = np.random.default_rng(3)


def t(dur):
    return np.arange(int(SR * dur)) / SR


def env(n, attack=0.005, release=0.2, curve=4.0):
    """Envolvente con ataque lineal y caída exponencial."""
    x = np.arange(n) / SR
    a = np.clip(x / max(attack, 1e-4), 0, 1)
    r = np.exp(-curve * np.maximum(x - attack, 0) / max(release, 1e-4))
    return a * r


def note(freq, dur, kind="bell", **kw):
    x = t(dur)
    if kind == "bell":  # campanita: parciales inarmónicos que caen a distinto ritmo
        out = sum(a * np.sin(2 * np.pi * freq * m * x) * np.exp(-x * d)
                  for m, a, d in ((1, 1, 4), (2.76, 0.35, 7), (5.4, 0.15, 12), (8.93, 0.06, 18)))
    elif kind == "pluck":  # cuerda pulsada (Karplus-Strong)
        n, period = len(x), max(2, int(SR / freq))
        buf = rng.uniform(-1, 1, period)
        out = np.empty(n)
        for i in range(n):
            out[i] = buf[i % period]
            buf[i % period] = 0.5 * (buf[i % period] + buf[(i + 1) % period]) * kw.get("decay", 0.996)
    elif kind == "glass":
        out = sum(a * np.sin(2 * np.pi * freq * m * x) for m, a in ((1, 1), (2.01, 0.5), (3.98, 0.25), (6.3, 0.12)))
        out *= np.exp(-x * 5)
    elif kind == "wood":
        out = np.sin(2 * np.pi * freq * x) * np.exp(-x * 40) + 0.4 * np.sin(2 * np.pi * freq * 2.3 * x) * np.exp(-x * 60)
    else:
        out = np.sin(2 * np.pi * freq * x)
    return out * env(len(x), kw.get("attack", 0.003), kw.get("release", dur), kw.get("curve", 3))


def noise(dur, lo, hi, order=2):
    b, a = butter(order, [lo / (SR / 2), min(hi / (SR / 2), 0.99)], btype="band")
    return lfilter(b, a, rng.uniform(-1, 1, len(t(dur))))


def sweep(f0, f1, dur, shape="sine"):
    x = t(dur)
    f = f0 * (f1 / f0) ** (x / dur)
    phase = 2 * np.pi * np.cumsum(f) / SR
    return np.sin(phase) if shape == "sine" else np.sign(np.sin(phase)) * 0.3


def mix(*parts):
    n = max(len(p) for _, p in parts)
    out = np.zeros(n)
    for start, p in parts:
        s = int(start * SR)
        p = p[: max(0, n - s)]
        if s + len(p) > len(out):
            out = np.pad(out, (0, s + len(p) - len(out)))
        out[s:s + len(p)] += p
    return out


def reverb(x, wet=0.25, length=0.9):
    """Reverberación barata: ruido con caída exponencial como respuesta al impulso."""
    ir = rng.uniform(-1, 1, int(SR * length)) * np.exp(-np.arange(int(SR * length)) / SR * 6)
    ir[0] = 0
    tail = np.convolve(x, ir)[: len(x) + len(ir)] * 0.04
    dry = np.pad(x, (0, len(tail) - len(x)))
    return dry * (1 - wet) + tail * wet


def fade_out(x, dur=0.03):
    n = min(len(x), int(SR * dur))
    x = x.copy()
    x[-n:] *= np.linspace(1, 0, n)
    return x


def save(folder, name, x, gain=0.8):
    x = fade_out(x)
    x = x / (np.max(np.abs(x)) + 1e-9) * gain
    wavfile.write(os.path.join(folder, name + ".wav"), SR, (x * 32767).astype(np.int16))


def voice(vowels, dur, f0, f1=None, breath=0.08):
    """Vocecita tipo "animalés": pulsos glotales filtrados por formantes de vocal.
    vowels: lista de vocales que se reparten en el tiempo (p. ej. "ua" para "¡yua!")."""
    formants = {"a": (850, 1610, 2900), "e": (520, 2100, 2950), "i": (330, 2600, 3300),
                "o": (500, 900, 2700), "u": (380, 1000, 2600), "m": (300, 1300, 2500)}
    x = t(dur)
    f = f0 * ((f1 or f0) / f0) ** (x / dur) * (1 + 0.02 * np.sin(2 * np.pi * 6 * x))
    phase = np.cumsum(f) / SR
    pulses = (phase % 1.0) ** 6 * 2 - 0.3  # tren de pulsos con muchos armónicos
    pulses += rng.uniform(-1, 1, len(x)) * breath
    out = np.zeros(len(x))
    chunk = len(x) // len(vowels)
    for k, v in enumerate(vowels):
        seg = pulses[k * chunk:(k + 1) * chunk if k < len(vowels) - 1 else len(x)]
        y = np.zeros(len(seg))
        for i, fr in enumerate(formants[v]):
            # formantes subidos: voz pequeña y aguda
            fr *= 1.45
            b, a = butter(2, [fr * 0.85 / (SR / 2), min(fr * 1.15 / (SR / 2), 0.99)], btype="band")
            y += lfilter(b, a, seg) * (1.0, 0.6, 0.25)[i]
        out[k * chunk:k * chunk + len(y)] = y
    # suavizar las uniones entre vocales
    b, a = butter(2, 7000 / (SR / 2))
    out = lfilter(b, a, out)
    return out * env(len(x), 0.015, dur * 0.6, 2)


# Re mayor: D E F# G A B C#
D5, E5, Fs5, A5, B5, D6, E6, Fs6, A6 = 587.3, 659.3, 740.0, 880.0, 987.8, 1174.7, 1318.5, 1480.0, 1760.0


def build(folder):
    os.makedirs(folder, exist_ok=True)

    # Salto: soplido corto que sube + un "tic" de madera suave.
    jump = mix((0, noise(0.16, 900, 4000) * env(int(SR * 0.16), 0.02, 0.08) * 0.35),
               (0, sweep(320, 620, 0.12) * env(int(SR * 0.12), 0.004, 0.06) * 0.6))
    save(folder, "fase_salto", jump, 0.55)

    # Aterrizaje: golpe sordo.
    land = mix((0, sweep(160, 70, 0.12) * env(int(SR * 0.12), 0.002, 0.05)),
               (0, noise(0.09, 200, 1200) * env(int(SR * 0.09), 0.001, 0.03) * 0.5))
    save(folder, "fase_aterrizaje", land, 0.6)

    # Muelle: "boing" con vibrato.
    x = t(0.45)
    f = 220 + 260 * (1 - np.exp(-x * 9)) + 30 * np.sin(2 * np.pi * 14 * x) * np.exp(-x * 4)
    boing = np.sin(2 * np.pi * np.cumsum(f) / SR) * env(len(x), 0.003, 0.3)
    save(folder, "fase_muelle", reverb(boing, 0.15), 0.7)

    # Fragmento de luna: arpegio de campanitas Re-Fa#-La-Re con brillo.
    shard = mix(*[(i * 0.06, note(fr, 1.2, "bell", release=0.8) * (1 - i * 0.12)) for i, fr in enumerate((D6, Fs6, A6, D6 * 2))],
                (0, noise(0.6, 5000, 12000) * env(int(SR * 0.6), 0.05, 0.3) * 0.08))
    save(folder, "fase_fragmento", reverb(shard, 0.3), 0.7)

    # Punto de control: acorde de campana suave.
    cp = mix((0, note(D5, 1.6, "bell", release=1.2)), (0.02, note(A5, 1.6, "bell", release=1.1) * 0.7),
             (0.04, note(Fs6, 1.6, "bell", release=1.0) * 0.4))
    save(folder, "fase_control", reverb(cp, 0.35), 0.65)

    # Agarrar la línea: brillo que sube (aire + cristal).
    grab = mix((0, noise(0.25, 2500, 9000) * env(int(SR * 0.25), 0.08, 0.1) * 0.25),
               (0.02, sweep(880, 1760, 0.18) * env(int(SR * 0.18), 0.01, 0.1) * 0.35))
    save(folder, "fase_linea_agarrar", reverb(grab, 0.3), 0.45)

    # Soltar la línea: lo mismo bajando.
    rel = mix((0, noise(0.2, 2000, 7000) * env(int(SR * 0.2), 0.01, 0.08) * 0.2),
              (0, sweep(1320, 660, 0.16) * env(int(SR * 0.16), 0.005, 0.08) * 0.3))
    save(folder, "fase_linea_soltar", reverb(rel, 0.3), 0.4)

    # Tic de la línea al moverse: cristal muy corto.
    tick = note(A6 * 1.5, 0.08, "glass", release=0.03, curve=6)
    save(folder, "fase_linea_tic", tick, 0.3)

    # Aparece algo de día: madera que encaja.
    save(folder, "fase_aparece_dia", mix((0, note(392, 0.25, "wood")), (0.03, note(587, 0.25, "wood") * 0.6)), 0.6)

    # Aparece algo de noche: hielo que cristaliza.
    ice = mix((0, note(E6, 0.6, "glass", release=0.35)), (0.04, note(B5 * 2, 0.5, "glass", release=0.3) * 0.5),
              (0, noise(0.3, 6000, 14000) * env(int(SR * 0.3), 0.005, 0.1) * 0.15))
    save(folder, "fase_aparece_noche", reverb(ice, 0.25), 0.5)

    # Desaparece: brillo al revés (sube y se corta).
    vanish = note(Fs6, 0.5, "glass", release=0.3)[::-1] * np.linspace(0.2, 1, len(t(0.5)))
    save(folder, "fase_desaparece", vanish, 0.4)

    # Perder: nota que cae y un "puf" de ruido.
    die = mix((0, sweep(587, 196, 0.5) * env(int(SR * 0.5), 0.005, 0.35) * 0.6),
              (0.05, noise(0.4, 300, 3000) * env(int(SR * 0.4), 0.01, 0.15) * 0.4))
    save(folder, "fase_perder", reverb(die, 0.2), 0.6)

    # Meta: fanfarria corta con cuerda pulsada y campanas (Re-La-Re-Fa#).
    goal = mix(*[(i * 0.11, note(fr, 1.4, "pluck", decay=0.998) * 0.8) for i, fr in enumerate((D5, A5, D6, Fs6))],
               *[(0.44 + i * 0.03, note(fr, 2.0, "bell", release=1.5) * 0.5) for i, fr in enumerate((D6, Fs6, A6))])
    save(folder, "fase_meta", reverb(goal, 0.35), 0.75)

    # Voz de Lumi: grititos cortos, nunca palabras.
    for i, (v, f0, f1) in enumerate((("u", 560, 700), ("a", 600, 760), ("e", 580, 720))):
        save(folder, f"fase_voz_salto_{i}", voice(v, 0.12, f0, f1), 0.45)
    save(folder, "fase_voz_meta", reverb(mix((0, voice("ia", 0.32, 640, 900)), (0.34, voice("ai", 0.26, 760, 620))), 0.2), 0.55)
    save(folder, "fase_voz_perder", voice("oa", 0.35, 620, 380), 0.5)
    save(folder, "fase_voz_fragmento", voice("o", 0.22, 700, 880), 0.45)
    save(folder, "fase_voz_aterrizaje", voice("m", 0.1, 420, 360, breath=0.15), 0.3)


if __name__ == "__main__":
    build(sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "efectos_prueba"))
    print("Efectos generados")
