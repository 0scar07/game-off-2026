using UnityEngine;

namespace Fase
{
    /// <summary>Cámara lateral que sigue al personaje con suavidad y mira un poco hacia donde va.</summary>
    public class CameraFollow : MonoBehaviour
    {
        public Rigidbody2D target;
        public Vector2 offset = new Vector2(0f, 1.5f);
        [Tooltip("Cuánto se adelanta la cámara en la dirección del movimiento.")]
        public float lookAhead = 2f;
        public float smoothTime = 0.25f;
        [Tooltip("Límites del nivel para la cámara (x mínima/máxima).")]
        public float minX = -100f, maxX = 100f;

        Vector3 velocity;
        float ahead;

        void LateUpdate()
        {
            if (target == null) return;
            ahead = Mathf.Lerp(ahead, Mathf.Sign(target.linearVelocity.x) * Mathf.Min(Mathf.Abs(target.linearVelocity.x), 1f) * lookAhead, Time.deltaTime * 2f);
            Vector3 goal = target.position + offset + Vector2.right * ahead;
            goal.x = Mathf.Clamp(goal.x, minX, maxX);
            goal.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        }
    }
}
