# Cómo compilar FASE

Guía para generar las tres versiones del juego (web, Windows y Android) y publicar la web en itch.io.
Todo sale del script [`Assets/Editor/WebBuild.cs`](Assets/Editor/WebBuild.cs), que añade tres entradas al menú **Fase** de Unity.

Ver también: [ARCHITECTURE.md](ARCHITECTURE.md) · [CONTINUAR_DESARROLLO.md](CONTINUAR_DESARROLLO.md) · [docs/PULIDO.md](docs/PULIDO.md) (sección «Para seguir sin Claude»).

## Requisitos

| Qué | Detalle |
|---|---|
| Unity | **6000.6.7f1** exacta (`ProjectSettings/ProjectVersion.txt`), con URP 17.6.0 |
| Módulos de Unity | **WebGL**, **Windows** (*Windows Build Support*) y **Android** (se marcan en Unity Hub → Instalaciones → Añadir módulos) |
| Android | El SDK, el NDK y OpenJDK vienen con el módulo *Android Build Support*; no hay que instalarlos aparte |
| Escenas | Salen de **Fase → Generar niveles** (ver más abajo); si no, la lista de escenas de compilación queda desactualizada |
| Para la prueba web | Node.js 22 y Google Chrome (opcional en local; GitHub lo hace solo) |
| Para subir a itch | La `gh` CLI (para crear la release). `butler` **no** está instalado en este PC: la subida la hace GitHub Actions |

## Antes de compilar

1. Abre el proyecto en Unity 6000.6.7f1.
2. Si tocaste niveles (`Assets/Levels/*.txt`) o el audio: **Fase → Generar niveles**. Esto crea las escenas de `Assets/Scenes/Niveles`, el menú, el final y deja la lista de escenas de compilación en orden (inicio, menú, intro, niveles, final, Taller, Sin fin, desafíos diarios).
3. Opcional: `python Tools/comprobar_niveles.py` (con `PYTHONIOENCODING=utf-8` en Windows) debe decir que todos los niveles se pueden terminar.

Los tres menús compilan las escenas **activadas** en *Build Settings*.

## Consejos de memoria

- **Cierra todo** menos Unity (navegador, Discord, otros editores). La exportación web necesita mucha RAM y falla con el PC cargado.
- Si falla por memoria o por paginación: reinicia Unity y vuelve a exportar con el PC libre.
- Lo que ya se hizo para gastar menos: IL2CPP con `OptimizeSize` (menos código C++ generado) y `stripEngineCode`. Con 10 GB libres se quedaba sin memoria antes del cambio.
- La web tardó unos 63 minutos (10 de noviembre, con el PC libre); cuenta con **cerca de una hora**.

## Web (WebGL)

**Fase → Exportar web** → carpeta `Builds/Web`. El resultado (tamaño y minutos, o el error) queda escrito en `Logs/web-build.txt`.

Ajustes que fija el script cada vez:

- Nombre `FASE`, compañía `0scar07`.
- Compresión **Brotli** con *decompression fallback* (funciona en itch y en cualquier servidor sin configurar nada) y caché de datos activada.
- Pantalla por defecto 960×540, `runInBackground` apagado.
- Plantilla propia [`Assets/WebGLTemplates/Fase`](Assets/WebGLTemplates/Fase/index.html): pantalla completa, barra de carga, cursor propio y aviso de «Gira el móvil» cuando se abre en vertical en un dispositivo táctil.

### Exportar sin tocar menús (aviso `auto-build.flag`)

Si existe el archivo `Logs/auto-build.flag` **al abrir el editor**, el propio script lo borra y, una sola vez, ejecuta *Generar niveles* y después *Exportar web*. Sirve para lanzar todo desde fuera:

```
echo. > Logs/auto-build.flag      (luego abrir Unity)
```

Solo afecta a la web. Windows y Android se lanzan siempre desde el menú.

### Probar la web en local

```
cd Builds/Web
python -m http.server 8000
```

y abrir `http://localhost:8000/`. Para la prueba automática (Chrome sin ventana: espera a que cargue, pasa el inicio, entra al nivel 1, pulsa teclas y comprueba que no hay errores y que la imagen se mueve):

```
cd Tools/prueba_web
npm install
npx playwright install chrome
CHROME=chrome node prueba.mjs http://localhost:8000/ capturas
```

En PowerShell, la variable se pone antes: `$env:CHROME = "chrome"`. Las capturas quedan en la carpeta indicada (`capturas`). El resultado de referencia del 10 de noviembre: carga en 10,5 s y sin errores.

## Windows

**Fase → Exportar escritorio (Windows)** → `Builds/Windows/FASE.exe` (más sus carpetas de datos: se reparte la carpeta entera). Ventana sin bordes a pantalla completa, 1920×1080 por defecto. Resultado en `Logs/windows-build.txt`.

## Android

**Fase → Exportar Android (APK)** → `Builds/Android/FASE.apk`. Resultado en `Logs/android-build.txt`.

