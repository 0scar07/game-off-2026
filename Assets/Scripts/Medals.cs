using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Medallas de tiempo: Bronce, Plata, Oro y Luna (secreta, muy difícil). Los tiempos de cada
    /// nivel los pone el generador (cabecera "medallas:" del nivel o calculados por su largo).
    /// La mejor medalla de cada nivel se guarda en Progress.
    /// </summary>
    public static class Medals
    {
        public const int None = 0, Bronze = 1, Silver = 2, Gold = 3, Moon = 4;

        public static readonly Color[] Colors =
        {
            new Color(0.25f, 0.24f, 0.4f),
            new Color(0.82f, 0.54f, 0.32f),
            new Color(0.85f, 0.88f, 0.93f),
            new Color(1f, 0.83f, 0.35f),
            new Color(0.72f, 0.83f, 1f),
        };

        static readonly string[] NamesEs = { "", "Bronce", "Plata", "Oro", "Luna" };
        static readonly string[] NamesEn = { "", "Bronze", "Silver", "Gold", "Moon" };

        public static string Name(int medal) => Loc.Pick(NamesEs[medal], NamesEn[medal]);

        /// <summary>times = {bronce, plata, oro, luna} en segundos (cada uno más exigente).</summary>
        public static int For(float seconds, float[] times)
        {
            if (times == null || times.Length < 4 || seconds <= 0f) return None;
            for (int medal = Moon; medal >= Bronze; medal--)
                if (seconds <= times[medal - 1]) return medal;
            return None;
        }

        public static int Best(int level) => PlayerPrefs.GetInt("fase_medalla_" + level, None);

        /// <summary>Guarda la medalla si es mejor que la anterior. Devuelve true si mejoró.</summary>
        public static bool Save(int level, int medal)
        {
            if (medal <= Best(level)) return false;
            PlayerPrefs.SetInt("fase_medalla_" + level, medal);
            PlayerPrefs.Save();
            return true;
        }

        public static int CountAtLeast(int medal, int levels)
        {
            int count = 0;
            for (int level = 1; level <= levels; level++) if (Best(level) >= medal) count++;
            return count;
        }
    }
}
