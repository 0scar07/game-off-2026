using UnityEngine;

namespace Fase
{
    /// <summary>Cursor propio: una pequeña línea dorada con brillo (en ordenador; el móvil no tiene).</summary>
    public static class GoldCursor
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (Application.isMobilePlatform) return;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            // Segmento de la punta (2, 30) a (14, 6) en coordenadas de textura (y hacia arriba).
            Vector2 a = new Vector2(3f, 29f), b = new Vector2(15f, 7f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float t = Mathf.Clamp01(Vector2.Dot(p - a, b - a) / (b - a).sqrMagnitude);
                    float d = Vector2.Distance(p, a + (b - a) * t);
                    float core = Mathf.Clamp01(2.2f - d);
                    float glow = Mathf.Clamp01(1f - d / 6f) * 0.45f;
                    var gold = new Color(1f, 0.86f, 0.4f);
                    var c = Color.Lerp(gold * glow, new Color(1f, 0.97f, 0.85f), core);
                    c.a = Mathf.Max(core, glow);
                    // Borde oscuro fino para que se vea sobre fondos claros.
                    if (core <= 0f && d < 3.2f) c = new Color(0.15f, 0.08f, 0f, 0.7f);
                    texture.SetPixel(x, y, c);
                }
            texture.Apply();
            // El punto activo es la punta de arriba a la izquierda (en coordenadas de cursor, y hacia abajo).
            Cursor.SetCursor(texture, new Vector2(3f, size - 29f), CursorMode.Auto);
        }
    }
}
