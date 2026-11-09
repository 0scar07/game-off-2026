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
            // Color de la línea: el de la tienda (dorada) manda; si no, cada capítulo tiene el suyo.
            Color? core = null, halo = null;
            if (Shop.Worn("lineadorada")) { core = new Color(2.6f, 1.9f, 0.6f); halo = new Color(1f, 0.8f, 0.4f, 0.4f); }
            else
            {
                var hud = Hud.Instance != null ? Hud.Instance : FindAnyObjectByType<Hud>();
                var entry = hud != null ? LevelCatalog.Get.At(hud.levelNumber) : null;
                int chapter = entry != null ? entry.chapter : 0;
                if (chapter == 2) { core = new Color(2.7f, 1.6f, 0.7f); halo = new Color(1f, 0.65f, 0.35f, 0.38f); }       // ámbar
                else if (chapter == 3) { core = new Color(1.5f, 2.3f, 2.9f); halo = new Color(0.6f, 0.85f, 1f, 0.38f); }   // hielo
            }
            if (core.HasValue)
                foreach (var r in GetComponentsInChildren<Renderer>())
                {
                    if (r.name != "Nucleo" && r.name != "Halo") continue;
                    var block = new MaterialPropertyBlock();
                    block.SetColor("_Emission", core.Value);
                    block.SetColor("_Color", halo.Value);
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
