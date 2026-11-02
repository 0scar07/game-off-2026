"""
Dibuja los 11 niveles de FASE en Assets/Levels/*.txt encadenando "tramos" (puente, hielo, lago,
muelle, muros, criaturas...). Cada tramo empieza y termina en suelo firme (de los dos mundos),
así que entre tramo y tramo el jugador puede recolocar la línea con calma.

Filas: 0 arriba, 13 abajo. El suelo normal tiene la superficie en la fila 11 (3 casillas sobre el
fondo); lo que va encima del suelo se pone en la fila de arriba (top - 1).
Saltos: unas 2,5 casillas de alto y 4 de largo.

Los .txt se pueden retocar a mano después (ver Assets/Levels/LEEME.md).
"""
import os

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Levels")
ROWS = 14
TOP = 11


class Level:
    def __init__(self):
        self.grid = [[" "] * 400 for _ in range(ROWS)]
        self.x = 0
        self.top = TOP

    def put(self, char, col, row):
        if 0 <= row < ROWS:
            self.grid[row][col] = char

    def fill(self, char, cols, rows):
        for r in rows:
            for c in cols:
                self.put(char, c, r)

    def ground(self, a, b, top):
        self.fill("#", range(a, b), range(top, ROWS))

    def text(self):
        width = max(len("".join(r).rstrip()) for r in self.grid)
        return "\n".join("".join(r[:width]).rstrip() for r in self.grid)

    # ───────────── Tramos ─────────────

    def flat(self, n):
        self.ground(self.x, self.x + n, self.top)
        self.x += n
        return self

    def start(self):
        self.flat(6)
        self.put("S", 2, self.top - 1)
        return self

    def goal(self):
        self.flat(7)
        self.put("G", self.x - 3, self.top - 1)
        return self

    def check(self):
        """Punto de control sobre 4 casillas de suelo."""
        self.flat(4)
        self.put("C", self.x - 2, self.top - 1)
        return self

    def bridge(self, w=6):
        """Hueco con tablas de madera: solo existen de DÍA."""
        self.fill("d", range(self.x, self.x + w), [self.top])
        self.x += w
        return self.flat(3)

    def ice_bridge(self, w=6):
        """Hueco con placas de hielo: solo existen de NOCHE."""
        self.fill("n", range(self.x, self.x + w), [self.top])
        self.x += w
        return self.flat(3)

    def ice_steps(self, star=False):
        """Escalera de hielo (de noche) sobre un hueco: sube y baja."""
        t, x = self.top, self.x
        self.fill("n", range(x + 1, x + 3), [t - 1])
        self.fill("n", range(x + 5, x + 7), [t - 2])
        self.fill("n", range(x + 9, x + 11), [t - 1])
        if star:
            self.put("*", x + 6, t - 5)
        self.x += 12
        return self.flat(3)

    def crate_steps(self):
        """Cajas de madera (de día) para subir a una meseta 2 casillas más alta."""
        t, x = self.top, self.x
        self.ground(x, x + 5, t)
        self.put("D", x + 3, t - 1)
        self.fill("D", [x + 4], [t - 1, t - 2])
        self.x += 5
        self.top = t - 2
        return self.flat(4)

    def drop(self, rows=2):
        """Baja de nivel: el siguiente suelo está `rows` casillas más abajo."""
        self.top += rows
        return self

    def lake(self, w=8, star=False):
        """Lago: agua que hace perder de día, hielo que se pisa de noche."""
        self.fill("~", range(self.x, self.x + w), [self.top])
        self.ground(self.x, self.x + w, self.top + 1)
        if star:
            self.put("*", self.x + w // 2, self.top - 4)
        self.x += w
        return self.flat(3)

    def spring_wall(self, height=5):
        """Flor-muelle (de día) delante de un muro alto: arriba sigue una meseta."""
        t, x = self.top, self.x
        self.ground(x, x + 4, t)
        self.put("F", x + 2, t - 1)
        self.top = t - height
        self.x += 4
        return self.flat(5)

    def night_wall(self):
        """Muro de hielo (de noche) que corta el paso: hay que hacerlo de día."""
        t = self.top
        self.flat(5)
        self.fill("N", [self.x - 3], range(t - 4, t))
        return self

    def day_wall(self):
        """Muro de cajas (de día) que corta el paso: hay que hacerlo de noche."""
        t = self.top
        self.flat(5)
        self.fill("D", [self.x - 3], range(t - 4, t))
        return self

    def creature(self, n=12):
        """Sombra patrullando por el suelo: duérmela (día) o sáltala."""
        self.flat(n)
        self.put("M", self.x - n // 2, self.top - 1)
        return self

    def bee_over_ice(self, w=8):
        """Puente de hielo (noche) con una abeja de la noche volando a la altura de la cabeza."""
        x, t = self.x, self.top
        self.ice_bridge(w)
        self.put("B", x + w // 2, t - 2)
        return self

    def saws(self, n=12):
        """Sierras que suben y bajan: pasa cuando estén arriba."""
        self.flat(n)
        self.put("X", self.x - n + 3, self.top - 1)
        self.put("X", self.x - n + 8, self.top - 1)
        return self

    def spikes(self, n=8, star=False):
        self.flat(n)
        self.fill("^", range(self.x - n + 3, self.x - n + 5), [self.top - 1])
        if star:
            self.put("*", self.x - n + 4, self.top - 4)
        return self

    def secret_ledge(self, kind="d"):
        """Repisa opcional en lo alto (madera o hielo) con el cofre secreto encima."""
        t = self.top
        self.flat(9)
        x = self.x - 9
        self.fill(kind, range(x + 2, x + 4), [t - 2])
        self.fill(kind, range(x + 5, x + 8), [t - 4])
        self.put("K", x + 6, t - 5)
        return self


def save(file, chapter, name, name_en, hint, hint_en, line, level, extra=""):
    os.makedirs(OUT, exist_ok=True)
    body = (f"nombre: {name}\nname: {name_en}\npista: {hint}\nhint: {hint_en}\n"
            f"capitulo: {chapter}\nlinea: {line}\n{extra}---\n{level.text()}\n")
    with open(os.path.join(OUT, file), "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    width = max(len(r) for r in level.text().split("\n"))
    print(f"{file}: {width} columnas")


def clean():
    for f in os.listdir(OUT):
        if f.endswith(".txt"):
            os.remove(os.path.join(OUT, f))


clean()

# ───────────────────────── Capítulo 1 · La Pradera ─────────────────────────

L = Level().start().flat(6).bridge(5).flat(6).bridge(7).check().flat(4).spikes(star=True).flat(4).bridge(6).flat(3)
L.crate_steps().flat(3).drop(2).flat(4).secret_ledge().bridge(8).goal()
save("01-amanecer.txt", 1, "Amanecer", "Daybreak",
     "A la izquierda de la línea es de día; a la derecha, de noche.\\nMuévela con Shift (o Q/E), o arrastrándola.",
     "Left of the line is day; right of it, night.\\nMove it with Shift (or Q/E), or drag it.", 4, L)

L = Level().start().flat(4).ice_bridge(5).flat(5).ice_steps(star=True).check().flat(3).ice_bridge(8).flat(4)
L.ice_steps().flat(3).secret_ledge("n").ice_bridge(6).goal()
save("02-noche-estrellada.txt", 1, "Noche estrellada", "Starry Night",
     "Lo que brilla en azul solo existe de noche.", "What glows blue only exists at night.", 60, L)

L = Level().start().flat(3).bridge(5).ice_bridge(5).flat(3).night_wall().check().day_wall().flat(3)
L.bridge(6).ice_steps(star=True).flat(2).crate_steps().secret_ledge().drop(2).flat(3).night_wall().ice_bridge(6).goal()
save("03-las-dos-mitades.txt", 1, "Las dos mitades", "Two Halves",
     "A veces necesitas el día y la noche a la vez.", "Sometimes you need day and night at once.", 3, L)

L = Level().start().flat(3).lake(8, star=True).flat(3).spring_wall().flat(3).drop(5).flat(3).check().lake(10)
L.bridge(5).spring_wall(4).secret_ledge().drop(4).lake(6).flat(2).ice_bridge(5).goal()
save("04-lago-helado.txt", 1, "Lago helado", "Frozen Lake",
     "De día el lago es agua. De noche, hielo.\\nLas flores te impulsan, pero solo de día.",
     "By day the lake is water. By night, ice.\\nFlowers launch you, but only by day.", 30, L)

# ───────────────────────── Capítulo 2 · El Bosque Otoñal ─────────────────────────

L = Level().start().flat(3).creature(12).ice_bridge(5).creature(10).check().bridge(6).creature(14).flat(2)
L.ice_steps(star=True).crate_steps().secret_ledge().drop(2).creature(12).goal()
save("05-criaturas.txt", 2, "Criaturas", "Creatures",
     "Las Sombras duermen de día: se vuelven de piedra.", "Shadows sleep by day: they turn to stone.", 3, L)

L = Level().start().flat(3).bee_over_ice(8).flat(3).bridge(6).bee_over_ice(10).check().ice_steps(star=True)
L.flat(2).bee_over_ice(7).secret_ledge("n").lake(7).bee_over_ice(8).goal()
save("06-alas-en-la-noche.txt", 2, "Alas en la noche", "Wings at Night",
     "Las abejas de la noche vuelan bajo: espera tu momento.", "Night bees fly low: wait for your moment.", 3, L)

L = Level().start().flat(3).bridge(6).ice_bridge(6).flat(3).night_wall().check().creature(10).lake(8)
L.spring_wall().secret_ledge().drop(5).day_wall().ice_steps(star=True).bridge(6).goal()
save("07-al-reves.txt", 2, "Al revés", "Upside Down",
     "Cuidado: aquí el día está a la DERECHA de la línea.", "Careful: here day is on the RIGHT of the line.", 80, L, "dia: derecha\n")

L = Level().start().flat(3).saws(12).bridge(5).saws(10).check().ice_bridge(6).saws(14).spikes(star=True)
L.crate_steps().secret_ledge().drop(2).saws(10).creature(10).goal()
save("08-sierras.txt", 2, "Sierras del bosque", "Forest Saws",
     "Las sierras suben y bajan. Mira antes de saltar.", "The saws rise and fall. Look before you leap.", 3, L)

# ───────────────────────── Capítulo 3 · La Cumbre Helada ─────────────────────────

# La torre: subida en zigzag con la línea atrapada entre dos postes.
T = Level()
T.ground(0, 40, TOP)
T.put("S", 2, TOP - 1)
for col, row, kind in [(13, 10, "D"), (17, 9, "N"), (13, 7, "D"), (17, 5, "N"), (13, 3, "D"), (17, 1, "N")]:
    T.fill(kind, [col, col + 1], [row])
T.fill("#", range(22, 30), [1])
T.put("*", 15, 2)
T.put("K", 24, 0)
T.put("G", 28, 0)
T.put("C", 7, TOP - 1)
save("09-la-torre.txt", 3, "La torre", "The Tower",
     "La línea no puede salir de entre los postes rojos.\\nSube en zigzag.",
     "The line can't leave the red posts.\\nClimb in a zigzag.", 15, T, "limites: 15 17\n")

L = Level().start().flat(3).lake(8).flat(2).spring_wall().flat(2).drop(5).lake(10, star=True).check().spring_wall(4)
L.secret_ledge().drop(4).lake(8).flat(2).bee_over_ice(7).goal()
save("10-rio-nocturno.txt", 3, "Río nocturno", "Night River",
     "Una cosa a cada lado de la línea.", "One thing on each side of the line.", 12, L)

L = Level().start().flat(2).lake(6).bridge(5).creature(10).check().ice_steps(star=True).saws(10).crate_steps()
L.night_wall().spring_wall(3).bee_over_ice(7).check().secret_ledge("n").drop(5).day_wall().spikes(star=True)
L.lake(8).creature(12).bridge(6).goal()
save("11-eclipse.txt", 3, "Eclipse", "Eclipse",
     "El último paso. Usa todo lo que has aprendido.", "The final step. Use everything you've learned.", 3, L)
