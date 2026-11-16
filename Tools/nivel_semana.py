"""Nivel de la semana: genera semana/nivel.txt (lo descarga el juego desde el repositorio).

Formato de semana/nivel.txt (una cosa por línea):
    1. Nombre en español | nombre en inglés
    2. Autor (nombre corto)
    3. Código FASE-... (el que da "Copiar código" en el Taller)

Para cambiar el nivel de la semana basta con editar ese archivo en GitHub (o pegar el código de
un nivel enviado con la plantilla "Nivel de la comunidad"). Este script crea el primero.

Uso: python Tools/nivel_semana.py
"""
import base64
import os

ROWS, COLS = 14, 64


def encode(rows):
    out = []
    for row in rows:
        i = 0
        while i < len(row):
            run = 1
            while i + run < len(row) and row[i + run] == row[i] and run < 99:
                run += 1
            out.append(f"{run}{row[i]}" if run > 2 else row[i] * run)
            i += run
        out.append("|")
    b64 = base64.b64encode("".join(out).encode("utf-8")).decode("ascii")
    return "FASE-" + b64.rstrip("=").replace("+", "-").replace("/", "_")


def level():
    g = [[" "] * COLS for _ in range(ROWS)]

    def put(r, c, ch):
        g[r][c] = ch

    # Suelo con huecos que cruzan, por turnos, tablas de día (d) y placas de hielo de noche (n).
    gaps = [(12, 16, "d"), (24, 28, "n"), (40, 44, "d"), (52, 55, "n")]
    for r in range(11, 14):
        for c in range(COLS):
            put(r, c, "#")
    for a, b, bridge in gaps:
        for r in range(11, 14):
            for c in range(a, b):
                put(r, c, " ")
        for c in range(a, b):
            put(11, c, bridge)
    # Salida, control a mitad y meta.
    put(10, 2, "S")
    put(10, 33, "C")
    put(10, 61, "G")
    # Una flor-muelle hasta una repisa alta con una luna.
    put(10, 19, "F")
    for c in range(18, 23):
        put(6, c, "#")
    put(5, 20, "*")
    # Pinchos que se saltan y una Sombra dormida que se cruza de día.
    put(10, 36, "^")
    put(10, 37, "^")
    put(10, 47, "M")
    # Segunda luna sobre el último puente de hielo.
    put(8, 53, "*")
    return ["".join(row).rstrip() for row in g]


if __name__ == "__main__":
    code = encode(level())
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    os.makedirs(os.path.join(root, "semana"), exist_ok=True)
    with open(os.path.join(root, "semana", "nivel.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("Puentes alternos | Alternating bridges\n0scar07\n" + code + "\n")
    print(code)
