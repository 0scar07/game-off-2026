using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Menú Fase → Exportar web: compila la versión WebGL en Builds/Web, lista para subir a itch.io
/// (comprimir la carpeta en un .zip, o subirla con butler). Usa Brotli con "decompression fallback",
/// que funciona en itch y en cualquier servidor sin configurar nada.
/// </summary>
public static class WebBuild
{
    public const string OutputFolder = "Builds/Web";

    // ───────── Cola de exportaciones ─────────
    // Cada plataforma se exporta con el editor YA cambiado a ella y recargado. Si se cambia de
    // plataforma y se exporta en la misma llamada, Unity decide qué recursos de render quitar con
    // los datos de la plataforma anterior: en Windows faltaban los del SSAO y salía la pantalla negra.

    const string QueueKey = "fase_exportar_cola";

    /// <summary>
    /// Logs/auto-build.flag al abrir el editor: vacío = generar niveles y exportar la web; si no,
    /// la lista de plataformas separadas por comas (p. ej. "windows,android,web").
    /// </summary>
    [InitializeOnLoadMethod]
    static void ResumeQueue()
    {
        const string flag = "Logs/auto-build.flag";
        if (System.IO.File.Exists(flag))
        {
            string wanted = System.IO.File.ReadAllText(flag).Trim();
            System.IO.File.Delete(flag);
            if (wanted.Length == 0) { EditorApplication.delayCall += () => { LevelBuilder.BuildAll(); Enqueue("web"); }; return; }
            SessionState.SetString(QueueKey, wanted);
        }
        EditorApplication.delayCall += RunQueue;
    }

    static void Enqueue(string targets)
    {
        string queue = SessionState.GetString(QueueKey, "");
        SessionState.SetString(QueueKey, queue.Length > 0 ? queue + "," + targets : targets);
        RunQueue();
    }

    static void RunQueue()
    {
        if (BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling) { EditorApplication.delayCall += RunQueue; return; }
        var items = SessionState.GetString(QueueKey, "").Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();
        if (items.Count == 0) return;
        var (group, target) = items[0] == "windows" ? (BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64)
            : items[0] == "android" ? (BuildTargetGroup.Android, BuildTarget.Android)
            : (BuildTargetGroup.WebGL, BuildTarget.WebGL);
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            // Al cambiar de plataforma Unity recompila y recarga; ResumeQueue sigue después.
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            EditorApplication.delayCall += RunQueue;
            return;
        }
        SessionState.SetString(QueueKey, string.Join(",", items.Skip(1)));
        if (items[0] == "windows") BuildWindowsNow();
        else if (items[0] == "android") BuildAndroidNow();
        else BuildWebNow();
        if (items.Count > 1) EditorApplication.delayCall += RunQueue;
    }

    [MenuItem("Fase/Exportar web")]
    public static void Build() => Enqueue("web");

    [MenuItem("Fase/Exportar escritorio (Windows)")]
    public static void BuildWindows() => Enqueue("windows");

    [MenuItem("Fase/Exportar Android (APK)")]
    public static void BuildAndroid() => Enqueue("android");

    [MenuItem("Fase/Exportar las tres (Windows, Android y web)")]
    public static void BuildAll() => Enqueue("windows,android,web");

    static void BuildWebNow()
    {
        PlayerSettings.productName = "FASE";
        PlayerSettings.companyName = "0scar07";
        // Brotli comprime un 15-20 % más que Gzip; con "decompression fallback" funciona en itch
        // y en cualquier servidor sin configurar nada.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        PlayerSettings.runInBackground = false;
        // Plantilla propia (Assets/WebGLTemplates/Fase): pantalla completa, barra de carga y aviso de girar el móvil.
        PlayerSettings.WebGL.template = "PROJECT:Fase";
        // Menos código C++ genérico: compila con mucha menos memoria (con 10 GB se quedaba sin ella)
        // y el juego pesa menos; la diferencia de velocidad en este juego no se nota.
        PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.WebGL, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);


        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputFolder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });

        var summary = report.summary;
        string result = summary.result == BuildResult.Succeeded
            ? $"Web exportada en {OutputFolder} ({summary.totalSize / (1024f * 1024f):F1} MB, {summary.totalTime.TotalMinutes:F1} min)"
            : $"La exportación web falló: {summary.result}, {summary.totalErrors} errores";
        System.IO.File.WriteAllText("Logs/web-build.txt", result);
        Debug.Log(result);
    }

    /// <summary>Versión descargable para Windows (Builds/Windows).</summary>
    static void BuildWindowsNow()
    {
        PlayerSettings.productName = "FASE";
        PlayerSettings.companyName = "0scar07";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/FASE.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        });
        string result = report.summary.result == BuildResult.Succeeded
            ? $"Windows exportado en Builds/Windows ({report.summary.totalSize / (1024f * 1024f):F1} MB)"
            : $"La exportación de Windows falló: {report.summary.result}";
        System.IO.File.WriteAllText("Logs/windows-build.txt", result);
        Debug.Log(result);
    }

    /// <summary>APK para instalar a mano (Builds/Android/FASE.apk).</summary>
    static void BuildAndroidNow()
    {
        PlayerSettings.productName = "FASE";
        PlayerSettings.companyName = "0scar07";
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.oscar07.fase");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Android/FASE.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None,
        });
        string result = report.summary.result == BuildResult.Succeeded
            ? $"Android exportado en Builds/Android ({report.summary.totalSize / (1024f * 1024f):F1} MB)"
            : $"La exportación de Android falló: {report.summary.result}";
        System.IO.File.WriteAllText("Logs/android-build.txt", result);
        Debug.Log(result);
    }
}
