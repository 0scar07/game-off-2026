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

        float stopLeft;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            _ = Instance;
            GameEvents.Collected += () => HitStop(0.06f);
            GameEvents.Landed += speed => { if (speed > 16f) HitStop(0.035f); };
        }

        /// <summary>
        /// Micro-pausa al impactar (unos fotogramas congelados): da peso a coger luna, golpear al
        /// jefe o aterrizar fuerte. Nada si el juego está en pausa o en el modo foto.
        /// </summary>
        public static void HitStop(float seconds)
        {
            if (PauseMenu.Paused || PhotoMode.Active || Settings.ReduceEffects) return;
            var t = Instance;
            t.stopLeft = Mathf.Max(t.stopLeft, seconds);
            Time.timeScale = 0f;
        }

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
            if (instance != null) instance.slowLeft = instance.stopLeft = 0f;
            Time.timeScale = PauseMenu.Paused ? 0f : BaseSpeed;
        }

        /// <summary>Cámara lenta breve. Se ignora si el juego está en pausa.</summary>
        public static void SlowMotion(float seconds = 0.5f, float scale = 0.3f)
        {
            if (PauseMenu.Paused || PhotoMode.Active) return;
            var t = Instance;
            t.slowLeft = seconds;
            t.slowScale = scale;
        }

        void Update()
        {
            // En pausa o en el modo foto el juego está congelado: la cámara lenta no lo toca.
            if (PauseMenu.Paused || PhotoMode.Active) return;
            if (stopLeft > 0f)
            {
                stopLeft -= Time.unscaledDeltaTime;
                if (stopLeft > 0f) return;
                Time.timeScale = slowLeft > 0f ? slowScale * BaseSpeed : BaseSpeed;
            }
            if (slowLeft <= 0f) return;
            slowLeft -= Time.unscaledDeltaTime;
            // Entra de golpe y sale suave en el último tercio.
            float k = Mathf.Clamp01(slowLeft / 0.18f);
            Time.timeScale = Mathf.Lerp(BaseSpeed, slowScale * BaseSpeed, k);
            if (slowLeft <= 0f) Time.timeScale = BaseSpeed;
        }
    }
}