El script fija: identificador `com.oscar07.fase`, IL2CPP, solo **ARM64**, orientación horizontal (los dos lados), APK (no *App Bundle*). La versión mínima de Android es la 26 y la versión del paquete 0.1.0 (código 1) según `ProjectSettings/ProjectSettings.asset`.

### Avisos honestos sobre Android

- **El APK se firma con la clave de depuración** porque `AndroidKeystoreName` está vacío en `ProjectSettings`. Sirve para instalarlo a mano, pero **no para Google Play**.
- Una actualización solo se instala encima de la anterior si está firmada con **la misma clave**. La clave de depuración de Unity es propia de cada PC/usuario: si compilas en otro equipo, habrá que desinstalar antes.
- Para publicar de verdad hay que crear un keystore propio (Project Settings → Player → Android → Publishing Settings), guardarlo **fuera del repositorio** y no perder la contraseña.
- Los botones táctiles, el giro de pantalla y el rendimiento en un móvil real siguen **sin probar** ([docs/PULIDO.md](docs/PULIDO.md)).

## Publicar la web en itch.io

La exportación se hace en el PC; la prueba y la subida las hace GitHub ([`.github/workflows/itch-web.yml`](.github/workflows/itch-web.yml)).

1. **Fase → Exportar web** (ver arriba) y comprobar `Logs/web-build.txt`.
2. Comprimir el *contenido* de `Builds/Web` (`index.html` y la carpeta `Build` en la raíz del zip, rutas con `/`) en `Builds/FASE-web.zip`. Por ejemplo, desde la raíz del proyecto:
   ```
   python -c "import shutil; shutil.make_archive('Builds/FASE-web', 'zip', 'Builds/Web')"
   ```
3. Crear la release con una etiqueta que empiece por `web-`:
   ```
   gh release create web-AAAAMMDD-N Builds/FASE-web.zip --target fase --title "FASE web AAAA-MM-DD"
   ```
   (`N` es el número de subida del día; la última fue `web-20261110-1`.)
4. GitHub Actions hace dos trabajos seguidos:
   - **probar**: descarga el zip, lo sirve con `python3 -m http.server 8000` y ejecuta `Tools/prueba_web/prueba.mjs` con Google Chrome. Las capturas quedan como artefacto del run (`capturas-<etiqueta>`).
   - **upload** (solo si la prueba pasa): descarga el zip, comprueba que hay `index.html` y sube con **butler** al canal `html5` (`butler push web USUARIO/JUEGO:html5 --userversion <etiqueta>`).
5. Ver el progreso en la pestaña *Actions*. También se puede lanzar a mano con *Run workflow* indicando la etiqueta (`workflow_dispatch`).

Solo las etiquetas `web-…` tocan itch: las versiones del juego (`v1.0`…) no.

## Publicar Windows y Android en itch.io

Lo hace [`.github/workflows/itch-builds.yml`](.github/workflows/itch-builds.yml) con las etiquetas `build-…`.

1. Exportar las dos versiones (ver arriba) y preparar los archivos desde la raíz del proyecto:
   ```
   python -c "import shutil; shutil.make_archive('Builds/FASE-windows', 'zip', 'Builds/Windows')"
   copy Builds\Android\FASE.apk Builds\FASE-android.apk
   ```
   (En el zip de Windows no hace falta la carpeta `FASE_BurstDebugInformation_DoNotShip`.)
2. Crear la release:
   ```
   gh release create build-AAAAMMDD-N Builds/FASE-windows.zip Builds/FASE-android.apk --target fase --title "FASE Windows y Android AAAA-MM-DD"
   ```
3. GitHub sube la carpeta de Windows al canal `win64` y el APK al canal `android` con butler. Son
   los mismos canales que usaba la versión anterior del juego, así que sustituyen a esos archivos.
   Si en la página de itch queda algún archivo viejo en otro canal, se borra a mano en *Edit game →
   Uploads*.

### Configuración que necesita el repositorio

| Dónde | Nombre | Qué es |
|---|---|---|
| Secreto | `BUTLER_API_KEY` | Clave de API de itch.io |
| Variable | `ITCH_USER` | Usuario de itch.io |
| Variable | `ITCH_GAME` | Parte final de la URL del juego |

Nunca pongas la clave en un archivo del proyecto: va solo en *Settings → Secrets and variables → Actions* de GitHub.

## Si algo falla

| Síntoma | Qué mirar |
|---|---|
| La exportación web se queda sin memoria | Cerrar todo, reiniciar Unity, reintentar (ver «Consejos de memoria») |
| El menú **Fase** no sale | Esperar a que Unity termine de compilar; si hay errores de código, no aparece |
| Faltan escenas en la compilación | Hacer **Fase → Generar niveles** |
| La prueba de GitHub falla | Descargar las capturas del run y mirar `Logs/web-build.txt` del último export |
| Letras como cuadros vacíos en la web | La fuente *Fredoka Fase* (`Tools/fuente_simbolos.py`) debe tener el símbolo |
