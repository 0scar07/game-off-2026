# Cómo ayudar con FASE

¡Gracias por querer ayudar! Estas son las formas más sencillas, de menos a más trabajo.

## 1. Jugar y contar qué tal

- **Opinión:** la plantilla [Prueba de juego](https://github.com/0scar07/game-off-2026/issues/new?template=prueba.yml)
  (también sale con el botón *Tu opinión* al terminar el juego).
- **Fallos:** la plantilla [Fallo](https://github.com/0scar07/game-off-2026/issues/new?template=fallo.yml).
  Ayuda mucho decir el nivel, el navegador o dispositivo y, si puedes, una captura.
- **Ideas:** la plantilla [Idea](https://github.com/0scar07/game-off-2026/issues/new?template=idea.yml).

## 2. Mandar un nivel (sin programar)

1. En el juego, abre el **Taller** (barra del menú).
2. Haz tu nivel: pon una **salida**, una **meta** y lo que quieras en medio. Pruébalo con *Probar*.
3. Pulsa **Copiar código** y pégalo en la plantilla
   [Nivel de la comunidad](https://github.com/0scar07/game-off-2026/issues/new?template=nivel.yml).

Los mejores salen como **nivel de la semana**. Para ponerlo, quien mantiene el repo edita
[`semana/nivel.txt`](semana/nivel.txt):

```
Nombre en español | Name in English
autor
FASE-...código...
```

El juego lo descarga solo; no hace falta publicar una versión nueva.

## 3. Traducir

Los textos están en [`Assets/Scripts/Loc.cs`](Assets/Scripts/Loc.cs) (español e inglés) y el portugués
y el francés en [`LocTranslations.cs`](Assets/Scripts/LocTranslations.cs) y
[`LocTranslations2.cs`](Assets/Scripts/LocTranslations2.cs), buscados por el texto en español.

- Para añadir traducciones: [`Tools/anadir_traducciones.py`](Tools/anadir_traducciones.py).
- Para ver qué falta: juega en portugués o francés dentro de Unity; lo que salga en inglés por falta de
  traducción queda apuntado en `Logs/sin-traducir.txt`.
- La fuente es [`FredokaFase.ttf`](Assets/Art/Fonts): si usas un símbolo nuevo, tiene que estar en ella
  (en la web no hay fuentes de reserva). [`Tools/fuente_simbolos.py`](Tools/fuente_simbolos.py) los añade.

## 4. Programar o hacer niveles de la historia

1. Instala **Unity 6000.6.7f1** y abre la carpeta como proyecto (ver el [README](README.md#abrir-el-proyecto)).
2. Los niveles son texto: edita [`Tools/niveles.py`](Tools/niveles.py), ejecútalo y luego
   **Fase → Generar niveles** en Unity. El formato está en [`Assets/Levels/LEEME.md`](Assets/Levels/LEEME.md).
3. Antes de mandar un cambio:
   - `python Tools/comprobar_niveles.py` (todos los niveles deben poder terminarse),
   - prueba en Unity que la escena carga sin errores,
   - si tocas textos, que existan en los cuatro idiomas.
4. Abre un *pull request* contra la rama `fase` con una frase de qué cambia y por qué.

### Estilo

- Código en C# con comentarios en español, cortos y que expliquen el porqué.
- Nada de dependencias nuevas sin hablarlo antes: el juego tiene que exportar a la web.
- La interfaz se diseña a 1920×1080 y tiene que verse bien también a 4:3, 21:9 y en el móvil.

## Licencia de las contribuciones

El código que mandes queda bajo la [licencia MIT](LICENSE) del proyecto. Los niveles de la comunidad
llevan el nombre de su autor.
