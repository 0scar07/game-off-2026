# Pulido final de FASE

Lista de comprobación de todo lo que tiene el juego. Nada se da por bueno sin verlo funcionar en
Unity (o en la web). Regla: no se añade nada nuevo; solo se arregla, se aclara y se pule.

**Estados:** ⬜ sin probar · ✅ probado bien · ❌ con fallo · 🔧 arreglado y vuelto a probar

**Plan (8 días, del 10 al 18 de noviembre):** días 1-2 inventario, pruebas y web · días 3-5
arreglar y pulir lo visual y el sonido · día 6 experiencia de usuario (con alguien que no conozca
el juego) · día 7 repetir todas las pruebas · día 8 versión final, README, créditos e itch.

---

## Diario de trabajo

### Día 1 · 10 de noviembre
- Inventario hecho (este documento).
- Las 60 escenas cargan en Play sin un solo error ni excepción (gira automática moviendo la línea).
- Música y voz medidas: volúmenes entre −17 y −21 dB (los tambores del jefe, más fuertes a
  propósito); los bucles no tienen cortes (los "saltos" medidos son el golpe de tambor del primer
  compás). La voz no tiene silencios raros al principio ni al final.
- Arreglado: los niveles secretos y extra mostraban un número de la historia ("27. Ciudad de la
  aurora"); ahora llevan ★.
- Web exportada con el PC libre (cerrando Unity y abriéndolo solo para exportar): 25,7 MB, 63 min.
  La prueba automática la abre en 10,5 s, pasa el inicio, entra al nivel 1 y juega sin errores.
- Subida a itch con la release `web-20261110-1`: la prueba de GitHub pasó y la subida también.
- Fase 2, menús: gira automática por los 21 paneles en los 4 idiomas a 1920×1080, con foto de cada
  uno y un detector de textos que no caben. Arreglados: contadores y subtítulos encima del título
  (Logros, Tienda, Diario, Galería, Vestuario), filas que tapaban "Volver" (Logros, Niveles extra),
  iconos que pisaban el texto (Logros, Tienda, Extras, Estadísticas), nombres encima de las muestras
  del vestuario, textos que no se encogían (faltaba el ajuste de línea) y el fondo de Opciones y los
  botones del menú, que salían ovalados (al sprite le faltaba el borde de 9 cortes).
- Portugués y francés completados: faltaban ~400 textos (logros, diario, vestuario, tienda, taller,
  curiosidades, historia, pistas, niveles extra y desafíos). Los textos con números ahora tienen sus
  cuatro idiomas (Loc.Pick de 4). En el editor, Loc apunta en Logs/sin-traducir.txt lo que salga en
  inglés por falta de traducción: tras la gira de paneles y de las 60 escenas en portugués, vacío.
- Arreglado: al coger un copo salía "¡Una copo!"; la tienda no cambiaba de idioma hasta reiniciar;
  en Curiosidades el texto largo se salía de la tarjeta.
- Resoluciones: a 21:9 todo bien. A 4:3 se cortaban los lados (barra del menú, niveles 1 y 19 del
  mapa): ahora en pantallas más estrechas que 16:9 la interfaz escala por la anchura (AspectFit).
  Con el móvil en vertical, en los niveles sale un aviso para girarlo (se ve muy poco camino).
- Pausa y resultados vistos en español y francés. Resultados: el mensaje de la meta del HUD se
  quedaba grande y borroso detrás de las medallas, "¡Nueva medalla!" pisaba el nombre del nivel y
  "Código de fantasma" tapaba la línea de estadísticas. Arreglados.
- Bot de pruebas: ya salta los pinchos del nivel 1 (usaba su propio cálculo de suelo, que fallaba).
  Sigue sin resolver niveles enteros; las rutas las comprueba Tools/comprobar_niveles.py.
- Final: la pregunta "¿Qué harás con la luna?" no se leía sobre el sol (ahora va en una tarjeta) y
  "Reconstruirla" era blanco sobre amarillo (ahora texto oscuro). La luna que se forma salía como dos
  cuadrados crema encima del título "Amaneció": ahora es un círculo con halo, arriba en el cielo. La
  tarjeta para compartir ya no pisa el panel.

---

### Día 2 · 16 de noviembre
- Nuevo: **nivel de la semana** (43) descargado del repositorio (`semana/nivel.txt`), **encuesta**
  al terminar (62, botón «Tu opinión»), **Twitch** (120: el chat vota la línea, sin cuenta) y
  **récords en línea y analítica anónima** (80 y 140, con Supabase; ver más abajo).
- Resultados revisados con todas las medallas, con algunas y con ninguna, en los cuatro idiomas y
  a 16:9, 21:9 y 4:3: alineados y ordenados. Arreglado: la pista de la luciérnaga asomaba entre
  los botones.
- Gira de las 60 escenas con capturas: todo carga sin errores. Arreglado: los bocadillos de Lumi y
  Chispa se podían salir de la pantalla.
- README reescrito, CONTRIBUTING.md nuevo y descripción y temas del repositorio mejorados.

## 1 · Pantallas y paneles

| Qué | Estado | Notas |
|---|---|---|
| Inicio (logo partido por la línea) | ✅ | Carga sin errores |
| Intro (historia con voz) | ✅ | Carga sin errores · ⬜ escuchar la voz entera |
| Menú principal y su barra | ✅ | Barra con iconos grandes (tanda G) |
| El viaje (mapa) | ✅ | Lumi camina de nivel en nivel |
| Opciones | 🔧 | Fondo y botones ya no salen ovalados |
| Pausa y mapa de pausa | ✅ | Mapa oculto en Taller y Sin fin |
| Resultados | 🔧 | Ver fallo 10 |
| Final y final secreto | 🔧 | Final normal visto entero · ⬜ final secreto y "dejar el mundo partido" |
| Páginas de historia y celebración de capítulo | ⬜ | |
| Decisión del capítulo II | ✅ | Se ve y se elige |
| Extras: estadísticas, banda sonora, galería, álbum, mapa de capítulos | 🔧 | Iconos y nombre de la galería recolocados |
| Extras: retos de la semana, ayudas, niveles extra | 🔧 | Niveles extra ya no tapan Volver |
| Extras: retos, con amigos, habilidades | ✅ | Abren y se leen |
| Logros, diario, vestuario, tienda, curiosidades | 🔧 | Ver fallo 4 |

## 2 · Niveles

Todos cargan sin errores. Falta jugarlos de principio a fin y revisar cámara, pistas y lunas.

| Grupo | Niveles | Estado |
|---|---|---|
| Capítulo I · La Pradera | 1-6 (jefe: Garra del Alba) | ⬜ |
| Capítulo II · El Bosque | 7-12 (jefe: Reina de Escarcha) | ⬜ |
| Capítulo III · La Cumbre | 13-19 (jefe: La Gran Sombra) | ⬜ |
| Secretos | Jardín del viento, Bosque oscuro, Aurora | ⬜ |
| Extra · Pradera | Trucos de la línea, Atardecer, Llaves | ⬜ |
| Extra · Bosque | Espejos, La línea pesa, La noche te sigue, Ritmo | ⬜ |
| Extra · Cumbre | A oscuras, Línea curva, Sombras y estaciones, Escalada | ⬜ |
| Extra · Ciudad | Ciudad de la aurora, Tejados, La avenida | ⬜ |
| Extra · Mar | Marea y viento, El arrecife, Corrientes, Marea profunda | ⬜ |
| Desafíos diarios | 14 | ⬜ |
| Taller y Sin fin | | ⬜ |

## 3 · Mecánicas y piezas

| Qué | Estado |
|---|---|
| Línea vertical, inclinada, tumbada, doble, curva, que pesa, que persigue | ⬜ |
| Burbuja de noche del farol | ⬜ |
| Puentes de día, hielo de noche, lago (agua/hielo), cajas mágicas | ⬜ |
| Flores-muelle, corrientes, relojes de sol, luciérnagas | ⬜ |
| Sombras, abejas, sierras, pinchos | ⬜ |
| Brotes, espejos y dianas, llaves y puertas, palancas, cajas que se llevan | ⬜ |
| Marea, viento, estaciones, plataformas al ritmo, puertas de línea, sombra que corre | ⬜ |
| Puntos de control, meta, lunas, cofres | ⬜ |

## 4 · Personajes

| Qué | Estado |
|---|---|
| Lumi: correr, saltar, doble salto, planear, caer, aterrizar, dormir, celebrar, morir | ⬜ |
| Nox (Lumi de la noche) | ⬜ |
| Chispa (niveles 2, 7, 10, 14 y 17) | ✅ aparece y habla |
| Sombras dormidas y despiertas, abejas | ⬜ |
| Jefes en cada fase | ⬜ |
| Fauna (mariposas, pájaros...) | ⬜ |

## 5 · Recompensas y progreso

| Qué | Estado |
|---|---|
| Lunas (cogerlas, contador con iconos, toda la luna) | ⬜ |
| Medallas de tiempo (bronce, plata, oro, luna) y si los tiempos son alcanzables | ⬜ |
| Medallas de estilo | ⬜ |
| 30 logros: icono bloqueado/desbloqueado, texto, que se consigan bien | ⬜ |
| Plumas, hojas y copos | ⬜ |
| Estrellas de Chispa y farol Chispa | ⬜ |
| Páginas del diario | ⬜ |
| 14 cosméticos del vestuario, tienda y faroles | ⬜ |
| Habilidades (doble salto, planear, burbuja grande) | ⬜ |
| Récords, racha del desafío diario, tarjeta para compartir | ⬜ |

## 6 · Interfaz

| Qué | Estado |
|---|---|
| HUD (número, lunas, pista en tarjeta, tiempo, pausa, farol) | ✅ se ve bien |
| Símbolos de la fuente (todos los del código están en FredokaFase) | ✅ |
| Bocadillos de Lumi y Chispa | ✅ |
| Transiciones entre escenas | ⬜ |
| Controles táctiles y glifos de mando | ⬜ |
| Cuatro idiomas sin textos que se salgan | 🔧 menús y niveles sin textos cortados ni sin traducir |
| Texto grande de accesibilidad | ⬜ |
| Resoluciones 16:9, 21:9, 4:3 y móvil | 🔧 4:3 y vertical arreglados · ⬜ probar en un móvil real |

## 7 · Sonido

| Qué | Estado |
|---|---|
| 21 pistas de música y ambiente: volumen y bucles | ✅ medidas |
| Paso entre día y noche | ⬜ escuchar |
| Que la música no se reinicie al morir o cambiar de pantalla | ⬜ |
| Efectos (pasos, saltos, aparecer/desaparecer, recoger, botones) | ⬜ |
| Voz de la intro (es, en) | ✅ medida · ⬜ escuchar |

## 8 · Visuales

| Qué | Estado |
|---|---|
| Cielos, colinas y decoración de los cinco biomas, de día y de noche | ✅ capturas revisadas |
| Agua con olas, hielo con brillo | ✅ |
| Chispas de la línea, piezas que se deshacen, sombra de Lumi | ⬜ verlas en movimiento |
| Posprocesado y luces | ⬜ |

## 9 · Versión web

| Qué | Estado |
|---|---|
| Exportar sin quedarse sin memoria | 🔧 con el PC libre y código IL2CPP optimizado por tamaño |
| Prueba automática (Tools/prueba_web) | ✅ carga en 10,5 s, sin errores |
| Probar en navegador y en móvil | ⬜ |
| Subir a itch | ✅ release web-20261110-1 |

---

## Fallos encontrados

| # | Dónde | Qué pasa | Gravedad | Estado |
|---|---|---|---|---|
| 1 | HUD de niveles secretos y extra | Salía un número de la historia ("27.") | Molesta | 🔧 |
| 2 | Exportación web | Se queda sin memoria | Bloquea | 🔧 |
| 3 | Bot de pruebas | No salta los pinchos del nivel 1 | Herramienta | 🔧 |
| 4 | Menús (11 paneles) | Textos pisándose, filas bajo "Volver", iconos sobre el texto | Molesta | 🔧 |
| 5 | Opciones y botones del menú | Se veían ovalados (sprite sin borde de 9 cortes) | Molesta | 🔧 |
| 6 | Portugués y francés | ~400 textos salían en inglés | Molesta | 🔧 |
| 7 | Plumas del capítulo III | "¡Una copo!" | Molesta | 🔧 |
| 8 | Tienda | Los nombres no cambiaban de idioma | Molesta | 🔧 |
| 9 | Pantallas 4:3 y móvil | Se cortaban los lados de la interfaz | Molesta | 🔧 |
| 10 | Resultados | Texto de la meta borroso detrás de las medallas; textos que se pisaban | Se nota | 🔧 |
| 11 | Final | Luna cuadrada tapando el título; pregunta y botón que no se leían | Se nota | 🔧 |

---

## Para seguir sin Claude

Todo lo necesario para mantener y publicar FASE a mano.

### Probar un cambio
1. Abre el proyecto en Unity 6000.6.7f1 y, si tocaste niveles, **Fase → Generar niveles**.
2. Pulsa Play en la escena `Menu` y prueba lo que cambiaste.
3. `python Tools/comprobar_niveles.py` (con `PYTHONIOENCODING=utf-8` en Windows) debe decir que
   todos los niveles se pueden terminar.
4. Para ver textos sin traducir: cambia el idioma a portugués o francés, juega un poco y mira
   `Logs/sin-traducir.txt`. Se añaden con `Tools/anadir_traducciones.py`.

### Publicar la versión web en itch
1. **Cierra todo** menos Unity (Discord, navegador, Unity Hub…): la exportación necesita mucha
   memoria. Si aun así falla por memoria, reinicia Unity antes de exportar.
2. **Fase → Exportar web** (tarda ~1 hora). El resultado queda en `Logs/web-build.txt`.
3. Comprime el *contenido* de `Builds/Web` (index.html y la carpeta Build) en `Builds/FASE-web.zip`.
4. Crea la release: `gh release create web-AAAAMMDD-1 Builds/FASE-web.zip --target fase --title "FASE web AAAA-MM-DD"`.
   GitHub la prueba en Chrome y, si va bien, la sube a itch sola (pestaña *Actions*).

### Cambiar el nivel de la semana
Edita `semana/nivel.txt` en GitHub (línea 1: «nombre | name», línea 2: autor, línea 3: código
`FASE-…`). Los jugadores mandan los suyos con la plantilla «Nivel de la comunidad». No hace falta
publicar una versión nueva: el juego lo descarga al abrir el panel.

### Récords en línea y analítica (Supabase)
- Las tablas se crean pegando `Tools/supabase.sql` en Supabase → SQL Editor → Run.
- La URL del proyecto y la clave pública (*anon*) van en `Assets/Scripts/Online.cs` (`Url` y `Key`).
  Con las dos vacías, todo lo en línea se apaga solo.
- Para ver dónde se atasca la gente: Supabase → SQL Editor →
  `select level, round(x) as x, count(*) from events where kind = 'death' group by 1, 2 order by 1, 3 desc;`
- Para borrar un récord falso: Supabase → Table Editor → `records` → borrar la fila.

### Twitch
No necesita nada: Extras → Twitch → nombre del canal → Conectar. El chat vota con `!izquierda` /
`!derecha` (o `<` / `>`).

### Lo que queda por hacer (ver también PENDIENTES.md)
- 84 · Cooperativo en línea (otro PC): necesita un servidor de juego en tiempo real; no se hizo.
- 92 y 142 · Tráiler nuevo, campaña y lanzamiento: no son programación.
- Final secreto y «dejar el mundo partido»: comprobados en código, falta verlos jugando.
- Probar en un móvil de verdad (táctil, rendimiento, giro de pantalla).

### Herramientas útiles
| Herramienta | Para qué |
|---|---|
| `Tools/niveles.py` | Escribe los niveles en texto (luego Fase → Generar niveles) |
| `Tools/comprobar_niveles.py` | Comprueba que todos los niveles se pueden terminar |
| `Tools/nivel_semana.py` | Crea un nivel de la semana de ejemplo |
| `Tools/anadir_traducciones.py` | Añade traducciones a portugués y francés |
| `Tools/fuente_simbolos.py` | Añade símbolos a la fuente FredokaFase |
| `Tools/prueba_web` | Abre la web exportada en Chrome y la prueba sola |
| Fase → Probar niveles jugando (bot) | Un jugador automático recorre los niveles |
