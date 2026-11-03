using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Corriente de aire: de día empuja hacia arriba a quien está dentro (Lumi sube como una
    /// hoja); de noche hay calma. Las partículas de la corriente se apagan en el lado de noche
    /// gracias al shader de partículas.
    /// </summary>
    public class AirCurrent : MonoBehaviour
    {
        public Vector2 size = new Vector2(2f, 6f);
        public float lift = 55f;

        static readonly List<AirCurrent> zones = new List<AirCurrent>();

        void OnEnable() => zones.Add(this);
        void OnDisable() => zones.Remove(this);

        /// <summary>Aceleración hacia arriba en `position` (0 fuera de las corrientes o de noche).</summary>
        public static float Lift(Vector2 position)
        {
            float total = 0f;
            foreach (var zone in zones)
            {
                Vector2 c = zone.transform.position;
                if (Mathf.Abs(position.x - c.x) > zone.size.x * 0.5f || position.y < c.y - zone.size.y * 0.5f || position.y > c.y + zone.size.y * 0.5f) continue;
                if (!ShiftLine.IsDay(position)) continue;
                // Más fuerza abajo que arriba: Lumi flota cerca del techo de la corriente.
                float height = Mathf.InverseLerp(c.y - zone.size.y * 0.5f, c.y + zone.size.y * 0.5f, position.y);
                total += zone.lift * (1.2f - height);
            }
            return total;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.4f);
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
