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
            // La misma línea en el borde izquierdo: la cortina entra por la derecha y sale por la izquierda.
            foreach (var (name, width, color) in new[] { ("LineaIzq", 10f, new Color(1f, 0.97f, 0.9f)), ("HaloIzq", 70f, new Color(1f, 0.85f, 0.6f, 0.25f)) })
            {
                var left = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
                left.SetParent(curtain, false);
                left.anchorMin = new Vector2(0f, 0f);
                left.anchorMax = new Vector2(0f, 1f);
                left.sizeDelta = new Vector2(width, 0f);
                left.gameObject.AddComponent<Image>().color = color;
            }
            // Número y nombre del nivel que viene.
            var title = new GameObject("Titulo", typeof(RectTransform)).GetComponent<RectTransform>();
            title.SetParent(curtain, false);
            title.anchorMin = new Vector2(0.05f, 0.5f);
            title.anchorMax = new Vector2(0.95f, 0.5f);
            title.sizeDelta = new Vector2(0f, 260f);
            title.anchoredPosition = new Vector2(0f, 60f);
            instance.title = title.gameObject.AddComponent<Text>();
            instance.title.font = UiKit.Font;
            UiKit.SetSize(instance.title, 64);
            instance.title.alignment = TextAnchor.MiddleCenter;
            instance.title.color = Color.white;
            instance.title.horizontalOverflow = HorizontalWrapMode.Wrap;
            instance.title.verticalOverflow = VerticalWrapMode.Overflow;

            // Consejo en la cortina mientras carga.
            var tip = new GameObject("Consejo", typeof(RectTransform)).GetComponent<RectTransform>();
            tip.SetParent(curtain, false);
            tip.anchorMin = new Vector2(0.1f, 0f);
            tip.anchorMax = new Vector2(0.9f, 0f);
            tip.sizeDelta = new Vector2(0f, 120f);
            tip.anchoredPosition = new Vector2(0f, 110f);
            instance.tip = tip.gameObject.AddComponent<Text>();
            instance.tip.font = UiKit.Font;
            UiKit.SetSize(instance.tip, 34);
            instance.tip.alignment = TextAnchor.MiddleCenter;
            instance.tip.color = new Color(0.8f, 0.84f, 1f, 0.9f);
            instance.tip.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Sombra del pliegue, por encima de todo lo de la cortina.
            var shade = new GameObject("Pliegue", typeof(RectTransform)).GetComponent<RectTransform>();
            shade.SetParent(curtain, false);
            shade.anchorMin = Vector2.zero;
            shade.anchorMax = Vector2.one;
            shade.offsetMin = shade.offsetMax = Vector2.zero;
            instance.fold = shade.gameObject.AddComponent<Image>();
            instance.fold.raycastTarget = false;

            instance.curtain = curtain;
            instance.SetCover(-1f);
        }

        /// <summary>-1 = fuera por la izquierda, 0 = tapando, 1 = fuera por la derecha.</summary>
        /// <summary>
        /// La cortina se despliega como una página: entra doblándose desde el borde izquierdo y
        /// sale plegándose hacia el derecho. Una sombra marca el pliegue.
        /// </summary>
        /// <summary>
        /// Barrido con la línea: t = -1 fuera por la derecha, 0 tapando la pantalla, 1 fuera por la
        /// izquierda. La cortina entra empujada por su línea izquierda y sale dejando ver el nivel nuevo
        /// detrás de su línea derecha.
        /// </summary>
        void SetCover(float t)
        {
            float width = ((RectTransform)transform).rect.width + 160f;
            curtain.pivot = new Vector2(0.5f, 0.5f);
            curtain.localRotation = Quaternion.identity;
            curtain.anchoredPosition = new Vector2(-t * width, 0f);
            if (fold != null) fold.color = Color.clear;
            curtain.gameObject.SetActive(t > -0.999f && t < 0.999f);
        }

        Text title;

        /// <summary>"2 · Noche estrellada" si la escena es un nivel de la historia (★ si es secreto).</summary>
        static string TitleFor(string scene)
        {
            var levels = LevelCatalog.Get.levels;
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].scene != scene) continue;
                string number = levels[i].secret ? "★" : (i + 1).ToString();
                return $"<size=150>{number}</size>\n{Loc.Pick(levels[i].name, levels[i].nameEn)}";
            }
            return "";
        }

        Image fold;

        static int caught;

        IEnumerator CatchStars(float since)
        {
            var stars = new System.Collections.Generic.List<RectTransform>();
            var label = new GameObject("Atrapadas", typeof(RectTransform)).GetComponent<RectTransform>();
            label.SetParent(curtain, false);
            label.anchorMin = label.anchorMax = new Vector2(1f, 1f);
            label.anchoredPosition = new Vector2(-120f, -50f);
            label.sizeDelta = new Vector2(200f, 60f);
            var text = label.gameObject.AddComponent<Text>();
            text.font = UiKit.Font; text.fontSize = 34; text.alignment = TextAnchor.MiddleRight; text.color = new Color(1f, 0.85f, 0.45f);
            text.text = caught > 0 ? $"★ {caught}" : "";
            float spawn = 0f;
            var size = ((RectTransform)transform).rect.size;
            while (Time.unscaledTime - since < 1.3f)
            {
                spawn -= Time.unscaledDeltaTime;
                if (spawn <= 0f)
                {
                    spawn = 0.18f;
                    var star = new GameObject("Estrella", typeof(RectTransform)).GetComponent<RectTransform>();
                    star.SetParent(curtain, false);
                    star.sizeDelta = new Vector2(46f, 46f);
                    star.anchoredPosition = new Vector2(Random.Range(-size.x * 0.4f, size.x * 0.4f), size.y * 0.55f);
                    var t = star.gameObject.AddComponent<Text>();
                    t.font = UiKit.Font; t.fontSize = 40; t.alignment = TextAnchor.MiddleCenter; t.text = "★"; t.color = new Color(1f, 0.9f, 0.5f);
                    stars.Add(star);
                }
                Vector2 pointer = UnityEngine.InputSystem.Pointer.current != null ? UnityEngine.InputSystem.Pointer.current.position.ReadValue() : new Vector2(-9999f, 0f);
                for (int i = stars.Count - 1; i >= 0; i--)
                {
                    var s2 = stars[i];
                    s2.anchoredPosition += Vector2.down * 520f * Time.unscaledDeltaTime;
                    if (RectTransformUtility.RectangleContainsScreenPoint(s2, pointer, null))
                    {
                        caught++;
                        text.text = $"★ {caught}";
                        Destroy(s2.gameObject);
                        stars.RemoveAt(i);
                    }
                    else if (s2.anchoredPosition.y < -size.y * 0.6f) { Destroy(s2.gameObject); stars.RemoveAt(i); }
                }
                yield return null;
            }
            foreach (var s3 in stars) Destroy(s3.gameObject);
            Destroy(label.gameObject);
        }

        IEnumerator Run(string scene)
        {
            busy = true;
            Time.timeScale = 1f;
            var t0 = Tips[Random.Range(0, Tips.Length)];
            if (tip != null) tip.text = $"{Loc.Get("tip")}: {Loc.Pick(t0.es, t0.en)}";
            if (title != null) title.text = TitleFor(scene);

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Duration)
            {
                SetCover(Mathf.SmoothStep(-1f, 0f, t));
                yield return null;
            }
            SetCover(0f);
            float covered = Time.unscaledTime;
            var load = SceneManager.LoadSceneAsync(scene);
            while (!load.isDone) yield return null;
            // Un momento para leer el consejo... y atrapar estrellas que caen por la cortina.
            yield return CatchStars(covered);
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
