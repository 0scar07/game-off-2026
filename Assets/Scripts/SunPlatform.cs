using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Reloj de sol mecánico: plataforma que va y viene mientras le da el día y se para en seco
    /// de noche (justo al revés que las Sombras). Lumi viaja encima (ver PlayerController).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SunPlatform : MonoBehaviour
    {
        public float range = 3f;
        public float speed = 2f;
        [Tooltip("Engranaje que gira mientras se mueve.")]
        public Transform gear;

        Rigidbody2D body;
        float startX, direction = 1f;

        public Vector2 Velocity { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            startX = body.position.x;
        }

        void FixedUpdate()
        {
            bool day = ShiftLine.IsDay(body.position);
            float step = day ? direction * speed : 0f;
            Vector2 next = body.position + Vector2.right * step * Time.fixedDeltaTime;
            if (next.x > startX + range) { next.x = startX + range; direction = -1f; }
            if (next.x < startX - range) { next.x = startX - range; direction = 1f; }
            Velocity = (next - body.position) / Time.fixedDeltaTime;
            body.MovePosition(next);
            if (gear != null) gear.Rotate(0f, 0f, -Velocity.x * 120f * Time.fixedDeltaTime, Space.Self);
        }
    }
}
