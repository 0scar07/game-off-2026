using UnityEditor;

/// <summary>
/// Los FBX de Quaternius vienen de Blender con los ejes cambiados: sin esto, las animaciones
/// tumban al personaje. "Bake Axis Conversion" corrige los ejes al importar.
/// </summary>
public class QuaterniusImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!assetPath.StartsWith("Assets/Art/Quaternius/")) return;
        var importer = (ModelImporter)assetImporter;
        importer.bakeAxisConversion = true;
        importer.animationType = ModelImporterAnimationType.Generic;
    }
}
