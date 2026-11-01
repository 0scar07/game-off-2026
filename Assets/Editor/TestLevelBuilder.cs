using Fase;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Menú Fase → Crear nivel de prueba: monta desde cero la escena Assets/Scenes/Prueba.unity
/// con el suelo, el puente de día, las estrellas de noche, el muro de noche, la meta,
/// el personaje, la cámara y la línea. Se puede volver a ejecutar para regenerarla.
/// </summary>
public static class TestLevelBuilder
{
    const string MaterialsFolder = "Assets/Materials";
    const string ScenePath = "Assets/Scenes/Prueba.unity";

    [MenuItem("Fase/Crear nivel de prueba")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Materiales: cada uno dice a qué mundo pertenece (0 día, 1 noche, 2 ambos).
        var rock = Mat("Roca", new Color(0.55f, 0.5f, 0.45f), 2);
        var grass = Mat("Hierba", new Color(0.45f, 0.75f, 0.35f), 2);
        var wood = Mat("Madera", new Color(0.7f, 0.48f, 0.25f), 0);
        var star = Mat("Estrella", new Color(0.55f, 0.5f, 1f), 1, new Color(0.25f, 0.2f, 0.7f));
        var nightWall = Mat("MuroNoche", new Color(0.35f, 0.2f, 0.6f), 1, new Color(0.25f, 0.08f, 0.45f));
        var skyDay = Mat("CieloDia", Color.black, 0, new Color(0.35f, 0.55f, 0.75f));
        var skyNight = Mat("CieloNoche", Color.black, 1, new Color(0.006f, 0.007f, 0.03f));
        var sun = Mat("Sol", Color.black, 0, new Color(1.6f, 1.3f, 0.7f));
        var starDot = Mat("PuntoEstrella", Color.black, 1, new Color(1.2f, 1.2f, 1.5f));
        var player = Mat("Jugador", new Color(1f, 0.55f, 0.25f), 2, new Color(0.15f, 0.05f, 0f));
        var lineMat = Mat("Linea", Color.black, 2, new Color(1.8f, 1.6f, 1.2f));
        var goalMat = Mat("Meta", new Color(0.3f, 1f, 0.6f), 2, new Color(0.4f, 1.6f, 0.8f));

        // Luz y ambiente.
        var sunLight = new GameObject("Luz").AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.intensity = 1.3f;
        sunLight.shadows = LightShadows.Soft;
        sunLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.48f, 0.55f);

        // La línea empieza a la izquierda: el puente (de día) aún no existe.
        var line = new GameObject("Linea").AddComponent<ShiftLine>();
        line.x = -6f;
        line.minX = -14f;
        line.maxX = 17f;
        var lineVisual = Block("DibujoLinea", new Vector3(0f, 5f, -1.6f), new Vector3(0.12f, 60f, 0.12f), lineMat, false);
        lineVisual.AddComponent<LineVisual>();

        // Fondo: el mismo cielo en dos versiones.
        Block("CieloDia", new Vector3(0f, 5f, 20f), new Vector3(300f, 120f, 1f), skyDay, false);
        Block("CieloNoche", new Vector3(0f, 5f, 20f), new Vector3(300f, 120f, 1f), skyNight, false);
        Sphere("Sol", new Vector3(-6f, 11f, 18f), 4f, sun);
        var random = new System.Random(7);
        for (int i = 0; i < 70; i++)
            Sphere("Estrella", new Vector3(random.Next(-40, 40), random.Next(-2, 22), 18f), 0.12f + (float)random.NextDouble() * 0.15f, starDot);

