using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Ciclo vivo: pájaros que cruzan el cielo y que, al pasar al lado de noche, se vuelven
    /// murciélagos (las dos versiones van juntas y el shader enseña la de su mundo).
    /// También cierra las flores de la decoración cuando les llega la noche.
    /// </summary>
    public class Wildlife : MonoBehaviour
    {
        public int birds = 5;
        public float minX = 0f, maxX = 100f, minY = 6f, maxY = 11f;

        Transform[] flock;
        float[] speed, phase;
        Transform[] flowers;
        Vector3[] flowerScale;
        Camera cam;

        void Start()
        {
            cam = Camera.main;
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) { enabled = false; return; }
            var dayMat = Wing(shader, new Color(0.25f, 0.18f, 0.2f, 0.9f), 0f);
            var nightMat = Wing(shader, new Color(0.35f, 0.2f, 0.55f, 0.95f), 1f);
            var wingMesh = WingMesh();
            flock = new Transform[birds];
            speed = new float[birds];
            phase = new float[birds];
            for (int i = 0; i < birds; i++)
            {
                var bird = new GameObject("Ave").transform;
                bird.SetParent(transform);
                bird.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 3f + Random.Range(0f, 4f));
                Part(bird, wingMesh, dayMat, new Vector3(0.5f, 0.35f, 1f));
                Part(bird, wingMesh, nightMat, new Vector3(0.6f, 0.45f, 1f)); // murciélago: alas más anchas
                flock[i] = bird;
                speed[i] = Random.Range(1.2f, 2.4f) * (Random.value < 0.5f ? -1f : 1f);
                phase[i] = Random.value * 10f;
            }

            // Flores de la decoración (las que trae el modelo "flower").
            var found = new System.Collections.Generic.List<Transform>();
            foreach (var t in FindObjectsByType<Transform>())
                if (t.name.StartsWith("flower") && t.parent != null && t.parent.name == "Decoracion") found.Add(t);
            flowers = found.ToArray();
            flowerScale = new Vector3[flowers.Length];
            flowerDay = new bool[flowers.Length];
            for (int i = 0; i < flowers.Length; i++) { flowerScale[i] = flowers[i].localScale; flowerDay[i] = ShiftLine.IsDay(flowers[i].position); }
            petals = Petals(shader);
            BuildOwls(shader);
        }

        // ───────── Pétalos al florecer ─────────

        bool[] flowerDay;
        ParticleSystem petals;

        /// <summary>Pétalos rosas y amarillos que saltan de una flor cuando le vuelve a dar el día.</summary>
        ParticleSystem Petals(Shader shader)
        {
            var go = new GameObject("Petalos");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.7f, 0.85f), new Color(1f, 0.95f, 0.6f));
            main.gravityModifier = 0.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.2f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", UiKit.Rounded.texture);
            mat.SetFloat("_World", 0f);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
            ps.Play();
            return ps;
        }

        // ───────── Búhos en los pinos de noche ─────────

        readonly System.Collections.Generic.List<(MeshRenderer renderer, float next)> owls = new System.Collections.Generic.List<(MeshRenderer, float)>();
        Material owlOpen, owlBlink;

        /// <summary>Uno de cada cuatro pinos de noche tiene un búho en la copa (solo se ve de noche).</summary>
        void BuildOwls(Shader shader)
        {
            owlOpen = OwlMaterial(shader, false);
            owlBlink = OwlMaterial(shader, true);
            int count = 0;
            foreach (var t in FindObjectsByType<Transform>())
            {
                if (!t.name.StartsWith("tree_pine") || t.parent == null || t.parent.name != "Decoracion") continue;
                if (count++ % 4 != 1) continue;
                var renderers = t.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                var owl = Shapes.Create(PrimitiveType.Quad, "Buho");
                owl.transform.SetParent(transform, false);
                owl.transform.position = new Vector3(b.center.x + Random.Range(-0.2f, 0.2f), b.min.y + b.size.y * 0.62f, b.min.z - 0.05f);
                owl.transform.localScale = Vector3.one * 0.75f;
                var mr = owl.GetComponent<MeshRenderer>();
                mr.sharedMaterial = owlOpen;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                owls.Add((mr, Random.Range(1f, 5f)));
            }
        }

        static Material OwlMaterial(Shader shader, bool blink)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int py = 0; py < size; py++)
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f, y = (py + 0.5f) / size * 2f - 1f;
                    var c = Color.clear;
                    bool body = x * x / 0.42f + (y + 0.1f) * (y + 0.1f) / 0.72f < 1f;
                    bool ear = (Mathf.Abs(x) > 0.25f && Mathf.Abs(x) < 0.55f && y > 0.45f && y < 0.8f - (0.55f - Mathf.Abs(x)) * 0.8f);
                    if (body || ear) c = new Color(0.42f, 0.34f, 0.58f);
                    if (body && y < -0.2f && x * x / 0.2f + (y + 0.45f) * (y + 0.45f) / 0.18f < 1f) c = new Color(0.62f, 0.55f, 0.76f);
                    foreach (float ex in new[] { -0.26f, 0.26f })
                    {
                        float dx = x - ex, dy = y - 0.18f;
                        if (blink) { if (Mathf.Abs(dy) < 0.04f && Mathf.Abs(dx) < 0.17f) c = new Color(0.15f, 0.1f, 0.25f); }
                        else if (dx * dx + dy * dy < 0.035f) c = dx * dx + dy * dy < 0.008f ? new Color(0.1f, 0.06f, 0.2f) : new Color(1f, 0.85f, 0.35f);
                    }
                    if (Mathf.Abs(x) < 0.07f && y > -0.02f && y < 0.12f && y > 0.12f - (0.07f - Mathf.Abs(x)) * 2.4f - 0.1f) c = new Color(1f, 0.7f, 0.3f);
                    tex.SetPixel(px, py, c);
                }
            tex.Apply();
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", tex);
            mat.SetFloat("_World", 1f);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3001;
            return mat;
        }

        static Material Wing(Shader shader, Color color, float world)
        {
            var mat = new Material(shader);
            mat.SetColor("_Color", color);
            mat.SetFloat("_World", world);
            mat.SetFloat("_Additive", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 3000;
            return mat;
        }

        /// <summary>Una "V" de dos alas.</summary>
        static Mesh WingMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { new Vector3(0f, 0f, 0f), new Vector3(-1f, 0.5f, 0f), new Vector3(-0.5f, 0.05f, 0f), new Vector3(1f, 0.5f, 0f), new Vector3(0.5f, 0.05f, 0f) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 1, 0, 3, 4, 0, 4, 3 };
            mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white, Color.white };
            mesh.uv = new[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f) };
            return mesh;
        }

        static void Part(Transform parent, Mesh mesh, Material mat, Vector3 scale)
        {
            var go = new GameObject("Alas");
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        void Update()
        {
            float camX = cam != null ? cam.transform.position.x : (minX + maxX) * 0.5f;
            for (int i = 0; i < flock.Length; i++)
            {
                var bird = flock[i];
                var p = bird.position + Vector3.right * speed[i] * Time.deltaTime;
                p.y += Mathf.Sin(Time.time * 1.3f + phase[i]) * 0.004f;
                // Al salir de la vista, vuelve por el otro lado.
                if (Mathf.Abs(p.x - camX) > 22f) p.x = camX - Mathf.Sign(speed[i]) * 21f;
                bird.position = p;
                // Aleteo: las alas se aplastan y se abren. Los murciélagos aletean más rápido.
                bool night = !ShiftLine.IsDay(p);
                float flap = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.time * (night ? 14f : 8f) + phase[i]));
                bird.localScale = new Vector3(Mathf.Sign(speed[i]), flap, 1f);
            }
            // Los búhos parpadean de vez en cuando.
            for (int i = 0; i < owls.Count; i++)
            {
                var (renderer, nextBlink) = owls[i];
                if (renderer == null) continue;
                if (Time.time > nextBlink)
                {
                    bool closing = renderer.sharedMaterial == owlOpen;
                    renderer.sharedMaterial = closing ? owlBlink : owlOpen;
                    owls[i] = (renderer, Time.time + (closing ? 0.15f : Random.Range(2f, 6f)));
                }
            }
            for (int i = 0; i < flowers.Length; i++)
            {
                bool day = ShiftLine.IsDay(flowers[i].position);
                // Al volver el día, la flor se abre soltando unos pétalos.
                if (day && !flowerDay[i] && petals != null && !Settings.ReduceEffects)
                {
                    petals.transform.position = flowers[i].position + Vector3.up * 0.3f + Vector3.back * 0.3f;
                    petals.Emit(8);
                }
                flowerDay[i] = day;

                var target = day ? flowerScale[i] : new Vector3(flowerScale[i].x * 0.55f, flowerScale[i].y * 0.7f, flowerScale[i].z * 0.55f);
                flowers[i].localScale = Vector3.Lerp(flowers[i].localScale, target, Time.deltaTime * 3f);
            }
        }
    }
}
