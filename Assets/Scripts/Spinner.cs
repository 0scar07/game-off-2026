using UnityEngine;

namespace Fase
{
    /// <summary>Gira sin parar (sierras) y, opcionalmente, va y viene entre dos puntos.</summary>
    public class Spinner : MonoBehaviour
    {
        public float degreesPerSecond = -280f;
        public Transform spinning;
        public Vector3 travel;
        public float travelSpeed = 0.5f;

        Vector3 start;

        void Start() => start = transform.position;

        void Update()
        {
            if (spinning != null) spinning.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.World);
            if (travel != Vector3.zero)
                transform.position = start + travel * (0.5f - 0.5f * Mathf.Cos(Time.time * travelSpeed * Mathf.PI));
        }
    }
}
