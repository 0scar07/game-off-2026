using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Menú Fase → Exportar web: compila la versión WebGL en Builds/Web, lista para subir a itch.io
/// (comprimir la carpeta en un .zip, o subirla con butler). Usa Gzip con "decompression fallback",
/// que funciona en itch y en cualquier servidor sin configurar nada.
/// </summary>
public static class WebBuild
{
    public const string OutputFolder = "Builds/Web";

    /// <summary>
    /// Si existe Logs/auto-build.flag al abrir el editor, genera los niveles y exporta la web
    /// una sola vez (borra el aviso). Sirve para lanzar todo sin tocar menús.
    /// </summary>
    [InitializeOnLoadMethod]
    static void AutoBuild()
    {
        const string flag = "Logs/auto-build.flag";
        if (!System.IO.File.Exists(flag)) return;
        System.IO.File.Delete(flag);
        EditorApplication.delayCall += () =>
        {
            LevelBuilder.BuildAll();
            Build();
        };
    }

    [MenuItem("Fase/Exportar web")]
    public static void Build()
    {
        PlayerSettings.productName = "FASE";
        PlayerSettings.companyName = "0scar07";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        PlayerSettings.runInBackground = false;
        // Plantilla propia (Assets/WebGLTemplates/Fase): pantalla completa, barra de carga y aviso de girar el móvil.
        PlayerSettings.WebGL.template = "PROJECT:Fase";

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

    /// <summary>Menú Fase → Exportar escritorio: versión descargable para Windows (Builds/Windows).</summary>
    [MenuItem("Fase/Exportar escritorio (Windows)")]
    public static void BuildWindows()
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

    /// <summary>Menú Fase → Exportar Android: APK para instalar a mano (Builds/Android/FASE.apk).</summary>
    [MenuItem("Fase/Exportar Android (APK)")]
    public static void BuildAndroid()
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
