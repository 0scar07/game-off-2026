using UnityEngine;

namespace Fase
{
    /// <summary>Punto de control: al pasar por él, si el personaje cae vuelve aquí.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip("Dibujo que se ilumina/levanta al activarse.")]
        public Transform visual;

        bool reached;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null || reached) return;
            reached = true;
            player.SetCheckpoint(transform.position + Vector3.up * 0.5f);
            if (visual != null) visual.localScale *= 1.25f;
            GameEvents.RaiseCheckpoint();
        }
    }
}
