using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Bufanda de Lumi: una cadena de puntos con física sencilla (Verlet) que cuelga del cuello,
    /// se queda atrás al correr y ondea con el viento. Se dibuja con un LineRenderer.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class Scarf : MonoBehaviour
    {
        public Transform anchor;
        public int segments = 9;
        public float segmentLength = 0.13f;
        public Vector3 gravity = new Vector3(0f, -6f, 0f);
        public float wind = 1.6f;
        [Range(0.8f, 1f)] public float damping = 0.94f;

        Vector3[] points, previous;
        LineRenderer line;

        void Start()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = segments;
            line.useWorldSpace = true;
            points = new Vector3[segments];
            previous = new Vector3[segments];
            for (int i = 0; i < segments; i++)
                points[i] = previous[i] = anchor.position + Vector3.down * segmentLength * i;
        }

        void LateUpdate()
        {
            if (anchor == null) return;
            float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
            // Ondea: el viento empuja hacia atrás y hacia arriba de forma irregular.
            Vector3 gust = new Vector3(-wind * (0.6f + 0.4f * Mathf.Sin(Time.time * 3.1f)), wind * 0.35f * Mathf.Sin(Time.time * 5.3f), 0f);
            points[0] = anchor.position;
            for (int i = 1; i < segments; i++)
            {
                Vector3 velocity = (points[i] - previous[i]) * damping;
                previous[i] = points[i];
                points[i] += velocity + (gravity + gust * (i / (float)segments)) * dt * dt;
                points[i].z = anchor.position.z + 0.15f;
            }
            // Mantener la longitud de cada tramo.
            for (int iteration = 0; iteration < 4; iteration++)
                for (int i = 1; i < segments; i++)
                {
                    Vector3 delta = points[i] - points[i - 1];
                    float distance = delta.magnitude;
                    if (distance > 1e-5f) points[i] = points[i - 1] + delta / distance * segmentLength;
                }
            line.SetPositions(points);
        }
    }
}
