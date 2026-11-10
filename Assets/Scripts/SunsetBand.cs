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
            band = Shapes.Glow(transform, "Ocaso", Vector3.zero, new Vector2(3.2f, 80f), new Color(1f, 0.55f, 0.25f, 0.16f)).transform;
            core = Shapes.Glow(transform, "OcasoNucleo", Vector3.zero, new Vector2(1.1f, 80f), new Color(1f, 0.75f, 0.45f, 0.14f)).transform;
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
