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
    public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler
    {
        // ───────── Colocación propia (se edita desde la pausa: TouchEditor) ─────────

        /// <summary>Mientras se editan, los botones se arrastran en vez de pulsarse.</summary>
        public static bool Editing;
        public static MobileButton Selected;
        Vector2 defaultPosition;
        Vector3 defaultScale;
        bool layoutKnown;
        string LayoutKey => "fase_tactil_" + kind;

        void LoadLayout()
        {
            var rect = (RectTransform)transform;
            if (!layoutKnown) { defaultPosition = rect.anchoredPosition; defaultScale = rect.localScale; layoutKnown = true; }
            var parts = PlayerPrefs.GetString(LayoutKey, "").Split(';');
            if (parts.Length != 3) return;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            rect.anchoredPosition = new Vector2(float.Parse(parts[0], inv), float.Parse(parts[1], inv));
            rect.localScale = defaultScale * float.Parse(parts[2], inv);
        }

        public void SaveLayout()
        {
            var rect = (RectTransform)transform;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            float scale = defaultScale.x != 0f ? rect.localScale.x / defaultScale.x : 1f;
            PlayerPrefs.SetString(LayoutKey, string.Format(inv, "{0};{1};{2}", rect.anchoredPosition.x, rect.anchoredPosition.y, scale));
        }

        public void ResetLayout()
        {
            var rect = (RectTransform)transform;
            rect.anchoredPosition = defaultPosition;
            rect.localScale = defaultScale;
            PlayerPrefs.DeleteKey(LayoutKey);
        }

        /// <summary>Más grande o más pequeño (entre la mitad y el doble).</summary>
        public void Resize(float factor)
        {
            float current = defaultScale.x != 0f ? transform.localScale.x / defaultScale.x : 1f;
            transform.localScale = defaultScale * Mathf.Clamp(current * factor, 0.5f, 2f);
        }

        public void OnDrag(PointerEventData pointer)
        {
            if (!Editing) return;
            var canvas = GetComponentInParent<Canvas>();
            ((RectTransform)transform).anchoredPosition += pointer.delta / (canvas != null ? canvas.scaleFactor : 1f);
        }


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

        void OnEnable()
        {
            active.Add((RectTransform)transform);
            LoadLayout();
        }

        void OnDisable()
        {
            active.Remove((RectTransform)transform);
            Set(false);
        }

        public void OnPointerDown(PointerEventData pointer)
        {
            if (Editing) { Selected = this; return; }
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
