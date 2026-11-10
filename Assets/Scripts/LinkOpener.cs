using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Enlaces a niveles del Taller: si el juego se abre con "#nivel=FASE-..." (o "?nivel=") en la
    /// dirección, va directo al Taller con ese nivel cargado. Funciona donde el juego tiene su
    /// propia dirección (GitHub Pages); dentro del marco de itch, la dirección no llega al juego,
    /// así que allí se sigue usando el código pegado a mano.
    /// </summary>
    public static class LinkOpener
    {
        public static string PendingCode { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Read()
        {
            string url = Application.absoluteURL ?? "";
            int at = url.IndexOf("nivel=");
            if (at < 0) return;
            string code = url.Substring(at + 6);
            int end = code.IndexOfAny(new[] { '&', '#' });
            if (end >= 0) code = code.Substring(0, end);
            code = System.Uri.UnescapeDataString(code);
            if (LevelCode.Decode(code) != null) PendingCode = code;
        }

        /// <summary>Enlace para compartir un nivel del Taller.</summary>
        public static string Link(string code) => $"{Share.Url}#nivel={System.Uri.EscapeDataString(code)}";
    }
}
