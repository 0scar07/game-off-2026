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
            float scaleX = transform.lossyScale.x, scaleY = transform.lossyScale.y;
            Vector2 center = (Vector2)transform.position + Vector2.Scale(fullOffset, new Vector2(scaleX, scaleY));
            Vector2 half = Vector2.Scale(fullSize, new Vector2(Mathf.Abs(scaleX), Mathf.Abs(scaleY))) * 0.5f;
            var line = ShiftLine.Instance;
            Vector2 n = line != null ? line.Normal : Vector2.right;
            // Se recorta a lo largo del eje que más se parece a la normal: x si la línea es casi
            // vertical, y si está casi tumbada.
            bool alongX = Mathf.Abs(n.x) >= Mathf.Abs(n.y);
            float left = alongX ? center.x - half.x : center.y - half.y;
            float right = alongX ? center.x + half.x : center.y + half.y;

            // Burbuja de noche del farol (o zona de un mundo): si cubre el centro de la pieza, manda ella.
            bool lantern = LanternPower.Contains(center);
            bool zone = !lantern && WorldZone.Contains(center);
            bool bubble = lantern || zone;
            if (bubble)
            {
                bool night = lantern || WorldZone.Night;
                if ((world == World.Day) == night) right = left;
            }

            else
            {
                // Trozos del mundo de esta pieza entre las líneas; con dos líneas puede haber
                // dos trozos sueltos y la colisión se queda con el más grande.
                var lines = ShiftLine.Lines;
                float bestLeft = 0f, bestRight = 0f;
                float regionStart = float.NegativeInfinity;
                for (int i = 0; i <= lines.Length; i++)
                {
                    // Posición de la línea en el eje de recorte, a la altura (o anchura) del centro.
                    float regionEnd = i < lines.Length ? AxisPosition(lines[i], n, center, alongX) : float.PositiveInfinity;
                    float probe = float.IsNegativeInfinity(regionStart) ? regionEnd - 1f : float.IsPositiveInfinity(regionEnd) ? regionStart + 1f : (regionStart + regionEnd) * 0.5f;
                    Vector2 probePoint = alongX ? new Vector2(probe, center.y) : new Vector2(center.x, probe);
                    if (ShiftLine.IsDay(probePoint) == (world == World.Day))
                    {
                        float lo = Mathf.Max(left, regionStart), hi = Mathf.Min(right, regionEnd);
                        if (hi - lo > bestRight - bestLeft) { bestLeft = lo; bestRight = hi; }
                    }
                    regionStart = regionEnd;
                }
                left = bestLeft;
                right = bestRight;
            }

            bool exists = right - left > 0.01f;
            if (exists != box.enabled && clippedOnce)
            {
                if (exists) GameEvents.RaisePieceAppeared(transform.position, world);
                else GameEvents.RaisePieceVanished(transform.position, world);
            }
            clippedOnce = true;
            box.enabled = exists;
            if (!exists) return;
            if (bubble) { box.size = fullSize; box.offset = fullOffset; return; }
            if (alongX)
            {
                box.size = new Vector2((right - left) / Mathf.Abs(scaleX), fullSize.y);
                box.offset = new Vector2(((left + right) * 0.5f - transform.position.x) / scaleX, fullOffset.y);
            }
            else
            {
                box.size = new Vector2(fullSize.x, (right - left) / Mathf.Abs(scaleY));
                box.offset = new Vector2(fullOffset.x, ((left + right) * 0.5f - transform.position.y) / scaleY);
            }
        }

        /// <summary>Dónde cruza la línea (offset a lo largo de la normal n) el eje de recorte que pasa por el centro.</summary>
        static float AxisPosition(float offset, Vector2 n, Vector2 center, bool alongX)
        {
            // n·p = offset  →  x = (offset - n.y·y) / n.x   o   y = (offset - n.x·x) / n.y
            return alongX ? (offset - n.y * center.y) / n.x : (offset - n.x * center.x) / n.y;
        }
    }
}
