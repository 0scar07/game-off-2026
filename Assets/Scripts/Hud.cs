using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Interfaz del nivel: al empezar muestra el nombre y la pista en grande y luego los deja
    /// pequeños; arriba a la derecha cuenta los fragmentos de luna; al llegar a la meta anuncia
    /// "¡Nivel completado!". Los textos los rellena el generador de niveles.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        public static Hud Instance { get; private set; }

        public int levelNumber = 1;
        public string levelName = "";
        [TextArea] public string hint = "";
        public int fragmentsTotal;

        public Text title;
        public Text hintText;
        public Text corner;
        public Text counter;
        public Text banner;

        int fragments;
        float age;

        public int Fragments => fragments;

        void Awake() => Instance = this;

        void OnEnable()
        {
            GameEvents.Collected += OnCollected;
            GameEvents.GoalReached += OnGoal;
        }

        void OnDisable()
        {
            GameEvents.Collected -= OnCollected;
            GameEvents.GoalReached -= OnGoal;
        }

        void Start()
        {
            title.text = levelName;
            hintText.text = hint;
            corner.text = levelName;
            banner.text = "";
            UpdateCounter();
        }

        void Update()
        {
            age += Time.deltaTime;
            // El título se ve 2,5 s y se desvanece; la pista se queda un poco más.
            SetAlpha(title, Fade(age, 2.5f, 0.8f));
            SetAlpha(hintText, Fade(age, 6f, 1f));
            SetAlpha(corner, 1f - Fade(age, 2.5f, 0.8f));
        }

        static float Fade(float t, float hold, float duration) => 1f - Mathf.Clamp01((t - hold) / duration);

        static void SetAlpha(Text text, float alpha)
        {
            if (text == null) return;
            var c = text.color;
            c.a = alpha;
            text.color = c;
        }

        void OnCollected()
        {
            fragments++;
            UpdateCounter();
        }

        void UpdateCounter()
        {
            counter.text = fragmentsTotal > 0 ? $"Luna {fragments}/{fragmentsTotal}" : "";
        }

        void OnGoal()
        {
            banner.text = fragmentsTotal > 0 && fragments == fragmentsTotal
                ? "¡Nivel completado!\nCon toda la luna"
                : "¡Nivel completado!";
        }
    }
}
