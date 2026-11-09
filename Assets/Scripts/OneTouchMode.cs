using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// Modo de un toque (pensado para el móvil): Lumi avanza sola, la línea se va colocando donde
    /// aconseja LineAdvisor y tocar en cualquier sitio de la pantalla es saltar. Mantener el dedo
    /// la para. Se activa en Extras → Ayudas.
    /// </summary>
    public class OneTouchMode : MonoBehaviour
    {
        PlayerController player;
        float held;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            if (player == null || Hud.Instance == null || Hud.Instance.sandbox) enabled = false;
        }

        void OnDisable() { MobileInput.Move = 0f; MobileInput.ReleaseJump(); }

        void Update()
        {
            if (!Settings.OneTouch || PauseMenu.Paused || PhotoMode.Active) { if (Settings.OneTouch) MobileInput.Move = 0f; return; }
            var pointer = Pointer.current;
            bool pressing = pointer != null && pointer.press.isPressed && !MobileButton.Contains(pointer.position.ReadValue());
            if (pointer != null && pointer.press.wasPressedThisFrame && !MobileButton.Contains(pointer.position.ReadValue())) MobileInput.PressJump();
            if (pointer != null && pointer.press.wasReleasedThisFrame) MobileInput.ReleaseJump();
            held = pressing ? held + Time.deltaTime : 0f;
            // Avanza sola; con el dedo apoyado más de medio segundo, se para.
            MobileInput.Move = held > 0.5f ? 0f : 1f;
            var line = ShiftLine.Instance;
            var target = LineAdvisor.Suggest(player.transform.position, 1f);
            if (line != null && target.HasValue)
                line.x = Mathf.MoveTowards(line.x, target.Value, line.keySpeed * Time.deltaTime);
        }
    }
}
