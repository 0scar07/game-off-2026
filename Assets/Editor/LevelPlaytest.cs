using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fase;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fase → Probar niveles jugando: entra en Play y deja que PlayBot juegue cada nivel de la
/// historia (y los extra) uno detrás de otro. Apunta en Logs/prueba-jugada.txt si llegó a la
/// meta, en cuánto tiempo, cuántas caídas y hasta dónde llegó si no. Sirve para encontrar niveles
/// rotos o bloqueos sin jugarlos a mano; un "no llegó" en un puzle difícil es normal.
/// </summary>
[InitializeOnLoad]
public static class LevelPlaytest
{
    const string Flag = "fase_prueba_jugada";
    const string Report = "Logs/prueba-jugada.txt";
    const float Limit = 90f;

    static LevelPlaytest() => EditorApplication.playModeStateChanged += OnPlayMode;

    [MenuItem("Fase/Probar niveles jugando (bot)")]
    static void Start()
    {
        if (EditorApplication.isPlaying) return;
        EditorPrefs.SetBool(Flag, true);
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
        EditorApplication.isPlaying = true;
    }

    static void OnPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool(Flag, false))
        {
            EditorPrefs.SetBool(Flag, false);
            var runner = new GameObject("PruebaJugada").AddComponent<UiRunner>();
            Object.DontDestroyOnLoad(runner.gameObject);
            runner.StartCoroutine(Run());
        }
        if (state == PlayModeStateChange.ExitingPlayMode) PlayBot.Enable(false);
    }

    static IEnumerator Run()
    {
        var levels = LevelCatalog.Get.levels;
        var report = new StringBuilder($"Prueba jugada por el bot · {System.DateTime.Now:yyyy-MM-dd HH:mm}\n\n");
        int reached = 0;
        PlayBot.Enable(true);
        for (int i = 0; i < levels.Length; i++)
        {
            var entry = levels[i];
            bool goal = false;
            int deaths = 0;
            System.Action onGoal = () => goal = true, onDied = () => deaths++;
            GameEvents.GoalReached += onGoal;
            GameEvents.Died += onDied;
            Time.timeScale = 1f;
            SceneManager.LoadScene(entry.scene);
            yield return null;
            yield return null;
            var bot = new GameObject("Bot").AddComponent<PlayBot>();
            var player = Object.FindAnyObjectByType<PlayerController>();
            var target = Object.FindAnyObjectByType<LevelGoal>();
            float startX = player != null ? player.transform.position.x : 0f, endX = target != null ? target.transform.position.x : startX + 1f, best = startX;
            float t = 0f;
            while (!goal && t < Limit && deaths < 25)
            {
                t += Time.deltaTime;
                if (player != null) best = Mathf.Max(best, player.transform.position.x);
                yield return null;
            }
            GameEvents.GoalReached -= onGoal;
            GameEvents.Died -= onDied;
            int progress = Mathf.RoundToInt(100f * Mathf.Clamp01((best - startX) / Mathf.Max(1f, endX - startX)));
            if (goal) reached++;
            string line = goal ? $"✔ {i + 1,3}. {entry.name,-28} meta en {t,5:0.0} s, {deaths} caídas"
                               : $"✖ {i + 1,3}. {entry.name,-28} no llegó ({progress} % del camino, {deaths} caídas{(t >= Limit ? ", sin tiempo" : "")})";
            report.AppendLine(line);
            File.WriteAllText(Report, report.ToString());
            Object.Destroy(bot.gameObject);
        }
        PlayBot.Enable(false);
        report.AppendLine($"\nLlegó a la meta en {reached} de {levels.Length} niveles.");
        File.WriteAllText(Report, report.ToString());
        Debug.Log($"Prueba jugada: {reached}/{levels.Length} niveles. Informe en {Report}");
        EditorApplication.isPlaying = false;
    }
}
