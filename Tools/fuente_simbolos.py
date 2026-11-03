"""Crea "Fredoka Fase": Fredoka (OFL) con los símbolos que usa la interfaz del juego, que la
fuente original no trae (★ ☾ ▶ ◀ ✔ ✖ ← → ∞ ≤ ⏱ ✎ ✦). En la web no hay fuentes de reserva, así
que sin esto saldrían cuadros vacíos. Los símbolos se dibujan aquí como polígonos.

Uso: python Tools/fuente_simbolos.py
     → Assets/Art/Fonts/FredokaFase.ttf y Assets/Resources/Fredoka.ttf
"""
import math
import os
import shutil

from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

ROOT = os.path.join(os.path.dirname(__file__), "..", "Assets")
SRC = os.path.join(ROOT, "Art", "Fonts", "Fredoka.ttf")
OUT = os.path.join(ROOT, "Art", "Fonts", "FredokaFase.ttf")
RES = os.path.join(ROOT, "Resources", "Fredoka.ttf")

W = 1000        # ancho de cada símbolo
CY = 330        # centro vertical (a media altura de las mayúsculas)


def area(pts):
    return sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in zip(pts, pts[1:] + pts[:1])) / 2


def oriented(pts, hole=False):
    """TrueType: contornos exteriores en sentido horario, huecos al revés."""
    clockwise = area(pts) < 0
    return pts if clockwise != hole else pts[::-1]


def circle(cx, cy, r, n=48, start=0.0, end=2 * math.pi):
    return [(cx + r * math.cos(start + (end - start) * i / n), cy + r * math.sin(start + (end - start) * i / n)) for i in range(n)]


def thick(a, b, w):
    """Barra gruesa de a a b."""
    (x0, y0), (x1, y1) = a, b
    dx, dy = x1 - x0, y1 - y0
    length = math.hypot(dx, dy)
    nx, ny = -dy / length * w / 2, dx / length * w / 2
    return [(x0 + nx, y0 + ny), (x1 + nx, y1 + ny), (x1 - nx, y1 - ny), (x0 - nx, y0 - ny)]


def star(points, outer, inner, cx=W / 2, cy=CY):
    pts = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        a = math.pi / 2 + i * math.pi / points
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return [pts]


def crescent():
    """Luna creciente: el círculo grande menos otro desplazado arriba a la derecha."""
    cx, cy, r = 470, CY, 370
    ix, iy, ir = cx + 190, cy + 70, 320
    n = 720

    def arc(ccx, ccy, rr, keep, step):
        pts = [(ccx + rr * math.cos(2 * math.pi * i / n), ccy + rr * math.sin(2 * math.pi * i / n)) for i in range(n)]
        flags = [keep(p) for p in pts]
        # Empieza justo donde entra en la zona que se queda y la recorre entera.
        start = next(i for i in range(n) if flags[i] and not flags[i - 1])
        out, i = [], start
        while flags[i % n] and len(out) < n:
            out.append(pts[i % n])
            i += 1
        return out if step > 0 else out[::-1]

    outer = arc(cx, cy, r, lambda p: math.hypot(p[0] - ix, p[1] - iy) > ir, 1)
    inner = arc(ix, iy, ir, lambda p: math.hypot(p[0] - cx, p[1] - cy) < r, -1)
    return [outer + inner]


def ring(cx, cy, r, w, n=48):
    return [circle(cx, cy, r, n), circle(cx, cy, r - w, n)[::-1]]


SHAPES = {
    0x2605: lambda: star(5, 400, 170),                                         # ★
    0x2726: lambda: star(4, 400, 110),                                         # ✦
    0x263E: crescent,                                                          # ☾
    0x25B6: lambda: [[(220, CY - 330), (220, CY + 330), (800, CY)]],           # ▶
    0x25C0: lambda: [[(780, CY - 330), (200, CY), (780, CY + 330)]],           # ◀
    0x2714: lambda: [thick((180, CY), (400, CY - 230), 130), thick((400, CY - 230), (830, CY + 300), 130)],  # ✔
    0x2716: lambda: [thick((220, CY - 300), (780, CY + 300), 140), thick((220, CY + 300), (780, CY - 300), 140)],  # ✖
    0x2192: lambda: [thick((150, CY), (650, CY), 120), [(600, CY - 260), (600, CY + 260), (880, CY)]],      # →
    0x2190: lambda: [thick((350, CY), (850, CY), 120), [(400, CY + 260), (400, CY - 260), (120, CY)]],      # ←
    0x2264: lambda: [thick((780, CY + 260), (240, CY + 40), 110), thick((240, CY + 40), (780, CY - 180), 110),
                     thick((240, CY - 320), (780, CY - 320), 100)],            # ≤
    0x23F1: lambda: ring(W / 2, CY - 30, 340, 95) + [thick((W / 2, CY - 30), (W / 2 + 150, CY + 120), 90),
                                                      thick((W / 2 - 90, CY + 400), (W / 2 + 90, CY + 400), 90)],  # ⏱
    0x270E: lambda: [thick((300, CY - 220), (760, CY + 300), 170), [(170, CY - 380), (330, CY - 320), (230, CY - 230)]],  # ✎
}


