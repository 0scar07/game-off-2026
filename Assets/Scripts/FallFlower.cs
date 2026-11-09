using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Recuerda dónde se cae Lumi en cada nivel (por tramos de 4 casillas) y, si en algún sitio
    /// se ha caído 3 veces o más, crece allí una flor que brilla. Al pasar cerca dice cuántas.
    /// </summary>
    public class FallFlower : MonoBehaviour
    {
        const float Bucket = 4f;
        const int MinFalls = 3;

        int level;
        Transform player, flower;
        CanvasGroup label;
        Text labelText;
        int shownFalls;

        string Key(int bucket) => $"fase_caidas_{level}_{bucket}";

        void Start()
        {
            var hud = Hud.Instance;
            if (hud == null || hud.Daily || hud.sandbox) { enabled = false; return; }
            level = hud.levelNumber;
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
            GameEvents.Died += OnDied;
            // El tramo con más caídas de otras veces.
            var shift = ShiftLine.Instance;
            float lo = shift != null ? shift.minX : 0f, hi = shift != null ? shift.maxX : 200f;
            int best = -1, most = 0;
            for (int b = Mathf.FloorToInt(lo / Bucket) - 2; b <= Mathf.CeilToInt(hi / Bucket) + 2; b++)
            {
                int falls = PlayerPrefs.GetInt(Key(b), 0);
                if (falls > most) { most = falls; best = b; }
            }
            if (most >= MinFalls) Grow(best, most);
        }

        void OnDestroy() => GameEvents.Died -= OnDied;

        void OnDied()
        {
            int b = Mathf.FloorToInt(PlayerController.LastDeathPosition.x / Bucket);
            PlayerPrefs.SetInt(Key(b), PlayerPrefs.GetInt(Key(b), 0) + 1);
        }

        /// <summary>Busca suelo firme en el tramo y planta la flor encima.</summary>
        void Grow(int bucket, int falls)
        {
            for (float x = bucket * Bucket; x < (bucket + 1) * Bucket + 6f; x += 0.5f)
            {
                var hit = Physics2D.Raycast(new Vector2(x, 60f), Vector2.down, 120f);
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.GetComponentInParent<WorldPiece>() != null) continue;
                Plant(new Vector3(x, hit.point.y, -0.6f), falls);
                return;
            }
        }

        void Plant(Vector3 at, int falls)
        {
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) return;
            flower = new GameObject("FlorDeLasCaidas").transform;
            flower.SetParent(transform, false);
            flower.position = at;
            var stem = Shapes.Create(PrimitiveType.Quad, "Tallo");
            stem.transform.SetParent(flower, false);
            stem.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            stem.transform.localScale = new Vector3(0.06f, 0.6f, 1f);
            stem.GetComponent<MeshRenderer>().sharedMaterial = Flat(shader, new Color(0.3f, 0.7f, 0.35f), UiKit.Rounded.texture, false);
            var bloom = Shapes.Create(PrimitiveType.Quad, "Flor");
            bloom.transform.SetParent(flower, false);
            bloom.transform.localPosition = new Vector3(0f, 0.65f, -0.01f);
            bloom.transform.localScale = Vector3.one * 0.38f;
            bloom.GetComponent<MeshRenderer>().sharedMaterial = Flat(shader, new Color(1f, 0.5f, 0.75f), Petals(), false);
            var glow = Shapes.Create(PrimitiveType.Quad, "Brillo");
            glow.transform.SetParent(flower, false);
            glow.transform.localPosition = new Vector3(0f, 0.65f, 0.02f);
            glow.transform.localScale = Vector3.one * 1.3f;
            glow.GetComponent<MeshRenderer>().sharedMaterial = Flat(shader, new Color(1f, 0.6f, 0.85f, 0.45f), UiKit.Rounded.texture, true);

            var canvasGo = new GameObject("Rotulo", typeof(RectTransform));
            canvasGo.transform.SetParent(flower, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(600f, 60f);
            rect.localPosition = new Vector3(0f, 1.4f, 0f);
            rect.localScale = Vector3.one * 0.005f;
            labelText = UiKit.Text(canvasGo.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 60f), 40, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.93f));
            Destroy(labelText.GetComponent<TextScaler>());
            label = canvasGo.AddComponent<CanvasGroup>();
            label.alpha = 0f;
            shownFalls = falls;
        }

        static Material Flat(Shader shader, Color color, Texture texture, bool additive)
        {
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", texture);
            mat.SetColor("_Color", color);
            mat.SetFloat("_World", 2f);
            mat.SetFloat("_Additive", additive ? 1f : 0f);
            if (!additive)
            {
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            mat.renderQueue = 3000;
            return mat;
        }

        /// <summary>Cinco pétalos redondos con el centro amarillo.</summary>
        static Texture2D Petals()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int py = 0; py < size; py++)
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f, y = (py + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(x * x + y * y), a = Mathf.Atan2(y, x);
                    float edge = 0.62f + 0.3f * Mathf.Abs(Mathf.Cos(a * 2.5f));
                    var c = r < 0.28f ? new Color(1f, 0.9f, 0.4f) : r < edge ? Color.white : Color.clear;
                    tex.SetPixel(px, py, c);
                }
            tex.Apply();
            return tex;
        }

        void Update()
        {
            if (flower == null || player == null) return;
            flower.localScale = Vector3.one * (1f + 0.05f * Mathf.Sin(Time.time * 2f));
            bool near = Mathf.Abs(flower.position.x - player.position.x) < 3f;
            label.alpha = Mathf.MoveTowards(label.alpha, near ? 1f : 0f, Time.deltaTime * 2.5f);
            labelText.text = Loc.Pick($"Aquí te caíste {shownFalls} veces", $"You fell here {shownFalls} times");
        }
    }
}
