using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Nivel a oscuras (cabecera "oscuro: si"): todo negro salvo un círculo de luz alrededor del
    /// farol de Lumi y un brillo tenue sobre la línea.
    /// </summary>
    public class DarkLevel : MonoBehaviour
    {
        RectTransform hole, canvas;
        Transform player;

        void Start()
        {
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
            var c = UiKit.Canvas("Oscuridad", -5);
            c.transform.SetParent(transform, false);
            canvas = (RectTransform)c.transform;
            // Un círculo transparente con bordes negros muy grandes alrededor: tapa todo lo demás.
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f)) / (size * 0.5f);
                    tex.SetPixel(x, y, new Color(0.01f, 0.01f, 0.03f, Mathf.SmoothStep(0f, 1f, (d - 0.55f) / 0.45f)));
                }
            tex.Apply();
            hole = UiKit.Rect(canvas, "Luz", new Vector2(0f, 0f), Vector2.zero, new Vector2(760f, 760f));
            var image = hole.gameObject.AddComponent<RawImage>();
            image.texture = tex;
            image.raycastTarget = false;
            // Bordes negros alrededor del círculo, hasta fuera de la pantalla.
            foreach (var (anchor, pivot) in new[] { (new Vector2(0.5f, 1f), new Vector2(0.5f, 0f)), (new Vector2(0.5f, 0f), new Vector2(0.5f, 1f)), (new Vector2(0f, 0.5f), new Vector2(1f, 0.5f)), (new Vector2(1f, 0.5f), new Vector2(0f, 0.5f)) })
            {
                var edge = UiKit.Box(hole, "Borde", anchor, Vector2.zero, new Vector2(6000f, 6000f), new Color(0.01f, 0.01f, 0.03f, 1f), false);
                edge.rectTransform.pivot = pivot;
            }
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            if (player == null || cam == null) return;
            var screen = cam.WorldToScreenPoint(player.position + Vector3.up * 0.4f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out var local);
            hole.anchoredPosition = local + canvas.rect.size * 0.5f;
            hole.localScale = Vector3.one * (1f + 0.03f * Mathf.Sin(Time.time * 3f));
        }
    }
}
