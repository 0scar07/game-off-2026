using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Bocadillo blanco que sigue a algo del mundo (Lumi en el nivel o en el menú) unos segundos:
    /// "¡Achís!", "¡Has vuelto!"... Uno por escena; si se pide otro, sustituye al anterior.
    /// </summary>
    public class LumiBubble : MonoBehaviour
    {
        static LumiBubble current;

        Transform target;
        Vector3 offset;
        float left, total;
        RectTransform bubble, canvas;
        Text label;

        public static void Say(Transform target, string text, float seconds = 3f, float height = 2.3f)
        {
            if (target == null) return;
            if (current == null)
            {
                var c = UiKit.Canvas("Bocadillo", 28);
                current = c.gameObject.AddComponent<LumiBubble>();
                current.Build((RectTransform)c.transform);
            }
            current.target = target;
            current.offset = Vector3.up * height;
            current.left = current.total = seconds;
            current.label.text = text;
            float width = Mathf.Clamp(text.Length * 16f + 60f, 180f, 560f);
            current.bubble.sizeDelta = new Vector2(width, 86f);
            current.label.rectTransform.sizeDelta = new Vector2(width - 30f, 80f);
            current.bubble.gameObject.SetActive(true);
        }

        public static void Hide() { if (current != null) current.bubble.gameObject.SetActive(false); }

        void Build(RectTransform root)
        {
            canvas = root;
            bubble = UiKit.Box(root, "Globo", new Vector2(0f, 0f), Vector2.zero, new Vector2(300f, 86f), new Color(1f, 1f, 1f, 0.94f)).rectTransform;
            bubble.pivot = new Vector2(0.5f, 0f);
            var tail = UiKit.Box(bubble, "Pico", new Vector2(0.5f, 0f), new Vector2(0f, -9f), new Vector2(26f, 26f), new Color(1f, 1f, 1f, 0.94f), false);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            label = UiKit.Text(bubble, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280f, 80f), 30, TextAnchor.MiddleCenter, new Color(0.23f, 0.16f, 0.38f));
            Destroy(label.GetComponent<Shadow>());
            bubble.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!bubble.gameObject.activeSelf) return;
            left -= Time.unscaledDeltaTime;
            var cam = Camera.main;
            if (left <= 0f || target == null || cam == null) { bubble.gameObject.SetActive(false); return; }
            var screen = cam.WorldToScreenPoint(target.position + offset);
            if (screen.z > 0f && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out var local))
                bubble.anchoredPosition = local + canvas.rect.size * 0.5f;
            // Aparece con un pequeño salto y se va encogiendo al final.
            float born = total - left;
            bubble.localScale = Vector3.one * Mathf.Min(Mathf.Min(1f, born * 6f), Mathf.Clamp01(left * 4f));
        }
    }
}
