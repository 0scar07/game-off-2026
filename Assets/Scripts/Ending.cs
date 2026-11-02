using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Final: la línea recorre el mundo hasta que amanece del todo, y aparecen el mensaje, la luna
    /// reunida y los créditos. El botón vuelve al menú.
    /// </summary>
    public class Ending : MonoBehaviour
    {
        public ShiftLine line;
        public float from = -2f, to = 42f, duration = 9f;
        public CanvasGroup texts;
        public Text moonLabel;
        public int levels = 11;
        public int fragmentsTotal;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            texts.alpha = 0f;
            texts.interactable = texts.blocksRaycasts = false;
            int moon = 0;
            for (int level = 1; level <= levels; level++) moon += Progress.Fragments(level);
            moonLabel.text = $"Luna reunida: {moon}/{fragmentsTotal}";

            // La noche se retira: la línea avanza hasta que todo es de día.
            line.autoSweep = false;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                line.x = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            line.x = to;

            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                texts.alpha = t / 1.5f;
                yield return null;
            }
            texts.alpha = 1f;
            texts.interactable = texts.blocksRaycasts = true;
        }

        public void ToMenu() => SceneManager.LoadScene(PauseMenu.MenuScene);
    }
}
