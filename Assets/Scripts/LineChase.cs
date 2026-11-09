using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Persecución (cabecera "persecucion: velocidad"): la noche avanza sola desde la izquierda y
    /// no se puede parar. Si alcanza a Lumi un momento, vuelve al punto de control (y la noche
    /// retrocede un poco para dar margen).
    /// </summary>
    public class LineChase : MonoBehaviour
    {
        public float speed = 2.5f;
        float caught;

        void Start()
        {
            var line = ShiftLine.Instance;
            if (line == null) { enabled = false; return; }
            line.locked = true;
            line.dayOnLeft = false;   // la noche queda detrás (a la izquierda) y avanza
            GameEvents.Died += OnDied;
        }

        void OnDestroy() => GameEvents.Died -= OnDied;

        void OnDied()
        {
            var line = ShiftLine.Instance;
            var player = FindAnyObjectByType<PlayerController>();
            if (line != null && player != null) line.x = Mathf.Max(line.minX, player.transform.position.x - 7f);
            caught = 0f;
        }

        void Update()
        {
            var line = ShiftLine.Instance;
            var player = FindAnyObjectByType<PlayerController>();
            if (line == null || player == null || PauseMenu.Paused) return;
            line.x = Mathf.Min(line.maxX, line.x + speed * Time.deltaTime);
            caught = ShiftLine.IsDay(player.transform.position) ? 0f : caught + Time.deltaTime;
            if (caught > 0.6f) player.Die();
        }
    }
}
