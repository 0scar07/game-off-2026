using UnityEngine;

namespace Fase
{
    /// <summary>
    /// ¿Está Lumi bajo techo (algo sólido encima y paredes cerca)? Entonces GameAudio repite cada
    /// sonido más bajo unas décimas después: un eco de cueva. (La web no admite filtros de audio
    /// como la reverberación, así que se hace a mano.)
    /// </summary>
    public class Enclosure : MonoBehaviour
    {
        public static bool Active { get; private set; }

        Transform player;
        float wait;

        void Start()
        {
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
        }

        void OnDestroy() => Active = false;

        void Update()
        {
            wait -= Time.deltaTime;
            if (wait > 0f || player == null) return;
            wait = 0.25f;
            Vector2 head = (Vector2)player.position + Vector2.up * 0.9f;
            bool roof = Solid(Physics2D.Raycast(head, Vector2.up, 4f));
            bool left = Solid(Physics2D.Raycast(head, Vector2.left, 7f));
            bool right = Solid(Physics2D.Raycast(head, Vector2.right, 7f));
            Active = roof && (left || right);
        }

        static bool Solid(RaycastHit2D hit) => hit.collider != null && !hit.collider.isTrigger && hit.collider.GetComponent<PlayerController>() == null;
    }
}
