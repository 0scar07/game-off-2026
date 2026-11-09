using System;
using UnityEngine;

namespace Fase
{
    /// <summary>Nombre del jugador (para "¡Hola, Oscar!" y las frases de Lumi). Vacío = sin nombre.</summary>
    public static class PlayerName
    {
        const string Key = "fase_nombre";
        public const int MaxLength = 16;

        public static string Value
        {
            get => PlayerPrefs.GetString(Key, "");
            set
            {
                string clean = (value ?? "").Trim();
                if (clean.Length > MaxLength) clean = clean.Substring(0, MaxLength);
                PlayerPrefs.SetString(Key, clean);
                PlayerPrefs.Save();
            }
        }

        public static bool Has => Value.Length > 0;
    }

    /// <summary>Racha de días seguidos terminando el desafío diario.</summary>
    public static class DailyStreak
    {
        const string LastKey = "fase_racha_ultimo", CountKey = "fase_racha";

        static int DayNumber(DateTime date) => (int)(date.Date - new DateTime(2026, 1, 1)).TotalDays;
        static int Today => DayNumber(DateTime.Now);

        /// <summary>Se llama al terminar el desafío de hoy.</summary>
        public static void Register()
        {
            int last = PlayerPrefs.GetInt(LastKey, int.MinValue);
            int count = PlayerPrefs.GetInt(CountKey, 0);
            if (last == Today) return;
            count = last == Today - 1 ? count + 1 : 1;
            PlayerPrefs.SetInt(LastKey, Today);
            PlayerPrefs.SetInt(CountKey, count);
            PlayerPrefs.Save();
        }

        /// <summary>Días seguidos que siguen vivos (si ayer o hoy se jugó); si no, 0.</summary>
        public static int Current
        {
            get
            {
                int last = PlayerPrefs.GetInt(LastKey, int.MinValue);
                return last >= Today - 1 ? PlayerPrefs.GetInt(CountKey, 0) : 0;
            }
        }

        public static bool DoneToday => PlayerPrefs.GetInt(LastKey, int.MinValue) == Today;
    }
}
