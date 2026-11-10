using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Habilidades que se desbloquean jugando (Extras → Habilidades, donde también se apagan):
    /// doble salto (8 plumas), planear con el farol (capítulo II terminado) y burbuja de noche
    /// más grande (15 plumas).
    /// </summary>
    public static class Skills
    {
        public static readonly (string id, string es, string en, string howEs, string howEn)[] All =
        {
            ("doble", "Doble salto", "Double jump", "Consigue 8 plumas, hojas o copos", "Find 8 feathers, leaves or snowflakes"),
            ("planear", "Planear con el farol", "Lantern glide", "Termina el capítulo II (mantén saltar al caer)", "Finish chapter II (hold jump while falling)"),
            ("burbuja", "Burbuja más grande", "Bigger bubble", "Consigue 15 plumas, hojas o copos", "Find 15 feathers, leaves or snowflakes"),
        };

        public static bool Unlocked(string id) => id switch
        {
            "doble" => Feathers.Count(0) >= 8,
            "planear" => LevelCatalog.Get.ChapterDone(2),
            "burbuja" => Feathers.Count(0) >= 15,
            _ => false,
        };

        public static bool Enabled(string id) => PlayerPrefs.GetInt("fase_habilidad_" + id, 1) == 1;
        public static void SetEnabled(string id, bool value) { PlayerPrefs.SetInt("fase_habilidad_" + id, value ? 1 : 0); PlayerPrefs.Save(); }

        public static bool Active(string id) => Unlocked(id) && Enabled(id);
        public static bool DoubleJump => Active("doble");
        public static bool Glide => Active("planear");
        public static bool BigBubble => Active("burbuja");
    }
}
