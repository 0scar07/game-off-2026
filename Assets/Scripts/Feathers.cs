using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Coleccionable de cada capítulo, uno por nivel: una pluma en la Pradera, una hoja en el
    /// Bosque y un copo en la Cumbre. Está escondido en lo alto, más o menos a dos tercios del
    /// nivel. Se ven en Extras → Mapa de capítulos.
    /// </summary>
    public static class Feathers
    {
        public static bool Has(int level) => PlayerPrefs.GetInt("fase_pluma_" + level, 0) == 1;

        public static int Count(int chapter)
        {
            int n = 0;
            var levels = LevelCatalog.Get.levels;
            for (int i = 0; i < levels.Length; i++)
                if (!levels[i].secret && (chapter == 0 || levels[i].chapter == chapter) && Has(i + 1)) n++;
            return n;
        }

        public static int Total(int chapter)
        {
            int n = 0;
            foreach (var e in LevelCatalog.Get.levels) if (!e.secret && (chapter == 0 || e.chapter == chapter)) n++;
            return n;
        }

        public static string Name(int chapter, bool plural) => chapter == 2 ? Loc.Pick(plural ? "hojas" : "hoja", plural ? "leaves" : "leaf")
            : chapter == 3 ? Loc.Pick(plural ? "copos" : "copo", plural ? "snowflakes" : "snowflake")
            : Loc.Pick(plural ? "plumas" : "pluma", plural ? "feathers" : "feather");

        public static void Take(int level)
        {
            PlayerPrefs.SetInt("fase_pluma_" + level, 1);
            PlayerPrefs.Save();
            if (Count(0) >= 10) Achievements.Unlock("plumas");
        }

        /// <summary>Dibujo de la pluma, la hoja o el copo (64×64).</summary>
        public static Texture2D Texture(int chapter)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int py = 0; py < size; py++)
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f, y = (py + 0.5f) / size * 2f - 1f;
                    bool inside;
                    if (chapter == 3)
                    {
                        float a = Mathf.Atan2(y, x), r = Mathf.Sqrt(x * x + y * y);
                        inside = (r < 0.85f && Mathf.Abs(Mathf.Sin(a * 3f)) < 0.12f / Mathf.Max(r, 0.2f)) || r < 0.18f;
                    }
                    else
                    {
                        // Pluma u hoja: un óvalo inclinado con su nervio.
                        float u = (x + y) * 0.7071f, v = (y - x) * 0.7071f;
                        inside = u * u / 0.75f + v * v / (chapter == 2 ? 0.16f : 0.08f) < 1f && !(Mathf.Abs(v) < 0.02f && u > -0.7f);
                        if (Mathf.Abs(v) < 0.04f && u < -0.7f && u > -1f) inside = true;
                    }
                    tex.SetPixel(px, py, inside ? Color.white : Color.clear);
                }
            tex.Apply();
            return tex;
        }

        public static Color Tint(int chapter) => chapter == 2 ? new Color(1f, 0.6f, 0.25f) : chapter == 3 ? new Color(0.75f, 0.92f, 1.3f) : new Color(1f, 0.97f, 0.9f);
    }
}
