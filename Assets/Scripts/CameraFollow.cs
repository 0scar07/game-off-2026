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
        public Vector2 offset = new Vector2(0f, 1.5f);
        [Tooltip("Cuánto se adelanta la cámara en la dirección del movimiento.")]
        public float lookAhead = 2f;
        public float smoothTime = 0.25f;
        [Tooltip("Límites del nivel para la cámara (x mínima/máxima).")]
        public float minX = -100f, maxX = 100f;
        [Tooltip("Distancia a la que se acerca en la meta (0 = no se acerca).")]
        public float goalZoom = 6f;

        Vector3 velocity;
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
            Vector3 goal = target.position + offset + Vector2.right * ahead;
            goal.x = Mathf.Clamp(goal.x, minX, maxX);
            goal.z = transform.position.z;
            Vector3 position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);

            // Acercarse a la meta: avanzar en z poco a poco.
            if (zoom > 0f)
            {
                float step = Mathf.Min(zoom, Time.deltaTime * 3f);
                zoom -= step;
                position.z += step;
            }
            transform.position = position;

            if (shake > 0f)
            {
                transform.position += (Vector3)(Random.insideUnitCircle * shake);
                shake = Mathf.Max(0f, shake - Time.deltaTime * 1.5f);
            }
        }
    }
}
