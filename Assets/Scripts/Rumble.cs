using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>Vibración del mando: pulsos cortos al aterrizar fuerte, coger luna o vencer.</summary>
    public class Rumble : MonoBehaviour
    {
        static Rumble instance;
        float left;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (instance != null) return;
            instance = new GameObject("Vibracion").AddComponent<Rumble>();
            DontDestroyOnLoad(instance.gameObject);
        }

        public static void Pulse(float seconds, float low = 0.25f, float high = 0.6f)
        {
            var pad = Gamepad.current;
            if (pad == null || instance == null) return;
            pad.SetMotorSpeeds(low, high);
            instance.left = Mathf.Max(instance.left, seconds);
        }

        void OnEnable()
        {
            GameEvents.Landed += OnLanded;
            GameEvents.Collected += OnCollected;
        }

        void OnDisable()
        {
            GameEvents.Landed -= OnLanded;
            GameEvents.Collected -= OnCollected;
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }

        void OnLanded(float speed) { if (speed > 14f && Settings.Vibration) Pulse(0.08f, 0.4f, 0.2f); }
        void OnCollected() { if (Settings.Vibration) Pulse(0.1f, 0.1f, 0.5f); }

        void Update()
        {
            if (left <= 0f) return;
            left -= Time.unscaledDeltaTime;
            if (left <= 0f) Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }
}
