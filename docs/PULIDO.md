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

---

## 1 · Pantallas y paneles

| Qué | Estado | Notas |
|---|---|---|
| Inicio (logo partido por la línea) | ✅ | Carga sin errores |
| Intro (historia con voz) | ✅ | Carga sin errores · ⬜ escuchar la voz entera |
| Menú principal y su barra | ✅ | Barra con iconos grandes (tanda G) |
| El viaje (mapa) | ✅ | Lumi camina de nivel en nivel |
| Opciones | 🔧 | Fondo y botones ya no salen ovalados |
| Pausa y mapa de pausa | ⬜ | Mapa oculto en Taller y Sin fin |
| Resultados | ⬜ | |
| Final y final secreto | ⬜ | |
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
| 3 | Bot de pruebas | No salta los pinchos del nivel 1 | Herramienta | ❌ |
| 4 | Menús (11 paneles) | Textos pisándose, filas bajo "Volver", iconos sobre el texto | Molesta | 🔧 |
| 5 | Opciones y botones del menú | Se veían ovalados (sprite sin borde de 9 cortes) | Molesta | 🔧 |
| 6 | Portugués y francés | ~400 textos salían en inglés | Molesta | 🔧 |
| 7 | Plumas del capítulo III | "¡Una copo!" | Molesta | 🔧 |
| 8 | Tienda | Los nombres no cambiaban de idioma | Molesta | 🔧 |
| 9 | Pantallas 4:3 y móvil | Se cortaban los lados de la interfaz | Molesta | 🔧 |

---

## Para seguir sin Claude

*(Se completa antes del día 8.)*
