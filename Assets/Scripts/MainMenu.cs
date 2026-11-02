using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Menú principal: Jugar (sigue por el último nivel desbloqueado), Niveles (los
    /// desbloqueados con su luna conseguida) y Opciones (volumen). Lo monta el generador.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public GameObject mainPanel, levelsPanel, optionsPanel;

        [Header("Niveles (en orden)")]
        public string[] scenes;
        public string[] names;
        public int[] fragmentsTotal;
        public Button[] levelButtons;
        public Text[] levelLabels;

        [Header("Opciones")]
        public Slider music, sfx;
        public Text playLabel;

        void Start()
        {
            Time.timeScale = 1f;
            music.SetValueWithoutNotify(Progress.MusicVolume);
            sfx.SetValueWithoutNotify(Progress.SfxVolume);
            ShowMain();
        }

        public void Play() => LoadLevel(Mathf.Clamp(Progress.Unlocked, 1, scenes.Length));

        public void LoadLevel(int level) => SceneManager.LoadScene(scenes[level - 1]);

        public void ShowMain() => Show(mainPanel);
        public void ShowOptions() => Show(optionsPanel);

        public void ShowLevels()
        {
            int unlocked = Progress.Unlocked;
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int level = i + 1;
                bool open = level <= unlocked;
                levelButtons[i].interactable = open;
                string moon = fragmentsTotal[i] > 0 ? $"\nLuna {Progress.Fragments(level)}/{fragmentsTotal[i]}" : "";
                levelLabels[i].text = open ? $"{level}. {names[i]}{moon}" : $"{level}. ???";
            }
            Show(levelsPanel);
        }

        public void OnMusicChanged(float value) => Progress.MusicVolume = value;
        public void OnSfxChanged(float value) => Progress.SfxVolume = value;

        public void ResetProgress()
        {
            float m = Progress.MusicVolume, s = Progress.SfxVolume;
            Progress.ResetAll();
            Progress.MusicVolume = m;
            Progress.SfxVolume = s;
            UpdatePlayLabel();
        }

        void Show(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            levelsPanel.SetActive(panel == levelsPanel);
            optionsPanel.SetActive(panel == optionsPanel);
            UpdatePlayLabel();
        }

        void UpdatePlayLabel()
        {
            if (playLabel != null) playLabel.text = Progress.Unlocked > 1 ? "Continuar" : "Jugar";
        }
    }
}
