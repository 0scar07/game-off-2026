using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Desafíos con condiciones (Extras → Retos): eliges una condición y vale para el siguiente
    /// nivel que juegues. Si la rompes, el nivel vuelve a empezar; si llegas a la meta, queda
    /// apuntado (✦) para ese nivel.
    /// </summary>
    public static class Conditions
    {
        public static readonly (string es, string en)[] Names =
        {
            ("Sin mover la línea a la izquierda", "Never move the line left"),
            ("Solo 5 agarres de la línea", "Only 5 line grabs"),
            ("Sin caer ni una vez", "Without a single fall"),
        };

        /// <summary>Condición elegida para el próximo nivel (-1 = ninguna).</summary>
        public static int Armed = -1;

        public static bool Done(int condition, int level) => PlayerPrefs.GetInt($"fase_condicion_{condition}_{level}", 0) == 1;

        public static int Count(int condition)
        {
            int n = 0;
            for (int i = 1; i <= LevelCatalog.Get.levels.Length; i++) if (Done(condition, i)) n++;
            return n;
        }

        public static void MarkDone(int condition, int level)
        {
            PlayerPrefs.SetInt($"fase_condicion_{condition}_{level}", 1);
            PlayerPrefs.Save();
        }
    }
}