def infinity():
    # Dos anillos que se tocan.
    return ring(320, CY, 230, 90) + ring(680, CY, 230, 90)


SHAPES[0x221E] = infinity                                                      # ∞


def sun():
    """Sol: disco y ocho rayos."""
    cx, cy = W / 2, CY
    rays = [thick((cx + 250 * math.cos(a), cy + 250 * math.sin(a)), (cx + 400 * math.cos(a), cy + 400 * math.sin(a)), 80)
            for a in (i * math.pi / 4 for i in range(8))]
    return [circle(cx, cy, 190)] + rays


def gear():
    """Engranaje: anillo grueso con ocho dientes."""
    cx, cy = W / 2, CY
    teeth = [thick((cx + 230 * math.cos(a), cy + 230 * math.sin(a)), (cx + 390 * math.cos(a), cy + 390 * math.sin(a)), 130)
             for a in (i * math.pi / 4 for i in range(8))]
    return ring(cx, cy, 300, 150) + teeth


def grid():
    """Cuadrícula de 3×3 casillas."""
    cells = []
    for i in range(3):
        for j in range(3):
            x, y = 180 + i * 220, CY - 330 + j * 220
            cells.append([(x, y), (x + 180, y), (x + 180, y + 180), (x, y + 180)])
    return cells


def face():
    """Cara sonriente: anillo, dos ojos y la sonrisa."""
    cx, cy = W / 2, CY
    smile = [thick(p0, p1, 70) for p0, p1 in zip(circle(cx, cy - 20, 190, 10, math.pi * 1.15, math.pi * 1.85),
                                                 circle(cx, cy - 20, 190, 10, math.pi * 1.15, math.pi * 1.85)[1:])]
    return ring(cx, cy, 400, 80) + [circle(cx - 130, cy + 110, 60, 24), circle(cx + 130, cy + 110, 60, 24)] + smile


SHAPES.update({0x2600: sun, 0x2699: gear, 0x25A6: grid, 0x263A: face})   # ☀ ⚙ ▦ ☺
HOLES = {infinity: (1, 3), gear: (1,), face: (1,)}


def main():
    font = TTFont(SRC)
    # Fija el peso y el ancho por defecto (el aspecto que ya tiene el juego) y quita las variaciones.
    font = instancer.instantiateVariableFont(font, {"wght": 300, "wdth": 100})
    glyf, hmtx = font["glyf"], font["hmtx"]
    order = font.getGlyphOrder()
    for code, shape in SHAPES.items():
        name = f"uni{code:04X}"
        pen = TTGlyphPen(None)
        contours = shape()
        for i, pts in enumerate(contours):
            # En los anillos, el segundo contorno de cada par es el hueco.
            hole = i in HOLES.get(shape, ()) or (code == 0x23F1 and i == 1)
            pts = oriented([(round(x), round(y)) for x, y in pts], hole)
            pen.moveTo(pts[0])
            for p in pts[1:]:
                pen.lineTo(p)
            pen.closePath()
        glyph = pen.glyph()
        glyph.recalcBounds(glyf)
        if name not in order:
            order.append(name)
        glyf[name] = glyph
        hmtx[name] = (W, glyph.xMin if hasattr(glyph, "xMin") else 0)
        for table in font["cmap"].tables:
            if table.isUnicode() and (table.format in (4, 12)):
                table.cmap[code] = name
    font.setGlyphOrder(order)
    font["maxp"].numGlyphs = len(order)
    for record in font["name"].names:
        if record.nameID in (1, 4, 16):
            record.string = "Fredoka Fase"
        elif record.nameID == 6:
            record.string = "FredokaFase-Regular"
    font.save(OUT)
    os.makedirs(os.path.dirname(RES), exist_ok=True)
    shutil.copyfile(OUT, RES)
    print("Fuente con símbolos:", OUT)


if __name__ == "__main__":
    main()
