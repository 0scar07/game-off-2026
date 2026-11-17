# Política de seguridad

FASE es un videojuego gratuito de código abierto (Unity, versión 0.1.0) que se juega en navegador, Windows y Android.

## Cómo reportar un problema de seguridad

- Usa el **aviso de seguridad privado de GitHub** de este repositorio (pestaña *Security* → *Report a vulnerability*), o
- escribe al autor, [0scar07](https://github.com/0scar07), por GitHub.

Por favor, no abras una issue pública con los detalles de una vulnerabilidad hasta que se haya podido arreglar. Para fallos normales del juego (no de seguridad) usa la plantilla *Fallo* de las issues.

Incluye, si puedes: qué has encontrado, cómo reproducirlo y en qué plataforma (web, Windows o Android). Es un proyecto personal: no hay plazos de respuesta garantizados.

## Qué está en alcance

- El juego (código de `Assets/`, builds web, Windows y Android).
- La configuración de Supabase que usa el juego (`Tools/supabase.sql`: tablas, reglas RLS y funciones).
- Los workflows de `.github/workflows` (subida a itch, comprobación de niveles).

## Qué no es un problema de seguridad

- **La clave `anon` de Supabase es pública por diseño.** Está en el código del cliente a propósito; lo que protege los datos son las reglas de seguridad por filas (RLS) de la base de datos. Sí interesa que se reporte si con esa clave se puede leer, modificar o borrar algo que no debería.
- Los récords en línea no están protegidos contra tiempos falsos dentro de los límites permitidos (3 s a 1 h); es una limitación conocida (ver `FASE_KNOWN_ISSUES.md`).
- El APK está firmado con la clave de depuración mientras no haya un keystore propio.

## Datos y privacidad

- No hay servicio de pago ni cuentas de usuario.
- No se guardan datos personales, salvo el **nombre corto opcional** (hasta 12 caracteres) que el jugador escribe para aparecer en los récords en línea.
- La analítica es anónima (nivel, posición y tiempo; sin identificadores) y se puede apagar en Opciones → Controles.
- La integración con Twitch es de solo lectura, como invitado anónimo: no usa cuenta, OAuth ni secretos.

## Versiones con soporte

Solo se mantiene la última versión publicada en [itch.io](https://0scar07.itch.io/game-off-2026) y la rama `fase` del repositorio.
