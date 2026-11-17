# FASE — Plan maestro de cierre

Plan vivo para dejar FASE listo en Web, Windows y Android. Se actualiza mientras se trabaja.
Detalle diario en [`docs/PULIDO.md`](docs/PULIDO.md); ideas numeradas en [`docs/PENDIENTES.md`](docs/PENDIENTES.md).

## Reconocimiento (16 de noviembre de 2026)

| Cosa | Estado |
|---|---|
| Proyecto | `C:\Users\osky7\Downloads\FASE`, rama `fase`, repo público `0scar07/game-off-2026` |
| Unity | 6000.6.7f1 con módulos **WebGL**, **Windows** y **Android** (SDK, NDK y OpenJDK incluidos) |
| Herramientas | git 2.51, gh 2.98, Node 22, Python 3.11. **Butler no está instalado**: la subida a itch la hace GitHub Actions |
| Exportación | `Assets/Editor/WebBuild.cs`: menús Fase → Exportar web / escritorio (Windows) / Android (APK) |
| Publicación | Release `web-AAAAMMDD-N` → GitHub prueba la web en Chrome → butler la sube al canal `html5` de itch |
| Versión | `bundleVersion` 0.1.0, `AndroidBundleVersionCode` 1 |
| Android | Paquete `com.oscar07.fase`, IL2CPP ARM64, horizontal. **Sin keystore propio**: firma de depuración |
| Calidad gráfica | Perfiles `Mobile` (web y móvil) y `PC` (ordenador, automático). **Sin opción dentro del juego** |
| En línea | Supabase (proyecto «fase») con récords y analítica, encendido y probado hoy |
| Niveles | 54 de 54 se pueden terminar (`Tools/comprobar_niveles.py`) |

## Prioridades y estado

Leyenda: ✅ hecho y comprobado · 🔧 hecho sin comprobar · ⏳ en curso · ⬜ pendiente · ⛔ bloqueado

### Crítico
| # | Tarea | Estado |
|---|---|---|
| 1 | El nombre del nivel salía dos veces al empezar | ✅ visto en Unity |
| 1b | En la cortinilla, el consejo se montaba encima del nombre y del récord | ✅ visto a 16:9, 4:3 y 21:9 |
| 2 | Al salir de un nivel, la analítica creaba un objeto mientras se cerraba la escena (error en consola) | ✅ |
| 3 | Fin de nivel, medallas, guardado y desbloqueo: 12 fallos de la revisión de código | ✅ arreglados; meta, resultados y cronómetro vistos en Unity |
| 4 | Pantalla de resultados alineada en 4 idiomas y 3 proporciones | ✅ (día 2, capturas) |

### Alto
| # | Tarea | Estado |
|---|---|---|
| 5 | Récords en línea (80) y analítica (140) | ✅ tablas y reglas probadas; 🔧 falta verlo dentro del juego |
| 6 | Nivel de la semana (43) y encuesta (62) | ✅ |
| 7 | Twitch (120): el chat vota con IRC anónimo, sin OAuth ni secretos | ✅ en el editor; 🔧 en la web |
| 8 | Cooperativo en línea (84) | ⛔ necesita un servidor de juego en tiempo real (Supabase no lo sustituye). Hay cooperativo local |
| 9 | Opción de calidad gráfica (Auto / Baja / Media / Alta / Máxima, pantalla completa, VSync, FPS) | ✅ panel visto en Unity; 🔧 el efecto real se ve en la build |

### Medio
| # | Tarea | Estado |
|---|---|---|
| 10 | Arreglos que encuentre la auditoría de código | ✅ |
| 11 | Documentación: BUILDING, ARCHITECTURE, ROADMAP, CHANGELOG, CONTINUAR_DESARROLLO | ✅ |

### Distribución
| # | Tarea | Estado |
|---|---|---|
| 12 | Build Web nueva con todo lo de hoy, probada en Chrome con servidor HTTP | ✅ en itch (canal html5, web-20261117-1) |
| 13 | Build Windows (`Builds/Windows/FASE.exe`), probada abriendo el .exe | ✅ en itch (canal win64, 52 MB) |
| 14 | Build Android (`Builds/Android/FASE.apk`) | ✅ en itch (canal android, 47 MB); 🔧 sin probar en un móvil; firma de depuración |
| 15 | Subir a itch las tres | ✅ 17 nov. Falta: hacer pública la página y borrar el GameOff2026.exe viejo (canal windows) |

### Final
| # | Tarea | Estado |
|---|---|---|
| 16 | Tráiler, campaña y lanzamiento (92, 142) | ⬜ lo último, antes de enviar el juego |

## Reglas de trabajo
- Una sola instancia de Unity abierta a la vez (nunca dos editores sobre el mismo proyecto).
- Nada se marca ✅ sin haberlo visto funcionar.
- Commits locales por bloque; **push y publicación en itch solo con permiso**.
- Nunca se suben los scripts de prueba locales de `Assets/Editor` (no versionados).
