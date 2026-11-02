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
| `M` | criatura que patrulla y hace perder | noche |
| `*` | fragmento de luna (coleccionable) | los dos |
| `C` | punto de control | los dos |
| `S` | salida del personaje | — |
| `G` | meta | los dos |
| espacio | aire | — |

Los árboles, flores y setas se colocan solos encima del suelo.

`Tools/niveles.py` dibuja los mapas de los niveles 1 a 6. Puedes usarlo o editar los `.txt` a mano.
