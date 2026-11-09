using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Espejo (casilla E): si le da el día, o si Lumi le acerca el farol de noche, refleja la luz
    /// hasta su diana (casilla T más cercana) y allí crea un círculo de día, aunque esté lejos de
    /// la línea. Las piezas de día de ese círculo aparecen.
    /// </summary>
    public class Mirror : Mechanic
    {
        public float radius = 2.6f;
        Transform target;
        LineRenderer beam;
        float power;

        void Start()
        {
            float best = 40f;
            foreach (var t in FindObjectsByType<MirrorTarget>())
            {
                float d = Vector2.Distance(t.transform.position, transform.position);
                if (d < best) { best = d; target = t.transform; }
            }
            var glass = Shapes.Block(transform, "Espejo", transform.position + Vector3.up * 0.6f, new Vector3(0.9f, 0.12f, 0.8f), new Color(0.8f, 0.92f, 1f));
            glass.transform.rotation = Quaternion.Euler(0f, 0f, 35f);
            Shapes.Block(transform, "Pie", transform.position + Vector3.up * 0.25f, new Vector3(0.12f, 0.5f, 0.12f), new Color(0.5f, 0.45f, 0.6f));
            var shader = Shader.Find("Fase/Particle");
            beam = new GameObject("Rayo").AddComponent<LineRenderer>();
            beam.transform.SetParent(transform, false);
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetTexture("_MainTex", UiKit.Rounded.texture);
                mat.SetFloat("_World", 2f);
                mat.SetFloat("_Additive", 1f);
                mat.renderQueue = 3100;
                beam.sharedMaterial = mat;
            }
            beam.positionCount = 2;
            beam.widthMultiplier = 0.25f;
            beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void Update()
        {
            if (target == null) return;
            var player = Player;
            bool sun = ShiftLine.IsDay(transform.position);
            bool lantern = !sun && player != null && Vector2.Distance(player.transform.position, transform.position) < 2.2f;
            power = Mathf.MoveTowards(power, sun || lantern ? 1f : 0f, Time.deltaTime * 2.5f);
            if (power > 0.02f) WorldZone.Set(target.position, radius * power, false);
            else if (WorldZone.Active && (WorldZone.Center - (Vector2)target.position).sqrMagnitude < 0.01f) WorldZone.Clear();
            beam.enabled = power > 0.02f;
            beam.SetPosition(0, transform.position + new Vector3(0f, 0.6f, -1.2f));
            beam.SetPosition(1, target.position + new Vector3(0f, 0f, -1.2f));
            var c = lantern ? new Color(1f, 0.75f, 0.4f, power * 0.8f) : new Color(1f, 0.95f, 0.7f, power * 0.8f);
            beam.startColor = beam.endColor = c;
        }
    }

}
