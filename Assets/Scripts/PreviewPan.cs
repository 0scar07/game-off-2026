using UnityEngine;

namespace Fase
{
    /// <summary>Recorre despacio el plano de un nivel en la ficha del mapa de viaje (ida y vuelta, con pausa en cada punta).</summary>
    public class PreviewPan : MonoBehaviour
    {
        float travel, t;
        RectTransform rect;

        void Awake() => rect = (RectTransform)transform;

        public void Restart(float distance)
        {
            if (rect == null) rect = (RectTransform)transform;
            travel = distance;
            t = 0f;
            rect.anchoredPosition = Vector2.zero;
        }

        void Update()
        {
            if (travel <= 0f) return;
            t += Time.unscaledDeltaTime;
            float duration = Mathf.Max(6f, travel / 70f);
            // Ciclo: 0→1 ida, pausa, 1→0 vuelta, pausa.
            float cycle = Mathf.Repeat(t / duration, 2.5f);
            float k = cycle < 1f ? cycle : cycle < 1.25f ? 1f : cycle < 2.25f ? 1f - (cycle - 1.25f) : 0f;
            rect.anchoredPosition = new Vector2(-travel * Mathf.SmoothStep(0f, 1f, k), 0f);
        }
    }
}
