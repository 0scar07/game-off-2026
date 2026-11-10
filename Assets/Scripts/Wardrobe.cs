using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Bufandas y faroles de Lumi. Cada uno se desbloquea con algo del juego (fragmentos,
    /// capítulos, medallas, logros) y se elige en el menú (pantalla "Lumi").
    /// </summary>
    public static class Wardrobe
    {
        public class Item
        {
            public string id, nameEs, nameEn, howEs, howEn;
            public Color color, color2;
            public System.Func<bool> unlocked;
            public string Name => Loc.Pick(nameEs, nameEn);
            public string How => Loc.Pick(howEs, howEn);
            public bool Unlocked => unlocked == null || unlocked();
        }

        static int TotalFragments()
        {
            int total = 0;
            for (int level = 1; level <= LevelCatalog.Get.levels.Length; level++) total += Progress.Fragments(level);
            return total;
        }

        static int Levels => LevelCatalog.Get.levels.Length;

        public static readonly Item[] Scarves =
        {
            new Item { id = "naranja", nameEs = "Atardecer", nameEn = "Sunset", howEs = "La de siempre", howEn = "The usual one", color = new Color(1f, 0.45f, 0.18f), color2 = new Color(0.95f, 0.3f, 0.12f) },
            new Item { id = "noche", nameEs = "Noche azul", nameEn = "Blue night", howEs = "5 fragmentos de luna", howEn = "5 moon fragments", color = new Color(0.3f, 0.45f, 1f), color2 = new Color(0.55f, 0.3f, 0.95f), unlocked = () => TotalFragments() >= 5 },
            new Item { id = "pradera", nameEs = "Flor de pradera", nameEn = "Meadow flower", howEs = "Termina el capítulo I", howEn = "Finish chapter I", color = new Color(1f, 0.48f, 0.66f), color2 = new Color(1f, 0.82f, 0.35f), unlocked = () => LevelCatalog.Get.ChapterDone(1) },
            new Item { id = "bosque", nameEs = "Hoja de otoño", nameEn = "Autumn leaf", howEs = "Termina el capítulo II", howEn = "Finish chapter II", color = new Color(0.95f, 0.55f, 0.15f), color2 = new Color(0.55f, 0.75f, 0.25f), unlocked = () => LevelCatalog.Get.ChapterDone(2) },
            new Item { id = "cumbre", nameEs = "Nieve", nameEn = "Snow", howEs = "Termina el capítulo III", howEn = "Finish chapter III", color = new Color(0.92f, 0.97f, 1f), color2 = new Color(0.55f, 0.85f, 1f), unlocked = () => LevelCatalog.Get.ChapterDone(3) },
            new Item { id = "oro", nameEs = "Dorada", nameEn = "Golden", howEs = "5 medallas de oro", howEn = "5 gold medals", color = new Color(1f, 0.84f, 0.3f), color2 = new Color(1f, 0.65f, 0.15f), unlocked = () => Medals.CountAtLeast(Medals.Gold, Levels) >= 5 },
            new Item { id = "arcoiris", nameEs = "Arcoíris", nameEn = "Rainbow", howEs = "Todos los fragmentos", howEn = "Every fragment", color = new Color(1f, 0.35f, 0.4f), color2 = new Color(0.35f, 0.6f, 1f), unlocked = () => Achievements.Has("coleccionista") },
            new Item { id = "rayas", nameEs = "Rayas", nameEn = "Stripes", howEs = "Completa los retos de una semana", howEn = "Complete a week's challenges", color = new Color(0.95f, 0.95f, 1f), color2 = new Color(0.9f, 0.3f, 0.4f), unlocked = () => WeeklyChallenges.EverCompleted },
            new Item { id = "luna", nameEs = "Luz de luna",
 nameEn = "Moonlight", howEs = "Una medalla de Luna", howEn = "A Moon medal", color = new Color(0.75f, 0.88f, 1f), color2 = new Color(1f, 1f, 1f), unlocked = () => Medals.CountAtLeast(Medals.Moon, Levels) >= 1 },
        };

        public static readonly Item[] Lanterns =
        {
            new Item { id = "ambar", nameEs = "Ámbar", nameEn = "Amber", howEs = "El de siempre", howEn = "The usual one", color = new Color(1.2f, 0.8f, 0.35f) },
            new Item { id = "rosa", nameEs = "Rosa", nameEn = "Pink", howEs = "Termina un nivel sin perder", howEn = "Finish a level without falling", color = new Color(1.3f, 0.45f, 0.8f), unlocked = () => Achievements.Has("sinmorir") },
            new Item { id = "verde", nameEs = "Luciérnaga", nameEn = "Firefly", howEs = "Consigue 8 logros", howEn = "Earn 8 achievements", color = new Color(0.6f, 1.3f, 0.45f), unlocked = () => Achievements.Count >= 8 },
            new Item { id = "chispa", nameEs = "Chispa", nameEn = "Spark", howEs = "Encuentra las 5 estrellas de Chispa", howEn = "Find Spark's 5 stars", color = new Color(1.3f, 1.2f, 0.9f), unlocked = () => Companion.AllFound },
            new Item { id = "eclipse", nameEs = "Eclipse",
 nameEn = "Eclipse", howEs = "Vence a la Gran Sombra", howEn = "Defeat the Great Shadow", color = new Color(0.45f, 0.65f, 1.5f), unlocked = () => Achievements.Has("eclipse") },
        };

        /// <summary>Nox: la Lumi de la noche (se abre al terminar el capítulo I).</summary>
        public static bool NoxUnlocked => LevelCatalog.Get.ChapterDone(1);

        public static bool IsNox
        {
            get => NoxUnlocked && PlayerPrefs.GetInt("fase_nox", 0) == 1;
            set
            {
                PlayerPrefs.SetInt("fase_nox", value ? 1 : 0);
                PlayerPrefs.Save();
                foreach (var style in Object.FindObjectsByType<LumiStyle>()) style.Apply();
            }
        }

        public static Item Scarf => Find(Scarves, PlayerPrefs.GetString("fase_bufanda", "naranja"));
        public static Item Lantern => Find(Lanterns, PlayerPrefs.GetString("fase_farol", "ambar"));

        static Item Find(Item[] items, string id)
        {
            foreach (var item in items) if (item.id == id && item.Unlocked) return item;
            return items[0];
        }

        public static void Wear(Item item)
        {
            if (!item.Unlocked) return;
            bool scarf = System.Array.IndexOf(Scarves, item) >= 0;
            PlayerPrefs.SetString(scarf ? "fase_bufanda" : "fase_farol", item.id);
            PlayerPrefs.Save();
            if (item != Scarves[0] && item != Lanterns[0]) Achievements.Unlock("estilo");
            foreach (var style in Object.FindObjectsByType<LumiStyle>()) style.Apply();
        }
    }
}
