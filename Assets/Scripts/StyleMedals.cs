using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Medallas de estilo: premios por jugar un nivel de una forma concreta. Las cuenta
    /// StyleTracker mientras se juega y se dan al llegar a la meta.
    /// </summary>
    public static class StyleMedals
    {
        public class Info
        {
            public string id, nameEs, nameEn, howEs, howEn;
            public string Name => Loc.Pick(nameEs, nameEn);
            public string How => Loc.Pick(howEs, howEn);
        }

        public static readonly Info[] All =
        {
            new Info { id = "sinnoche", nameEs = "Sin pisar la noche", nameEn = "Never by night", howEs = "Termina un nivel sin pasar al lado de noche", howEn = "Finish a level without stepping into the night" },
            new Info { id = "funambulista", nameEs = "Funambulista", nameEn = "Tightrope", howEs = "Cruza la línea 50 veces en un nivel", howEn = "Cross the line 50 times in one level" },
            new Info { id = "rasante", nameEs = "Vuelo rasante", nameEn = "Long flight", howEs = "Recorre 10 casillas en un solo salto", howEn = "Cover 10 tiles in a single jump" },
            new Info { id = "pies", nameEs = "Pies ligeros", nameEn = "Light feet", howEs = "Termina un nivel sin aterrizar fuerte", howEn = "Finish a level without a hard landing" },
        };

        public static bool Has(string id) => PlayerPrefs.GetInt("fase_estilo_" + id, 0) == 1;

        public static int Count { get { int n = 0; foreach (var m in All) if (Has(m.id)) n++; return n; } }

        public static void Give(string id, Transform near)
        {
            if (Has(id)) return;
            PlayerPrefs.SetInt("fase_estilo_" + id, 1);
            PlayerPrefs.Save();
            foreach (var m in All)
                if (m.id == id && near != null) LumiBubble.Say(near, "✦ " + m.Name, 2.5f);
            Achievements.Unlock("estiloso");
        }
    }
}
