using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Botón táctil: ponlo en una Image de la interfaz. Funciona con varios dedos a la vez
    /// (moverse y saltar al mismo tiempo). Se ilumina mientras está pulsado.
    /// </summary>
    public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Kind { Left, Right, Jump }
        public Kind kind;

        static readonly List<RectTransform> active = new List<RectTransform>();

        /// <summary>true si el punto de la pantalla cae sobre algún botón táctil visible.</summary>
        public static bool Contains(Vector2 screen)
        {
            foreach (var rect in active)
                if (RectTransformUtility.RectangleContainsScreenPoint(rect, screen, null)) return true;
            return false;
        }

        bool held;
        int pointerId = int.MinValue;
        Image image;
        Color idleColor;

        void Awake()
        {
            image = GetComponent<Image>();
            if (image != null) idleColor = image.color;
        }

        void OnEnable() => active.Add((RectTransform)transform);

        void OnDisable()
        {
            active.Remove((RectTransform)transform);
            Set(false);
        }

        public void OnPointerDown(PointerEventData pointer)
        {
            if (held || PauseMenu.Paused || PhotoMode.Active) return;
            pointerId = pointer.pointerId;
            Set(true);
        }

        public void OnPointerUp(PointerEventData pointer)
        {
            if (pointer.pointerId == pointerId) Set(false);
        }

        public void OnPointerExit(PointerEventData pointer) => OnPointerUp(pointer);

        void Update()
        {
            if (PauseMenu.Paused || PhotoMode.Active) Set(false);
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused) Set(false);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Set(false);
        }

        void Set(bool pressed)
        {
            if (held == pressed) return;
            held = pressed;
            if (!pressed) pointerId = int.MinValue;
            if (image != null) image.color = pressed ? new Color(1f, 1f, 1f, 0.55f) : idleColor;
            transform.localScale = Vector3.one * (pressed ? 0.92f : 1f);
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
