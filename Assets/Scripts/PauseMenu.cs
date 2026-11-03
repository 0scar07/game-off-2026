using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Pausa: Esc / P / Start del mando, o el botón II en el móvil. Para el tiempo del juego y
    /// muestra Continuar, Reiniciar nivel y Menú.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public const string MenuScene = "Menu";

        public static bool Paused { get; private set; }

        public GameObject panel;

        void Start() => SetPaused(false);

        void OnDestroy()
        {
            Paused = false;
            TimeControl.Normal();
        }

        void Update()
        {
            // El modo foto usa Esc para salir, y en el Taller se escriben códigos con P o R.
            if (PhotoMode.Active || UiKit.Typing) return;
            var kb = Keyboard.current;
            var pad = Gamepad.current;
            if ((kb != null && (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame))
                || (pad != null && pad.startButton.wasPressedThisFrame))
                Toggle();
            // R: reintento rápido (salvo con la pantalla de resultados, que tiene la suya).
            if (kb != null && kb.rKey.wasPressedThisFrame && FindAnyObjectByType<ResultsScreen>() == null) Restart();
        }

        /// <summary>Si la ventana pierde el foco (otra pestaña, otra app), pausar.</summary>
        void OnApplicationFocus(bool focus)
        {
            if (!focus && !Paused) SetPaused(true);
        }

        public void Toggle() => SetPaused(!Paused);
        public void Resume() => SetPaused(false);

        public void Restart()
        {
            SetPaused(false);
            SceneFader.Load(SceneManager.GetActiveScene().name);
        }

        public void ToMenu()
        {
            SetPaused(false);
            SceneFader.Load(MenuScene);
        }

        void SetPaused(bool paused)
        {
            Paused = paused;
            TimeControl.Normal();
            if (panel != null) panel.SetActive(paused);
        }
    }
}
