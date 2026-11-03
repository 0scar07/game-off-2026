# Niveles de FASE

Cada archivo `.txt` de esta carpeta es un nivel. Se juegan en orden alfabético (por eso empiezan por `01-`, `02-`...). Después de editar uno, en Unity: **Fase → Generar niveles**.

## Cabecera

```
nombre: Amanecer
pista: Texto que aparece al empezar. \n hace un salto de línea.
linea: 4
---
(mapa)
```

`linea` es la columna donde empieza la línea. A su izquierda es de día y a su derecha de noche.

Opcionales:
- `dia: derecha` — el mundo al revés: de día a la derecha de la línea y de noche a la izquierda.
- `limites: 10 30` — la línea solo puede moverse entre esas columnas (se marcan con postes rojos).
- `linea2: 24` — segunda línea: entre las dos, el otro mundo.
- `energia: 40` — mover la línea gasta energía; los puntos de control la rellenan.
- `medallas: 90 65 48 39` — tiempos de bronce, plata, oro y luna (si no, se calculan por el largo).
- `secreto: si` — nivel secreto (se abre con todos los fragmentos de su capítulo).

Los desafíos diarios están en `Diarios/`.

## Mapa

Cada letra es una casilla de 1×1. El personaje mide 2 casillas de alto, salta unas 2,5 casillas hacia arriba y unas 4 de lado. La fila de más abajo está en el suelo del mundo; si caes por debajo, vuelves al último punto de control.

| Letra | Qué es | Mundo |
|---|---|---|
| `#` | suelo de hierba y roca | los dos |
| `D` | caja de madera | día |
| `d` | tabla de madera (fina) | día |
| `N` | bloque de hielo | noche |
| `n` | placa de hielo (fina) | noche |
| `~` | lago: agua que hace perder de día, hielo que se pisa de noche | los dos |
| `^` | pinchos | los dos |
| `F` | flor-muelle: te impulsa hacia arriba | día |
| `M` | Sombra: patrulla y hace perder; de día es de piedra | noche |
| `B` | Sombra voladora (abeja) que va y viene | noche |
| `X` | sierra que sube y baja | los dos |
| `K` | cofre secreto: da un fragmento de luna | los dos |
| `*` | fragmento de luna (coleccionable) | los dos |
| `C` | punto de control | los dos |
| `O` | reloj de sol: plataforma que se mueve de día y se para de noche | los dos |
| `W` | corriente de aire (columna): de día empuja hacia arriba | día |
| `Y` | losa de luciérnagas: aparece de noche junto al farol | noche |
| `J` | la Gran Sombra (jefe); la meta aparece al vencerla | los dos |
| `S` | salida del personaje | — |
| `G` | meta | los dos |
| espacio | aire | — |

Los árboles, flores y setas se colocan solos encima del suelo.

`Tools/niveles.py` crea los 11 niveles encadenando tramos (puente, lago, muelle, muros...). Puedes usarlo o editar los `.txt` a mano.
