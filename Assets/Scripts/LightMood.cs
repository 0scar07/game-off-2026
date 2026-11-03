using UnityEngine;

namespace Fase
{
    /// <summary>
    /// La luz principal sigue a Lumi: en el lado de día es un sol cálido que viene de la izquierda;
    /// al cruzar a la noche gira y se enfría como la luna, así que las sombras cambian de lado.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LightMood : MonoBehaviour
    {
        public Color moon = new Color(0.62f, 0.72f, 1f);
        public float moonIntensity = 0.9f;

        Light sun;
        Color sunColor;
        float sunIntensity, night;
        Quaternion dayRotation, nightRotation;
        Transform player;

        void Start()
        {
            sun = GetComponent<Light>();
            sunColor = sun.color;
            sunIntensity = sun.intensity;
            var e = transform.eulerAngles;
            dayRotation = transform.rotation;
            nightRotation = Quaternion.Euler(e.x, -e.y, e.z);
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
        }

        void LateUpdate()
        {
            if (player == null) return;
            float target = ShiftLine.IsDay(player.position) ? 0f : 1f;
            night = Mathf.MoveTowards(night, target, Time.deltaTime * 1.2f);
            float k = Mathf.SmoothStep(0f, 1f, night);
            sun.color = Color.Lerp(sunColor, moon, k);
            sun.intensity = Mathf.Lerp(sunIntensity, moonIntensity, k);
            transform.rotation = Quaternion.Slerp(dayRotation, nightRotation, k);
        }
    }
}
