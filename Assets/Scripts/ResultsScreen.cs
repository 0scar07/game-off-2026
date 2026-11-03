using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Pantalla de "¡Nivel completado!": tiempo, fragmentos, las cuatro medallas (la conseguida
    /// brilla y entra con un golpe) y botones Siguiente, Repetir y Compartir.
    /// Enter/Espacio = siguiente, R = repetir.
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        string nextScene;
        Text shareLabel;
        bool leaving;

        public static void Show(string nextScene, float delay)
        {
            var screen = new GameObject("Resultados").AddComponent<ResultsScreen>();
            screen.nextScene = nextScene;
            screen.StartCoroutine(screen.Run(delay));
        }

        IEnumerator Run(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            var hud = Hud.Instance;
            int level = hud != null ? hud.levelNumber : 1;
            float time = hud != null ? hud.Elapsed : 0f;
            float[] times = hud != null ? hud.medalTimes : null;
            bool assist = Settings.Assist;
            int medal = assist || hud == null || hud.Daily ? Medals.None : Medals.For(time, times);
            bool better = medal > 0 && Medals.Save(level, medal);
            if (better) Achievements.CheckMedals();

            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("Resultados", 40);
            canvas.transform.SetParent(transform, false);
            var root = canvas.transform;
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            var shade = UiKit.Fill(root, "Fondo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.02f, 0.02f, 0.08f, 0.62f);

            string name = hud != null ? Loc.Pick(hud.levelName, hud.levelNameEn) : "";
            UiKit.Text(root, "Titulo", Loc.Get("complete"), new Vector2(0.5f, 0.5f), new Vector2(0f, 400f), new Vector2(1600f, 130f), 104);
            UiKit.Text(root, "Nivel", hud != null && hud.Daily ? Loc.Get("daily") : $"{level} · {name}", new Vector2(0.5f, 0.5f), new Vector2(0f, 310f), new Vector2(1400f, 60f), 44, TextAnchor.MiddleCenter, UiKit.Soft);

            // Medallas.
            var medals = new RectTransform[4];
            for (int i = 0; i < 4; i++)
            {
                int m = i + 1;
                bool got = medal >= m;
                bool hidden = m == Medals.Moon && !got && Medals.Best(level) < Medals.Moon;
                float x = (i - 1.5f) * 300f;
                var holder = UiKit.Rect(root, "Medalla" + m, new Vector2(0.5f, 0.5f), new Vector2(x, 60f), new Vector2(240f, 340f));
                medals[i] = holder;
                var c = got ? Medals.Colors[m] : Medals.Colors[0];
                if (got) UiKit.Box(holder, "Brillo", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(260f, 260f), new Color(1f, 0.85f, 0.5f, 0.25f));
                UiKit.Box(holder, "Cinta1", new Vector2(0.5f, 0.5f), new Vector2(-34f, -50f), new Vector2(46f, 120f), got ? new Color(1f, 0.48f, 0.24f) : c, false).rectTransform.localRotation = Quaternion.Euler(0, 0, -12f);
                UiKit.Box(holder, "Cinta2", new Vector2(0.5f, 0.5f), new Vector2(34f, -50f), new Vector2(46f, 120f), got ? new Color(0.91f, 0.35f, 0.16f) : c, false).rectTransform.localRotation = Quaternion.Euler(0, 0, 12f);
                UiKit.Box(holder, "Disco", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(170f, 170f), c);
                UiKit.Box(holder, "Brillo", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(124f, 124f), new Color(1f, 1f, 1f, got ? 0.2f : 0.06f));
                UiKit.Text(holder, "Icono", m == Medals.Moon ? "☾" : "★", new Vector2(0.5f, 0.5f), new Vector2(0f, 42f), new Vector2(120f, 120f), 78, TextAnchor.MiddleCenter, new Color(0f, 0f, 0f, got ? 0.25f : 0.35f));
                UiKit.Text(holder, "Nombre", hidden ? "???" : Medals.Name(m), new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(260f, 50f), 40, TextAnchor.MiddleCenter, got ? Color.white : new Color(0.6f, 0.6f, 0.75f));
                string limit = times != null && times.Length >= 4 && !hidden ? Hud.FormatTime(times[m - 1]) : "--:--";
                UiKit.Text(holder, "Tiempo", limit, new Vector2(0.5f, 0.5f), new Vector2(0f, -185f), new Vector2(260f, 40f), 30, TextAnchor.MiddleCenter, got ? UiKit.Gold : new Color(0.6f, 0.6f, 0.75f));
                holder.localScale = Vector3.zero;
            }

            // Tiempo y fragmentos.
            string moon = hud != null && hud.fragmentsTotal > 0 ? $"    ·    ☾ {hud.Fragments}/{hud.fragmentsTotal}" : "";
            UiKit.Box(root, "FondoTiempo", new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(760f, 84f), new Color(1f, 1f, 1f, 0.1f));
            UiKit.Text(root, "Tiempo", $"⏱ {Hud.FormatTime(time)}{moon}", new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(760f, 84f), 44);
            string note = assist ? Loc.Get("assistnomedal") : better ? Loc.Get("newmedal") + " " + Medals.Name(medal) : hud != null && hud.IsRecord ? Loc.Get("record") : "";
            var noteText = UiKit.Text(root, "Nota", note, new Vector2(0.5f, 0.5f), new Vector2(0f, -275f), new Vector2(1200f, 50f), 36, TextAnchor.MiddleCenter, UiKit.Gold);

            // Botones.
            var next = UiKit.Button(root, "Siguiente", Loc.Get("next"), new Vector2(0.5f, 0.5f), new Vector2(330f, -385f), new Vector2(380f, 110f), Next, 50);
            UiKit.Button(root, "Repetir", Loc.Get("retry"), new Vector2(0.5f, 0.5f), new Vector2(-80f, -385f), new Vector2(360f, 110f), Retry, 46);
            var share = UiKit.Button(root, "Compartir", Loc.Get("share"), new Vector2(0.5f, 0.5f), new Vector2(-470f, -385f), new Vector2(360f, 110f), null, 46);
            shareLabel = share.GetComponentInChildren<Text>();
            int shownMedal = medal;
            share.onClick.AddListener(() => shareLabel.text = Loc.Get(Share.Text(Share.LevelTime(level, name, time, shownMedal))));
            next.Select();

            // Entrada: fundido y medallas una a una con un pequeño golpe.
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime) { group.alpha = t / 0.4f; yield return null; }
            group.alpha = 1f;
            for (int i = 0; i < 4; i++)
            {
                bool got = medal >= i + 1;
                for (float t = 0f; t < 0.22f; t += Time.unscaledDeltaTime)
                {
                    float k = t / 0.22f;
                    medals[i].localScale = Vector3.one * (got ? 1f + Mathf.Sin(k * Mathf.PI) * 0.35f : k);
                    yield return null;
                }
                medals[i].localScale = Vector3.one;
                if (got) Share.Vibrate(20);
            }
            noteText.transform.localScale = Vector3.one * 1.2f;
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime) { noteText.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, t / 0.3f); yield return null; }
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || leaving) return;
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) Next();
            else if (kb.rKey.wasPressedThisFrame) Retry();
        }

        void Next()
        {
            if (leaving) return;
            leaving = true;
            SceneFader.Load(string.IsNullOrEmpty(nextScene) ? SceneManager.GetActiveScene().name : nextScene);
        }

        void Retry()
        {
            if (leaving) return;
            leaving = true;
            SceneFader.Load(SceneManager.GetActiveScene().name);
        }
    }
}
