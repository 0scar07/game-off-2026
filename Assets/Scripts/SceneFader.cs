using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Transición entre escenas: una cortina de noche entra barriendo de izquierda a derecha con
    /// la línea brillante en el borde, se carga la escena y la cortina sale por la derecha.
    /// Uso: SceneFader.Load("Menu"). Se crea sola la primera vez y sobrevive entre escenas.
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        const float Duration = 0.55f;

        static SceneFader instance;
        RectTransform curtain;
        bool busy;

        public static void Load(string scene)
        {
            if (instance == null) Create();
            if (!instance.busy) instance.StartCoroutine(instance.Run(scene));
        }

        static void Create()
        {
            var go = new GameObject("Transicion");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneFader>();
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var curtain = new GameObject("Cortina", typeof(RectTransform)).GetComponent<RectTransform>();
            curtain.SetParent(go.transform, false);
            curtain.anchorMin = new Vector2(0f, 0f);
            curtain.anchorMax = new Vector2(1f, 1f);
            curtain.offsetMin = curtain.offsetMax = Vector2.zero;
            curtain.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.02f, 0.09f);

            // Borde brillante (la línea) a la derecha de la cortina.
            var edge = new GameObject("Linea", typeof(RectTransform)).GetComponent<RectTransform>();
            edge.SetParent(curtain, false);
            edge.anchorMin = new Vector2(1f, 0f);
            edge.anchorMax = new Vector2(1f, 1f);
            edge.sizeDelta = new Vector2(10f, 0f);
            edge.gameObject.AddComponent<Image>().color = new Color(1f, 0.97f, 0.9f);
            var glow = new GameObject("Halo", typeof(RectTransform)).GetComponent<RectTransform>();
            glow.SetParent(curtain, false);
            glow.anchorMin = new Vector2(1f, 0f);
            glow.anchorMax = new Vector2(1f, 1f);
            glow.sizeDelta = new Vector2(70f, 0f);
            glow.gameObject.AddComponent<Image>().color = new Color(1f, 0.85f, 0.6f, 0.25f);

            instance.curtain = curtain;
            instance.SetCover(-1f);
        }

        /// <summary>-1 = fuera por la izquierda, 0 = tapando, 1 = fuera por la derecha.</summary>
        void SetCover(float t)
        {
            float width = ((RectTransform)transform).rect.width + 100f;
            curtain.anchoredPosition = new Vector2(t * width, 0f);
            curtain.gameObject.SetActive(t > -0.999f && t < 0.999f);
        }

        IEnumerator Run(string scene)
        {
            busy = true;
            Time.timeScale = 1f;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Duration)
            {
                SetCover(Mathf.SmoothStep(-1f, 0f, t));
                yield return null;
            }
            SetCover(0f);
            var load = SceneManager.LoadSceneAsync(scene);
            while (!load.isDone) yield return null;
            yield return null;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Duration)
            {
                SetCover(Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            SetCover(1f);
            busy = false;
        }
    }
}
