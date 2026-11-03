using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Sombra: criatura de la noche. Despierta (en el lado de noche) patrulla y hace perder si te
    /// toca; dormida (en el lado de día) se convierte en piedra y se queda quieta.
    /// Las de suelo se dan la vuelta ante una pared o un borde; las voladoras van y vienen en el aire.
    /// </summary>
    public class Monster : MonoBehaviour
    {
        public float speed = 2.2f;
        public float halfWidth = 0.45f;
        public float height = 1f;
        public LayerMask groundMask = ~0;
        [Header("Voladora")]
        public bool flying;
        public float flyRange = 3f;

        [Header("Dibujo")]
        [Tooltip("Modelo animado de noche (y el que gira hacia donde va).")]
        public Transform visual;
        public Animator animator;
        [Tooltip("Estatua de piedra que se ve de día.")]
        public Transform statue;

        float direction = 1f;
        float startX;
        bool wasAwake = true;
        float playerSide;
        PlayerController player;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            startX = transform.position.x;
        }

        void Update()
        {
            bool awake = !ShiftLine.IsDay(transform.position.x);
            if (awake) { if (flying) Fly(); else Patrol(); }
            if (awake != wasAwake && animator != null) animator.speed = awake ? 1f : 0f;
            wasAwake = awake;

            // Igual que Lumi: con giro 0 miran a la cámara, así que la derecha es -90.
            var facing = Quaternion.Euler(0f, -direction * 90f, 0f);
            if (visual != null) visual.localRotation = Quaternion.Slerp(visual.localRotation, facing, Time.deltaTime * 10f);
            if (statue != null) statue.localRotation = visual != null ? visual.localRotation : facing;
            if (awake && player != null) CheckHit();

            // Lumi pasó al lado de esta Sombra mientras dormía (logro "Piedra, papel...").
            if (player != null)
            {
                float side = Mathf.Sign(player.transform.position.x - transform.position.x);
                if (!awake && playerSide != 0f && side != playerSide && Mathf.Abs(player.transform.position.y - transform.position.y) < 3f)
                    Achievements.PassedSleepingShadow();
                playerSide = side;
            }
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

        void Fly()
        {
            var p = transform.position;
            p.x += direction * speed * Time.deltaTime;
            if (p.x > startX + flyRange) direction = -1f;
            if (p.x < startX - flyRange) direction = 1f;
            transform.position = p;
            if (visual != null) visual.localPosition = Vector3.up * Mathf.Sin(Time.time * 3f) * 0.25f;
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
