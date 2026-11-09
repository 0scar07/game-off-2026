using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Línea con peso (cabecera "peso: x"): si no la sujetas, cae hacia x y se balancea como un
    /// péndulo hasta pararse. Hay que llevarla y mantenerla donde haga falta.
    /// </summary>
    public class LineWeight : MonoBehaviour
    {
        public float rest;
        public float pull = 6f, damping = 0.6f;
        float velocity;

        void Update()
        {
            var line = ShiftLine.Instance;
            if (line == null || PauseMenu.Paused) return;
            if (line.Holding) { velocity = 0f; return; }
            velocity += (rest - line.x) * pull * Time.deltaTime;
            velocity *= 1f - damping * Time.deltaTime;
            line.x = Mathf.Clamp(line.x + velocity * Time.deltaTime, line.minX, line.maxX);
        }
    }
}
