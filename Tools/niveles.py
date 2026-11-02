"""
Dibuja los mapas de los niveles de FASE en Assets/Levels/*.txt.
Los .txt se pueden editar a mano directamente; este script solo sirve para crearlos sin
errores de alineación. Ver Assets/Levels/LEEME.md para el significado de cada letra.

Coordenadas: fila 0 arriba, fila 11 abajo. El suelo normal ocupa las filas 9-11 (su
superficie está a 3 casillas del fondo); lo que se pone encima del suelo va en la fila 8.
"""
import os

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Levels")
ROWS = 12


class Map:
    def __init__(self, width):
        self.grid = [[" "] * width for _ in range(ROWS)]

    def fill(self, char, cols, rows):
        for r in rows:
            for c in cols:
                self.grid[r][c] = char
        return self

    def put(self, char, col, row):
        self.grid[row][col] = char
        return self

    def ground(self, cols, top_row=9):
        return self.fill("#", cols, range(top_row, ROWS))

    def text(self):
        return "\n".join("".join(row).rstrip() for row in self.grid)


def save(file, name, hint, line, m):
    os.makedirs(OUT, exist_ok=True)
    body = f"nombre: {name}\npista: {hint}\nlinea: {line}\n---\n{m.text()}\n"
    with open(os.path.join(OUT, file), "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    print(f"== {file}: {name}\n{m.text()}\n")


def r(a, b):
    return range(a, b + 1)


# 1. Amanecer: el puente de madera solo existe de día. Mover la línea para que aparezca.
m = Map(30).ground(r(0, 9)).fill("d", r(10, 15), [9]).ground(r(16, 29))
m.put("S", 2, 8).put("G", 27, 8).put("*", 13, 6)
save("01-amanecer.txt", "Amanecer",
     "A la izquierda de la línea es de día; a la derecha, de noche.\\nMuévela con Shift (o Q/E), o arrastrándola.", 4, m)

# 2. Noche estrellada: plataformas de hielo que solo existen de noche.
m = Map(34).ground(r(0, 7)).ground(r(25, 33))
m.fill("n", r(9, 10), [8]).fill("n", r(13, 14), [7]).fill("n", r(17, 18), [6]).fill("n", r(21, 22), [7])
m.put("S", 2, 8).put("G", 31, 8).put("*", 18, 3)
save("02-noche-estrellada.txt", "Noche estrellada",
     "Lo que brilla en azul solo existe de noche.", 30, m)

# 3. Las dos mitades: puente de día y hielo de noche a la vez; luego un muro de noche.
m = Map(40).ground(r(0, 8)).fill("d", r(9, 14), [9]).ground(r(25, 39))
m.fill("n", r(17, 18), [8]).fill("n", r(21, 22), [7])
m.fill("N", [31], r(5, 8))
m.put("S", 2, 8).put("C", 27, 8).put("G", 37, 8).put("*", 20, 4)
save("03-las-dos-mitades.txt", "Las dos mitades",
     "Deja el puente de día y el hielo de noche a la vez.", 3, m)

# 4. Lago helado: el lago se hiela de noche; la flor-muelle solo funciona de día.
m = Map(46).ground(r(0, 7)).fill("~", r(8, 17), [9]).ground(r(8, 17), 10).ground(r(18, 27))
m.ground(r(28, 45), 5)
m.put("F", 25, 8)
m.put("S", 2, 8).put("C", 20, 8).put("G", 43, 4).put("*", 12, 5)
save("04-lago-helado.txt", "Lago helado",
     "De día el lago es agua. De noche, hielo.\\nLas flores te impulsan, pero solo de día.", 20, m)

# 5. Criaturas: los monstruos solo están despiertos de noche.
m = Map(44).ground(r(0, 21)).ground(r(28, 43))
m.fill("n", r(23, 24), [8]).fill("n", r(26, 27), [7])
m.put("M", 14, 8).put("M", 37, 8)
m.put("S", 2, 8).put("C", 29, 8).put("G", 42, 8).put("*", 25, 4)
save("05-criaturas.txt", "Criaturas",
     "Las criaturas de la noche duermen cuando es de día.", 3, m)

# 6. Todo a la vez: lago, pinchos, puente, cajas, acantilado y puente de hielo.
m = Map(56).ground(r(0, 7)).fill("~", r(8, 13), [9]).ground(r(8, 13), 10).ground(r(14, 19))
m.fill("^", r(16, 17), [8])
m.fill("d", r(20, 25), [9]).ground(r(26, 31))
m.fill("D", r(29, 30), [8]).fill("D", [31], [7])
m.ground(r(32, 40), 6).fill("n", r(41, 46), [6]).ground(r(47, 55), 6)
m.put("S", 2, 8).put("C", 27, 8).put("C", 36, 5).put("G", 53, 5).put("*", 17, 5)
save("06-todo-a-la-vez.txt", "Todo a la vez",
     "Paso a paso: cada tramo pide la línea en un sitio distinto.", 3, m)
