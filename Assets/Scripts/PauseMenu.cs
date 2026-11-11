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

        void Start()
        {
            // Pausa en gris (salvo la línea) y el reloj de lo jugado hoy.
            if (GetComponent<PauseMood>() == null) gameObject.AddComponent<PauseMood>();
            SetPaused(false);
        }


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
            // R ya no reinicia al instante: mantenerla vuelve al punto de control (HoldToReturn).

        }

        /// <summary>Si la ventana pierde el foco (otra pestaña, otra app), pausar.</summary>
        void OnApplicationFocus(bool focus)
        {
            if (!focus && !Paused && !PlayBot.Active) SetPaused(true);
        }

        public void Toggle() => SetPaused(!Paused);
        public void Resume() => SetPaused(false);

        public void Restart()
        {
            SetPaused(false);
            // Reiniciar es empezar de cero, no desde el último punto de control.
            if (Hud.Instance != null) SaveGuard.ClearCheckpoint(Hud.Instance.levelNumber);
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
