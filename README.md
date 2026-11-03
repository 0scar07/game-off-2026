<div align="center">

# FASE

**Mueve la línea. Cambia el mundo.** · *Shift the line. Change the world.*

Un plataformas 2.5D para el **GitHub Game Off 2026** (tema **SHIFT**).
Juega en el navegador, en el ordenador y en el móvil.

![FASE](docs/fase.gif)

</div>

---

## Español

Una línea parte el mundo en dos. A un lado es de **día**: puentes de madera, flores que te lanzan
al cielo, relojes de sol que se mueven. Al otro es de **noche**: hielo, luciérnagas y Sombras
que despiertan en la oscuridad. Eres **Lumi** y puedes **mover la línea**: un puente solo existe
mientras le da el sol, un lago helado solo te sostiene mientras hay luna.

| | |
|---|---|
| ![Día y noche](docs/dia-noche.jpg) | ![El lago](docs/lago.jpg) |
| ![Las Sombras](docs/sombras.jpg) | ![Dos líneas](docs/dos-lineas.jpg) |

### Qué tiene

- **19 niveles** en 3 capítulos, **3 jefes** (Garra del Alba, Reina de Escarcha y la Gran Sombra) y **3 niveles secretos**.
- La línea puede ser vertical, **inclinada**, **tumbada** o **doble**; el farol de Lumi abre una burbuja de noche.
- Relojes de sol, corrientes de aire, caminos de luciérnagas, cajas que cambian de madera a hielo, energía de la línea.
- **Medallas de tiempo** (Bronce, Plata, Oro y Luna), **22 logros**, desafíos por nivel, fantasma de tu récord y repeticiones.
- **Diario de Lumi**, dos finales, cooperativo local (uno mueve a Lumi y otro la línea) y Nox, la Lumi de la noche.
- **Taller de niveles** con códigos para compartir, **modo sin fin**, **desafío diario**, tienda de cosméticos y modo foto.
- Accesibilidad: modo asistido, alto contraste, teclas configurables, vibración, velocidad de la línea.
- Música y efectos propios que cambian entre el día y la noche, en español e inglés.

### Controles

| Acción | Teclado | Ratón / táctil | Mando |
|---|---|---|---|
| Moverse | A / D o ← → | botones en pantalla | stick izquierdo |
| Saltar | Espacio / W / ↑ | botón en pantalla | A / ✕ |
| Mover la línea | Q / E o Shift | arrastrar la línea | — |
| Burbuja de noche | L | botón del farol | Y / △ |
| Pausa · reintentar · foto | Esc · R · F | botón de pausa | Start |

---

## English

A line splits the world in two. On one side it's **day**: wooden bridges, flowers that launch you
skyward, sundials that move. On the other it's **night**: ice, fireflies and Shadows that wake in
the dark. You are **Lumi**, and you can **shift the line**. A bridge only exists while the sun
shines on it. A frozen lake only holds you while the moon is up.

19 levels across 3 chapters, 3 bosses, secret levels, time medals, achievements, a level workshop
with shareable codes, an endless mode, a daily challenge, local co-op, two endings, and English and
Spanish throughout.

---

## Cómo está hecho · How it's made

- **Motor:** Unity 6.6 (URP), C#. Web (WebGL), escritorio y móvil.
- **Los niveles son texto.** Cada nivel es un `.txt` en [`Assets/Levels`](Assets/Levels) (ver
  [LEEME](Assets/Levels/LEEME.md)). El menú **Fase → Generar niveles** de Unity los convierte en
  escenas con arte, colisiones, luces, música e interfaz.
- **Shaders propios** ([`Assets/Shaders`](Assets/Shaders)): recortan cada objeto por la línea
  en pantalla, así el corte es limpio para lo cercano y para el fondo.
- **Herramientas** ([`Tools`](Tools)) en Python: niveles (`niveles.py`), música (`musica*.py`),
  efectos y voz de Lumi (`efectos.py`), capas que reaccionan al juego (`musica_extra.py`)
  y el montaje del tráiler (`montar_trailer.py`).
- **Tráiler:** un director de cámara dentro del juego graba los planos fotograma a fotograma
  ([`TrailerDirector.cs`](Assets/Scripts/TrailerDirector.cs)).

### Abrir el proyecto

1. Instala **Unity 6000.6.7f1** desde Unity Hub (con *WebGL Build Support*).
2. Abre esta carpeta como proyecto.
3. Menú **Fase → Generar niveles** y pulsa *Play* en la escena `Menu`.
4. **Fase → Exportar web** deja la versión de navegador en `Builds/Web`.

### Estructura

```
Assets/
  Levels/        niveles en texto (+ Diarios/ con los desafíos diarios)
  Scripts/       el juego (línea, Lumi, jefes, menús, logros, taller...)
  Editor/        el generador de niveles y las exportaciones
  Shaders/       recorte por la línea, partículas
  Art/ Audio/    modelos, fuentes, música, efectos y voz
Tools/           scripts de Python (niveles, música, efectos, tráiler)
Trailer/         títulos y música del tráiler
docs/            capturas para este README
```

---

## Créditos

- Diseño, niveles y programación: **[0scar07](https://github.com/0scar07)**
- Modelos 3D: **[Quaternius](https://quaternius.com)** y **[Kenney](https://kenney.nl)** (CC0)
- Fuente: **Fredoka** (SIL Open Font License), con símbolos añadidos para el juego (*Fredoka Fase*)
- Música, efectos y voz: generados para este juego (Python y voces del sistema)
- Hecho con ayuda de IA (Claude, de Anthropic) para el código, las herramientas y los textos;
  las ideas, el diseño y las pruebas son del autor.

Hecho para el [GitHub Game Off 2026](https://itch.io/jam/game-off-2026).

## Licencia · License

- **Código** (scripts, shaders, herramientas): [MIT](LICENSE).
- **Modelos** de Quaternius y Kenney: CC0 (dominio público).
- **Fuente** Fredoka y su versión *Fredoka Fase*: [SIL Open Font License 1.1](Assets/Art/Fonts/OFL-Fredoka.txt).
- **Música, efectos, voz, ilustraciones y niveles** hechos para FASE: © 2026 0scar07.
