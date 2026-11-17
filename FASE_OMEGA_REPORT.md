# FASE · Informe de cierre (segunda etapa)

Fecha: 17 de noviembre de 2026 · Rama `fase` · Unity 6000.6.7f1 · Plataformas: Web, Windows, Android.
Documentos de detalle: [tablero](FASE_TASK_BOARD.md) · [requisitos](FASE_REQUIREMENTS_MATRIX.md) ·
[pruebas](FASE_TEST_MATRIX.md) · [decisiones](FASE_DECISIONS.md) · [problemas conocidos](FASE_KNOWN_ISSUES.md) ·
[continuar](CONTINUAR_DESARROLLO.md).

## Resumen ejecutivo

FASE está en estado **RELEASE CANDIDATE** para la GitHub Game Off: las tres versiones se exportan,
la web y Windows se han ejecutado sin errores y están publicadas en itch (página aún restringida).
No es **GOLD MASTER** porque el APK no se ha probado en un móvil real y la página de itch no es
pública. Los récords en línea ya están protegidos en el servidor (SQL aplicado el 17 nov).

## Cómo se trabajó

- Una sola instancia de Unity para escenas, pruebas en el editor, exportaciones y fotos.
- En paralelo, sin tocar Unity: una **revisión independiente de código** de todos los cambios desde
  el 16 de noviembre, una **revisión de seguridad** (secretos en todo el historial, Supabase con
  peticiones no destructivas, flujos de GitHub, entradas del cliente) y la **documentación de
  control** (matrices, decisiones, problemas conocidos, `SECURITY.md`, Dependabot).
- Cada arreglo se compiló fuera de Unity y se comprobó en el editor con capturas antes del commit.

## Errores corregidos en esta etapa

| Área | Corrección |
|---|---|
| Títulos | El nombre del nivel salía dos veces (cortinilla + título con línea de luz); un solo título por nivel |
| Cortinilla | El consejo se montaba sobre el nombre y el récord; ahora se apilan y se encogen |
| Interfaz | Aviso de logro con la estrella sobre el texto; contador de Sin fin desalineado; pista del jefe en una caja enorme; letrero de la flor cortado; pausa con el panel sobre el mapa, la pista visible detrás y el reloj pisando su texto; Opciones que no cabía; Controles pegado a «Volver» |
| Resultados | Farol y controles táctiles tapaban la tabla y los botones; «Compartir» tocaba «Código de fantasma»; Enter en la celebración saltaba los resultados; varias repeticiones a la vez |
| Juego | Cronómetro corriendo bajo la cortinilla; caídas contadas después de la meta; Lumi cayendo al vacío tras la meta; mutadores dando récord; contrarreloj contando cargas y empezando desde un punto de control |
| Guardado | Fantasma del récord que podía llenar el guardado de la web (1 MB) y bloquear la meta |
| Móvil | Botones táctiles agrandados que volvían a su tamaño; fotos que no se guardaban en Android |
| Gráficos | Opción nueva (Auto/Baja/Media/Alta/Máxima, pantalla completa, VSync, FPS); el ajuste automático ya no pisa la calidad elegida; la web deja el ritmo al navegador |
| Exportación | Windows salía en **pantalla negra** (recursos de SSAO quitados al compilar): cola que cambia de plataforma y recarga antes de compilar, sin bucles |
| Seguridad (cliente) | Códigos de nivel con tope (antes podían agotar la memoria); nombres en línea y del nivel semanal limpios; canal de Twitch validado; nombre filtrado igual que el servidor; analítica sin valores no válidos ni datos de las pruebas automáticas |
| CI | Permisos mínimos del token; token solo en los pasos que descargan |

## Rendimiento medido

Windows, nivel 5, 1920×1080, gráfica integrada AMD Vega 3, build final: **Auto 41–50 FPS · Baja 60 ·
Máxima 20–24 FPS** (escala 1,5 respetada), 0 excepciones, ~256 MB de memoria. Detalle y método en
[`FASE_TEST_MATRIX.md`](FASE_TEST_MATRIX.md).

## Estado por área

| Área | Estado |
|---|---|
| Supabase | Récords y analítica funcionando (probado con curl). Sin secretos expuestos. `Tools/supabase-endurecer.sql` **aplicado el 17 nov** y probado: permisos mínimos, tiempo mínimo por nivel, nombres y topes |
| Cooperativo en línea | No implementado (decisión documentada); cooperativo local sí |
| Twitch | IRC anónimo de solo lectura; probado en el editor; sin secretos en el cliente |
| Nivel de la semana / encuesta | Funcionan en el editor; la «encuesta» es un botón a la plantilla de GitHub |
| Windows | Exportado, ejecutado, sin excepciones, rendimiento medido; no jugado con teclado en la build |
| Android | APK generado y revisado (`com.oscar07.fase`, minSdk 26, arm64, firma de depuración); **sin probar en un móvil** |
| Web | Exportada y probada en Chrome (local y CI) sin errores |
| GitHub | Historial a nombre del autor; flujos endurecidos; `SECURITY.md` y Dependabot. Pendiente: rama `main` con un flujo viejo que sube a itch |
| itch.io | Tres canales con la build final (`web-20261117-2`, `build-20261117-2`); página **RESTRICTED**; queda un `GameOff2026.exe` viejo |

## Riesgos principales

1. APK sin probar en dispositivo real.
2. Tiempos falsos pero verosímiles (por encima del mínimo) siguen siendo posibles sin cuentas de jugador.
3. Rama `main` con `build.yml` que podría sobrescribir itch.
4. Proyecto de Supabase gratis que se pausa tras una semana sin uso.

## Próximos pasos (en orden)

1. Con permiso del autor: quitar la rama `main` o su `build.yml` (commits, versiones nuevas, SQL de
   Supabase y borrado de los 40 eventos de prueba ya hechos el 17 nov; la base queda a cero).
2. Probar el APK en un móvil (USB con depuración) y jugar la build de Windows con teclado.
3. Hacer pública la página de itch, cambiar el título a «FASE», borrar el `.exe` viejo y enviar a la
   jam antes del 1 de diciembre.
4. Tráiler y campaña cuando el autor lo pida (textos en `docs/lanzamiento/CAMPANA.md`).

## Resultados de la exportación final

Todas exportadas con los arreglos de las revisiones (`9a09faa`) y empaquetadas en `Builds/`.
La cola del editor sacó Windows y Android; el PC se reinició antes de la web, que se exportó
después con Unity en modo batch (`-buildTarget WebGL`, sin cambio de plataforma en caliente).

| Versión | Archivo | Tamaño | Verificación |
|---|---|---|---|
| Web | `Builds/FASE-web.zip` (`index.html` en la raíz) | 25,6 MB | Chrome local: carga 6,5 s, menú, nivel 1 y teclas sin errores de consola; un solo título por nivel |
| Windows | `Builds/FASE-windows.zip` (315 archivos, sin carpetas de depuración) | 51,6 MB | Dos ejecuciones de 32 s en el nivel 5: 0 excepciones; rendimiento arriba |
| Android | `Builds/FASE-android.apk` | 47,4 MB | `aapt2`: `com.oscar07.fase` 0.1.0 (1), target 36, arm64-v8a. `apksigner`: firma de depuración válida. Sin probar en un móvil |

**Publicadas el 17 nov 2026 (15:20 UTC)** con permiso del autor: releases `web-20261117-2` y
`build-20261117-2`. `butler status` con ✓ en `html5` (`web-20261117-2`), `win64` y `android`
(`build-20261117-2`); la prueba web de CI pasó antes de subir.
