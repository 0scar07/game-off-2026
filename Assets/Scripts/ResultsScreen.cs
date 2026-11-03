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
            string moon = hud != null && hud.fragmentsTotal > 0 ? $"   ·   ☾ {hud.Fragments}/{hud.fragmentsTotal}" : "";
            int deaths = hud != null ? hud.Deaths : 0;
            string falls = Loc.English ? (deaths == 1 ? "1 fall" : $"{deaths} falls") : (deaths == 1 ? "1 caída" : $"{deaths} caídas");
            UiKit.Box(root, "FondoTiempo", new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(900f, 80f), new Color(1f, 1f, 1f, 0.1f));
            UiKit.Text(root, "Tiempo", $"⏱ {Hud.FormatTime(time)}{moon}   ·   {falls}", new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(900f, 80f), 40);
            string note = LevelGoal.LastWasNightRoute ? Loc.Pick("☾ ¡Ruta de la noche!", "☾ Night route!")
                : assist ? Loc.Get("assistnomedal") : better ? Loc.Get("newmedal") + " " + Medals.Name(medal) : hud != null && hud.IsRecord ? Loc.Get("record") : "";
            var noteText = UiKit.Text(root, "Nota", note, new Vector2(0.5f, 0.5f), new Vector2(0f, 245f), new Vector2(1200f, 50f), 36, TextAnchor.MiddleCenter, UiKit.Gold);

            // Desafíos del nivel.
            int done = 0;
            if (hud != null)
            {
                if (hud.Deaths == 0) done |= Stats.ChallengeNoFall;
                if (hud.fragmentsTotal == 0 || hud.Fragments >= hud.fragmentsTotal) done |= Stats.ChallengeAllMoon;
                if (hud.Grabs <= Stats.GrabPar) done |= Stats.ChallengeFewGrabs;
                if (!hud.Daily) Stats.SaveChallenges(level, done);
            }
            var challenges = new[]
            {
                (Stats.ChallengeNoFall, Loc.Pick("Sin caer", "No falls")),
                (Stats.ChallengeAllMoon, Loc.Pick("Toda la luna", "Whole moon")),
                (Stats.ChallengeFewGrabs, Loc.Pick($"Línea ≤ {Stats.GrabPar} veces", $"Line ≤ {Stats.GrabPar} times")),
            };
            for (int i = 0; i < challenges.Length; i++)
            {
                bool ok = (done & challenges[i].Item1) != 0;
                float cx = (i - 1) * 400f;
                UiKit.Box(root, "Reto" + i, new Vector2(0.5f, 0.5f), new Vector2(cx, -262f), new Vector2(380f, 48f), ok ? new Color(0.45f, 0.85f, 0.55f, 0.25f) : new Color(1f, 1f, 1f, 0.06f));
                UiKit.Text(root, "RetoTexto" + i, (ok ? "✔  " : "✖  ") + challenges[i].Item2, new Vector2(0.5f, 0.5f), new Vector2(cx, -262f), new Vector2(380f, 48f), 26, TextAnchor.MiddleCenter, ok ? new Color(0.8f, 1f, 0.85f) : new Color(0.6f, 0.6f, 0.75f));
            }
            UiKit.Text(root, "Estadisticas", Stats.Summary(), new Vector2(0f, 0f), new Vector2(40f, 30f), new Vector2(1200f, 40f), 24, TextAnchor.LowerLeft, UiKit.Soft).rectTransform.pivot = new Vector2(0f, 0f);

            // Botones.
            var next = UiKit.Button(root, "Siguiente", Loc.Get("next"), new Vector2(0.5f, 0.5f), new Vector2(330f, -385f), new Vector2(380f, 110f), Next, 50);
            UiKit.Button(root, "Repetir", Loc.Get("retry"), new Vector2(0.5f, 0.5f), new Vector2(-80f, -385f), new Vector2(360f, 110f), Retry, 46);
            var share = UiKit.Button(root, "Compartir", Loc.Get("share"), new Vector2(0.5f, 0.5f), new Vector2(-470f, -385f), new Vector2(360f, 110f), null, 46);
            shareLabel = share.GetComponentInChildren<Text>();
            if (Ghost.LastRun != null && Ghost.LastRun.Length > 10)
                UiKit.Button(root, "Repeticion", "▶ " + Loc.Get("replay"), new Vector2(1f, 0f), new Vector2(-40f, 30f), new Vector2(300f, 80f), () => StartCoroutine(Replay(canvas)), 32)
                    .GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
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
            if (medal >= Medals.Gold) StartCoroutine(Confetti(root));
            noteText.transform.localScale = Vector3.one * 1.2f;
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime) { noteText.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, t / 0.3f); yield return null; }
        }

        /// <summary>Repetición: Lumi de luz rehace tu recorrido a triple velocidad y la cámara la sigue.</summary>
        IEnumerator Replay(Canvas results)
        {
            var run = Ghost.LastRun;
            var player = FindAnyObjectByType<PlayerController>();
            var cam = Camera.main;
            if (run == null || player == null || cam == null) yield break;
            results.enabled = false;
            var copy = Ghost.BuildCopy(player, new Color(1f, 0.85f, 0.55f, 0.6f));
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
            Vector3 camStart = cam.transform.position;
            var bars = UiKit.Canvas("Repeticion", 41);
            UiKit.Box(bars.transform, "Arriba", new Vector2(0.5f, 1f), Vector2.zero, new Vector2(4000f, 110f), Color.black, false);
            UiKit.Box(bars.transform, "Abajo", new Vector2(0.5f, 0f), Vector2.zero, new Vector2(4000f, 110f), Color.black, false);
            UiKit.Text(bars.transform, "Rotulo", "▶ " + Loc.Get("replay") + "  ×3", new Vector2(0f, 1f), new Vector2(60f, -55f), new Vector2(600f, 60f), 36, TextAnchor.MiddleLeft, UiKit.Gold).rectTransform.pivot = new Vector2(0f, 0.5f);
            float duration = run.Length * 0.05f / 3f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float f = t / duration * (run.Length - 1);
                int i = Mathf.Min((int)f, run.Length - 2);
                Vector3 p = Vector2.Lerp(run[i], run[i + 1], f - i);
                copy.position = p + player.visual.localPosition;
                float dx = run[i + 1].x - run[i].x;
                if (Mathf.Abs(dx) > 0.01f) copy.rotation = Quaternion.Euler(0f, -Mathf.Sign(dx) * 90f, 0f);
                cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(p.x, p.y + 1.5f, camStart.z), Time.unscaledDeltaTime * 5f);
                yield return null;
            }
            Destroy(copy.gameObject);
            Destroy(bars.gameObject);
            cam.transform.position = camStart;
            results.enabled = true;
        }

        /// <summary>Lluvia de estrellitas doradas al sacar oro (o Luna).</summary>
        IEnumerator Confetti(Transform root)
        {
            var colors = new[] { new Color(1f, 0.83f, 0.35f), new Color(1f, 0.95f, 0.7f), new Color(1f, 0.6f, 0.3f), new Color(0.75f, 0.85f, 1f) };
            var pieces = new RectTransform[90];
            var velocity = new Vector2[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i] = UiKit.Box(root, "Confeti", new Vector2(0.5f, 0.5f), new Vector2(Random.Range(-500f, 500f), 120f), new Vector2(14f, 22f), colors[i % colors.Length], false).rectTransform;
                pieces[i].localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                velocity[i] = new Vector2(Random.Range(-500f, 500f), Random.Range(500f, 1100f));
            }
            for (float t = 0f; t < 3f; t += Time.unscaledDeltaTime)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    velocity[i] += Vector2.down * 1400f * Time.unscaledDeltaTime;
                    velocity[i].x *= 1f - Time.unscaledDeltaTime;
                    pieces[i].anchoredPosition += velocity[i] * Time.unscaledDeltaTime;
                    pieces[i].Rotate(0f, 0f, 360f * Time.unscaledDeltaTime * (i % 2 == 0 ? 1f : -1f));
                }
                yield return null;
            }
            foreach (var p in pieces) Destroy(p.gameObject);
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
