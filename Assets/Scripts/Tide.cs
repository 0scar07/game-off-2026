using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Marea (cabecera "marea: casillas"): un agua que sube cuanta más pantalla es de noche y
    /// baja con el día. Tocarla hace perder. Hay que dejar el día suficiente para pasar por abajo.
    /// </summary>
    public class Tide : MonoBehaviour
    {
        public float rise = 3f;
        public float baseY = 2.4f;
        public float minX, maxX;
        float level;
        Transform water, surface;

        void Start()
        {
            float width = maxX - minX + 40f, center = (minX + maxX) * 0.5f;
            water = Shapes.Glow(transform, "Agua", new Vector3(center, baseY - 10f, -0.9f), new Vector2(width, 20f), new Color(0.25f, 0.45f, 0.95f, 0.55f), 2f, false).transform;
            surface = Shapes.Glow(transform, "Superficie", new Vector3(center, baseY, -0.95f), new Vector2(width, 0.12f), new Color(0.75f, 0.88f, 1f, 0.9f), 2f, false).transform;
        }

        void Update()
        {
            var line = ShiftLine.Instance;
            float night = 0f;
            if (line != null)
            {
                float screenX = Mathf.Clamp01(line.ScreenX());
                night = line.dayOnLeft ? 1f - screenX : screenX;
            }
            level = Mathf.MoveTowards(level, night * rise, Time.deltaTime * 1.2f);
            float top = baseY + level;
            water.position = new Vector3(water.position.x, top - 10f, water.position.z);
            surface.position = new Vector3(surface.position.x, top + Mathf.Sin(Time.time * 2f) * 0.04f, surface.position.z);
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null && player.transform.position.y - 0.7f < top) player.Die();
        }
    }
}
