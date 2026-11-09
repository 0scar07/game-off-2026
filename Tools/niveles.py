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
        elif char in "*KGHSC":
            # Un fragmento o una meta fuera del mapa se perdía sin avisar (pasó en el Jardín del viento).
            raise ValueError(f"'{char}' fuera del mapa (columna {col}, fila {row})")

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

    def goal(self, alt=False):
        """Meta. Con alt=True hay además una meta de noche en lo alto de unas placas de hielo."""
        if alt:
            t, x = self.top, self.x
            self.flat(14)
            self.fill("n", range(x + 2, x + 4), [t - 2])
            self.fill("n", range(x + 5, x + 7), [t - 4])
            self.fill("n", range(x + 8, x + 11), [t - 6])
            self.put("H", x + 9, t - 7)
            self.put("G", self.x - 2, t - 1)
            return self
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

    # ───────────── Tramos de la segunda tanda ─────────────

    def sun_gap(self, w=9, star=False):
        """Hueco con un reloj de sol: la plataforma solo se mueve mientras le da el día."""
        self.put("O", self.x + w // 2, self.top)
        if star:
            self.put("*", self.x + w // 2, self.top - 4)
        self.x += w
        return self.flat(3)

    def updraft(self, height=6):
        """Corriente de aire (de día) junto a un muro alto: sube flotando y sigue arriba."""
        t, x = self.top, self.x
        self.ground(x, x + 3, t)
        self.fill("W", [x + 1], range(t - height - 1, t))
        self.top = t - height
        self.x += 3
        return self.flat(5)

    def firefly_gap(self, w=8):
        """Hueco con losas escondidas: aparecen de noche cuando el farol se acerca."""
        self.fill("Y", range(self.x, self.x + w), [self.top])
        self.x += w
        return self.flat(3)

    def triple_bridge(self, w=5):
        """Madera, hielo y madera seguidos: hace falta día | noche | día (dos líneas)."""
        self.fill("d", range(self.x, self.x + w), [self.top])
        self.fill("n", range(self.x + w, self.x + 2 * w), [self.top])
        self.fill("d", range(self.x + 2 * w, self.x + 3 * w), [self.top])
        self.x += 3 * w
        return self.flat(3)

    def box_wall(self):
        """Caja que cambia delante de un muro de 3: empújala hasta el muro (de noche resbala) y súbete."""
        t, x = self.top, self.x
        self.flat(12)
        self.put("Z", x + 3, t - 1)
        self.fill("#", [x + 10, x + 11], range(t - 3, t))
        self.top = t - 3
        return self.flat(4)

    def boss_arena(self, kind="J", stairs="D"):
        """Arena de un jefe: escalera (de cajas de día o de hielo de noche) para subir a su cabeza."""
        t, x = self.top, self.x
        self.ground(x, x + 30, t)
        self.put("G", x + 4, t - 1)
        self.put(stairs, x + 12, t - 1)
        self.fill(stairs, [x + 13], [t - 1, t - 2])
        self.fill(stairs, [x + 14], [t - 1, t - 2, t - 3])
        self.put(kind, x + 18, t - 1)
        self.x += 30
        return self


def save(file, chapter, name, name_en, hint, hint_en, line, level, extra=""):
    os.makedirs(os.path.dirname(os.path.join(OUT, file)), exist_ok=True)
    # Una pista con saltos de línea iría en varias líneas y el juego solo leería la primera.
    hint, hint_en = hint.replace("\n", "\\n"), hint_en.replace("\n", "\\n")
    body = (f"nombre: {name}\nname: {name_en}\npista: {hint}\nhint: {hint_en}\n"
            f"capitulo: {chapter}\nlinea: {line}\n{extra}---\n{level.text()}\n")
    with open(os.path.join(OUT, file), "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    width = max(len(r) for r in level.text().split("\n"))
    print(f"{file}: {width} columnas")


def clean():
    for folder in (OUT, os.path.join(OUT, "Diarios")):
        if not os.path.isdir(folder):
            continue
        for f in os.listdir(folder):
            if f.endswith(".txt"):
                os.remove(os.path.join(folder, f))


clean()

# ───────────────────────── Capítulo 1 · La Pradera ─────────────────────────

L = Level().start().flat(6).bridge(5).flat(6).bridge(7).check().flat(4).spikes(star=True).flat(4).bridge(6).flat(3)
L.crate_steps().flat(3).drop(2).flat(4).secret_ledge().bridge(8).goal()
save("01-amanecer.txt", 1, "Amanecer", "Daybreak",
     "A la izquierda de la línea es de día; a la derecha, de noche.\\nMuévela con Shift (o Q/E), o arrastrándola.",
     "Left of the line is day; right of it, night.\\nMove it with Shift (or Q/E), or drag it.", 4, L)

# Dos muros de hielo: el primero pide dejar la línea justo entre el muro y el puente; el segundo,
# devolverla a la derecha después de las placas. Así hay que colocarla, no solo moverla una vez.
L = Level().start().flat(4).ice_bridge(5).flat(5).ice_steps(star=True).check().night_wall().ice_bridge(8).flat(4)
L.ice_steps().night_wall().secret_ledge("n").ice_bridge(6).goal(alt=True)
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
L.flat(2).bee_over_ice(7).secret_ledge("n").lake(7).bee_over_ice(8).goal(alt=True)
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
L.secret_ledge().drop(4).lake(8).flat(2).bee_over_ice(7).goal(alt=True)
save("10-rio-nocturno.txt", 3, "Río nocturno", "Night River",
     "Una cosa a cada lado de la línea.", "One thing on each side of the line.", 12, L)

L = Level().start().flat(2).lake(6).bridge(5).creature(10).check().ice_steps(star=True).saws(10).crate_steps()
L.night_wall().spring_wall(3).bee_over_ice(7).check().secret_ledge("n").drop(5).day_wall().spikes(star=True)
L.lake(8).creature(12).bridge(6).goal()
save("11-eclipse.txt", 3, "Eclipse", "Eclipse",
     "El último paso. Usa todo lo que has aprendido.", "The final step. Use everything you've learned.", 3, L)

# ───────────────────────── Niveles de la segunda tanda ─────────────────────────

L = Level().start().flat(3).sun_gap().flat(2).sun_gap(11).check().box_wall().updraft(4).flat(2).bridge(5)
L.drop(5).sun_gap().flat(2).updraft(4).secret_ledge().drop(4).spikes(star=True).goal()
save("04b-relojes-de-sol.txt", 1, "Relojes de sol", "Sundials",
     "Las plataformas con engranaje solo se mueven de día.\nEl aire de día te eleva.",
     "Geared platforms only move by day.\nDaytime air lifts you up.", 3, L)

L = Level().start().flat(3).firefly_gap(6).flat(3).ice_bridge(6).check().firefly_gap(9).creature(10)
L.flat(2).firefly_gap(7).secret_ledge("n").bridge(6).check().firefly_gap(10).ice_steps(star=True).goal()
save("08b-luciernagas.txt", 2, "Luciérnagas", "Fireflies",
     "Donde bailan las luciérnagas hay un camino escondido.\nSolo lo ve tu farol, y solo de noche. La línea tiene poca energía.",
     "Where fireflies dance, there's a hidden path.\nOnly your lantern sees it, only at night. The line has little energy.", 3, L,
     "energia: 45\n")

L = Level().start().flat(3).bridge(5).ice_bridge(5).check().triple_bridge(5).flat(3).creature(10)
L.triple_bridge(4).check().secret_ledge().drop(2).triple_bridge(6).flat(2).ice_steps(star=True).goal()
save("10b-dos-lineas.txt", 3, "Dos líneas", "Two Lines",
     "Ahora hay DOS líneas: entre ellas, el otro mundo.\nCon el teclado se mueve la más cercana a ti.",
     "Now there are TWO lines: between them, the other world.\nThe keyboard moves the one closest to you.", 3, L,
     "linea2: 24\n")

L = Level().start().flat(4).check().flat(6).boss_arena()
save("12-la-gran-sombra.txt", 3, "La Gran Sombra", "The Great Shadow",
     "Dale luz de día para volverla piedra.\nSube a su cabeza y toca el fragmento. Tres veces.",
     "Give it daylight to turn it to stone.\nClimb to its head and touch the fragment. Three times.", 8, L)

# ───────────────────────── Niveles secretos ─────────────────────────

L = Level().start().updraft(6).sun_gap(11, star=True).bridge(7).updraft(4).sun_gap(13).spikes().flat(3).goal()
save("90-secreto-jardin-del-viento.txt", 1, "Jardín del viento", "Wind Garden",
     "Nivel secreto. Todo el viento de la pradera.", "Secret level. All the meadow's wind.", 3, L, "secreto: si\n")

L = Level().start().firefly_gap(8).creature(8).firefly_gap(10).bee_over_ice(8).firefly_gap(12).spikes(star=True).goal()
save("91-secreto-bosque-oscuro.txt", 2, "Bosque oscuro", "Dark Wood",
     "Nivel secreto. Muy poca energía: piensa antes de mover la línea.", "Secret level. Very little energy: think before you move the line.", 3, L,
     "secreto: si\nenergia: 30\n")

L = Level().start().triple_bridge(5).sun_gap().triple_bridge(6).updraft(5).firefly_gap(6).triple_bridge(7).spikes(star=True).goal()
save("92-secreto-aurora.txt", 3, "Aurora", "Aurora",
     "Nivel secreto. Dos líneas, viento y luciérnagas.", "Secret level. Two lines, wind and fireflies.", 3, L,
     "secreto: si\nlinea2: 20\n")

# Nivel de práctica (Extras → Nivel de trucos): no cuenta como secreto del mapa.
L = Level().start().flat(3).bridge(5).ice_bridge(5).flat(3).check().triple_bridge(5).flat(3)
L.firefly_gap(8).flat(2).bridge(4).ice_bridge(4).flat(3).goal()
save("95-trucos-de-la-linea.txt", 3, "Trucos de la línea", "Line tricks",
     "Práctica: una cosa a cada lado de la línea, DOS líneas a la vez\ny la burbuja de noche del farol (L) donde bailan las luciérnagas.",
     "Practice: one thing on each side of the line, TWO lines at once\nand the lantern's night bubble (L) where fireflies dance.", 3, L,
     "secreto: si\npractica: si\nlinea2: 30\n")

# ───────────────────────── Desafíos diarios ─────────────────────────
# Cortos y distintos cada día: tramos al azar con una semilla fija por número.

import random

TRAMOS = [
    lambda L: L.bridge(random.choice([5, 6, 7])), lambda L: L.ice_bridge(random.choice([5, 6, 7])),
    lambda L: L.ice_steps(), lambda L: L.crate_steps(), lambda L: L.lake(random.choice([6, 8])),
    lambda L: L.spring_wall(4), lambda L: L.creature(10), lambda L: L.saws(10), lambda L: L.spikes(),
    lambda L: L.sun_gap(), lambda L: L.updraft(5), lambda L: L.firefly_gap(6), lambda L: L.night_wall(),
    lambda L: L.box_wall(),
]

for n in range(1, 15):
    random.seed(1000 + n)
    L = Level().start().flat(3)
    for i in range(6):
        random.choice(TRAMOS)(L)
        while L.top < 7:   # deja sitio por encima para muros y muelles
            L.drop(2)
        L.flat(2)
        if i == 2:
            L.check()
    L.spikes(star=True).goal()
    save(f"Diarios/diario-{n:02}.txt", 1 + (n % 3), f"Desafío {n}", f"Challenge {n}",
         "Desafío diario: mañana habrá otro.", "Daily challenge: there's a new one tomorrow.", 3, L)

# ───────────────────────── Línea tumbada e inclinada ─────────────────────────

def vertical_tower():
    """La subida: la línea está tumbada (arriba día, abajo noche). Plataformas de hielo, madera y
    piedra en zigzag: hielo por debajo de la línea, madera por encima, piedra para descansar."""
    width, rows = 24, 36
    grid = [[" "] * width for _ in range(rows)]
    for r in range(rows - 3, rows):
        for c in range(width):
            grid[r][c] = "#"
    grid[rows - 4][3] = "S"
    kinds = ["n", "d", "#"]
    top = rows - 3
    for i in range(14):
        top -= 2
        left = 3 if i % 2 == 0 else 10
        kind = kinds[i % 3]
        for c in range(left, left + 6):
            grid[top][c] = kind
        if i == 6:
            grid[top - 1][left + 3] = "*"
        if i % 3 == 2:
            grid[top - 1][left + 1] = "C" if i < 12 else " "
    grid[top - 1][12] = "G"
    grid[top - 3][17] = "K"   # secreto: hace falta la burbuja de noche del farol
    for c in range(16, 20):
        grid[top - 2][c] = "n"
    return "\n".join("".join(r).rstrip() for r in grid)


def save_text(file, chapter, name, name_en, hint, hint_en, line, text, extra=""):
    # Una pista con saltos de línea iría en varias líneas y el juego solo leería la primera.
    hint, hint_en = hint.replace("\n", "\\n"), hint_en.replace("\n", "\\n")
    body = (f"nombre: {name}\nname: {name_en}\npista: {hint}\nhint: {hint_en}\n"
            f"capitulo: {chapter}\nlinea: {line}\n{extra}---\n{text}\n")
    with open(os.path.join(OUT, file), "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    print(f"{file}: torre")


save_text("10c-la-subida.txt", 3, "La subida", "The Climb",
          "La línea ahora está TUMBADA: arriba es de día, abajo de noche.\nSúbela y bájala con Q/E o arrastrándola.",
          "The line is now LYING DOWN: day above, night below.\nRaise and lower it with Q/E or by dragging.",
          5, vertical_tower(), "angulo: 90\ndia: derecha\n")

L = Level().start().flat(3).bridge(5).flat(2).ice_bridge(6).check().box_wall().bridge(7).creature(10).ice_steps(star=True)
L.crate_steps().secret_ledge().drop(2).ice_bridge(7).bridge(6).goal()
save("11b-cuesta-de-la-luna.txt", 3, "Cuesta de la luna", "Moon Slope",
     "La línea se ha inclinado. Arriba y abajo ya no son lo mismo.", "The line has tilted. High and low are no longer the same.", 6, L,
     "angulo: 25\n")

# ───────────────────────── Jefes intermedios ─────────────────────────

L = Level().start().flat(4).box_wall().check().flat(6).boss_arena("P", "N")
save("04c-garra-del-alba.txt", 1, "Garra del Alba", "Dawn Talon",
     "Ojo: aquí el día está a la DERECHA. Un pájaro enorme que solo vive de día:\nllévale la noche para dormirlo y sube por el hielo hasta su cabeza. Dos veces.",
     "Careful: here day is on the RIGHT. A huge bird that only lives by day:\nbring it night to put it to sleep and climb the ice to its head. Twice.", 26, L, "dia: derecha\n")

L = Level().start().flat(4).check().flat(6).boss_arena("R", "D")
save("08c-reina-de-escarcha.txt", 2, "Reina de Escarcha", "Frost Queen",
     "La reina del hielo despierta de noche. Dale el día y sube por las cajas.",
     "The ice queen wakes at night. Give it daylight and climb the crates.", 8, L)
