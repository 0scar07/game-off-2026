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


def save(file, name, hint, line, m, extra=""):
    """`extra`: cabeceras opcionales, por ejemplo "dia: derecha\\n" o "limites: 10 18\\n"."""
    os.makedirs(OUT, exist_ok=True)
    body = f"nombre: {name}\npista: {hint}\nlinea: {line}\n{extra}---\n{m.text()}\n"
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

# 7. Al revés: aquí el día está a la DERECHA de la línea.
m = Map(36).ground(r(0, 7)).fill("d", r(8, 13), [9]).ground(r(14, 19)).ground(r(28, 35))
m.fill("n", r(21, 22), [8]).fill("n", r(25, 26), [7])
m.put("S", 2, 8).put("C", 16, 8).put("G", 33, 8).put("*", 24, 4)
save("07-al-reves.txt", "Al revés",
     "Cuidado: aquí el día está a la DERECHA de la línea.", 30, m, "dia: derecha\n")

# 8. La torre: subir en zigzag por cajas (día) y hielo (noche) con la línea atrapada en medio.
m = Map(30).ground(r(0, 29))
m.fill("D", r(11, 12), [8]).fill("N", r(15, 16), [7]).fill("D", r(11, 12), [5]).fill("N", r(15, 16), [3])
m.fill("#", r(20, 27), [1])
m.put("S", 2, 8).put("G", 25, 0).put("*", 13, 1)
save("08-la-torre.txt", "La torre",
     "La línea no puede salir de entre los postes rojos.\\nSube en zigzag.", 10, m, "limites: 10 18\n")

# 9. Guardianes: cruza el hielo y duerme (o salta) a la criatura antes de que te alcance.
m = Map(44).ground(r(0, 9)).fill("n", r(10, 17), [9]).fill("^", r(10, 17), [11])
m.ground(r(18, 29)).fill("n", r(30, 35), [9]).fill("^", r(30, 35), [11]).ground(r(36, 43))
m.put("M", 25, 8)
m.put("S", 2, 8).put("C", 19, 8).put("G", 41, 8).put("*", 24, 5)
save("09-guardianes.txt", "Guardianes",
     "Si no puedes evitarla, duérmela... o sáltala.", 40, m)

# 10. Río nocturno: flor de día y lago de noche, uno a cada lado de la línea.
m = Map(44).ground(r(0, 7)).fill("~", r(8, 15), [9]).ground(r(8, 15), 10).ground(r(16, 20))
m.ground(r(21, 22), 4).ground(r(23, 25)).fill("~", r(26, 33), [9]).ground(r(26, 33), 10).ground(r(34, 43))
m.put("F", 18, 8)
m.put("S", 2, 8).put("C", 17, 8).put("G", 41, 8).put("*", 21, 2)
save("10-rio-nocturno.txt", "Río nocturno",
     "Una cosa a cada lado de la línea.", 12, m, "limites: 4 25\n")

# 11. Eclipse: el final. Todo lo aprendido, tres fragmentos de luna.
m = Map(64).ground(r(0, 7)).fill("~", r(8, 13), [9]).ground(r(8, 13), 10).ground(r(14, 19))
m.put("D", 18, 8).put("D", 19, 7).ground(r(20, 27), 6)
m.put("M", 24, 5)
m.fill("n", r(28, 33), [6]).ground(r(34, 41), 6).fill("^", r(37, 38), [5]).put("F", 40, 5)
m.ground(r(43, 50), 2).ground(r(51, 63))
m.put("M", 57, 8)
m.put("S", 2, 8).put("C", 15, 8).put("C", 35, 5).put("C", 52, 8).put("G", 62, 8)
m.put("*", 11, 5).put("*", 24, 2).put("*", 47, 0)
save("11-eclipse.txt", "Eclipse",
     "El último paso. Usa todo lo que has aprendido.", 3, m)
