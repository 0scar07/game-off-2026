using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Dibujo de la línea: una franja brillante que late un poco. Se coloca donde la línea
    /// cae en la pantalla, aunque esté más cerca de la cámara que el nivel.
    /// </summary>
    [DefaultExecutionOrder(110)]
    public class LineVisual : MonoBehaviour
    {
        public float pulseSpeed = 3f;
        public float pulseAmount = 0.15f;
        [Tooltip("0 = primera línea, 1 = segunda (se oculta si el nivel no la tiene).")]
        public int index;

        Vector3 baseScale;
        Camera cam;

        ParticleSystem sparks;

        void Awake()
        {
            baseScale = transform.localScale;
            sparks = GetComponentInChildren<ParticleSystem>();
            cam = Camera.main;
            // Línea dorada de la tienda.
            if (Shop.Worn("lineadorada"))
                foreach (var r in GetComponentsInChildren<Renderer>())
                {
                    if (r.name != "Nucleo" && r.name != "Halo") continue;
                    var block = new MaterialPropertyBlock();
                    block.SetColor("_Emission", new Color(2.6f, 1.9f, 0.6f));
                    block.SetColor("_Color", new Color(1f, 0.8f, 0.4f, 0.4f));
                    r.SetPropertyBlock(block);
                }
        }

        void LateUpdate()
        {
            var line = ShiftLine.Instance;
            if (line == null || cam == null) return;
            if (index == 1 && !line.hasSecond) { transform.localScale = Vector3.zero; return; }
            // El dibujo está más cerca de la cámara que el nivel: se coloca donde la línea cae en
            // la pantalla, y se gira con ella si está inclinada o tumbada.
            Vector3 p = transform.position;
            float depth = p.z - cam.transform.position.z;
            Vector3 onScreen = cam.WorldToViewportPoint(line.PointOnLine(index == 0 ? line.x : line.x2));
            Vector3 world = cam.ViewportToWorldPoint(new Vector3(onScreen.x, onScreen.y, depth));
            transform.position = new Vector3(world.x, world.y, p.z);
            transform.rotation = Quaternion.Euler(0f, 0f, line.angle);
            // Late despacio; al moverla rápido vibra, se ensancha y suelta más chispas.
            float fast = Mathf.Clamp01(ShiftLine.Speed / 18f);
            float jitter = Settings.ReduceEffects ? 0f : (Mathf.PerlinNoise(Time.time * 40f, 0f) - 0.5f) * 0.6f * fast;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount + fast * 0.8f + jitter;
            transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z);
            if (sparks != null)
            {
                var emission = sparks.emission;
                emission.rateOverTime = 40f + fast * 260f;
            }
        }
    }
}
