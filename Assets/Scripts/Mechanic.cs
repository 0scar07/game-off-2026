using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Base de las piezas de las mecánicas nuevas (atardecer, brotes, espejos, llaves...): el
    /// generador las deja fuera del "static batching" porque cambian o se mueven.
    /// </summary>
    public abstract class Mechanic : MonoBehaviour
    {
        protected static PlayerController Player { get { if (player == null) player = FindAnyObjectByType<PlayerController>(); return player; } }
        static PlayerController player;

        /// <summary>Distancia de un punto a la línea más cercana, medida a lo largo de su normal.</summary>
        protected static float DistanceToLine(Vector2 point)
        {
            var line = ShiftLine.Instance;
            if (line == null) return 999f;
            float d = Vector2.Dot(point, line.Normal), best = 999f;
            foreach (float o in ShiftLine.Lines) best = Mathf.Min(best, Mathf.Abs(d - o));
            return best;
        }
    }
}
