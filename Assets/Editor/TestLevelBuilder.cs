using System.Collections.Generic;
using Fase;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Menú Fase → Crear nivel de prueba: monta desde cero Assets/Scenes/Prueba.unity con el arte de
/// Kenney (CC0). Las colisiones son cajas 2D invisibles y el arte va encima como hijo.
/// Cada modelo recibe materiales "Fase/World" de su mundo: día, noche o ambos.
/// Se puede volver a ejecutar para regenerar la escena.
/// </summary>
public static class TestLevelBuilder
{
    const string ScenePath = "Assets/Scenes/Prueba.unity";
    const string MaterialsFolder = "Assets/Materials";
    const string Platformer = "Assets/Art/Platformer/";
    const string Nature = "Assets/Art/Nature/";
    const string HeroModel = Platformer + "character-oobi.fbx";
    const string HeroController = "Assets/Art/Jugador.controller";

    const int Day = 0, Night = 1, Both = 2;

    // Aspecto de lo que solo existe de noche: más frío y con brillo propio.
    static readonly Color NightTint = new Color(0.75f, 0.8f, 1.25f);
    static readonly Color IceGlow = new Color(0.08f, 0.16f, 0.45f);
    static readonly Color CrystalGlow = new Color(0.25f, 0.08f, 0.45f);

