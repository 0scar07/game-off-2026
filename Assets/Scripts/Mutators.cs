using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Mutadores (Extras → Retos, tras terminar el juego): cambian las reglas y se combinan.
    /// Gravedad baja, línea invertida (día y noche cambiados), Lumi gigante y a oscuras.
    /// No cuentan para medallas.
    /// </summary>
    public static class Mutators
    {
        public static readonly (string id, string es, string en)[] All =
        {
            ("gravedad", "Gravedad baja", "Low gravity"),
            ("invertida", "Línea invertida", "Inverted line"),
            ("gigante", "Lumi gigante", "Giant Lumi"),
            ("oscuras", "A oscuras", "In the dark"),
        };

        public static bool On(string id) => PlayerPrefs.GetInt("fase_mutador_" + id, 0) == 1;
        public static void Set(string id, bool value) { PlayerPrefs.SetInt("fase_mutador_" + id, value ? 1 : 0); PlayerPrefs.Save(); }
        public static bool Any { get { foreach (var m in All) if (On(m.id)) return true; return false; } }
        public static bool Unlocked => MirrorMode.Unlocked;
    }
}
