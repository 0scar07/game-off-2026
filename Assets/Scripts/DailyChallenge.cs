using System;

namespace Fase
{
    /// <summary>
    /// Desafío diario: cada día toca uno de los niveles cortos de Assets/Levels/Diarios (los
    /// genera Tools/niveles.py), el mismo para todo el mundo ese día. No guarda medallas ni
    /// desbloquea nada; al terminar vuelve al menú.
    /// </summary>
    public static class DailyChallenge
    {
        public const int Count = 14;

        /// <summary>Número del desafío de hoy (1..Count), según la fecha UTC.</summary>
        public static int Today
        {
            get
            {
                int days = (int)(DateTime.UtcNow.Date - new DateTime(2026, 11, 1)).TotalDays;
                return ((days % Count) + Count) % Count + 1;
            }
        }

        public static string SceneName => $"diario-{Today:00}";
    }
}
