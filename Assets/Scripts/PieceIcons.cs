using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Accesibilidad: un icono de sol en las piezas de día y de luna en las de noche, para no
    /// depender del color. Cada icono se recorta con la línea igual que su pieza (solo se ve
    /// donde la pieza existe). Se enciende en Opciones → Controles → "Formas en las piezas".
    /// </summary>
    public class PieceIcons : MonoBehaviour
    {
        readonly List<GameObject> icons = new List<GameObject>();
        static Texture2D sun, moon;

        void Start()
        {
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) return;
            var dayMat = Material(shader, 0f, Sun());
            var nightMat = Material(shader, 1f, Moon());
            foreach (var piece in FindObjectsByType<WorldPiece>())
            {
                var box = piece.GetComponent<BoxCollider2D>();
                if (box == null) continue;
                Vector2 center = piece.transform.TransformPoint(box.offset);
                Vector2 size = Vector2.Scale(box.size, piece.transform.lossyScale);
                size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
                // Uno cada 2 casillas a lo largo de la pieza, centrados.
                int count = Mathf.Max(1, Mathf.RoundToInt(size.x / 2f));
                float icon = Mathf.Clamp(Mathf.Max(size.y, 0.55f) * 0.75f, 0.45f, 0.7f);
                for (int i = 0; i < count; i++)
                {
                    float x = center.x - size.x * 0.5f + size.x * (i + 0.5f) / count;
                    var quad = Shapes.Create(PrimitiveType.Quad, "Icono");
                    quad.transform.SetParent(transform, false);
                    quad.transform.position = new Vector3(x, center.y, -0.8f);
                    quad.transform.localScale = Vector3.one * icon;
                    var r = quad.GetComponent<MeshRenderer>();
                    r.sharedMaterial = piece.world == World.Day ? dayMat : nightMat;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    icons.Add(quad);
                }
            }
            Apply();
            Settings.Changed += Apply;
        }

        void OnDestroy() => Settings.Changed -= Apply;

        void Apply()
        {
            bool on = Settings.PieceIcons;
            foreach (var icon in icons) if (icon != null) icon.SetActive(on);
        }

        static Material Material(Shader shader, float world, Texture2D texture)
        {
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", texture);
            mat.SetFloat("_World", world);
            mat.SetFloat("_Additive", 0f);
            mat.SetColor("_Color", new Color(1f, 1f, 1f, 0.92f));
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3050;
            return mat;
        }

        const int Size = 64;

        /// <summary>Sol: círculo con ocho rayos, con borde oscuro para que se lea sobre la madera.</summary>
        static Texture2D Sun()
        {
            if (sun != null) return sun;
            sun = Draw((x, y) =>
            {
                float r = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x);
                bool disc = r < 0.42f;
                bool ray = r > 0.55f && r < 0.92f && Mathf.Abs(Mathf.Sin(a * 4f)) < 0.28f;
                return disc || ray;
            });
            return sun;
        }

        /// <summary>Luna creciente.</summary>
        static Texture2D Moon()
        {
            if (moon != null) return moon;
            moon = Draw((x, y) => x * x + y * y < 0.8f && (x - 0.38f) * (x - 0.38f) + (y - 0.12f) * (y - 0.12f) > 0.42f);
            return moon;
        }

        static Texture2D Draw(System.Func<float, float, bool> inside)
        {
            var t = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int py = 0; py < Size; py++)
                for (int px = 0; px < Size; px++)
                {
                    float x = (px + 0.5f) / Size * 2f - 1f, y = (py + 0.5f) / Size * 2f - 1f;
                    bool fill = inside(x, y);
                    // Borde: algún vecino cercano está dentro.
                    bool edge = !fill && (inside(x + 0.09f, y) || inside(x - 0.09f, y) || inside(x, y + 0.09f) || inside(x, y - 0.09f));
                    t.SetPixel(px, py, fill ? Color.white : edge ? new Color(0.1f, 0.06f, 0.2f, 0.8f) : Color.clear);
                }
            t.Apply();
            return t;
        }
    }
}
