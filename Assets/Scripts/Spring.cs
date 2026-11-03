using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Flor-muelle: de día impulsa al personaje hacia arriba. De noche está cerrada (no existe).
    /// El dibujo se recorta con el shader; aquí solo se decide si impulsa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Spring : MonoBehaviour
    {
        public float launchSpeed = 19f;
        [Tooltip("Dibujo que se aplasta al rebotar.")]
        public Transform visual;

        float cooldown;
        Vector3 visualScale;

        void Awake()
        {
            if (visual != null) visualScale = visual.localScale;
        }

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void Update()
        {
            cooldown -= Time.deltaTime;
            if (visual != null)
                visual.localScale = Vector3.Lerp(visual.localScale, visualScale, Time.deltaTime * 10f);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (cooldown > 0f || !ShiftLine.IsDay(transform.position)) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null || other.attachedRigidbody.linearVelocity.y > 1f) return;
            player.Bounce(launchSpeed);
            cooldown = 0.3f;
            if (visual != null) visual.localScale = new Vector3(visualScale.x * 1.3f, visualScale.y * 0.6f, visualScale.z * 1.3f);
            GameEvents.RaiseBounced();
        }
    }
}