    static readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    [MenuItem("Fase/Crear nivel de prueba")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        materialCache.Clear();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Luz y ambiente.
        var sunLight = new GameObject("Luz").AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.intensity = 1.25f;
        sunLight.color = new Color(1f, 0.96f, 0.88f);
        sunLight.shadows = LightShadows.Soft;
        sunLight.transform.rotation = Quaternion.Euler(40f, -35f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.52f, 0.6f);

        // La línea empieza a la izquierda: el puente (de día) todavía no existe.
        var line = new GameObject("Linea").AddComponent<ShiftLine>();
        line.x = -6f;
        line.minX = -14f;
        line.maxX = 17f;
        var lineVisual = Primitive(PrimitiveType.Cube, "DibujoLinea", new Vector3(0f, 5f, -1.6f), new Vector3(0.1f, 60f, 0.1f),
            Flat("Linea", Both, new Color(1.8f, 1.6f, 1.2f)));
        lineVisual.AddComponent<LineVisual>();

        // Cielo en dos versiones, sol, luna y estrellas.
        Primitive(PrimitiveType.Cube, "CieloDia", new Vector3(0f, 5f, 30f), new Vector3(400f, 160f, 1f), Flat("CieloDia", Day, new Color(0.35f, 0.55f, 0.75f)));
        Primitive(PrimitiveType.Cube, "CieloNoche", new Vector3(0f, 5f, 30f), new Vector3(400f, 160f, 1f), Flat("CieloNoche", Night, new Color(0.006f, 0.007f, 0.03f)));
        Primitive(PrimitiveType.Sphere, "Sol", new Vector3(-8f, 13f, 28f), Vector3.one * 5f, Flat("Sol", Day, new Color(1.6f, 1.3f, 0.7f)));
        Primitive(PrimitiveType.Sphere, "Luna", new Vector3(14f, 13f, 28f), Vector3.one * 4f, Flat("Luna", Night, new Color(0.9f, 0.95f, 1.2f)));
        var starMat = Flat("PuntoEstrella", Night, new Color(1.2f, 1.2f, 1.5f));
        var random = new System.Random(7);
        for (int i = 0; i < 90; i++)
            Primitive(PrimitiveType.Sphere, "Estrella", new Vector3(random.Next(-50, 60), random.Next(-2, 25), 28f),
                Vector3.one * (0.15f + (float)random.NextDouble() * 0.2f), starMat);

        // Bosque del fondo: en el mismo sitio hay árboles de día y pinos de noche.
        var backdrop = new GameObject("Fondo").transform;
        for (float x = -34f; x <= 44f; x += 4.5f)
        {
            float z = 9f + (float)random.NextDouble() * 6f;
            float s = 3.5f + (float)random.NextDouble() * 1.5f;
            Model(Nature + "tree_default.fbx", new Vector3(x, -2.5f, z), Vector3.one * s, Day, backdrop);
            Model(Nature + "tree_pineRoundA.fbx", new Vector3(x, -2.5f, z), Vector3.one * s * 1.1f, Night, backdrop,
                NightTint, new Color(0.02f, 0.05f, 0.12f));
        }
        Tile(Platformer + "block-grass-large.fbx", -40f, 50f, -3.5f, new Vector3(2.08f, 1f, 2.08f), 1f, 14f, Both, backdrop, 16f);

        // Nivel. La parte de arriba del suelo está en y = 0.
        var level = new GameObject("Nivel").transform;
        var start = Solid("SueloInicio", new Vector2(-9f, -1.5f), new Vector2(10f, 3f), null, level);
        Tile(Platformer + "block-grass-large-tall.fbx", -14f, -4f, -2f, new Vector3(2.08f, 2f, 2.08f), 2f, 3f, Both, start.transform);

        var bridge = Solid("PuenteDia", new Vector2(-1.5f, -0.25f), new Vector2(5f, 0.5f), World.Day, level);
        Tile(Nature + "bridge_wood.fbx", -4f, 1f, -0.42f, new Vector3(1.04f, 0.4f, 1.04f), 0.45f, 2.2f, Day, bridge.transform);

        float[][] ice = { new[] { 2.4f, 0.3f }, new[] { 4.5f, 1.2f }, new[] { 6.4f, 0.4f } };
        for (int i = 0; i < ice.Length; i++)
        {
            var block = Solid("HieloNoche" + (i + 1), new Vector2(ice[i][0], ice[i][1]), new Vector2(1.4f, 0.4f), World.Night, level);
            Tile(Platformer + "block-snow-low.fbx", ice[i][0] - 0.7f, ice[i][0] + 0.7f, ice[i][1] - 0.2f,
                new Vector3(1.08f, 0.5f, 1.08f), 0.4f, 1.4f, Night, block.transform, 0f, NightTint, IceGlow);
        }

        var end = Solid("SueloFinal", new Vector2(12.5f, -1.5f), new Vector2(11f, 3f), null, level);
        Tile(Platformer + "block-grass-large-tall.fbx", 7f, 18f, -2f, new Vector3(2.08f, 2f, 2.08f), 2f, 3f, Both, end.transform);

        var wall = Solid("MuroNoche", new Vector2(11f, 2f), new Vector2(0.8f, 4f), World.Night, level);
        Model(Platformer + "block-snow-large-tall.fbx", new Vector3(11f, 0f, 0f), new Vector3(0.8f / 2.08f, 2f, 2f / 2.08f), Night,
            wall.transform, new Color(0.8f, 0.6f, 1.2f), CrystalGlow);

        // Decoración: en el mismo sitio, un árbol de día o un pino de noche; flores de día, setas que brillan de noche.
        var decor = new GameObject("Decoracion").transform;
        foreach (var spot in new[] { new Vector3(-12.5f, 0f, 0.9f), new Vector3(-6.5f, 0f, 1.1f), new Vector3(9f, 0f, 1.1f), new Vector3(15f, 0f, 1.2f) })
        {
            Model(Nature + "tree_oak.fbx", spot, Vector3.one * 2.3f, Day, decor);
            Model(Nature + "tree_pineRoundB.fbx", spot, Vector3.one * 2.5f, Night, decor, NightTint, new Color(0.02f, 0.06f, 0.14f));
        }
        string[] flowers = { "flower_redA", "flower_yellowB", "flower_purpleA", "plant_bushDetailed" };
        float[] flowerX = { -13.2f, -10.8f, -8.2f, -5f, 8f, 10f, 13.3f, 17f };
        for (int i = 0; i < flowerX.Length; i++)
        {
            var at = new Vector3(flowerX[i], 0f, -0.9f + (i % 2) * 0.4f);
            Model(Nature + flowers[i % flowers.Length] + ".fbx", at, Vector3.one * 2.2f, Day, decor);
            Model(Nature + "mushroom_redGroup.fbx", at, Vector3.one * 2.4f, Night, decor, new Color(0.6f, 0.7f, 1.3f), new Color(0.15f, 0.25f, 0.7f));
        }
        Model(Nature + "rock_largeA.fbx", new Vector3(-9.5f, 0f, 1.2f), Vector3.one * 1.8f, Both, decor);
        Model(Nature + "rock_largeC.fbx", new Vector3(12.8f, 0f, 1.1f), Vector3.one * 1.6f, Both, decor);

        // Meta.
        var goal = new GameObject("Meta");
        goal.transform.position = new Vector3(16.2f, 1f, 0f);
        var goalBox = goal.AddComponent<BoxCollider2D>();
        goalBox.isTrigger = true;
        goalBox.size = new Vector2(1f, 2f);
        goal.AddComponent<LevelGoal>();
        Model(Platformer + "flag.fbx", new Vector3(16.2f, 0f, 0f), Vector3.one * 2.4f, Both, goal.transform);

        // Personaje.
        var hero = new GameObject("Jugador");
        hero.transform.position = new Vector3(-11f, 1.2f, 0f);
        var body = hero.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        var capsule = hero.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.8f, 1.8f);
        var controller = hero.AddComponent<PlayerController>();
        var heroVisual = Model(HeroModel, hero.transform.position + new Vector3(0f, -0.9f, 0f), Vector3.one * 2f, Both, hero.transform);
        heroVisual.name = "Dibujo";
        controller.visual = heroVisual.transform;
        var animator = heroVisual.GetComponentInChildren<Animator>();
        if (animator == null) animator = heroVisual.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = HeroAnimator();
        controller.animator = animator;

