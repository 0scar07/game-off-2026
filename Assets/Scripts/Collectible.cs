using UnityEngine;

namespace Fase
{
    /// <summary>Fragmento de luna: gira, flota y se recoge al tocarlo. Existe en los dos mundos.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        public float spinSpeed = 120f;
        public float bobHeight = 0.15f;

        Vector3 start;

        void Awake() => start = transform.position;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            transform.position = start + Vector3.up * Mathf.Sin(Time.time * 2.5f) * bobHeight;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            GameEvents.RaiseCollected();
            Destroy(gameObject);
        }
    }
}
