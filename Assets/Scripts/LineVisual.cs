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

        void Awake()
        {
            baseScale = transform.localScale;
            cam = Camera.main;
        }

        void LateUpdate()
        {
            var line = ShiftLine.Instance;
            if (line == null || cam == null) return;
            if (index == 1 && !line.hasSecond) { transform.localScale = Vector3.zero; return; }
            Vector3 p = transform.position;
            float depth = p.z - cam.transform.position.z;
            p.x = cam.ViewportToWorldPoint(new Vector3(line.ScreenX(index == 0 ? line.x : line.x2), 0.5f, depth)).x;
            transform.position = p;
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z);
        }
    }
}
