using System.Runtime.InteropServices;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Compartir un tiempo ("Hice el nivel 4 en 0:44 🌗") y vibrar el móvil. En la web usa
    /// Plugins/WebGL/Share.jslib; en el editor solo copia al portapapeles.
    /// </summary>
    public static class Share
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern int FaseShare(string text);
        [DllImport("__Internal")] static extern void FaseVibrate(int ms);
        [DllImport("__Internal")] static extern void FaseDownload(string name, string base64);
#endif

        public const string Url = "https://0scar07.itch.io/game-off-2026";

        /// <summary>Devuelve la clave del texto a mostrar: "shared", "copied" o "sharefail".</summary>
        public static string Text(string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int result = FaseShare(text);
            return result == 1 ? "shared" : result == 2 ? "copied" : "sharefail";
#else
            GUIUtility.systemCopyBuffer = text;
            return "copied";
#endif
        }

        public static string LevelTime(int level, string name, float seconds, int medal)
        {
            string medalText = medal > 0 ? $" · {Medals.Name(medal)}" : "";
            return Loc.Pick(
                $"Hice el nivel {level} \"{name}\" de FASE en {Hud.FormatTime(seconds)}{medalText} 🌗 ¿Me superas? {Url}",
                $"I beat level {level} \"{name}\" of FASE in {Hud.FormatTime(seconds)}{medalText} 🌗 Can you beat it? {Url}",
                $"Fiz o nível {level} \"{name}\" de FASE em {Hud.FormatTime(seconds)}{medalText} 🌗 Consegue me superar? {Url}",
                $"J'ai fait le niveau {level} \"{name}\" de FASE en {Hud.FormatTime(seconds)}{medalText} 🌗 Tu me bats ? {Url}");
        }

        /// <summary>Guarda una captura: en la web la descarga; en el editor, en la carpeta del proyecto.</summary>
        public static void SavePicture(Texture2D picture)
        {
            byte[] png = picture.EncodeToPNG();
            string name = $"FASE-{System.DateTime.Now:yyyyMMdd-HHmmss}.png";
#if UNITY_WEBGL && !UNITY_EDITOR
            FaseDownload(name, System.Convert.ToBase64String(png));
#else
            System.IO.File.WriteAllBytes(name, png);
#endif
        }

        /// <summary>Vibración corta (si el jugador la tiene activada y el móvil la admite).</summary>
        public static void Vibrate(int ms)
        {
            if (!Settings.Vibration) return;
            Rumble.Pulse(ms / 1000f);
#if UNITY_WEBGL && !UNITY_EDITOR
            FaseVibrate(ms);
#elif UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
