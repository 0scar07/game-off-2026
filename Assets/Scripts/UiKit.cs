using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Piezas de interfaz creadas desde código mientras se juega (resultados, logros, armario,
    /// avisos). Mismo estilo que la interfaz que monta el generador: Fredoka, paneles morados
    /// redondeados y lienzo de referencia 1920×1080.
    /// </summary>
    public static class UiKit
    {
        public static readonly Color Panel = new Color(0.05f, 0.04f, 0.15f, 0.9f);
        public static readonly Color ButtonColor = new Color(0.16f, 0.13f, 0.36f, 0.95f);
        public static readonly Color Gold = new Color(1f, 0.85f, 0.55f);
        public static readonly Color Soft = new Color(0.8f, 0.84f, 1f);

        static Font font;
        static Sprite rounded;

        public static Font Font => font != null ? font : font = Resources.Load<Font>("Fredoka");

        /// <summary>Círculo de 64 px con 9-slice: sirve para botones, píldoras y paneles.</summary>
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var pixels = new Color32[size * size];
                float r = size * 0.5f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(r - d) * 255));
                    }
                texture.SetPixels32(pixels);
                texture.Apply();
                rounded = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(31, 31, 31, 31));
                return rounded;
            }
        }

        public static Canvas Canvas(string name, int order, bool persistent = false)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            go.AddComponent<GraphicRaycaster>();
            if (persistent) Object.DontDestroyOnLoad(go);
            return canvas;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>Rectángulo que ocupa todo el padre.</summary>
        public static RectTransform Fill(Transform parent, string name)
        {
            var rect = Rect(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            return rect;
        }

        public static Image Box(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color, bool rounded = true)
        {
            var image = Rect(parent, name, anchor, position, size).gameObject.AddComponent<Image>();
            if (rounded) { image.sprite = Rounded; image.type = Image.Type.Sliced; }
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Text Text(Transform parent, string name, string text, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor align = TextAnchor.MiddleCenter, Color? color = null)
        {
            var label = Rect(parent, name, anchor, position, size).gameObject.AddComponent<Text>();
            label.font = Font;
            label.fontSize = fontSize;
            label.fontStyle = fontSize >= 40 ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = align;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.color = color ?? Color.white;
            label.text = text;
            label.raycastTarget = false;
            var shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0.1f, 0.6f);
            shadow.effectDistance = new Vector2(0f, -3f);
            label.gameObject.AddComponent<TextScaler>().Init(label);
            return label;

        }

        /// <summary>Cambia el tamaño de un texto respetando el tamaño de texto de Accesibilidad.</summary>
        public static void SetSize(Text text, int size)
        {
            var scaler = text.GetComponent<TextScaler>();
            if (scaler != null) scaler.SetBase(size); else text.fontSize = size;
        }

        public static Button Button(
Transform parent, string name, string text, Vector2 anchor, Vector2 position, Vector2 size, UnityAction onClick, int fontSize = 44)
        {
            var image = Box(parent, name, anchor, position, size, ButtonColor);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.1f, 0.85f);
            colors.pressedColor = new Color(0.8f, 0.9f, 1.2f);
            colors.selectedColor = new Color(1.15f, 1.05f, 0.9f);
            colors.colorMultiplier = 1.4f;
            button.colors = colors;
            Text(image.transform, "Texto", text, new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize);
            if (onClick != null) button.onClick.AddListener(onClick);
            return button;
        }

        /// <summary>Se está escribiendo en un campo de texto (las teclas no deben mover el juego).</summary>
        public static bool Typing
        {
            get
            {
                var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                var field = selected != null ? selected.GetComponent<InputField>() : null;
                return field != null && field.isFocused;
            }
        }

        /// <summary>Hay un EventSystem en la escena (los botones lo necesitan).</summary>
        public static void EnsureEvents()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }
    }
}
