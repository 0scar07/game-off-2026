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
}
