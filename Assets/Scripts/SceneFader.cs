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
        Text tip;

        static readonly (string es, string en)[] Tips =
        {
            ("Arrastra la línea con el dedo o el ratón: es más rápido que el teclado.", "Drag the line with your finger or mouse: it's faster than the keyboard."),
            ("Las Sombras duermen de día. Sobre su piedra se puede caminar... casi.", "Shadows sleep by day. You can almost walk past their stone."),
            ("El lago es agua de día y hielo de noche.", "The lake is water by day and ice by night."),
            ("Las plataformas con engranaje solo se mueven al sol.", "Geared platforms only move in the sun."),
            ("Donde bailan las luciérnagas hay un camino escondido.", "Where fireflies dance, there's a hidden path."),
            ("Las medallas de Luna son secretas. Y muy difíciles.", "Moon medals are secret. And very hard."),
            ("Con todos los fragmentos de un capítulo se abre algo en el mapa.", "Collect every fragment in a chapter and something opens on the map."),
            ("¿Te atascas? Prueba el modo asistido en Opciones → Controles.", "Stuck? Try assist mode in Options → Controls."),
            ("Las huellas de Lumi se borran al cruzar la línea.", "Lumi's footprints vanish when they cross the line."),
            ("Quédate justo encima de la línea y escucha la música.", "Stand right on the line and listen to the music."),
        };
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

            // Consejo en la cortina mientras carga.
            var tip = new GameObject("Consejo", typeof(RectTransform)).GetComponent<RectTransform>();
            tip.SetParent(curtain, false);
            tip.anchorMin = new Vector2(0.1f, 0f);
            tip.anchorMax = new Vector2(0.9f, 0f);
            tip.sizeDelta = new Vector2(0f, 120f);
            tip.anchoredPosition = new Vector2(0f, 110f);
            instance.tip = tip.gameObject.AddComponent<Text>();
            instance.tip.font = UiKit.Font;
            instance.tip.fontSize = 34;
            instance.tip.alignment = TextAnchor.MiddleCenter;
            instance.tip.color = new Color(0.8f, 0.84f, 1f, 0.9f);
            instance.tip.horizontalOverflow = HorizontalWrapMode.Wrap;

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
            var t0 = Tips[Random.Range(0, Tips.Length)];
            if (tip != null) tip.text = $"{Loc.Get("tip")}: {Loc.Pick(t0.es, t0.en)}";
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Duration)
            {
                SetCover(Mathf.SmoothStep(-1f, 0f, t));
                yield return null;
            }
            SetCover(0f);
            float covered = Time.unscaledTime;
            var load = SceneManager.LoadSceneAsync(scene);
            while (!load.isDone) yield return null;
            // Un momento para leer el consejo.
            while (Time.unscaledTime - covered < 1.3f) yield return null;
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
