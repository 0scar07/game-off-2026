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

        // Al cruzar la línea: un zumbido agudo y suave al entrar en el día, uno grave al entrar en la noche.
        Transform player;
        bool wasDay = true, known;
        float searchIn;


        void Update()
        {
            searchIn -= Time.unscaledDeltaTime;
            if (player == null && searchIn <= 0f) { searchIn = 1f; var hero = FindAnyObjectByType<PlayerController>(); if (hero != null) { player = hero.transform; known = false; } }
            if (player != null)
            {
                bool day = ShiftLine.IsDay(player.position);
                if (known && day != wasDay && Settings.Vibration) { if (day) Pulse(0.07f, 0.05f, 0.35f); else Pulse(0.12f, 0.45f, 0.05f); }
                wasDay = day;
                known = true;
            }
            if (left <= 0f) return;

            left -= Time.unscaledDeltaTime;
            if (left <= 0f) Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }
}
