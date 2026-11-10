using UnityEngine;

namespace Fase
{
    /// <summary>Aplica los mutadores activos al empezar un nivel (lo añade LevelLife).</summary>
    public class MutatorApplier : MonoBehaviour
    {
        void Start()
        {
            if (!Mutators.Unlocked || !Mutators.Any) return;
            var player = FindAnyObjectByType<PlayerController>();
            var line = ShiftLine.Instance;
            if (player != null && Mutators.On("gravedad")) player.GetComponent<Rigidbody2D>().gravityScale *= 0.6f;
            if (player != null && Mutators.On("gigante")) player.transform.localScale *= 1.4f;
            if (line != null && Mutators.On("invertida")) line.dayOnLeft = !line.dayOnLeft;
            if (Mutators.On("oscuras") && FindAnyObjectByType<DarkLevel>() == null) gameObject.AddComponent<DarkLevel>();
        }
    }
}
