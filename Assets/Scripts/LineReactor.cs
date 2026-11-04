using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// El mundo reacciona a la línea: las plantas se doblan hacia ella (más cuanto más rápido se
    /// mueve), suelta hojas al arrastrarla y los charcos son agua de día y hielo de noche (el
    /// shader recorta cada mitad, así que la línea los congela al pasar).
    /// </summary>
    public class LineReactor : MonoBehaviour
    {
        Transform player;
        ParticleSystem dayLeaves, nightLeaves;
        readonly List<Transform> plants = new List<Transform>();
        readonly List<Quaternion> baseRotations = new List<Quaternion>();
        readonly List<float> angles = new List<float>();
        float fast;

        void Start()
        {
            var controller = FindAnyObjectByType<PlayerController>();
            if (controller != null) player = controller.transform;
            foreach (var sway in FindObjectsByType<Sway>())
            {
                plants.Add(sway.transform);
                baseRotations.Add(sway.transform.rotation);
                angles.Add(0f);
            }
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) return;
            int chapter = 1;
            var hud = Hud.Instance;
            if (hud != null && LevelCatalog.Get.At(hud.levelNumber) is LevelCatalog.Entry entry) chapter = entry.chapter;
            Color day = chapter == 2 ? new Color(1f, 0.55f, 0.2f) : chapter == 3 ? new Color(0.95f, 0.97f, 1f) : new Color(0.55f, 0.88f, 0.35f);
            dayLeaves = Leaves(shader, 0f, day);
            nightLeaves = Leaves(shader, 1f, new Color(0.55f, 0.7f, 1.6f));
            BuildPuddles(shader);
        }

        // ───────── Hojas ─────────

        static Texture2D leaf;

        static Texture2D LeafTexture()
        {
            if (leaf != null) return leaf;
            const int size = 32;
            leaf = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // Rombo con los lados curvos: una hoja simple.
                    float u = Mathf.Abs(x + 0.5f - size * 0.5f) / (size * 0.28f), v = Mathf.Abs(y + 0.5f - size * 0.5f) / (size * 0.48f);
                    float d = Mathf.Pow(u, 1.3f) + Mathf.Pow(v, 1.3f);
                    leaf.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 6f)));
                }
            leaf.Apply();
            return leaf;
        }

        ParticleSystem Leaves(Shader shader, float world, Color color)
        {
            var go = new GameObject(world < 0.5f ? "HojasDia" : "HojasNoche");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = color;
            main.gravityModifier = 0.12f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.4f, 9f, 0.4f);
            shape.randomDirectionAmount = 1f;
            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
            var fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", LeafTexture());
            mat.SetFloat("_World", world);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
            ps.Play();
            return ps;
        }

        // ───────── Charcos ─────────

        void BuildPuddles(Shader shader)
        {
            var line = ShiftLine.Instance;
            if (line == null || line.angle != 0f) return;
            var water = PuddleMaterial(shader, 0f, new Color(0.45f, 0.78f, 1f, 0.8f), false);
            var ice = PuddleMaterial(shader, 1f, new Color(0.85f, 0.95f, 1.15f, 0.95f), true);
            var random = new System.Random(Mathf.RoundToInt(line.minX * 13f + line.maxX));
            int made = 0;
            for (float x = line.minX + 8f; x < line.maxX - 6f && made < 8; x += 12f + (float)random.NextDouble() * 14f)
            {
                if (!Flat(x, out float top)) continue;
                Puddle(new Vector3(x, top, 0f), water);
                Puddle(new Vector3(x, top + 0.002f, 0f), ice);
                made++;
            }
        }

        /// <summary>Suelo firme y llano (sin piezas de día o de noche) en x y a los lados.</summary>
        static bool Flat(float x, out float top)
        {
            top = 0f;
            float? first = null;
            foreach (float dx in new[] { -1f, 0f, 1f })
            {
                var hit = Physics2D.Raycast(new Vector2(x + dx, 60f), Vector2.down, 120f);
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.GetComponentInParent<WorldPiece>() != null
                    || hit.collider.attachedRigidbody != null && hit.collider.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic) return false;
                if (first == null) first = hit.point.y;
                else if (Mathf.Abs(hit.point.y - first.Value) > 0.02f) return false;
            }
            top = first ?? 0f;
            return true;
        }

        static Material PuddleMaterial(Shader shader, float world, Color color, bool cracks)
        {
            const int w = 128, h = 48;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var random = new System.Random(5);
            var lines = new List<(Vector2 a, Vector2 b)>();
            if (cracks) for (int i = 0; i < 6; i++) lines.Add((new Vector2((float)random.NextDouble() * w, (float)random.NextDouble() * h), new Vector2((float)random.NextDouble() * w, (float)random.NextDouble() * h)));
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w * 2f - 1f, v = (y + 0.5f) / h * 2f - 1f;
                    float d = u * u + v * v;
                    float alpha = Mathf.Clamp01((1f - d) * 8f);
                    // Brillo hacia el borde de arriba, como un reflejo del cielo.
                    float shade = 0.82f + 0.18f * Mathf.Clamp01(v + 0.4f);
                    if (cracks)
                        foreach (var (a, b) in lines)
                        {
                            var p = new Vector2(x, y);
                            float t = Mathf.Clamp01(Vector2.Dot(p - a, b - a) / (b - a).sqrMagnitude);
                            if (Vector2.Distance(p, a + (b - a) * t) < 0.9f) shade = 0.62f;
                        }
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            texture.Apply();
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", texture);
            mat.SetColor("_Color", color);
            mat.SetFloat("_World", world);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 2990;
            return mat;
        }

        void Puddle(Vector3 at, Material material)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<Collider>());
            quad.name = "Charco";
            quad.transform.SetParent(transform, false);
            // Tumbado sobre el suelo, un poco inclinado hacia la cámara para que se vea.
            quad.transform.position = at + new Vector3(0f, 0.01f, -0.1f);
            quad.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
            quad.transform.localScale = new Vector3(2.4f, 0.8f, 1f);
            var r = quad.GetComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        // ───────── Cada frame ─────────

        void LateUpdate()
        {
            var line = ShiftLine.Instance;
            if (line == null) return;
            fast = Mathf.Lerp(fast, Mathf.Clamp01(ShiftLine.Speed / 12f), Time.deltaTime * 6f);
            Vector2 n = line.Normal;
            Vector2 along = new Vector2(-n.y, n.x);

            // Hojas: salen de la línea a la altura de Lumi mientras se arrastra.
            if (player != null && dayLeaves != null)
            {
                Vector2 p0 = line.PointOnLine(line.x);
                Vector2 near = p0 + along * Vector2.Dot((Vector2)player.position - p0, along);
                float rate = fast * (Settings.ReduceEffects ? 35f : 110f);
                foreach (var ps in new[] { dayLeaves, nightLeaves })
                {
                    ps.transform.SetPositionAndRotation(new Vector3(near.x, near.y, -0.6f), Quaternion.Euler(0f, 0f, line.angle));
                    var emission = ps.emission;
                    emission.rateOverTime = rate;
                }
            }

            // Plantas: se doblan hacia la línea más cercana.
            var offsets = ShiftLine.Lines;
            float t = Time.time;
            for (int i = 0; i < plants.Count; i++)
            {
                var plant = plants[i];
                if (plant == null) continue;
                float along_n = Vector2.Dot(plant.position, n);
                float best = float.MaxValue;
                foreach (float o in offsets) if (Mathf.Abs(along_n - o) < Mathf.Abs(best)) best = along_n - o;
                float k = Mathf.Clamp01(1f - Mathf.Abs(best) / 4f);
                float target = Mathf.Sign(best) * n.x * k * (5f + 24f * fast) + Mathf.Sin(t * 1.6f + i * 1.3f) * 2f;
                angles[i] = Mathf.Lerp(angles[i], target, Time.deltaTime * 6f);
                plant.rotation = Quaternion.Euler(0f, 0f, angles[i]) * baseRotations[i];
            }
        }
    }
}
