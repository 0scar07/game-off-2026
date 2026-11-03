using System.IO;
using Fase;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Piezas sueltas (Resources/Piezas/*.prefab) para montar niveles mientras se juega: el editor de
/// niveles y el modo sin fin. Cada pieza es una casilla hecha con las mismas funciones que los
/// niveles normales, así que se ve y se comporta igual.
/// </summary>
public static partial class LevelBuilder
{
    const string PiecesFolder = "Assets/Resources/Piezas";

    /// <summary>Letra del mapa → nombre del prefab (el suelo tiene dos: con hierba y de tierra).</summary>
    public static readonly (char code, string name)[] PieceNames =
    {
        ('#', "suelo"), ('t', "tierra"), ('D', "caja"), ('N', "hielo"), ('d', "tabla"), ('n', "placa"), ('~', "lago"),
        ('^', "pinchos"), ('F', "muelle"), ('M', "sombra"), ('B', "abeja"), ('X', "sierra"), ('*', "fragmento"),
        ('K', "cofre"), ('C', "control"), ('G', "meta"), ('O', "relojsol"), ('Y', "luciernaga"), ('Z', "cajacambia"),
    };

    static void BuildPieces(Biome biome)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(PiecesFolder)) AssetDatabase.CreateFolder("Assets/Resources", "Piezas");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        foreach (var (code, name) in PieceNames)
        {
            var root = new GameObject(name).transform;
            var cell = Vector3.zero;
            switch (code)
            {
                case '#': case 't': case 'D': case 'N': case 'd': case 'n': case '~':
                {
                    // Una casilla con vecinas iguales a los lados (pieza central del modelo modular).
                    char c = code == 't' ? '#' : code;
                    string top = code == 't' ? "#####" : "     ";
                    var level = new Level { name = "pieza", chapter = 1, rows = new[] { top, new string(c, 5), "#####" } };
                    BuildRun(level, c, 2, 2, 1, root);
                    // BuildRun coloca la casilla en x = 2.5, y = 1: se lleva al origen.
                    foreach (Transform child in root) child.position -= new Vector3(2.5f, 1f, 0f);
                    break;
                }
                case '^': BuildSpikes(cell, root); break;
                case 'F': BuildSpring(cell, root); break;
                case 'M': BuildMonster(cell, root, false); break;
                case 'B': BuildMonster(cell, root, true); break;
                case 'X': BuildSaw(cell, root); break;
                case '*': BuildFragment(cell, root); break;
                case 'K': BuildChest(cell, root); break;
                case 'C': BuildCheckpoint(cell, root); break;
                case 'G': BuildGoal(cell, "", root); break;
                case 'O': BuildSunPlatform(cell, root); break;
                case 'Y': BuildGlowTile(cell, root); break;
                case 'Z': BuildChangingBox(cell, root); break;
            }
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, $"{PiecesFolder}/{name}.prefab");
            Object.DestroyImmediate(root.gameObject);
        }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Escena base para el editor de niveles (Taller) y el modo sin fin (SinFin).</summary>
    static string BuildRuntimeScene(string sceneName, int width, bool endless)
    {
        var rows = new string[14];
        for (int i = 0; i < rows.Length; i++) rows[i] = "";
        var level = new Level
        {
            file = sceneName + ".txt", name = endless ? "Sin fin" : "Taller", nameEn = endless ? "Endless" : "Workshop",
            hint = endless ? "¿Hasta dónde llegas?" : "", hintEn = endless ? "How far can you go?" : "",
            line = 6f, chapter = endless ? 3 : 1, rows = rows, sandbox = true,
        };
        // Una fila invisible al fondo con el ancho completo, para que el cielo y la cámara cubran todo.
        level.rows[13] = new string(' ', width);
        string path = Build(level, 0, endless ? MenuSceneName : sceneName);
        var scene = EditorSceneManager.OpenScene(path);
        var runtime = new GameObject("NivelEnJuego").AddComponent<RuntimeLevel>();
        if (endless) runtime.gameObject.AddComponent<EndlessRunner>();
        else runtime.gameObject.AddComponent<Workshop>();
        EditorSceneManager.SaveScene(scene);
        return path;
    }
}
