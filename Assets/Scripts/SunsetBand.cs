using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Banda de ocaso: un resplandor cálido y suave pegado a la línea, como si en la frontera
    /// entre el día y la noche siempre estuviera atardeciendo.
    /// </summary>
    public class SunsetBand : MonoBehaviour
    {
        Transform band, core;
        ShiftLine line;

        void Start()
        {
            line = ShiftLine.Instance;
            if (line == null) { enabled = false; return; }
            // Suave: la línea ya brilla por sí misma y un halo fuerte tapaba a Lumi al cruzarla.
            band = Shapes.Glow(transform, "Ocaso", Vector3.zero, new Vector2(2.6f, 80f), new Color(1f, 0.5f, 0.2f, 0.09f)).transform;
            core = Shapes.Glow(transform, "OcasoNucleo", Vector3.zero, new Vector2(0.9f, 80f), new Color(1f, 0.7f, 0.4f, 0.07f)).transform;

        }

        void LateUpdate()
        {
            if (line == null) return;
            var p = line.PointOnLine(line.x);
            var rot = Quaternion.Euler(0f, 0f, line.angle);
            band.SetPositionAndRotation(new Vector3(p.x, p.y, 0.6f), rot);
            core.SetPositionAndRotation(new Vector3(p.x, p.y, 0.55f), rot);
            float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 1.3f);
            band.localScale = new Vector3(pulse, 1f, 1f);
        }
    }
}
