using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Una zona redonda de un mundo, por encima de lo que diga la línea (pero por debajo de la
    /// burbuja del farol): el día que crea un espejo o la noche que dibuja la línea curva. Solo hay
    /// una a la vez; la pinta el shader (_Zone) y la respetan las piezas y ShiftLine.IsDay.
    /// </summary>
    public static class WorldZone
    {
        public static bool Active { get; private set; }
        public static Vector2 Center { get; private set; }
        public static float Radius { get; private set; }
        public static bool Night { get; private set; }

        public static void Set(Vector2 center, float radius, bool night)
        {
            Active = radius > 0.01f;
            Center = center;
            Radius = radius;
            Night = night;
        }

        public static void Clear() => Active = false;

        public static bool Contains(Vector2 point) => Active && (point - Center).sqrMagnitude < Radius * Radius;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Reset() => UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) => Clear();
    }
}
