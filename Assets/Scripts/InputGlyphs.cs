using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// Nombres de los botones según el mando conectado (Xbox o PlayStation), para que las pistas y
    /// el tutorial digan "LB/RB" o "L1/R1" en vez de las teclas cuando se juega con mando.
    /// </summary>
    public static class InputGlyphs
    {
        public enum Pad { None, Xbox, PlayStation }

        public static Pad Current
        {
            get
            {
                var pad = Gamepad.current;
                if (pad == null) return Pad.None;
                string layout = pad.layout + " " + pad.displayName + " " + pad.description.product;
                bool sony = layout.Contains("DualShock") || layout.Contains("DualSense") || layout.Contains("PlayStation") || layout.Contains("Wireless Controller");
                return sony ? Pad.PlayStation : Pad.Xbox;
            }
        }

        public static string Jump => Current == Pad.PlayStation ? "✕" : "A";
        public static string LineButtons => Current == Pad.PlayStation ? "L1/R1" : "LB/RB";
        public static string Lantern => Current == Pad.PlayStation ? "△" : "Y";

        /// <summary>Cambia las teclas que nombran las pistas por los botones del mando, si hay uno.</summary>
        public static string Adapt(string text)
        {
            if (string.IsNullOrEmpty(text) || Current == Pad.None) return text;
            return text.Replace("Shift (o Q/E)", LineButtons).Replace("Shift (or Q/E)", LineButtons)
                       .Replace("Shift (ou Q/E)", LineButtons).Replace("Q/E", LineButtons).Replace("(L)", $"({Lantern})");
        }
    }
}
