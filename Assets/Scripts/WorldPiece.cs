using UnityEngine;

namespace Fase
{
    public enum World { Day, Night }

    /// <summary>
    /// Algo que solo existe en un mundo (un puente de día, una estrella de noche...).
    /// Recorta su BoxCollider2D cada paso de física para que solo sea sólido el trozo
    /// que queda en el lado de su mundo. El dibujo lo recorta el shader con _ShiftX
    /// (ver FaseLit.shader), así que aquí solo se toca la colisión.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class WorldPiece : MonoBehaviour
    {
        public World world = World.Day;

        BoxCollider2D box;
        Vector2 fullSize, fullOffset;
        bool clippedOnce;

        void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            fullSize = box.size;
            fullOffset = box.offset;
        }

        void FixedUpdate() => Clip();

        /// <summary>Ajusta la colisión al trozo visible. Público para que el nivel lo fuerce al cargar.</summary>
        public void Clip()
        {
            float scale = transform.lossyScale.x;
            float center = transform.position.x + fullOffset.x * scale;
            float half = fullSize.x * 0.5f * Mathf.Abs(scale);
            float left = center - half, right = center + half;

            // Trozos del mundo de esta pieza entre las líneas; con dos líneas puede haber dos
            // trozos sueltos y la colisión se queda con el más grande.
            var lines = ShiftLine.Lines;
            float bestLeft = 0f, bestRight = 0f;
            float regionStart = float.NegativeInfinity;
            for (int i = 0; i <= lines.Length; i++)
            {
                float regionEnd = i < lines.Length ? lines[i] : float.PositiveInfinity;
                float probe = float.IsNegativeInfinity(regionStart) ? regionEnd - 1f : float.IsPositiveInfinity(regionEnd) ? regionStart + 1f : (regionStart + regionEnd) * 0.5f;
                bool day = ShiftLine.IsDay(probe);
                if (day == (world == World.Day))
                {
                    float a = Mathf.Max(left, regionStart), b = Mathf.Min(right, regionEnd);
                    if (b - a > bestRight - bestLeft) { bestLeft = a; bestRight = b; }
                }
                regionStart = regionEnd;
            }
            left = bestLeft;
            right = bestRight;

            bool exists = right - left > 0.01f;
            if (exists != box.enabled && clippedOnce)
            {
                if (exists) GameEvents.RaisePieceAppeared(transform.position, world);
                else GameEvents.RaisePieceVanished(transform.position, world);
            }
            clippedOnce = true;
            box.enabled = exists;
            if (!exists) return;
            box.size = new Vector2((right - left) / Mathf.Abs(scale), fullSize.y);
            box.offset = new Vector2(((left + right) * 0.5f - transform.position.x) / scale, fullOffset.y);
        }
    }
}
