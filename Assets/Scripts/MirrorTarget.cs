using UnityEngine;

namespace Fase
{
    /// <summary>Diana de un espejo (casilla T): donde cae el reflejo se hace de día.</summary>
    public class MirrorTarget : Mechanic
    {
        void Start() => Shapes.Glow(transform, "Diana", transform.position + Vector3.back * 0.6f, new Vector2(0.9f, 0.9f), new Color(1f, 0.9f, 0.6f, 0.35f));
    }
}