        BuildMobileUi();

        // Cámara lateral en perspectiva (2.5D).
        var cameraObject = new GameObject("Camara") { tag = "MainCamera" };
        cameraObject.transform.position = new Vector3(-8f, 3f, -18f);
        var cam = cameraObject.AddComponent<Camera>();
        cam.fieldOfView = 35f;
        cam.farClipPlane = 200f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = body;
        follow.minX = -8f;
        follow.maxX = 12f;

        EditorSceneManager.SaveScene(scene, ScenePath);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Nivel de prueba creado en " + ScenePath + ". Dale a Play.");
    }

    // ───────────────────────── Interfaz táctil ─────────────────────────

    /// <summary>
    /// Botones de moverse (abajo a la izquierda) y saltar (abajo a la derecha). Solo se ven en
    /// móviles (MobileControls). La línea se mueve arrastrándola con otro dedo.
    /// </summary>
    static void BuildMobileUi()
    {
        var events = new GameObject("EventSystem");
        events.AddComponent<UnityEngine.EventSystems.EventSystem>();
        events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var canvasObject = new GameObject("ControlesMovil");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
        canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObject.AddComponent<MobileControls>();

        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        TouchButton(canvasObject.transform, "Izquierda", MobileButton.Kind.Left, new Vector2(0f, 0f), new Vector2(190f, 190f), "◄", knob, font);
        TouchButton(canvasObject.transform, "Derecha", MobileButton.Kind.Right, new Vector2(0f, 0f), new Vector2(440f, 190f), "►", knob, font);
        TouchButton(canvasObject.transform, "Saltar", MobileButton.Kind.Jump, new Vector2(1f, 0f), new Vector2(-220f, 220f), "▲", knob, font, 260f);
    }

    static void TouchButton(Transform parent, string name, MobileButton.Kind kind, Vector2 anchor, Vector2 position,
        string label, Sprite sprite, Font font, float size = 210f)
    {
        var button = new GameObject(name, typeof(RectTransform));
        button.transform.SetParent(parent, false);
        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(size, size);
        var image = button.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite;
        image.color = new Color(1f, 1f, 1f, 0.28f);
        button.AddComponent<MobileButton>().kind = kind;

        var text = new GameObject("Texto", typeof(RectTransform));
        text.transform.SetParent(button.transform, false);
        var textRect = (RectTransform)text.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var t = text.AddComponent<UnityEngine.UI.Text>();
        t.text = label;
        t.font = font;
        t.fontSize = (int)(size * 0.38f);
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(1f, 1f, 1f, 0.9f);
        t.raycastTarget = false;
    }

    // ───────────────────────── Colisiones ─────────────────────────

    /// <summary>Caja sólida invisible. Con `world` solo existe en ese mundo (WorldPiece).</summary>
    static GameObject Solid(string name, Vector2 center, Vector2 size, World? world, Transform parent)
    {
        var solid = new GameObject(name);
        solid.transform.SetParent(parent);
        solid.transform.position = center;
        solid.AddComponent<BoxCollider2D>().size = size;
        if (world.HasValue) solid.AddComponent<WorldPiece>().world = world.Value;
        return solid;
    }

    // ───────────────────────── Arte ─────────────────────────

    /// <summary>
    /// Coloca un modelo con materiales Fase/World del mundo dado. `tint` multiplica su color y
    /// `glow` le da brillo propio (para lo que existe de noche).
    /// </summary>
    static GameObject Model(string path, Vector3 position, Vector3 scale, int world, Transform parent,
        Color? tint = null, Color glow = default)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning("No encuentro el modelo " + path);
            return new GameObject("Falta " + path);
        }
        var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        model.transform.SetParent(parent, true);
        model.transform.position = position;
        // Multiplica la escala propia del modelo (algunos FBX vienen con la raíz escalada).
        model.transform.localScale = Vector3.Scale(asset.transform.localScale, scale);
        foreach (var collider in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
        foreach (var renderer in model.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                materials[i] = WorldMaterial(materials[i], world, tint ?? Color.white, glow);
            renderer.sharedMaterials = materials;
            if (world == Night && glow != default) renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        return model;
    }

    /// <summary>
    /// Repite un modelo a lo largo de [x0, x1], estirándolo para que cubra justo ese tramo.
    /// `size` es el tamaño original del modelo; `height` y `depth` el tamaño final.
    /// </summary>
    static void Tile(string path, float x0, float x1, float yBase, Vector3 size, float height, float depth, int world,
        Transform parent, float z = 0f, Color? tint = null, Color glow = default)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt((x1 - x0) / size.x));
        float width = (x1 - x0) / count;
        for (int i = 0; i < count; i++)
            Model(path, new Vector3(x0 + width * (i + 0.5f), yBase, z),
                new Vector3(width / size.x, height / size.y, depth / size.z), world, parent, tint, glow);
    }

    /// <summary>Copia un material importado como Fase/World (misma textura y color) para un mundo.</summary>
    static Material WorldMaterial(Material source, int world, Color tint, Color glow)
    {
        string sourceName = source != null ? source.name : "sin_material";
        string key = $"{sourceName}_{world}_{ColorUtility.ToHtmlStringRGB(tint)}_{ColorUtility.ToHtmlStringRGB(glow)}";
        if (materialCache.TryGetValue(key, out var cached)) return cached;

        var mat = LoadOrCreate($"{MaterialsFolder}/Arte/{key}.mat");
        var texture = source != null ? source.mainTexture : null;
        var color = source != null && source.HasProperty("_Color") ? source.color : Color.white;
        if (texture != null) color = Color.white;
        mat.SetTexture("_BaseMap", texture);
        mat.SetColor("_BaseColor", color * tint);
        mat.SetColor("_Emission", glow);
        mat.SetFloat("_World", world);
        EditorUtility.SetDirty(mat);
        materialCache[key] = mat;
        return mat;
    }

    /// <summary>Material liso que solo brilla (cielo, sol, línea).</summary>
    static Material Flat(string name, int world, Color glow)
    {
        var mat = LoadOrCreate($"{MaterialsFolder}/{name}.mat");
        mat.SetTexture("_BaseMap", null);
        mat.SetColor("_BaseColor", Color.black);
        mat.SetColor("_Emission", glow);
        mat.SetFloat("_World", world);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material LoadOrCreate(string path)
    {
        string folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            AssetDatabase.CreateFolder(parentFolder, System.IO.Path.GetFileName(folder));
        }
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Fase/World"));
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    static GameObject Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material mat)
    {
        var shape = GameObject.CreatePrimitive(type);
        shape.name = name;
        Object.DestroyImmediate(shape.GetComponent<Collider>());
        shape.transform.position = position;
        shape.transform.localScale = scale;
        var renderer = shape.GetComponent<Renderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return shape;
    }

    // ───────────────────────── Animaciones del personaje ─────────────────────────

    /// <summary>
    /// Animator con los estados idle, walk, jump y fall usando las animaciones del modelo.
    /// PlayerController cambia de estado por nombre, así que no hacen falta transiciones.
    /// </summary>
    static AnimatorController HeroAnimator()
    {
        // Que las animaciones que se repiten (andar, quieto, caer) hagan bucle.
        var importer = (ModelImporter)AssetImporter.GetAtPath(HeroModel);
        var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
        bool changed = false;
        foreach (var clip in clips)
        {
            bool loop = clip.name == "idle" || clip.name == "walk" || clip.name == "sprint" || clip.name == "fall";
            if (clip.loopTime != loop) { clip.loopTime = loop; changed = true; }
        }
        if (changed || importer.clipAnimations.Length == 0)
        {
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        AssetDatabase.DeleteAsset(HeroController);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(HeroController);
        var stateMachine = controller.layers[0].stateMachine;
        var byName = new Dictionary<string, AnimationClip>();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(HeroModel))
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview")) byName[clip.name] = clip;
        foreach (string state in new[] { "idle", "walk", "jump", "fall" })
        {
            var s = stateMachine.AddState(state);
            if (byName.TryGetValue(state, out var clip)) s.motion = clip;
            if (state == "idle") stateMachine.defaultState = s;
        }
        return controller;
    }
}
