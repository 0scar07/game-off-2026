"""Comprueba que todos los niveles de Assets/Levels se pueden terminar y que sus fragmentos,
cofres y puntos de control tienen camino. Es optimista (supone que la línea siempre se puede
poner donde haga falta), así que si algo sale inalcanzable aquí, seguro que es un fallo.

Uso: python Tools/comprobar_niveles.py [carpeta]   → sale con error si hay problemas
(GitHub lo ejecuta en cada cambio de los niveles: .github/workflows/niveles.yml)
"""
import glob
import os
import sys
from collections import deque

# Casillas donde se puede estar de pie (en algún mundo) y que no tapan el paso al saltar.
STAND = set("#DdNn~FOYZ")
WALL = set("#~F")
JUMP_UP, JUMP_SIDE, SPRING_UP = 2, 4, 7


def load(path):
    text = open(path, encoding="utf-8").read()
    head, body = text.split("---", 1)
    # Las filas vacías de arriba también cuentan (son cielo), igual que en el juego.
    rows = body.split("\n")[1:]
    while rows and not rows[-1].strip():
        rows.pop()
    width = max(len(r) for r in rows)
    header = dict(l.split(":", 1) for l in head.splitlines() if ":" in l)
    return {k.strip(): v.strip() for k, v in header.items()}, [r.ljust(width) for r in rows]


def check(path):
    header, g = load(path)
    H, W = len(g), len(g[0])
    at = lambda x, y: g[y][x] if 0 <= y < H and 0 <= x < W else (" " if y < 0 else "#")
    stand_on = lambda x, y: at(x, y) in STAND
    blocked = lambda x, y: at(x, y) in WALL
    free = lambda x, y: not blocked(x, y) and not blocked(x, y - 1)
    find = lambda chars: [(x, y) for y in range(H) for x in range(W) if g[y][x] in chars]
    problems, notes = [], []

    starts = find("S")
    if not starts:
        return ["no hay salida (S)"], notes
    if not find("GH"):
        problems.append("no hay meta (G)")

    # Corrientes de aire: desde su base se sube hasta su techo.
    lifts = {}
    for x, y in find("W"):
        if at(x, y + 1) != "W":
            top = y
            while at(x, top - 1) == "W":
                top -= 1
            lifts[x] = (top, y)

    horizontal = header.get("angulo", "0").strip() == "90"
    stand = {(x, y) for x in range(W) for y in range(H) if not stand_on(x, y) and free(x, y) and stand_on(x, y + 1)}
    if horizontal:
        # Torre con la línea tumbada: también se puede estar sobre plataformas aisladas.
        stand |= {(x, y) for x in range(W) for y in range(H) if at(x, y) == " " and stand_on(x, y + 1)}

    # Cajas que se empujan (Z): pueden acabar en cualquier punto de su fila hasta el siguiente muro,
    # y subirse encima sirve de escalón.
    for zx, zy in find("Z"):
        x = zx
        while x + 1 < W and not blocked(x + 1, zy):
            x += 1
            stand.add((x, zy - 1))

    # Relojes de sol (O): la plataforma va y viene 3 casillas a cada lado (y mide 3 de ancho).
    for ox, oy in find("O"):
        for x in range(ox - 4, ox + 5):
            if 0 <= x < W and free(x, oy - 1):
                stand.add((x, oy - 1))

    sx, sy = starts[0]
    while (sx, sy) not in stand and sy < H - 1:
        sy += 1
    seen = {(sx, sy)}
    queue = deque([(sx, sy)])
    while queue:
        x, y = queue.popleft()
        up = SPRING_UP if at(x, y + 1) == "F" else JUMP_UP
        for lx, (top, base) in lifts.items():
            if abs(lx - x) <= 2 and base >= y - 1:
                up = max(up, y - top + 2)
        side = JUMP_SIDE + (2 if up > JUMP_UP else 0)
        for (x2, y2) in stand:
            if (x2, y2) in seen or abs(x2 - x) > side + max(0, y2 - y) // 2 or y - y2 > up:
                continue
            apex = min(y, y2) - (1 if y2 <= y else 0)
            step = 1 if x2 > x else -1
            if any(not free(c, apex) for c in range(x + step, x2, step)) and up <= SPRING_UP:
                continue
            seen.add((x2, y2))
            queue.append((x2, y2))

    near = lambda cx, cy, reach: any(abs(x - cx) <= 2 and 0 <= y - cy <= reach for x, y in seen)
    for gx, gy in find("GH"):
        if not near(gx, gy, 1):
            problems.append(f"meta inalcanzable (columna {gx})")
    for chars, name, reach in (("*", "fragmento", 3), ("K", "cofre", 2), ("C", "punto de control", 1)):
        for cx, cy in find(chars):
            if not near(cx, cy, reach):
                problems.append(f"{name} inalcanzable (columna {cx}, fila {cy})")
    if not find("JPR") and not header.get("secreto") and not find("*K"):
        notes.append("no tiene fragmentos de luna")
    return problems, notes


def main():
    folder = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "Assets", "Levels")
    files = sorted(glob.glob(os.path.join(folder, "*.txt"))) + sorted(glob.glob(os.path.join(folder, "Diarios", "*.txt")))
    failed = 0
    for path in files:
        problems, notes = check(path)
        name = os.path.relpath(path, folder)
        status = "✗" if problems else "✓"
        print(f"{status} {name}" + (f"  ({'; '.join(notes)})" if notes else ""))
        for p in problems:
            print(f"    - {p}")
        failed += bool(problems)
    print(f"\n{len(files) - failed}/{len(files)} niveles bien")
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
