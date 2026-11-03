using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Velocidad del juego: la normal (algo más lenta en modo asistido), la pausa y la cámara
    /// lenta de medio segundo al coger un fragmento o salvarse por poco.
    /// </summary>
    public class TimeControl : MonoBehaviour
    {
        static TimeControl instance;
        float slowLeft, slowScale = 1f;

        public static float BaseSpeed => Settings.Assist ? 0.85f : 1f;

        static TimeControl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("Tiempo").AddComponent<TimeControl>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }

        /// <summary>Vuelve a la velocidad normal (al cargar escena o quitar la pausa).</summary>
        public static void Normal()
        {
            if (instance != null) instance.slowLeft = 0f;
            Time.timeScale = PauseMenu.Paused ? 0f : BaseSpeed;
        }

        /// <summary>Cámara lenta breve. Se ignora si el juego está en pausa.</summary>
        public static void SlowMotion(float seconds = 0.5f, float scale = 0.3f)
        {
            if (PauseMenu.Paused) return;
            var t = Instance;
            t.slowLeft = seconds;
            t.slowScale = scale;
        }

        void Update()
        {
            if (PauseMenu.Paused || slowLeft <= 0f) return;
            slowLeft -= Time.unscaledDeltaTime;
            // Entra de golpe y sale suave en el último tercio.
            float k = Mathf.Clamp01(slowLeft / 0.18f);
            Time.timeScale = Mathf.Lerp(BaseSpeed, slowScale * BaseSpeed, k);
            if (slowLeft <= 0f) Time.timeScale = BaseSpeed;
        }
    }
}
