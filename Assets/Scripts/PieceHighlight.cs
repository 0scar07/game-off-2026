using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Mientras se mueve la línea, las piezas de día o de noche que están a su alcance (las que
    /// van a aparecer o desaparecer) se rodean con un contorno dorado que late.
    /// </summary>
    public class PieceHighlight : MonoBehaviour
    {
        const float Reach = 4.5f;

        class Outline
        {
            public List<WorldPiece> pieces = new List<WorldPiece>();
            public LineRenderer line;
            public Bounds bounds;
            public float alpha;
        }

        readonly List<Outline> outlines = new List<Outline>();
        float holding;

        void Start()
        {
            var shader = Shader.Find("Fase/Particle");
            if (shader == null) return;
            var mat = new Material(shader);
            mat.SetFloat("_World", 2f);
            mat.SetFloat("_Additive", 1f);
            mat.renderQueue = 3100;
            // Las piezas sueltas de una misma fila (un puente de varias casillas) van en un solo contorno.
            var groups = new List<(World world, Bounds bounds, List<WorldPiece> pieces)>();
            var all = new List<WorldPiece>(FindObjectsByType<WorldPiece>());
            all.Sort((a, b) => a.transform.position.y != b.transform.position.y ? a.transform.position.y.CompareTo(b.transform.position.y) : a.transform.position.x.CompareTo(b.transform.position.x));
            foreach (var piece in all)
            {
                var box = piece.GetComponent<BoxCollider2D>();
                if (box == null) continue;
                var b = new Bounds((Vector3)(Vector2)piece.transform.TransformPoint(box.offset), Vector3.Scale(box.size, piece.transform.lossyScale) + Vector3.forward);
                b.size = new Vector3(Mathf.Abs(b.size.x), Mathf.Abs(b.size.y), 1f);
                int last = groups.Count - 1;
                if (last >= 0 && groups[last].world == piece.world && Mathf.Abs(groups[last].bounds.min.y - b.min.y) < 0.05f
                    && Mathf.Abs(groups[last].bounds.max.y - b.max.y) < 0.05f && b.min.x - groups[last].bounds.max.x < 0.1f)
                {
                    var g = groups[last];
                    g.bounds.Encapsulate(b);
                    g.pieces.Add(piece);
                    groups[last] = g;
                }
                else groups.Add((piece.world, b, new List<WorldPiece> { piece }));
            }
            foreach (var (world, bounds, pieces) in groups)
            {
                var go = new GameObject("Contorno");
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = mat;
                lr.loop = true;
                lr.useWorldSpace = true;
                lr.positionCount = 4;
                lr.widthMultiplier = 0.07f;
                lr.numCornerVertices = 3;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                float pad = 0.12f, z = -0.7f;
                lr.SetPositions(new[]
                {
                    new Vector3(bounds.min.x - pad, bounds.min.y - pad, z), new Vector3(bounds.min.x - pad, bounds.max.y + pad, z),
                    new Vector3(bounds.max.x + pad, bounds.max.y + pad, z), new Vector3(bounds.max.x + pad, bounds.min.y - pad, z),
                });
                lr.enabled = false;
                outlines.Add(new Outline { pieces = pieces, line = lr, bounds = bounds });
            }
        }

        void Update()
        {
            var line = ShiftLine.Instance;
            if (line == null) return;
            holding = ShiftLine.Speed > 0.4f ? 0.5f : holding - Time.deltaTime;
            Vector2 n = line.Normal;
            var offsets = ShiftLine.Lines;
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 7f);
            foreach (var o in outlines)
            {
                float target = 0f;
                if (holding > 0f && o.pieces[0] != null && o.pieces[0].gameObject.activeInHierarchy)

                {
                    // Distancia de la caja a la línea, medida a lo largo de la normal.
                    float half = Mathf.Abs(n.x) * o.bounds.extents.x + Mathf.Abs(n.y) * o.bounds.extents.y;
                    float center = Vector2.Dot(o.bounds.center, n);
                    foreach (float off in offsets)
                    {
                        float gap = Mathf.Max(0f, Mathf.Abs(center - off) - half);
                        if (gap < Reach) target = Mathf.Max(target, 1f - gap / Reach);
                    }
                }
                o.alpha = Mathf.MoveTowards(o.alpha, target, Time.deltaTime * 4f);
                bool on = o.alpha > 0.01f;
                if (o.line.enabled != on) o.line.enabled = on;
                if (!on) continue;
                var c = new Color(1f, 0.85f, 0.45f, o.alpha * pulse);
                o.line.startColor = o.line.endColor = c;
            }
        }
    }
}
