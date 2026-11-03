"""Monta el tráiler de FASE con ffmpeg a partir de los planos grabados (Trailer/planos/NN),
los títulos (Trailer/titulos/*.png) y la música (Trailer/musica.wav).
Uso: python Tools/montar_trailer.py es|en   → Trailer/FASE-trailer-es.mp4
"""
import os
import subprocess
import sys

ROOT = os.path.join(os.path.dirname(__file__), "..", "Trailer")
LANG = sys.argv[1] if len(sys.argv) > 1 else "es"
FPS = 60

# (inicio en la música, duración, plano, desde qué segundo del plano, título, destello blanco al entrar)
EDIT = [
    (0.0, 5.0, "01", 0.0, "c1", False),
    (5.0, 4.0, "02", 0.0, "c2", False),
    (9.0, 2.6, "03", 0.0, "c3", False),
    (11.6, 0.9, None, 0, None, False),          # negro: aspiración antes del título
    (12.5, 1.25, None, 0, "title", True),       # GOLPE: título
    (13.75, 2.5, "04", 1.0, "m1", True),
    (16.25, 2.5, "05", 1.2, "m2", False),
    (18.75, 2.5, "06", 1.0, "m3", False),
    (21.25, 2.5, "07", 0.3, "m4", False),
    (23.75, 2.5, "08", 0.4, "m5", False),
    (26.25, 2.5, "11", 0.3, "m7", False),
    (28.75, 2.5, "12", 0.3, "m8", False),
    (31.25, 2.75, "13", 0.2, "m6", False),
    (34.0, 6.0, "09", 1.5, "boss", True),       # la Gran Sombra
    (40.0, 5.0, "10", 0.0, "logo", True),       # amanecer + logo
    (45.0, 5.0, None, 0, "end", False),
]

GRADE = ("eq=contrast=1.1:saturation=1.15:gamma=0.97,"
         "vignette=PI/4.5,noise=alls=3:allf=t,"
         "drawbox=x=0:y=0:w=iw:h=64:color=black:t=fill,drawbox=x=0:y=ih-64:w=iw:h=64:color=black:t=fill")


def run(args):
    subprocess.run(["ffmpeg", "-loglevel", "error", "-y"] + args, check=True)


def segment(i, start, dur, shot, offset, title, flash):
    out = os.path.join(ROOT, "tmp", f"{i:02}.mp4")
    inputs, filters = [], []
    if shot:
        inputs += ["-framerate", str(FPS), "-start_number", str(int(offset * FPS)), "-i", os.path.join(ROOT, "planos", shot, "%05d.jpg")]
        filters.append(f"[0:v]trim=duration={dur},setpts=PTS-STARTPTS,{GRADE}[base]")
    else:
        inputs += ["-f", "lavfi", "-i", f"color=c=0x05040e:s=1920x1080:r={FPS}:d={dur}"]
        filters.append("[0:v]null[base]")
    last = "base"
    if title:
        inputs += ["-loop", "1", "-framerate", str(FPS), "-t", str(dur), "-i", os.path.join(ROOT, "titulos", f"{title}_{LANG}.png")]
        # Los títulos entran con fundido; el grande del principio entra de golpe y crece un poco.
        if title in ("title", "logo"):
            delay = 0.0 if title == "title" else 0.6
            filters.append(f"[1:v]format=rgba,scale=w='1920*(1.06-0.06*min(t/{dur},1))':h=-1:eval=frame,"
                           f"fade=t=in:st={delay}:d=0.25:alpha=1,fade=t=out:st={dur - 0.3}:d=0.3:alpha=1[tt]")
            filters.append(f"[{last}][tt]overlay=x=(W-w)/2:y=(H-h)/2:shortest=1[t1]")
        else:
            a, b = (0.5, dur - 0.5) if dur > 2 else (0.2, dur - 0.2)
            filters.append(f"[1:v]format=rgba,fade=t=in:st={a}:d=0.35:alpha=1,fade=t=out:st={b - 0.35}:d=0.35:alpha=1[tt]")
            filters.append(f"[{last}][tt]overlay=0:0:shortest=1[t1]")
        last = "t1"
    if flash:
        filters.append(f"[{last}]fade=t=in:st=0:d=0.35:color=white[f1]")
        last = "f1"
    if i == 0:
        filters.append(f"[{last}]fade=t=in:st=0:d=1.2[o]")
    elif start + dur >= 50:
        filters.append(f"[{last}]fade=t=out:st={dur - 1.5}:d=1.5[o]")
    else:
        filters.append(f"[{last}]null[o]")
    run(inputs + ["-filter_complex", ";".join(filters), "-map", "[o]", "-t", str(dur), "-r", str(FPS),
                  "-c:v", "libx264", "-preset", "medium", "-crf", "16", "-pix_fmt", "yuv420p", out])
    return out


def main():
    os.makedirs(os.path.join(ROOT, "tmp"), exist_ok=True)
    parts = [segment(i, *e) for i, e in enumerate(EDIT)] if "--solo-unir" not in sys.argv else         [os.path.join(ROOT, "tmp", f"{i:02}.mp4") for i in range(len(EDIT))]
    listing = os.path.join(ROOT, "tmp", "lista.txt")
    with open(listing, "w") as f:
        for p in parts:
            f.write(f"file '{os.path.abspath(p)}'\n")
    out = os.path.join(ROOT, f"FASE-trailer-{LANG}.mp4")
    # Todos los planos salen con el mismo códec: se unen sin recodificar (poca memoria).
    run(["-f", "concat", "-safe", "0", "-i", listing, "-i", os.path.join(ROOT, "musica.wav"),
         "-map", "0:v", "-map", "1:a", "-c:v", "copy", "-c:a", "aac", "-b:a", "320k",
         "-shortest", "-movflags", "+faststart", out])
    print(out)


main()
