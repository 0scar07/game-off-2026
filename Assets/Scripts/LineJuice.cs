using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// La línea reacciona al moverla: suelta chispas que se quedan atrás y unas estelas de luz
    /// según la velocidad. Y las piezas que dejan de existir se deshacen en trocitos que caen.
    /// </summary>
    public class LineJuice : MonoBehaviour
    {
        class Bit
        {
            public Transform t;
            public Material m;
            public Vector3 v;
            public float life, total;
            public Color color;
            public bool gravity;
        }

        const int Pool = 90;
        readonly List<Bit> bits = new List<Bit>();
        int next;
        ShiftLine line;
        Camera cam;
        float lastX, spawn;

        void Start()
        {
            line = ShiftLine.Instance;
            cam = Camera.main;
            if (line == null || cam == null) { enabled = false; return; }
            lastX = line.x;
            for (int i = 0; i < Pool; i++)
            {
                var go = Shapes.Glow(transform, "Chispa", Vector3.zero, Vector2.one * 0.2f, Color.clear, 2f, true, BlobShadow.Soft);
                go.SetActive(false);
                bits.Add(new Bit { t = go.transform, m = go.GetComponent<Renderer>().material });
            }
            GameEvents.PieceVanished += OnVanished;
        }

        void OnDestroy() => GameEvents.PieceVanished -= OnVanished;

        Bit Take()
        {
            var b = bits[next];
            next = (next + 1) % bits.Count;
            b.t.gameObject.SetActive(true);
            return b;
        }

        /// <summary>Una pieza que desaparece se deshace en trocitos de su color.</summary>
        void OnVanished(Vector3 at, World world)
        {
            if (bits.Count == 0) return;
            var color = world == World.Day ? new Color(1f, 0.78f, 0.5f, 1f) : new Color(0.55f, 0.8f, 1f, 1f);
            for (int i = 0; i < 7; i++)
            {
                var b = Take();
                b.t.position = at + new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-0.2f, 0.4f), -0.4f);
                b.t.localScale = Vector3.one * Random.Range(0.12f, 0.24f);
                b.v = new Vector3(Random.Range(-1.2f, 1.2f), Random.Range(0.5f, 2.2f), 0f);
                b.life = b.total = Random.Range(0.5f, 0.9f);
                b.color = color;
                b.gravity = true;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            float speed = dt > 0f ? (line.x - lastX) / dt : 0f;
            lastX = line.x;
            float fast = Mathf.Clamp01((Mathf.Abs(speed) - 2f) / 14f);
            if (fast > 0f && !PauseMenu.Paused)
            {
                spawn -= dt * (10f + 60f * fast);
                var n = line.Normal;
                var tangent = new Vector2(-n.y, n.x);
                Vector2 center = line.PointOnLine(line.x);
                float half = cam.orthographic ? cam.orthographicSize : 8f;
                while (spawn < 0f)
                {
                    spawn += 1f;
                    var b = Take();
                    var p = center + tangent * Random.Range(-half, half);
                    b.t.position = new Vector3(p.x, p.y, -0.3f);
                    bool streak = Random.value < 0.35f;
                    // Las chispas se quedan atrás: van en contra del movimiento.
                    b.v = (Vector3)(-n * Mathf.Sign(speed) * Random.Range(1f, 3f) * (streak ? 2.5f : 1f)) + new Vector3(0f, Random.Range(-0.3f, 0.6f), 0f);
                    b.t.localScale = streak ? new Vector3(Random.Range(0.8f, 1.6f) * (Mathf.Abs(n.x) > 0.5f ? 1f : 0.06f), Mathf.Abs(n.x) > 0.5f ? 0.05f : 1.2f, 1f) : Vector3.one * Random.Range(0.08f, 0.2f);
                    b.life = b.total = Random.Range(0.35f, 0.7f);
                    b.color = Random.value < 0.5f ? new Color(1f, 0.85f, 0.55f, 0.9f) : new Color(0.7f, 0.85f, 1f, 0.9f);
                    b.gravity = false;
                }
            }
            foreach (var b in bits)
            {
                if (!b.t.gameObject.activeSelf) continue;
                b.life -= dt;
                if (b.life <= 0f) { b.t.gameObject.SetActive(false); continue; }
                if (b.gravity) b.v += Vector3.down * 6f * dt;
                b.t.position += b.v * dt;
                var c = b.color;
                c.a *= b.life / b.total;
                b.m.SetColor("_Color", c);
            }
        }
    }
}
