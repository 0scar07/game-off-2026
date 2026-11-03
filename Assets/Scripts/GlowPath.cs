using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Camino de luciérnagas: losas escondidas que solo aparecen de noche y cuando el farol de
    /// Lumi está cerca. Las luciérnagas que revolotean encima marcan dónde están.
    /// </summary>
    public class GlowPath : MonoBehaviour
    {
        public float revealDistance = 4.5f;
        public Renderer[] renderers;
        public Collider2D solid;

        PlayerController player;
        float shown;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            Apply(0f);
        }

        void Update()
        {
            bool near = player != null && Vector2.Distance(player.transform.position, transform.position) < revealDistance;
            bool on = near && !ShiftLine.IsDay(transform.position);
            shown = Mathf.MoveTowards(shown, on ? 1f : 0f, Time.deltaTime * (on ? 4f : 1.5f));
            Apply(shown);
        }

        void Apply(float amount)
        {
            if (solid != null) solid.enabled = amount > 0.5f;
            transform.localScale = new Vector3(1f, Mathf.Lerp(0.05f, 1f, amount), 1f);
            foreach (var r in renderers) if (r != null) r.enabled = amount > 0.02f;
        }
    }
}