        // Nivel. La parte de arriba del suelo está en y = 0.
        Block("SueloInicio", new Vector3(-9f, -1.5f, 0f), new Vector3(10f, 3f, 3f), rock, true);
        Block("HierbaInicio", new Vector3(-9f, -0.05f, 0f), new Vector3(10f, 0.1f, 3.05f), grass, false);
        Piece("PuenteDia", new Vector3(-1.5f, -0.25f, 0f), new Vector3(5f, 0.5f, 2f), wood, World.Day);
        Piece("EstrellaNoche1", new Vector3(2.4f, 0.3f, 0f), new Vector3(1.4f, 0.4f, 1.4f), star, World.Night);
        Piece("EstrellaNoche2", new Vector3(4.5f, 1.2f, 0f), new Vector3(1.4f, 0.4f, 1.4f), star, World.Night);
        Piece("EstrellaNoche3", new Vector3(6.4f, 0.4f, 0f), new Vector3(1.4f, 0.4f, 1.4f), star, World.Night);
        Block("SueloFinal", new Vector3(12.5f, -1.5f, 0f), new Vector3(11f, 3f, 3f), rock, true);
        Block("HierbaFinal", new Vector3(12.5f, -0.05f, 0f), new Vector3(11f, 0.1f, 3.05f), grass, false);
        Piece("MuroNoche", new Vector3(11f, 2f, 0f), new Vector3(0.8f, 4f, 2f), nightWall, World.Night);

        var goal = Block("Meta", new Vector3(16f, 1f, 0f), new Vector3(0.6f, 2f, 0.6f), goalMat, false);
        goal.AddComponent<BoxCollider2D>().isTrigger = true;
        goal.AddComponent<LevelGoal>();

        // Personaje.
        var hero = new GameObject("Jugador");
        hero.transform.position = new Vector3(-11f, 1.2f, 0f);
        var body = hero.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        var capsule = hero.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.8f, 1.8f);
        var controller = hero.AddComponent<PlayerController>();
        var heroVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Object.DestroyImmediate(heroVisual.GetComponent<Collider>());
        heroVisual.name = "Dibujo";
        heroVisual.transform.SetParent(hero.transform, false);
        heroVisual.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
        heroVisual.GetComponent<Renderer>().sharedMaterial = player;
        controller.visual = heroVisual.transform;

        // Cámara lateral en perspectiva (2.5D).
        var cameraObject = new GameObject("Camara") { tag = "MainCamera" };
        cameraObject.transform.position = new Vector3(-8f, 3f, -18f);
        var cam = cameraObject.AddComponent<Camera>();
        cam.fieldOfView = 35f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = body;
        follow.minX = -8f;
        follow.maxX = 12f;

        EditorSceneManager.SaveScene(scene, ScenePath);
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        Debug.Log("Nivel de prueba creado en " + ScenePath + ". Dale a Play.");
    }

    static Material Mat(string name, Color color, int world, Color emission = default)
    {
        if (!AssetDatabase.IsValidFolder(MaterialsFolder)) AssetDatabase.CreateFolder("Assets", "Materials");
        string path = $"{MaterialsFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Fase/World"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Emission", emission);
        mat.SetFloat("_World", world);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>Cubo con el material dado. `solid` = se puede pisar (colisión 2D).</summary>
    static GameObject Block(string name, Vector3 center, Vector3 size, Material mat, bool solid)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        cube.transform.position = center;
        cube.transform.localScale = size;
        var renderer = cube.GetComponent<Renderer>();
        renderer.sharedMaterial = mat;
        // Lo que solo brilla (cielo, línea) no proyecta sombra.
        if (mat.GetColor("_BaseColor") == Color.black)
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        if (solid) cube.AddComponent<BoxCollider2D>();
        return cube;
    }

    /// <summary>Bloque que solo existe en un mundo.</summary>
    static void Piece(string name, Vector3 center, Vector3 size, Material mat, World world)
    {
        var block = Block(name, center, size, mat, true);
        block.AddComponent<WorldPiece>().world = world;
    }

    static void Sphere(string name, Vector3 center, float size, Material mat)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        Object.DestroyImmediate(sphere.GetComponent<Collider>());
        sphere.transform.position = center;
        sphere.transform.localScale = Vector3.one * size;
        var renderer = sphere.GetComponent<Renderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }
}
