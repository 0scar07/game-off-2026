using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Menú principal: Jugar/Continuar, "El viaje" (mapa de niveles por capítulos, con récord y
    /// luna de cada uno) y Opciones (volumen, idioma, ver la historia, borrar progreso).
    /// Lo monta el generador (LevelBuilder.Menus).
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public GameObject mainPanel, journeyPanel, optionsPanel;

        [Header("Niveles (en orden)")]
        public string[] scenes;
        public string[] names, namesEn;
        public int[] fragmentsTotal;
        public string introScene = "Intro";

        [Header("El viaje")]
        public Button[] nodes;
        public Image[] nodeImages;
        public RectTransform marker;
        public Text infoTitle, infoDetail;
        public Button infoPlay;

        [Header("Opciones")]
        public Slider music, sfx;
        public Text playLabel;
        [Tooltip("Debajo de Continuar: nivel por el que vas y luna reunida.")]
        public Text playInfo;

        static readonly Color Done = new Color(1f, 0.78f, 0.35f);
        static readonly Color Current = new Color(1f, 1f, 1f);
        static readonly Color Locked = new Color(0.25f, 0.24f, 0.4f, 0.85f);

        int selected = 1;
        Vector2 markerBase;

        void Start()
        {
            Time.timeScale = 1f;
            music.SetValueWithoutNotify(Progress.MusicVolume);
            sfx.SetValueWithoutNotify(Progress.SfxVolume);
            ShowMain();
        }

        void OnEnable() => Loc.Changed += Refresh;
        void OnDisable() => Loc.Changed -= Refresh;

        // Los niveles secretos van al final de `scenes` y no cuentan para "Continuar".
        int Unlocked => Mathf.Clamp(Progress.Unlocked, 1, Mathf.Max(1, nodes.Length));

        /// <summary>Jugar: la primera vez cuenta la historia; después, sigue por donde ibas.</summary>
        public void Play()
        {
            if (Progress.Unlocked <= 1 && PlayerPrefs.GetInt(Intro.SeenKey, 0) == 0 && !string.IsNullOrEmpty(introScene))
                SceneFader.Load(introScene);
            else
                LoadLevel(Unlocked);
        }

        public void LoadLevel(int level) => SceneFader.Load(scenes[level - 1]);

        public void ShowMain() => Show(mainPanel);
        public void ShowOptions() => Show(optionsPanel);

        public void ShowJourney()
        {
            Show(journeyPanel);
            SelectLevel(Unlocked);
        }

        public void SelectLevel(int level)
        {
            selected = level;
            bool open = level <= Unlocked;
            infoTitle.text = $"{level}. {(open ? Loc.Pick(names[level - 1], namesEn[level - 1]) : "???")}";
            if (!open)
                infoDetail.text = Loc.Get("locked");
            else
            {
                string moon = fragmentsTotal[level - 1] > 0 ? $"{Loc.Get("moon")} {Progress.Fragments(level)}/{fragmentsTotal[level - 1]}" : "";
                int medal = Medals.Best(level);
                string stars = $"<color=#ffd98a>{new string('★', Mathf.Min(medal, 3))}</color><color=#ffffff40>{new string('★', 3 - Mathf.Min(medal, 3))}</color>" + (medal >= Medals.Moon ? " ☾" : "");
                infoDetail.text = $"{stars}    ·    {Loc.Get("best")} {Hud.FormatTime(Progress.BestTime(level))}    ·    {moon}";
            }
            Selected?.Invoke(level, open);

            infoPlay.interactable = open;
            if (marker != null) markerBase = ((RectTransform)nodes[level - 1].transform).anchoredPosition + Vector2.up * 92f;
        }

        public void PlaySelected() => LoadLevel(selected);

        /// <summary>Se elige un nivel en el mapa (número, si está abierto).</summary>
        public event System.Action<int, bool> Selected;


        void Update()
        {
            if (!journeyPanel.activeSelf) return;
            int unlocked = Unlocked;
            for (int i = 0; i < nodes.Length; i++)
            {
                int level = i + 1;
                var color = level < unlocked ? Done : level == unlocked ? Current : Locked;
                if (level == selected) color = Color.Lerp(color, Color.white, 0.35f + 0.25f * Mathf.Sin(Time.time * 5f));
                nodeImages[i].color = color;
                float pulse = level == unlocked ? 1f + 0.08f * Mathf.Sin(Time.time * 4f) : 1f;
                nodes[i].transform.localScale = Vector3.one * pulse;
            }
            // Triángulo que apunta hacia abajo (-90) y bota encima del nivel elegido.
            if (marker != null)
            {
                marker.anchoredPosition = markerBase + Vector2.up * Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 14f;
                marker.localRotation = Quaternion.Euler(0f, 0f, -90f);
            }
        }

        public void OnMusicChanged(float value) => Progress.MusicVolume = value;
        public void OnSfxChanged(float value) => Progress.SfxVolume = value;
        public void ToggleLanguage() => Loc.English = !Loc.English;
        public void WatchIntro() => SceneFader.Load(introScene);

        public void ResetProgress()
        {
            float m = Progress.MusicVolume, s = Progress.SfxVolume;
            bool english = Loc.English;
            Progress.ResetAll();
            Progress.MusicVolume = m;
            Progress.SfxVolume = s;
            Loc.English = english;
            Refresh();
        }

        readonly System.Collections.Generic.List<GameObject> extraPanels = new System.Collections.Generic.List<GameObject>();

        /// <summary>Paneles que añade MenuExtras (logros, Lumi, accesibilidad...).</summary>
        public void AddPanel(GameObject panel)
        {
            extraPanels.Add(panel);
            panel.SetActive(false);
        }

        public void Show(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            journeyPanel.SetActive(panel == journeyPanel);
            optionsPanel.SetActive(panel == optionsPanel);
            foreach (var extra in extraPanels) extra.SetActive(panel == extra);
            Refresh();
            Shown?.Invoke(panel);
        }

        public event System.Action<GameObject> Shown;

        void Refresh()
        {
            if (playLabel != null) playLabel.text = "▶  " + Loc.Get(Progress.Unlocked > 1 ? "continue" : "play");
            if (playInfo != null)
            {
                int level = Unlocked, moon = 0, total = 0;
                for (int i = 0; i < nodes.Length; i++) { moon += Progress.Fragments(i + 1); total += fragmentsTotal[i]; }
                playInfo.text = $"{Loc.Pick("Nivel", "Level")} {level} · {Loc.Pick(names[level - 1], namesEn[level - 1])}   ·   ☾ {moon}/{total}";
            }
            if (journeyPanel.activeSelf) SelectLevel(selected);
        }
    }
}
