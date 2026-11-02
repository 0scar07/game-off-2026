using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Criatura de la noche: patrulla de un lado a otro y hace perder si te toca. En el lado de día
    /// no existe (se duerme): no se ve (shader) y no hace daño. Se da la vuelta ante una pared o
    /// un borde.
    /// </summary>
    public class Monster : MonoBehaviour
    {
        public float speed = 2.2f;
        public float halfWidth = 0.45f;
        public float height = 1f;
        public LayerMask groundMask = ~0;
        public Transform visual;

        float direction = 1f;
        PlayerController player;

        void Start() => player = FindAnyObjectByType<PlayerController>();

        void Update()
        {
            bool awake = !ShiftLine.IsDay(transform.position.x);
            if (awake) Patrol();
            if (visual != null)
                visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.Euler(0f, direction * 90f, 0f), Time.deltaTime * 10f);
            if (awake && player != null) CheckHit();
        }

        void Patrol()
        {
            Vector2 position = transform.position;
            Vector2 front = position + new Vector2(direction * (halfWidth + 0.05f), 0f);
            bool wall = Blocked(Physics2D.RaycastAll(front + Vector2.up * 0.3f, Vector2.right * direction, 0.1f, groundMask));
            bool floor = Blocked(Physics2D.RaycastAll(front + Vector2.up * 0.1f, Vector2.down, 0.4f, groundMask));
            if (wall || !floor) direction = -direction;
            transform.position += Vector3.right * direction * speed * Time.deltaTime;
        }

        static bool Blocked(RaycastHit2D[] hits)
        {
            foreach (var hit in hits)
                if (!hit.collider.isTrigger && hit.collider.attachedRigidbody == null) return true;
            return false;
        }

        void CheckHit()
        {
            Vector2 delta = player.transform.position - (transform.position + Vector3.up * height * 0.5f);
            if (Mathf.Abs(delta.x) < halfWidth + 0.35f && Mathf.Abs(delta.y) < height * 0.5f + 0.8f)
                player.Die();
        }
    }
}
