using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Interfaz del nivel: al empezar muestra el nombre y la pista en grande y luego los deja
    /// pequeños; arriba a la derecha cuenta los fragmentos de luna; abajo, el cronómetro; al llegar
    /// a la meta anuncia "¡Nivel completado!" (y si es récord). Los textos los pone el generador.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        public static Hud Instance { get; private set; }

        public int levelNumber = 1;
        public string levelName = "", levelNameEn = "";
        [TextArea] public string hint = "", hintEn = "";
        public int fragmentsTotal;

        public Text title;
        public Text hintText;
        public Text corner;
        public Text counter;
        public Text timer;
        public Text banner;

        int fragments;
        float age, elapsed;
        bool finished;

        public int Fragments => fragments;
        public float Elapsed => elapsed;

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
            string name = $"{levelNumber}. {Loc.Pick(levelName, levelNameEn)}";
            title.text = name;
            hintText.text = Loc.Pick(hint, hintEn);
            corner.text = name;
            banner.text = "";
            UpdateCounter();
        }

        void Update()
        {
            age += Time.deltaTime;
            if (!finished) elapsed += Time.deltaTime;
            // El título se ve 2,5 s y se desvanece; la pista se queda un poco más.
            SetAlpha(title, Fade(age, 2.5f, 0.8f));
            SetAlpha(hintText, Fade(age, 6f, 1f));
            SetAlpha(corner, 1f - Fade(age, 2.5f, 0.8f));
            if (timer != null) timer.text = FormatTime(elapsed);
        }

        public static string FormatTime(float seconds) =>
            seconds <= 0f ? "--:--.--" : $"{(int)(seconds / 60f)}:{seconds % 60f:00.00}";

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
            if (counter != null) counter.transform.localScale = Vector3.one * 1.35f;
        }

        void LateUpdate()
        {
            if (counter != null) counter.transform.localScale = Vector3.Lerp(counter.transform.localScale, Vector3.one, Time.deltaTime * 8f);
        }

        void UpdateCounter()
        {
            counter.text = fragmentsTotal > 0 ? $"{Loc.Get("moon")} {fragments}/{fragmentsTotal}" : "";
        }

        void OnGoal()
        {
            finished = true;
            bool record = Progress.SaveTime(levelNumber, elapsed);
            string text = Loc.Get("complete");
            if (fragmentsTotal > 0 && fragments == fragmentsTotal) text += "\n" + Loc.Get("allmoon");
            if (record) text += "\n" + Loc.Get("record") + "  " + FormatTime(elapsed);
            banner.text = text;
        }
    }
}
