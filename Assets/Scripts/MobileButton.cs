using UnityEngine;
using UnityEngine.EventSystems;

namespace Fase
{
    /// <summary>
    /// Botón táctil: ponlo en una Image de la interfaz. Funciona con varios dedos a la vez
    /// (moverse y saltar al mismo tiempo).
    /// </summary>
    public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Kind { Left, Right, Jump }
        public Kind kind;

        bool held;

        public void OnPointerDown(PointerEventData _) => Set(true);
        public void OnPointerUp(PointerEventData _) => Set(false);
        public void OnPointerExit(PointerEventData _) => Set(false);
        void OnDisable() => Set(false);

        void Set(bool pressed)
        {
            if (held == pressed) return;
            held = pressed;
            switch (kind)
            {
                case Kind.Left: MobileInput.Move += pressed ? -1f : 1f; break;
                case Kind.Right: MobileInput.Move += pressed ? 1f : -1f; break;
                case Kind.Jump:
                    if (pressed) MobileInput.PressJump();
                    else MobileInput.ReleaseJump();
                    break;
            }
        }
    }
}
