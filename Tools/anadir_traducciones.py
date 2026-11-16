"""Añade traducciones (portugués y francés) a Assets/Scripts/LocTranslations2.cs.

Cada entrada se busca por el texto en español exacto (el primer argumento de Loc.Pick o el
español de Loc.Get). Uso desde otro script:

    from anadir_traducciones import add
    add([("Texto en español", "Texto em português", "Texte en français")])

Las que ya existen se dejan como están. Para ver qué falta, juega en portugués o francés en el
editor: lo que salga en inglés por falta de traducción queda en Logs/sin-traducir.txt.
"""
import os
import re

PATH = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "Scripts", "LocTranslations2.cs")
LIT = r'"((?:[^"\\]|\\.)*)"'


def add(rows):
    src = open(PATH, encoding="utf-8").read()
    existing = set(m.group(1) for m in re.finditer(r'\[' + LIT + r'\]\s*=', src))
    new = []
    for es, pt, fr in rows:
        if es in existing:
            continue
        for s in (es, pt, fr):
            assert '"' not in s.replace('\\"', ''), s
        new.append(f'            ["{es}"] = ("{pt}", "{fr}"),')
        existing.add(es)
    if not new:
        return 0
    end = src.rindex("        };")
    src = src[:end] + "\n".join(new) + "\n" + src[end:]
    open(PATH, "w", encoding="utf-8", newline="\n").write(src)
    return len(new)


if __name__ == "__main__":
    print(add([
        ("Nivel de la semana", "Nível da semana", "Niveau de la semaine"),
    ]), "añadidas")
