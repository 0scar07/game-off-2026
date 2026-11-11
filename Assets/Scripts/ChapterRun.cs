using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Contrarreloj de un capítulo (Extras → Retos): todos sus niveles seguidos con un solo
    /// cronómetro, sin pantallas de resultados entre medias. Arriba se ve el tiempo y el tramo de
    /// cada nivel comparado con tu mejor tramo (verde si vas mejor). Al final, el resumen.
    /// </summary>
    public class ChapterRun : MonoBehaviour
    {
        public static bool Active => instance != null;
        static ChapterRun instance;

        int chapter;
        readonly List<string> scenes = new List<string>();
        readonly List<float> splits = new List<float>();
        int index;
        float total, levelTime;
        Text clock, split;

        static string BestKey(int chapter) => "fase_crono_" + chapter;
        static string SplitKey(int chapter, int i) => $"fase_crono_{chapter}_{i}";

        public static float Best(int chapter) => PlayerPrefs.GetFloat(BestKey(chapter), 0f);

        public static void Begin(int chapter)
        {
            if (instance != null) Destroy(instance.gameObject);
            var go = new GameObject("Contrarreloj");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ChapterRun>();
            instance.chapter = chapter;
            foreach (var e in LevelCatalog.Get.levels) if (!e.secret && e.chapter == chapter) instance.scenes.Add(e.scene);
            instance.Build();
            SceneFader.Load(instance.scenes[0]);
        }

        void Build()
        {
            var canvas = UiKit.Canvas("Crono", 35, false);
            canvas.transform.SetParent(transform, false);
            var back = UiKit.Box(canvas.transform, "Fondo", new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(520f, 110f), new Color(0.03f, 0.03f, 0.1f, 0.7f));
            back.rectTransform.pivot = new Vector2(0.5f, 1f);
            clock = UiKit.Text(back.transform, "Tiempo", "", new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(500f, 60f), 48);
            split = UiKit.Text(back.transform, "Tramo", "", new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(500f, 36f), 24, TextAnchor.MiddleCenter, UiKit.Soft);
            SceneManager.sceneLoaded += OnLoaded;
        }

        void OnDestroy() => SceneManager.sceneLoaded -= OnLoaded;

        void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            // Si se sale a otra escena que no es el siguiente nivel (el menú desde la pausa), se acaba.
            if (index < scenes.Count && scene.name != scenes[index]) Destroy(gameObject);
            levelTime = 0f;
        }

        void Update()
        {
            if (PauseMenu.Paused || FindAnyObjectByType<PlayerController>() == null) return;
            total += Time.unscaledDeltaTime;
            levelTime += Time.unscaledDeltaTime;
            clock.text = $"⏱ {Hud.FormatTime(total)}";
            float best = PlayerPrefs.GetFloat(SplitKey(chapter, index), 0f);
            string level = $"{index + 1}/{scenes.Count}";
            if (best <= 0f) split.text = $"{level}   ·   {Hud.FormatTime(levelTime)}";
            else
            {
                float diff = levelTime - best;
                split.text = $"{level}   ·   {Hud.FormatTime(levelTime)}   {(diff < 0f ? "<color=#7fdc6a>" : "<color=#ff8a8a>")}{(diff < 0f ? "−" : "+")}{Mathf.Abs(diff):0.0}</color>";
            }
        }

        /// <summary>Lo llama ResultsScreen al llegar a la meta: siguiente nivel o resumen.</summary>
        public static void LevelDone()
        {
            var run = instance;
            if (run == null) return;
            run.splits.Add(run.levelTime);
            run.index++;
            if (run.index < run.scenes.Count) SceneFader.Load(run.scenes[run.index]);
            else run.Finish();
        }

        void Finish()
        {
            bool record = Best(chapter) <= 0f || total < Best(chapter);
            if (record)
            {
                PlayerPrefs.SetFloat(BestKey(chapter), total);
                for (int i = 0; i < splits.Count; i++) PlayerPrefs.SetFloat(SplitKey(chapter, i), splits[i]);
                PlayerPrefs.Save();
            }
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("FinCrono", 45);
            var shade = UiKit.Fill(canvas.transform, "Fondo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.02f, 0.02f, 0.08f, 0.85f);
            UiKit.Text(canvas.transform, "Titulo", Loc.Pick($"Capítulo {chapter} contrarreloj", $"Chapter {chapter} time trial", $"Capítulo {chapter} contra o relógio", $"Chapitre {chapter} contre-la-montre"), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1400f, 100f), 72);
            UiKit.Text(canvas.transform, "Total", $"⏱ {Hud.FormatTime(total)}", new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(1000f, 130f), 110, TextAnchor.MiddleCenter, UiKit.Gold);
            UiKit.Text(canvas.transform, "Record", record ? Loc.Get("record") : $"{Loc.Get("best")}: {Hud.FormatTime(Best(chapter))}", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1000f, 60f), 40, TextAnchor.MiddleCenter, UiKit.Soft);
            for (int i = 0; i < splits.Count; i++)
                UiKit.Text(canvas.transform, "Tramo", $"{i + 1} · {Hud.FormatTime(splits[i])}", new Vector2(0.5f, 0.5f), new Vector2((i % 4 - 1.5f) * 300f, -40f - (i / 4) * 50f), new Vector2(280f, 46f), 30);
            UiKit.Button(canvas.transform, "Menu", Loc.Get("menu"), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(380f, 100f), () => { Destroy(gameObject); SceneFader.Load(PauseMenu.MenuScene); }, 44);
            canvas.transform.SetParent(transform, false);
            enabled = false;
        }
    }
}
