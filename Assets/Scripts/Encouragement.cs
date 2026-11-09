using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Tras varias caídas seguidas en el mismo tramo, Lumi dice algo para animar (un bocadillo
    /// sobre ella unos segundos). Se reinicia al pasar un punto de control o coger luna.
    /// </summary>
    public class Encouragement : MonoBehaviour
    {
        const int Every = 5;
        static readonly (string es, string en)[] Lines =
        {
            ("¡Casi! Una vez más.", "Almost! One more time."),
            ("Respira... ya lo tienes.", "Breathe... you've got this."),
            ("Mueve la línea antes de saltar.", "Move the line before you jump."),
            ("Cada caída enseña algo.", "Every fall teaches something."),
            ("¡Esta vez sí!", "This time for sure!"),
            ("Mira dónde brilla el azul.", "Look where the blue glows."),
        };

        PlayerController player;
        Camera cam;
        int falls, shown;
        RectTransform bubble;
        Text label;
        float left;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            cam = Camera.main;
            GameEvents.Died += OnDied;
            GameEvents.CheckpointReached += Reset;
            GameEvents.Collected += Reset;
        }

        void OnDestroy()
        {
            GameEvents.Died -= OnDied;
            GameEvents.CheckpointReached -= Reset;
            GameEvents.Collected -= Reset;
        }

        void Reset() => falls = 0;

        void OnDied()
        {
            falls++;
            if (falls % Every != 0) return;
            if (bubble == null) Build();
            var line = Lines[(shown++ + Random.Range(0, Lines.Length)) % Lines.Length];
            label.text = Loc.Pick(line.es, line.en);
            bubble.gameObject.SetActive(true);
            left = 4f;
        }

        void Build()
        {
            var canvas = UiKit.Canvas("Animo", 30);
            canvas.transform.SetParent(transform, false);
            bubble = UiKit.Box(canvas.transform, "Bocadillo", new Vector2(0f, 0f), Vector2.zero, new Vector2(460f, 100f), new Color(1f, 1f, 1f, 0.93f)).rectTransform;
            bubble.pivot = new Vector2(0.5f, 0f);
            var tail = UiKit.Box(bubble, "Pico", new Vector2(0.5f, 0f), new Vector2(0f, -10f), new Vector2(28f, 28f), new Color(1f, 1f, 1f, 0.93f), false);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            label = UiKit.Text(bubble, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440f, 90f), 32, TextAnchor.MiddleCenter, new Color(0.23f, 0.16f, 0.38f));
            Destroy(label.GetComponent<Shadow>());
        }

        void LateUpdate()
        {
            if (bubble == null || !bubble.gameObject.activeSelf) return;
            left -= Time.unscaledDeltaTime;
            if (left <= 0f || player == null || cam == null) { bubble.gameObject.SetActive(false); return; }
            var canvas = (RectTransform)bubble.parent;
            var screen = cam.WorldToScreenPoint(player.transform.position + Vector3.up * 2.3f);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out var local))
                bubble.anchoredPosition = local + canvas.rect.size * 0.5f;
            bubble.localScale = Vector3.one * Mathf.Min(1f, (4f - left) * 6f);
        }
    }
}
