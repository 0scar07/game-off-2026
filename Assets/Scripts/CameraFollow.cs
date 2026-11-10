using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Cámara lateral que sigue al personaje con suavidad y mira un poco hacia donde va.
    /// Tiembla al aterrizar fuerte y se acerca despacio al llegar a la meta.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Rigidbody2D target;
        /// <summary>Segundo jugador de la carrera a dos (null si no hay).</summary>
        [System.NonSerialized] public Rigidbody2D second;

        public Vector2 offset = new Vector2(0f, 1.5f);
        [Tooltip("Cuánto se adelanta la cámara en la dirección del movimiento.")]
        public float lookAhead = 2f;
        public float smoothTime = 0.25f;
        [Tooltip("Límites del nivel para la cámara (x mínima/máxima).")]
        public float minX = -100f, maxX = 100f;
        [Tooltip("Distancia a la que se acerca en la meta (0 = no se acerca).")]
        public float goalZoom = 6f;

        Vector3 velocity;
        float lineBias, pull, appliedPull;
        Boss boss;
        bool bossSearched;
        float ahead, shake, zoom;

        void OnEnable()
        {
            GameEvents.Landed += OnLanded;
            GameEvents.GoalReached += OnGoal;
        }

        void OnDisable()
        {
            GameEvents.Landed -= OnLanded;
            GameEvents.GoalReached -= OnGoal;
        }

        void OnLanded(float fallSpeed)
        {
            if (fallSpeed > 12f) shake = Mathf.Max(shake, Mathf.InverseLerp(12f, 24f, fallSpeed) * 0.35f + 0.08f);
        }

        void OnGoal() => zoom = goalZoom;

        void LateUpdate()
        {
            if (target == null) return;
            ahead = Mathf.Lerp(ahead, Mathf.Sign(target.linearVelocity.x) * Mathf.Min(Mathf.Abs(target.linearVelocity.x), 1f) * lookAhead, Time.deltaTime * 2f);
            // En la carrera a dos, la cámara mira entre los dos (y se aleja si se separan).
            Vector2 focus = second != null ? (target.position + second.position) * 0.5f : target.position;
            Vector3 goal = focus + offset + Vector2.right * ahead;
            // Mientras se mueve la línea, la cámara mira un poco hacia ella (donde está la acción).
            var line = ShiftLine.Instance;
            float toLine = 0f;
            if (line != null && ShiftLine.Speed > 0.5f && line.angle < 45f)
                toLine = Mathf.Clamp((line.PointOnLine(line.x).x - target.position.x) * 0.3f, -4f, 4f);
            lineBias = Mathf.Lerp(lineBias, toLine, Time.deltaTime * 2f);
            goal.x = Mathf.Clamp(goal.x + lineBias, minX, maxX);
            goal.z = transform.position.z;
            // En los momentos grandes (un jefe en pie) la cámara se aleja para que quepa todo.
            if (boss == null && !bossSearched) { boss = FindAnyObjectByType<Boss>(); bossSearched = true; }
            float wantedPull = boss != null && boss.isActiveAndEnabled && !boss.Defeated ? 5f : 0f;
            pull = Mathf.Lerp(pull, wantedPull, Time.deltaTime * 1.5f);
            goal.y += pull * 0.35f;
            Vector3 position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
            // La z no se suaviza (la cambia también el acercamiento de la meta): se suma solo lo nuevo.
            position.z -= pull - appliedPull;
            appliedPull = pull;

            // Acercarse a la meta: avanzar en z poco a poco.
            if (zoom > 0f)
            {
                float step = Mathf.Min(zoom, Time.deltaTime * 3f);
                zoom -= step;
                position.z += step;
            }
            transform.position = position;

            if (shake > 0f && !Settings.ReduceEffects)
            {
                transform.position += (Vector3)(Random.insideUnitCircle * shake);
                shake = Mathf.Max(0f, shake - Time.deltaTime * 1.5f);
            }
        }
    }
}
