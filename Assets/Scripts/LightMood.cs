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
        const float CloudTime = 7f;
        float cloudWait = 15f, cloudLeft;


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
            // De vez en cuando pasa una nube por delante del sol: el día se apaga un poco unos segundos.
            cloudWait -= Time.deltaTime;
            if (cloudWait <= 0f && cloudLeft <= 0f) { cloudLeft = CloudTime; cloudWait = Random.Range(22f, 40f); }
            float cloud = 0f;
            if (cloudLeft > 0f) { cloudLeft -= Time.deltaTime; cloud = Mathf.Sin(Mathf.PI * (1f - cloudLeft / CloudTime)); }
            sun.intensity = Mathf.Lerp(sunIntensity * (1f - 0.35f * cloud), moonIntensity, k);

            transform.rotation = Quaternion.Slerp(dayRotation, nightRotation, k);
        }
    }
}
