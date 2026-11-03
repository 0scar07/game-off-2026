using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Estadísticas de todo el juego (saltos, distancia, veces que se movió la línea, caídas) y
    /// desafíos de cada nivel: sin caer, toda la luna y mover la línea pocas veces.
    /// </summary>
    public static class Stats
    {
        public const int ChallengeNoFall = 1, ChallengeAllMoon = 2, ChallengeFewGrabs = 4;
        /// <summary>Máximo de agarres de la línea para el desafío "pocas veces".</summary>
        public const int GrabPar = 8;

        public static int Jumps => PlayerPrefs.GetInt("fase_cuenta_saltos", 0);
        public static int LineMoves => PlayerPrefs.GetInt("fase_cuenta_linea", 0);
        public static int Falls => PlayerPrefs.GetInt("fase_cuenta_caidas", 0);
        public static float Meters => PlayerPrefs.GetFloat("fase_cuenta_metros", 0f);

        static float pendingMeters;

        public static void AddLineMove() => PlayerPrefs.SetInt("fase_cuenta_linea", LineMoves + 1);
        public static void AddFall() => PlayerPrefs.SetInt("fase_cuenta_caidas", Falls + 1);

        /// <summary>Se acumula en memoria y se guarda de vez en cuando (no en cada paso).</summary>
        public static void AddMeters(float meters)
        {
            pendingMeters += meters;
            if (pendingMeters < 10f) return;
            PlayerPrefs.SetFloat("fase_cuenta_metros", Meters + pendingMeters);
            pendingMeters = 0f;
        }

        public static int Challenges(int level) => PlayerPrefs.GetInt("fase_retos_" + level, 0);

        public static void SaveChallenges(int level, int mask)
        {
            PlayerPrefs.SetInt("fase_retos_" + level, Challenges(level) | mask);
            PlayerPrefs.Save();
        }

        public static string Summary() => Loc.English
            ? $"Total: {Jumps:N0} jumps · {Meters / 1000f:0.0} km · line moved {LineMoves:N0} times"
            : $"Total: {Jumps:N0} saltos · {Meters / 1000f:0.0} km · línea movida {LineMoves:N0} veces";
    }
}
