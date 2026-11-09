using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Un dibujo propio para cada logro (sol, luna, árbol, cofre, línea...), pintado al vuelo en una
    /// textura pequeña. Cada forma es una función que dice si un punto (x, y entre -1 y 1, y hacia
    /// arriba) está dentro; el borde oscuro se añade solo.
    /// </summary>
    public static class AchievementIcons
    {
        delegate bool Shape(float x, float y);

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static bool Circle(float x, float y, float cx, float cy, float r) => (x - cx) * (x - cx) + (y - cy) * (y - cy) < r * r;
        static bool Box(float x, float y, float x0, float y0, float x1, float y1) => x > x0 && x < x1 && y > y0 && y < y1;
        static bool Tri(float x, float y, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = (x - bx) * (ay - by) - (ax - bx) * (y - by);
            float d2 = (x - cx) * (by - cy) - (bx - cx) * (y - cy);
            float d3 = (x - ax) * (cy - ay) - (cx - ax) * (y - ay);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }
        static bool Rays(float x, float y, float r0, float r1, int count)
        {
            float r = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x);
            return r > r0 && r < r1 && Mathf.Abs(Mathf.Sin(a * count * 0.5f)) < 0.25f;
        }
        static bool Star(float x, float y, float r)
        {
            float a = Mathf.Atan2(y, x) + Mathf.PI / 2f, d = Mathf.Sqrt(x * x + y * y);
            float k = Mathf.Cos(Mathf.PI / 5f) / Mathf.Cos(Mathf.Repeat(a, 2f * Mathf.PI / 5f) - Mathf.PI / 5f);
            return d < r * Mathf.Lerp(0.45f, 1f, Mathf.Pow(Mathf.Clamp01((k - 0.81f) / 0.19f), 2f));
        }

        static readonly Dictionary<string, (Shape shape, Color color)> Drawings = new Dictionary<string, (Shape, Color)>
        {
            ["amanecer"] = ((x, y) => (Circle(x, y, 0, -0.25f, 0.45f) && y > -0.25f) || (Rays(x, y + 0.25f, 0.58f, 0.9f, 10) && y > -0.2f) || Box(x, y, -0.9f, -0.42f, 0.9f, -0.3f), new Color(1f, 0.82f, 0.4f)),
            ["pradera"] = ((x, y) => Tri(x, y, -0.6f, -0.35f, 0f, 0.75f, 0.6f, -0.35f) || Box(x, y, -0.12f, -0.75f, 0.12f, -0.3f), new Color(0.5f, 0.86f, 0.42f)),
            ["bosque"] = ((x, y) => (Circle(x, y, 0f, 0.1f, 0.55f) && Circle(x, y, 0.35f, -0.25f, 0.85f) && Circle(x, y, -0.35f, 0.45f, 0.85f)) || Box(x, y, -0.04f, -0.8f, 0.04f, -0.2f), new Color(1f, 0.55f, 0.22f)),
            ["cumbre"] = ((x, y) => Tri(x, y, -0.85f, -0.6f, 0f, 0.7f, 0.85f, -0.6f), new Color(0.75f, 0.88f, 1f)),
            ["eclipse"] = ((x, y) => Circle(x, y, 0, 0, 0.7f) && !Circle(x, y, 0.12f, 0.08f, 0.6f), new Color(1f, 0.85f, 0.45f)),
            ["coleccionista"] = ((x, y) => Circle(x, y, 0, 0, 0.7f) && !Circle(x, y, 0.35f, 0.15f, 0.55f), new Color(1f, 0.9f, 0.55f)),
            ["cofre"] = ((x, y) => Box(x, y, -0.7f, -0.55f, 0.7f, 0.15f) || (Circle(x, y, 0, 0.15f, 0.7f) && y > 0.15f && Mathf.Abs(x) < 0.7f), new Color(0.85f, 0.58f, 0.32f)),
            ["equilibrista"] = ((x, y) => Box(x, y, -0.9f, -0.2f, -0.15f, 0f) || Box(x, y, 0.2f, -0.2f, 0.9f, 0f) || Circle(x, y, 0f, 0.45f, 0.22f), new Color(0.86f, 0.62f, 0.38f)),
            ["piedra"] = ((x, y) => Circle(x, y, -0.15f, -0.15f, 0.55f) || Circle(x, y, 0.3f, -0.25f, 0.4f) || Circle(x, y, 0.1f, 0.2f, 0.4f), new Color(0.6f, 0.6f, 0.68f)),
            ["sinmorir"] = ((x, y) => Circle(x, y, -0.3f, 0.2f, 0.36f) || Circle(x, y, 0.3f, 0.2f, 0.36f) || Tri(x, y, -0.64f, 0.12f, 0.64f, 0.12f, 0f, -0.7f), new Color(1f, 0.45f, 0.5f)),
            ["medioymedio"] = ((x, y) => Circle(x, y, 0, 0, 0.7f) && (x < -0.05f || x > 0.05f), new Color(0.75f, 0.8f, 1f)),
            ["oro"] = ((x, y) => Star(x, y, 0.85f), new Color(1f, 0.82f, 0.3f)),
            ["luna"] = ((x, y) => Circle(x, y, 0, 0, 0.62f) || Rays(x, y, 0.72f, 0.92f, 12), new Color(0.95f, 0.95f, 1f)),
            ["saltos"] = ((x, y) => Mathf.Abs(y - 0.18f * Mathf.Sin(x * 9f)) < 0.09f && Mathf.Abs(x) < 0.6f || Box(x, y, -0.6f, -0.62f, 0.6f, -0.5f) || Box(x, y, -0.6f, 0.5f, 0.6f, 0.62f), new Color(0.6f, 0.95f, 0.6f)),
            ["estilo"] = ((x, y) => Mathf.Abs(y - 0.25f * Mathf.Sin(x * 3f)) < 0.18f && Mathf.Abs(x) < 0.85f || Box(x, y, 0.3f, -0.75f, 0.5f, -0.05f), new Color(1f, 0.5f, 0.3f)),
            ["secreto"] = ((x, y) => (Circle(x, y, 0, 0.2f, 0.62f) || Box(x, y, -0.62f, -0.7f, 0.62f, 0.2f)) && !(Circle(x, y, 0, 0.05f, 0.18f) || Tri(x, y, -0.15f, 0f, 0.15f, 0f, 0f, -0.45f)), new Color(0.75f, 0.55f, 1f)),
            ["espejo"] = ((x, y) => Tri(x, y, -0.1f, -0.6f, -0.1f, 0.6f, -0.8f, 0f) || Tri(x, y, 0.1f, -0.6f, 0.1f, 0.6f, 0.8f, 0f), new Color(0.6f, 0.85f, 1f)),
            ["diario"] = ((x, y) => (Box(x, y, -0.7f, -0.7f, 0.7f, 0.55f) && !Box(x, y, -0.55f, -0.55f, 0.55f, 0.25f)) || Circle(x, y, 0f, -0.15f, 0.22f), new Color(1f, 0.75f, 0.4f)),
            ["noctambulo"] = ((x, y) => (Circle(x, y, -0.32f, 0.05f, 0.3f) && !Circle(x, y, -0.32f, 0.05f, 0.12f)) || (Circle(x, y, 0.32f, 0.05f, 0.3f) && !Circle(x, y, 0.32f, 0.05f, 0.12f)) || Tri(x, y, -0.1f, -0.25f, 0.1f, -0.25f, 0f, -0.5f), new Color(0.65f, 0.75f, 1f)),
            ["saltarin"] = ((x, y) => Tri(x, y, -0.6f, 0.05f, 0.6f, 0.05f, 0f, 0.7f) || Tri(x, y, -0.6f, -0.55f, 0.6f, -0.55f, 0f, 0.1f), new Color(0.5f, 0.95f, 0.75f)),
            ["lector"] = ((x, y) => (Box(x, y, -0.8f, -0.55f, -0.05f, 0.55f) || Box(x, y, 0.05f, -0.55f, 0.8f, 0.55f)) && !(Mathf.Abs(y - 0.25f) < 0.05f || Mathf.Abs(y) < 0.05f || Mathf.Abs(y + 0.25f) < 0.05f), new Color(0.95f, 0.88f, 0.7f)),
            ["fotografo"] = ((x, y) => (Box(x, y, -0.8f, -0.5f, 0.8f, 0.4f) || Box(x, y, -0.3f, 0.4f, 0.3f, 0.6f)) && !(Circle(x, y, 0, -0.05f, 0.3f) && !Circle(x, y, 0, -0.05f, 0.16f)), new Color(0.8f, 0.85f, 0.95f)),
        };

        /// <summary>Dibujo del logro (null si no tiene uno propio).</summary>
        public static Sprite Get(string id)
        {
            if (cache.TryGetValue(id, out var sprite)) return sprite;
            if (!Drawings.TryGetValue(id, out var entry)) return cache[id] = null;
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var edge = new Color(0.08f, 0.05f, 0.16f, 0.85f);
            for (int py = 0; py < size; py++)
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f, y = (py + 0.5f) / size * 2f - 1f;
                    bool inside = entry.shape(x, y);
                    bool border = !inside && (entry.shape(x + 0.06f, y) || entry.shape(x - 0.06f, y) || entry.shape(x, y + 0.06f) || entry.shape(x, y - 0.06f));
                    // Un poco de luz arriba para que no quede plano.
                    var fill = Color.Lerp(entry.color, Color.white, Mathf.Clamp01(y * 0.35f + 0.1f));
                    texture.SetPixel(px, py, inside ? fill : border ? edge : Color.clear);
                }
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return cache[id] = sprite;
        }
    }
}
