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
            for (int i = 0; i < flowers.Length; i++) flowerScale[i] = flowers[i].localScale;
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
            for (int i = 0; i < flowers.Length; i++)
            {
                bool day = ShiftLine.IsDay(flowers[i].position);
                var target = day ? flowerScale[i] : new Vector3(flowerScale[i].x * 0.55f, flowerScale[i].y * 0.7f, flowerScale[i].z * 0.55f);
                flowers[i].localScale = Vector3.Lerp(flowers[i].localScale, target, Time.deltaTime * 3f);
            }
        }
    }
}
