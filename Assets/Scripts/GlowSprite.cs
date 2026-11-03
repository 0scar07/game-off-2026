using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Halo de luz (un plano con textura difuminada que siempre mira a la cámara): el farol de
    /// Lumi, los ojos de las Sombras... Late un poco y puede brillar más de noche.
    /// </summary>
    public class GlowSprite : MonoBehaviour
    {
        public float size = 1f;
        public float pulse = 0.08f;
        public float pulseSpeed = 3f;
        [Tooltip("Tamaño de día comparado con el de noche (1 = igual).")]
        public float dayFactor = 0.45f;

        Camera cam;
        float phase;

        void Start()
        {
            cam = Camera.main;
            phase = Random.value * 10f;
        }

        void LateUpdate()
        {
            if (cam != null) transform.rotation = cam.transform.rotation;
            float day = ShiftLine.IsDay(transform.position) ? dayFactor : 1f;
            float s = size * day * (1f + Mathf.Sin(Time.time * pulseSpeed + phase) * pulse);
            transform.localScale = Vector3.one * s / Mathf.Max(transform.parent != null ? transform.parent.lossyScale.x : 1f, 1e-4f);
        }
    }
}
