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
        [Tooltip("Tiempos de medalla en segundos: bronce, plata, oro, luna.")]
        public float[] medalTimes = new float[4];
        [Tooltip("Nivel del desafío diario (no guarda récords ni medallas).")]
        public bool daily;
        [Tooltip("Taller o modo sin fin: sin récords, medallas ni número de nivel.")]
        public bool sandbox;

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
        public bool IsRecord { get; private set; }
        /// <summary>Caídas y agarres de la línea en este intento (para los desafíos).</summary>
        public int Deaths { get; private set; }
        public int Grabs { get; private set; }
        int deathStreak;
        public bool Daily => daily;
        /// <summary>El nivel se está jugando en modo espejo (el mundo dado la vuelta).</summary>
        public bool Mirror => MirrorMode.Active;

        void Awake() => Instance = this;

        void OnEnable()
        {
            GameEvents.Collected += OnCollected;
            GameEvents.GoalReached += OnGoal;
            GameEvents.Died += OnDied;
            GameEvents.LineGrabbed += OnGrab;
            GameEvents.CheckpointReached += OnCheckpoint;
        }

        void OnDisable()
        {
            GameEvents.Collected -= OnCollected;
            GameEvents.GoalReached -= OnGoal;
            GameEvents.Died -= OnDied;
            GameEvents.LineGrabbed -= OnGrab;
            GameEvents.CheckpointReached -= OnCheckpoint;
        }

        static readonly (string es, string en)[] Cheers =
        {
            ("¡Casi!", "So close!"), ("¡Tú puedes!", "You've got this!"), ("Prueba a mover la línea antes", "Try moving the line first"),
            ("La próxima sale", "Next one's it"), ("Respira... y otra vez", "Breathe... and again"),
        };

        void OnDied()
        {
            Deaths++;
            deathStreak++;
            Stats.AddFall();
            // Tras varias caídas seguidas, un ánimo pequeño en el centro.
            if (deathStreak >= 3 && banner != null)
            {
                var cheer = Cheers[Random.Range(0, Cheers.Length)];
                banner.text = Loc.Pick(cheer.es, cheer.en);
                cheerLeft = 2f;
                deathStreak = 0;
            }
        }

        void OnCheckpoint()
        {
            deathStreak = 0;
            // Si se cierra a mitad del nivel, se sigue desde aquí (y con el tiempo que llevaba).
            var player = FindAnyObjectByType<PlayerController>();
            if (!sandbox && !daily && player != null) SaveGuard.SaveCheckpoint(levelNumber, player.transform.position, elapsed);
        }

        /// <summary>Al empezar: si quedó un punto de control guardado, Lumi aparece allí.</summary>
        void Resume()
        {
            if (sandbox || daily || !SaveGuard.TryLoadCheckpoint(levelNumber, out var at, out float time)) return;
            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) return;
            player.transform.position = at;
            player.GetComponent<Rigidbody2D>().position = at;
            player.SetCheckpoint(at);
            elapsed = time;
            resumed = true;
        }

        bool resumed;

        void OnGrab()
        {
            Grabs++;
            Stats.AddLineMove();
        }

        float cheerLeft;

        void Start()
        {
            Resume();
            string name = sandbox ? Loc.Pick(levelName, levelNameEn) : daily ? $"{Loc.Get("daily")} · {Loc.Pick(levelName, levelNameEn)}" : $"{levelNumber}. {Loc.Pick(levelName, levelNameEn)}";
            title.text = name;
            hintText.text = resumed ? Loc.Pick("Sigues desde el último punto de control.", "Picking up from your last checkpoint.") : Loc.Pick(hint, hintEn);
            corner.text = name;
            banner.text = "";
            UpdateCounter();
            if (Settings.Coop)
            {
                // Quién controla qué, arriba en el centro.
                var p1 = UiKit.Box(transform, "J1", new Vector2(0.5f, 1f), new Vector2(-260f, -40f), new Vector2(480f, 60f), new Color(1f, 0.6f, 0.35f, 0.85f));
                UiKit.Text(p1.transform, "Texto", Loc.Pick("J1 · Lumi (A/D + Espacio)", "P1 · Lumi (A/D + Space)"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 60f), 28);
                var p2 = UiKit.Box(transform, "J2", new Vector2(0.5f, 1f), new Vector2(260f, -40f), new Vector2(480f, 60f), new Color(0.47f, 0.43f, 1f, 0.85f));
                UiKit.Text(p2.transform, "Texto", Loc.Pick("J2 · La línea (← → o ratón)", "P2 · The line (← → or mouse)"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 60f), 28);
            }
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
            if (cheerLeft > 0f)
            {
                cheerLeft -= Time.deltaTime;
                SetAlpha(banner, Mathf.Clamp01(cheerLeft));
                if (cheerLeft <= 0f) banner.text = "";
            }
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
            SaveGuard.ClearCheckpoint(levelNumber);
            bool record = Progress.SaveTime(levelNumber, elapsed);
            string text = Loc.Get("complete");
            if (fragmentsTotal > 0 && fragments == fragmentsTotal) text += "\n" + Loc.Get("allmoon");
            if (record) text += "\n" + Loc.Get("record") + "  " + FormatTime(elapsed);
            banner.text = text;
        }
    }
}
