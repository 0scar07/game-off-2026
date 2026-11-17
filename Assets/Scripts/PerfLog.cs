using System.Globalization;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Medición de rendimiento desde la línea de comandos (solo se activa con argumentos; en una
    /// partida normal no hace nada). Ejemplo en Windows:
    ///   FASE.exe -medir -calidad 4 -nivel 05-criaturas -salir 40
    /// Cada 5 s escribe en Player.log: FPS medios, tiempo medio y peor fotograma, memoria y calidad.
    /// -calidad N  elige la calidad gráfica (0 Auto, 1 Baja, 2 Media, 3 Alta, 4 Máxima) y la guarda.
    /// -nivel X    carga esa escena al empezar (sin pasar por el menú).
    /// -salir S    cierra el juego tras S segundos.
    /// </summary>
    public class PerfLog : MonoBehaviour
    {
        float window, worst, quitAfter = -1f, started;
        int frames;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            bool measure = false;
            string level = null;
            float quit = -1f;
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].ToLowerInvariant();
                string next = i + 1 < args.Length ? args[i + 1] : "";
                if (a == "-medir") measure = true;
                else if (a == "-calidad" && int.TryParse(next, out int tier)) Settings.GraphicsTier = tier;
                else if (a == "-nivel" && next.Length > 0) level = next;
                else if (a == "-salir" && float.TryParse(next, NumberStyles.Float, CultureInfo.InvariantCulture, out float s)) quit = s;
            }
            if (!measure && level == null && quit < 0f) return;
            var go = new GameObject("MedirRendimiento");
            DontDestroyOnLoad(go);
            var log = go.AddComponent<PerfLog>();
            log.quitAfter = quit;
            log.enabled = measure || quit > 0f;
            log.measuring = measure;
            if (level != null) SceneManager.LoadScene(level);
        }

        bool measuring;

        void Start() => started = Time.unscaledTime;

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (quitAfter > 0f && Time.unscaledTime - started > quitAfter) { Application.Quit(); return; }
            if (!measuring) return;
            frames++;
            window += dt;
            worst = Mathf.Max(worst, dt);
            if (window < 5f) return;
            var inv = CultureInfo.InvariantCulture;
            float scale = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp ? urp.renderScale : 1f;
            Debug.Log(string.Format(inv,
                "[Medir] escena={0} fps={1:0.0} ms={2:0.00} peor_ms={3:0.0} calidad={4} perfil={5} escala={6:0.00} pantalla={7}x{8} mono_mb={9:0.0} total_mb={10:0.0}",
                SceneManager.GetActiveScene().name, frames / window, window / frames * 1000f, worst * 1000f,
                Settings.GraphicsTier, QualitySettings.names[QualitySettings.GetQualityLevel()], scale,
                Screen.width, Screen.height, Profiler.GetMonoUsedSizeLong() / 1048576f, Profiler.GetTotalAllocatedMemoryLong() / 1048576f));
            frames = 0;
            window = worst = 0f;
        }
    }
}
