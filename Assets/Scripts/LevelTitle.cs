using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Al empezar un nivel, su nombre aparece grande arriba, "dibujado" por una línea de luz que lo
    /// recorre de izquierda a derecha; luego se desvanece.
    /// </summary>
    public class LevelTitle : MonoBehaviour
    {
        IEnumerator Start()
        {
            var hud = Hud.Instance;
            if (hud == null || hud.sandbox) yield break;
            string name = hud.Daily ? Loc.Get("daily") : Loc.Pick(hud.levelName, hud.levelNameEn);
            if (string.IsNullOrEmpty(name)) yield break;
            yield return new WaitForSeconds(0.5f);
            var canvas = UiKit.Canvas("TituloNivel", 44);
            canvas.transform.SetParent(transform, false);
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            const float width = 1400f;
            var mask = UiKit.Rect(canvas.transform, "Mascara", new Vector2(0.5f, 1f), new Vector2(-width * 0.5f, -300f), new Vector2(0f, 150f));
            mask.pivot = new Vector2(0f, 0.5f);
            mask.gameObject.AddComponent<RectMask2D>();
            var title = UiKit.Text(mask, "Nombre", name, new Vector2(0f, 0.5f), new Vector2(width * 0.5f, 0f), new Vector2(width, 150f), 96, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.88f));
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0.1f, 0.06f, 0.25f, 0.6f);
            var line = UiKit.Rect(canvas.transform, "Linea", new Vector2(0.5f, 1f), new Vector2(-width * 0.5f, -300f), new Vector2(8f, 170f));
            UiKit.Box(line, "Halo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, 190f), new Color(1f, 0.92f, 0.7f, 0.35f), false);
            UiKit.Box(line, "Nucleo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 170f), Color.white, false);
            // El ancho real del texto: la línea solo recorre lo que ocupa.
            float textWidth = Mathf.Min(width, title.preferredWidth + 40f);
            float from = -textWidth * 0.5f, to = textWidth * 0.5f;
            for (float t = 0f; t < 1.1f; t += Time.unscaledDeltaTime)
            {
                float x = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / 1.1f));
                mask.sizeDelta = new Vector2(x + width * 0.5f, 150f);
                line.anchoredPosition = new Vector2(x, -300f);
                group.alpha = PauseMenu.Paused ? 0f : 1f;
                yield return null;
            }
            mask.sizeDelta = new Vector2(width, 150f);
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime) { line.localScale = new Vector3(1f, 1f - t / 0.4f, 1f); yield return null; }
            Destroy(line.gameObject);
            yield return new WaitForSecondsRealtime(1.2f);
            for (float t = 0f; t < 0.8f; t += Time.unscaledDeltaTime) { group.alpha = 1f - t / 0.8f; yield return null; }
            Destroy(canvas.gameObject);
        }
    }
}
