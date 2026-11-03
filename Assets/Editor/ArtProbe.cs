using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menú Fase → Medir modelos: escribe en Logs/modelos.txt el tamaño de cada modelo importado,
/// sus materiales y las animaciones que trae. Sirve para colocar el arte a la escala correcta.
/// </summary>
public static class ArtProbe
{
    [MenuItem("Fase/Medir modelos")]
    public static void Probe()
    {
        var sb = new StringBuilder();
        foreach (string folder in new[] { "Assets/Art/Quaternius", "Assets/Art/Platformer", "Assets/Art/Characters", "Assets/Art/Nature" })
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) continue;
                var instance = Object.Instantiate(model);
                var bounds = new Bounds(instance.transform.position, Vector3.zero);
                bool first = true;
                var materials = new StringBuilder();
                foreach (var r in instance.GetComponentsInChildren<Renderer>())
                {
                    if (first) { bounds = r.bounds; first = false; } else bounds.Encapsulate(r.bounds);
                    foreach (var m in r.sharedMaterials)
                        if (m != null) materials.Append($"{m.name}{(m.mainTexture ? "[tex]" : "")}#{ColorUtility.ToHtmlStringRGB(m.color)} ");
                }
                Object.DestroyImmediate(instance);
                var clips = new StringBuilder();
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview")) clips.Append(clip.name + " ");
                sb.AppendLine($"{Path.GetFileNameWithoutExtension(path)} | center {bounds.center:F2} size {bounds.size:F2} | rot {model.transform.localEulerAngles:F0} scale {model.transform.localScale:F2} | {materials}| anims: {clips}");
                if (path.Contains("Quaternius") && (path.Contains("Character") || path.Contains("Enemy") || path.Contains("Bee")))
                {
                    var bones = new StringBuilder();
                    foreach (var t in model.GetComponentsInChildren<Transform>()) bones.Append(t.name + " ");
                    sb.AppendLine("   huesos: " + bones);
                }
            }
        }
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/modelos.txt", sb.ToString());
        Debug.Log("Medidas escritas en Logs/modelos.txt");
    }
}
