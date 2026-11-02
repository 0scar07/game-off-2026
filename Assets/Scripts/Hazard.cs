using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Zona que hace perder (pinchos, agua...). Puede estar activa siempre o solo en un mundo:
    /// el agua del lago solo es peligrosa de día; de noche está helada y se puede pisar.
    /// Se mira el mundo en la posición del personaje.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        public enum When { Always, DayOnly, NightOnly }
        public When active = When.Always;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null || !IsActive(other.transform.position.x)) return;
            player.Die();
        }

        bool IsActive(float x) =>
            active == When.Always || (active == When.DayOnly) == ShiftLine.IsDay(x);
    }
}
