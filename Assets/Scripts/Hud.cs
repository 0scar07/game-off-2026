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


        void OnDied()
        {
            Deaths++;
            Stats.AddFall();
            // El ánimo tras varias caídas lo da Lumi en su bocadillo (Encouragement) y la pista de
            // la línea (LineAdvisor): un cartel grande aquí se pisaba con los dos y con la pista.
            // Al caer, la pista del principio ya no hace falta.
            age = Mathf.Max(age, 7f);
        }

        void OnCheckpoint()
        {
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
            // Los niveles secretos y extra no son de la historia: llevan ★ en vez de número.
            string number = Secret ? "★" : levelNumber.ToString();
            string name = sandbox ? Loc.Pick(levelName, levelNameEn) : daily ? $"{Loc.Get("daily")} · {Loc.Pick(levelName, levelNameEn)}" : $"{number}. {Loc.Pick(levelName, levelNameEn)}";
            title.text = name;
            hintText.text = resumed ? Loc.Pick("Sigues desde el último punto de control.", "Picking up from your last checkpoint.") : InputGlyphs.Adapt(Loc.Pick(hint, hintEn));

            corner.text = sandbox || daily ? name : Loc.Pick(levelName, levelNameEn);
            banner.text = "";
            // El cartel grande nunca se sale de la pantalla.
            banner.horizontalOverflow = HorizontalWrapMode.Wrap;
            banner.resizeTextForBestFit = true;
            banner.resizeTextMinSize = 40;
            banner.resizeTextMaxSize = banner.fontSize;
            BuildHintBack();
            BuildCornerBadge();
            BuildMoonIcons();

            UpdateCounter();
            if (Settings.Coop)
            {
                // Quién controla qué, abajo en el centro (arriba van la energía de la línea y la vida del jefe).
                var p1 = UiKit.Box(transform, "J1", new Vector2(0.5f, 0f), new Vector2(-260f, 50f), new Vector2(480f, 60f), new Color(1f, 0.6f, 0.35f, 0.85f));
                UiKit.Text(p1.transform, "Texto", Loc.Pick("J1 · Lumi (A/D + Espacio)", "P1 · Lumi (A/D + Space)"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 60f), 28);
                var p2 = UiKit.Box(transform, "J2", new Vector2(0.5f, 0f), new Vector2(260f, 50f), new Vector2(480f, 60f), new Color(0.47f, 0.43f, 1f, 0.85f));
                UiKit.Text(p2.transform, "Texto", Loc.Pick("J2 · La línea (← → o ratón)", "P2 · The line (← → or mouse)"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 60f), 28);
            }
        }

        /// <summary>¿Es un nivel secreto o extra (fuera de la historia)?</summary>
        bool Secret
        {
            get
            {
                var levels = LevelCatalog.Get.levels;
                return levelNumber >= 1 && levelNumber <= levels.Length && levels[levelNumber - 1].secret;
            }
        }

        Image hintBack;
        CanvasGroup hintGroup;
        static readonly Color CardColor = new Color(0.07f, 0.05f, 0.2f, 0.82f);

        /// <summary>
        /// La pista va en una tarjeta oscura con borde dorado y un icono: se lee igual sobre la
        /// nieve, el sol o la línea.
        /// </summary>
        void BuildHintBack()
        {
            if (hintText == null || string.IsNullOrEmpty(hintText.text)) return;
            var rect = hintText.rectTransform;
            float width = Mathf.Min(hintText.preferredWidth, rect.sizeDelta.x) + 150f;
            hintBack = UiKit.Box(rect.parent, "FondoPista", rect.anchorMin, rect.anchoredPosition,
                new Vector2(width, hintText.preferredHeight + 40f), CardColor);
            hintBack.rectTransform.pivot = rect.pivot;
            hintBack.transform.SetSiblingIndex(rect.GetSiblingIndex());
            var outline = hintBack.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.54f, 0.55f);
            outline.effectDistance = new Vector2(2f, -2f);
            var icon = UiKit.Box(hintBack.transform, "Icono", new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(48f, 48f), UiKit.Gold);
            UiKit.Text(icon.transform, "Signo", "✦", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f), 28, TextAnchor.MiddleCenter, new Color(0.1f, 0.06f, 0.19f));
            Destroy(icon.GetComponentInChildren<Shadow>());
            // El texto se corre a la derecha del icono.
            rect.anchoredPosition += new Vector2(40f, 0f);
            hintGroup = hintBack.gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>Número del nivel en un círculo dorado, a la izquierda del nombre.</summary>
        void BuildCornerBadge()
        {
            var pill = transform.Find("FondoEsquina") as RectTransform;
            if (pill == null) return;
            float textX = 60f;
            if (!sandbox && !daily && levelNumber > 0)
            {
                var badge = UiKit.Box(pill, "Numero", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(50f, 50f), UiKit.Gold);
                badge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                var number = UiKit.Text(badge.transform, "Texto", Secret ? "★" : levelNumber.ToString(), new Vector2(0.5f, 0.5f), new Vector2(0f, -1f), new Vector2(50f, 50f), 28, TextAnchor.MiddleCenter, new Color(0.1f, 0.06f, 0.19f));
                number.fontStyle = FontStyle.Bold;
                Destroy(number.GetComponent<Shadow>());
                textX = 104f;
            }
            corner.rectTransform.anchoredPosition = new Vector2(textX, corner.rectTransform.anchoredPosition.y);
            // El fondo a la medida del texto (antes medía siempre lo mismo).
            pill.sizeDelta = new Vector2(Mathf.Clamp(corner.preferredWidth + textX - 4f, 200f, 900f), pill.sizeDelta.y);
        }

        RectTransform moonRow;
        Image[] moonIcons;
        static readonly Color MoonOn = new Color(1f, 0.86f, 0.5f), MoonOff = new Color(1f, 1f, 1f, 0.1f);

        /// <summary>Las lunas del nivel como iconos (llena si ya la tienes, vacía si no) en vez de "Luna 0/2".</summary>
        void BuildMoonIcons()
        {
            if (counter == null || fragmentsTotal <= 0) return;
            counter.enabled = false;
            var pill = transform.Find("FondoLuna") as RectTransform;
            const float step = 46f;
            float width = fragmentsTotal * step + 40f;
            if (pill != null) pill.sizeDelta = new Vector2(width, pill.sizeDelta.y);
            moonRow = UiKit.Rect(pill != null ? pill : transform, "Lunas", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, 60f));
            moonRow.pivot = new Vector2(0.5f, 0.5f);
            moonIcons = new Image[fragmentsTotal];
            for (int i = 0; i < fragmentsTotal; i++)
            {
                var icon = UiKit.Box(moonRow, "Luna", new Vector2(0.5f, 0.5f), new Vector2((i - (fragmentsTotal - 1) * 0.5f) * step, 0f), new Vector2(22f, 22f), MoonOff, false);
                icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var edge = icon.gameObject.AddComponent<Outline>();
                edge.effectColor = new Color(1f, 0.86f, 0.5f, 0.7f);
                edge.effectDistance = new Vector2(2f, -2f);
                moonIcons[i] = icon;
            }
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
            if (timer != null) timer.text = "⏱ " + FormatTime(elapsed);
            if (hintGroup != null) hintGroup.alpha = Fade(age, 6f, 1f);
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
            if (moonRow != null) moonRow.localScale = Vector3.one * 1.35f;
            else if (counter != null) counter.transform.localScale = Vector3.one * 1.35f;
        }

        void LateUpdate()
        {
            if (counter != null) counter.transform.localScale = Vector3.Lerp(counter.transform.localScale, Vector3.one, Time.deltaTime * 8f);
            if (moonRow != null) moonRow.localScale = Vector3.Lerp(moonRow.localScale, Vector3.one, Time.deltaTime * 8f);
        }

        void UpdateCounter()
        {
            counter.text = fragmentsTotal > 0 ? $"{Loc.Get("moon")} {fragments}/{fragmentsTotal}" : "";
            if (moonIcons == null) return;
            for (int i = 0; i < moonIcons.Length; i++)
            {
                bool got = i < fragments;
                moonIcons[i].color = got ? MoonOn : MoonOff;
                moonIcons[i].GetComponent<Outline>().enabled = !got;
            }
        }

        /// <summary>Quita el mensaje de la meta (la pantalla de resultados lo repite mejor).</summary>
        public void ClearBanner()
        {
            if (banner != null) banner.text = "";
        }

        void OnGoal()
        {
            finished = true;
            SaveGuard.ClearCheckpoint(levelNumber);
            IsRecord = !sandbox && !daily && !Settings.Assist && Progress.SaveTime(levelNumber, elapsed);
            bool record = IsRecord;
            string text = Loc.Get("complete");
            if (fragmentsTotal > 0 && fragments == fragmentsTotal) text += "\n" + Loc.Get("allmoon");
            if (record) text += "\n" + Loc.Get("record") + "  " + FormatTime(elapsed);
            banner.text = text;
        }
    }
}
