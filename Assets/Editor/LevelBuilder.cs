using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fase;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Menú Fase → Generar niveles: convierte cada archivo de Assets/Levels/*.txt en una escena
/// (Assets/Scenes/Niveles) con arte de Kenney, colisiones, sonido, música, interfaz y controles
/// táctiles, y las pone en orden en la lista de escenas del juego.
///
/// Formato de un nivel (ver Assets/Levels/LEEME.md):
///   nombre: Amanecer
///   pista: Mueve la línea con Shift o Q/E
///   linea: 6            ← columna donde empieza la línea
///   ---
///   (mapa: una letra por casilla de 1×1; abajo del todo es el suelo)
/// </summary>
public static class LevelBuilder
{
    const string LevelsFolder = "Assets/Levels";
    const string ScenesFolder = "Assets/Scenes/Niveles";
    const string MaterialsFolder = "Assets/Materials";
    const string Platformer = "Assets/Art/Platformer/";
    const string Nature = "Assets/Art/Nature/";
    const string Sfx = "Assets/Audio/Sfx/";
    const string Music = "Assets/Audio/Music/";
    const string HeroModel = Platformer + "character-oobi.fbx";
    const string HeroController = "Assets/Art/Jugador.controller";

    const int Day = 0, Night = 1, Both = 2;
    const float Depth = 2.4f;

    static readonly Color NightTint = new Color(0.75f, 0.8f, 1.25f);
    static readonly Color IceGlow = new Color(0.08f, 0.16f, 0.45f);
    static readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    class Level
    {
        public string file, name = "", hint = "";
        public float line;
        public string[] rows;
        public int Width => rows.Max(r => r.Length);
        public int Height => rows.Length;

        public char At(int col, int row) =>
            row < 0 || row >= rows.Length || col < 0 || col >= rows[row].Length ? ' ' : rows[row][col];

        /// <summary>Altura (y) de la parte de abajo de la fila; la última fila está en y = 0.</summary>
        public float Bottom(int row) => Height - 1 - row;
    }

    [MenuItem("Fase/Generar niveles")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var files = Directory.GetFiles(LevelsFolder, "*.txt").OrderBy(f => f).ToArray();
        if (files.Length == 0) { Debug.LogWarning("No hay niveles en " + LevelsFolder); return; }
        if (!AssetDatabase.IsValidFolder(ScenesFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Niveles");

        var scenes = new List<EditorBuildSettingsScene>();
        var levels = files.Select(Parse).ToList();
        for (int i = 0; i < levels.Count; i++)
        {
            string next = Path.GetFileNameWithoutExtension(levels[(i + 1) % levels.Count].file);
            string path = Build(levels[i], i + 1, next);
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(scenes[0].path);
        Debug.Log($"Generados {levels.Count} niveles en {ScenesFolder}.");
    }

    static Level Parse(string file)
    {
        var level = new Level { file = file };
        var lines = File.ReadAllLines(file);
        int separator = System.Array.FindIndex(lines, l => l.Trim() == "---");
        for (int i = 0; i < separator; i++)
        {
            int colon = lines[i].IndexOf(':');
            if (colon < 0) continue;
            string key = lines[i].Substring(0, colon).Trim().ToLowerInvariant();
            string value = lines[i].Substring(colon + 1).Trim();
            if (key == "nombre") level.name = value;
            else if (key == "pista") level.hint = value.Replace("\\n", "\n");
            else if (key == "linea") level.line = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        // Las filas vacías también cuentan (son aire); solo se quitan las del final del archivo.
        var rows = lines.Skip(separator + 1).Select(l => l.TrimEnd()).ToList();
        while (rows.Count > 0 && rows[rows.Count - 1].Length == 0) rows.RemoveAt(rows.Count - 1);
        level.rows = rows.ToArray();
        return level;
    }

    // ───────────────────────── Escena ─────────────────────────

    static string Build(Level level, int number, string nextScene)
    {
        materialCache.Clear();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int width = level.Width;
        var random = new System.Random(level.name.GetHashCode());

        BuildLight();
        var line = new GameObject("Linea").AddComponent<ShiftLine>();
        line.x = level.line;
        line.minX = 0.5f;
        line.maxX = width - 0.5f;
        var lineVisual = Primitive(PrimitiveType.Cube, "DibujoLinea", new Vector3(0f, 5f, -1.6f), new Vector3(0.1f, 80f, 0.1f),
            Flat("Linea", Both, new Color(1.8f, 1.6f, 1.2f)));
        lineVisual.AddComponent<LineVisual>();
        BuildSky(width, random);

        var root = new GameObject("Nivel").transform;
        var decor = new GameObject("Decoracion").transform;
        int fragments = 0;
        Vector3 start = new Vector3(1.5f, 2f, 0f);

        for (int row = 0; row < level.Height; row++)
        {
            float bottom = level.Bottom(row);
            // Tramos horizontales de la misma letra sólida: una sola colisión por tramo.
            for (int col = 0; col < width;)
            {
                char c = level.At(col, row);
                int end = col;
                while (IsRunChar(c) && level.At(end + 1, row) == c) end++;
                if (IsRunChar(c)) BuildRun(level, c, col, end, row, root);
                col = end + 1;
            }
            for (int col = 0; col < width; col++)
            {
                char c = level.At(col, row);
                var cell = new Vector3(col + 0.5f, bottom, 0f);
                switch (c)
                {
                    case 'S': start = cell + Vector3.up * 0.95f; break;
                    case 'G': BuildGoal(cell, nextScene, root); break;
                    case '^': BuildSpikes(cell, root); break;
                    case 'M': BuildMonster(cell, root); break;
                    case 'F': BuildSpring(cell, root); break;
                    case '*': BuildFragment(cell, root); fragments++; break;
                    case 'C': BuildCheckpoint(cell, root); break;
                }
                // Decoración encima del suelo: árbol de día y pino de noche en el mismo sitio, flores y setas.
                if (c == '#' && level.At(col, row - 1) == ' ' && level.At(col, row - 2) == ' ')
                    Decorate(cell + Vector3.up, random, decor);
            }
        }

        var body = BuildHero(start);
        BuildCamera(body, width);
        BuildAudio();
        BuildHud(level, number, fragments);
        BuildMobileUi();

        string path = $"{ScenesFolder}/{Path.GetFileNameWithoutExtension(level.file)}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    static bool IsRunChar(char c) => c == '#' || c == 'D' || c == 'N' || c == 'd' || c == 'n' || c == '~';

    static void BuildRun(Level level, char c, int from, int to, int row, Transform parent)
    {
        float bottom = level.Bottom(row);
        float left = from, right = to + 1, center = (left + right) * 0.5f, width = right - left;
        switch (c)
        {
            case '#':
                Solid("Suelo", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), null, parent);
                for (int col = from; col <= to; col++)
                {
                    if (level.At(col, row - 1) == '#') continue; // solo se dibuja desde la casilla de arriba
                    int depth = 1;
                    while (level.At(col, row + depth) == '#') depth++;
                    var x = col + 0.5f;
                    Fit(Platformer + "block-grass.fbx", new Vector3(x, bottom, 0f), new Vector3(1.02f, 1f, Depth), Both, parent);
                    if (depth > 1)
                        Fit(Nature + "cliff_block_rock.fbx", new Vector3(x, bottom - depth + 1, 0f), new Vector3(1.02f, depth - 1, Depth), Both, parent,
                            new Color(0.62f, 0.47f, 0.42f));
                }
                break;
            case 'D':
            {
                var solid = Solid("BloqueDia", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), World.Day, parent);
                for (int col = from; col <= to; col++)
                    Fit(Platformer + "crate.fbx", new Vector3(col + 0.5f, bottom, 0f), new Vector3(1f, 1f, 1.4f), Day, solid.transform);
                break;
            }
            case 'N':
            {
                var solid = Solid("BloqueNoche", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), World.Night, parent);
                for (int col = from; col <= to; col++)
                    Fit(Platformer + "block-snow.fbx", new Vector3(col + 0.5f, bottom, 0f), new Vector3(1f, 1f, 1.4f), Night, solid.transform, NightTint, IceGlow);
                break;
            }
            case 'd':
            {
                var solid = Solid("TablaDia", new Vector2(center, bottom + 0.85f), new Vector2(width, 0.3f), World.Day, parent);
                for (int col = from; col <= to; col++)
                    Fit(Nature + "bridge_wood.fbx", new Vector3(col + 0.5f, bottom + 0.6f, 0f), new Vector3(1.02f, 0.4f, 1.8f), Day, solid.transform);
                break;
            }
            case 'n':
            {
                var solid = Solid("TablaNoche", new Vector2(center, bottom + 0.85f), new Vector2(width, 0.3f), World.Night, parent);
                for (int col = from; col <= to; col++)
                    Fit(Platformer + "block-snow-low.fbx", new Vector3(col + 0.5f, bottom + 0.7f, 0f), new Vector3(1f, 0.3f, 1.4f), Night, solid.transform, NightTint, IceGlow);
                break;
            }
            case '~':
            {
                // De noche, hielo que se puede pisar; de día, agua que hace perder.
                var ice = Solid("HieloLago", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), World.Night, parent);
                for (int col = from; col <= to; col++)
                    Fit(Platformer + "block-snow-low.fbx", new Vector3(col + 0.5f, bottom, 0f), new Vector3(1.02f, 1f, Depth), Night, ice.transform,
                        new Color(0.6f, 0.9f, 1.2f), new Color(0.05f, 0.2f, 0.4f));
                var water = Primitive(PrimitiveType.Cube, "AguaDia", new Vector3(center, bottom + 0.4f, 0f), new Vector3(width, 0.8f, Depth),
                    Lit("Agua", Day, new Color(0.25f, 0.55f, 0.95f), new Color(0.05f, 0.12f, 0.25f)));
                water.transform.SetParent(parent, true);
                var danger = new GameObject("PeligroAgua");
                danger.transform.SetParent(parent);
                danger.transform.position = new Vector3(center, bottom + 0.35f, 0f);
                var box = danger.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = new Vector2(width - 0.1f, 0.6f);
                danger.AddComponent<Hazard>().active = Hazard.When.DayOnly;
                break;
            }
        }
    }

    static void BuildSpikes(Vector3 cell, Transform parent)
    {
        var spikes = Trigger("Pinchos", cell + new Vector3(0f, 0.25f, 0f), new Vector2(0.8f, 0.45f), parent);
        spikes.AddComponent<Hazard>();
        Fit(Platformer + "trap-spikes.fbx", cell, new Vector3(1f, 0.55f, 1.2f), Both, spikes.transform);
    }

    static void BuildMonster(Vector3 cell, Transform parent)
    {
        var monster = new GameObject("Monstruo");
        monster.transform.SetParent(parent);
        monster.transform.position = cell;
        var visual = Fit(Platformer + "character-oozi.fbx", cell, new Vector3(0.9f, 1.1f, 0.9f), Night, monster.transform,
            new Color(1.3f, 0.5f, 0.8f), new Color(0.35f, 0.05f, 0.25f));
        var m = monster.AddComponent<Monster>();
        m.visual = visual.transform;
        m.height = 1.1f;
    }

    static void BuildSpring(Vector3 cell, Transform parent)
    {
        var spring = Trigger("FlorMuelle", cell + new Vector3(0f, 0.4f, 0f), new Vector2(0.9f, 0.6f), parent);
        var visual = Fit(Platformer + "spring.fbx", cell, new Vector3(0.9f, 0.6f, 0.9f), Day, spring.transform);
        Fit(Nature + "flower_redA.fbx", cell + new Vector3(0f, 0f, 0.6f), new Vector3(0.6f, 1.2f, 0.6f), Day, spring.transform);
        spring.AddComponent<Spring>().visual = visual.transform;
    }

    static void BuildFragment(Vector3 cell, Transform parent)
    {
        var fragment = Trigger("FragmentoLuna", cell + new Vector3(0f, 0.5f, 0f), new Vector2(0.7f, 0.7f), parent);
        Fit(Platformer + "star.fbx", cell + new Vector3(0f, 0.2f, 0f), new Vector3(0.7f, 0.7f, 0.5f), Both, fragment.transform,
            new Color(1.2f, 1.1f, 0.7f), new Color(0.6f, 0.5f, 0.15f));
        fragment.AddComponent<Collectible>();
    }

    static void BuildCheckpoint(Vector3 cell, Transform parent)
    {
        var checkpoint = Trigger("PuntoControl", cell + new Vector3(0f, 1f, 0f), new Vector2(0.8f, 2f), parent);
        var visual = Fit(Platformer + "flag.fbx", cell, new Vector3(0.6f, 1.4f, 0.3f), Both, checkpoint.transform, new Color(0.7f, 0.9f, 1.2f));
        checkpoint.AddComponent<Checkpoint>().visual = visual.transform;
    }

    static void BuildGoal(Vector3 cell, string nextScene, Transform parent)
    {
        var goal = Trigger("Meta", cell + new Vector3(0f, 1f, 0f), new Vector2(1f, 2f), parent);
        goal.AddComponent<LevelGoal>().nextScene = nextScene;
        Fit(Platformer + "flag.fbx", cell, new Vector3(1f, 2.4f, 0.3f), Both, goal.transform);
    }

    static void Decorate(Vector3 top, System.Random random, Transform parent)
    {
        double roll = random.NextDouble();
        if (roll < 0.22)
        {
            var spot = top + new Vector3(0f, 0f, 0.8f + (float)random.NextDouble() * 0.4f);
            float s = 1.8f + (float)random.NextDouble() * 0.9f;
            Fit(Nature + "tree_oak.fbx", spot, new Vector3(s * 0.9f, s * 1.1f, s * 0.9f), Day, parent);
            Fit(Nature + "tree_pineRoundB.fbx", spot, new Vector3(s * 0.8f, s * 1.25f, s * 0.8f), Night, parent, NightTint, new Color(0.02f, 0.06f, 0.14f));
        }
        else if (roll < 0.5)
        {
            string[] flowers = { "flower_redA", "flower_yellowB", "flower_purpleA", "plant_bushDetailed", "grass_large" };
            var spot = top + new Vector3((float)random.NextDouble() * 0.6f - 0.3f, 0f, -0.7f);
            Fit(Nature + flowers[random.Next(flowers.Length)] + ".fbx", spot, new Vector3(0.45f, 0.55f, 0.45f), Day, parent);
            Fit(Nature + "mushroom_redGroup.fbx", spot, new Vector3(0.5f, 0.5f, 0.5f), Night, parent, new Color(0.6f, 0.7f, 1.3f), new Color(0.15f, 0.25f, 0.7f));
        }
    }

    // ───────────────────────── Ambiente ─────────────────────────

    static void BuildLight()
    {
        var sun = new GameObject("Luz").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.color = new Color(1f, 0.96f, 0.88f);
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(40f, -35f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.52f, 0.6f);
    }

    static void BuildSky(int width, System.Random random)
    {
        float mid = width * 0.5f;
        Primitive(PrimitiveType.Cube, "CieloDia", new Vector3(mid, 5f, 30f), new Vector3(width + 300f, 160f, 1f), Flat("CieloDia", Day, new Color(0.35f, 0.55f, 0.75f)));
        Primitive(PrimitiveType.Cube, "CieloNoche", new Vector3(mid, 5f, 30f), new Vector3(width + 300f, 160f, 1f), Flat("CieloNoche", Night, new Color(0.006f, 0.007f, 0.03f)));
        Primitive(PrimitiveType.Sphere, "Sol", new Vector3(mid - 10f, 14f, 28f), Vector3.one * 5f, Flat("Sol", Day, new Color(1.6f, 1.3f, 0.7f)));
        Primitive(PrimitiveType.Sphere, "Luna", new Vector3(mid + 10f, 14f, 28f), Vector3.one * 4f, Flat("Luna", Night, new Color(0.9f, 0.95f, 1.2f)));
        var starMat = Flat("PuntoEstrella", Night, new Color(1.2f, 1.2f, 1.5f));
        var stars = new GameObject("Estrellas").transform;
        for (int i = 0; i < 40 + width * 2; i++)
            Primitive(PrimitiveType.Sphere, "Estrella", new Vector3(random.Next(-40, width + 40), random.Next(-4, 26), 28f),
                Vector3.one * (0.15f + (float)random.NextDouble() * 0.2f), starMat).transform.SetParent(stars);

        var backdrop = new GameObject("Fondo").transform;
        for (float x = -30f; x <= width + 30f; x += 4.5f)
        {
            float z = 9f + (float)random.NextDouble() * 6f;
            float s = 3.5f + (float)random.NextDouble() * 1.5f;
            Model(Nature + "tree_default.fbx", new Vector3(x, -3.5f, z), Vector3.one * s, Day, backdrop);
            Model(Nature + "tree_pineRoundA.fbx", new Vector3(x, -3.5f, z), Vector3.one * s * 1.1f, Night, backdrop, NightTint, new Color(0.02f, 0.05f, 0.12f));
        }
        for (float x = -40f; x < width + 40f; x += 8f)
            Fit(Platformer + "block-grass-large.fbx", new Vector3(x + 4f, -4.5f, 16f), new Vector3(8f, 1f, 14f), Both, backdrop);
    }

    static Rigidbody2D BuildHero(Vector3 start)
    {
        var hero = new GameObject("Jugador");
        hero.transform.position = start;
        var body = hero.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var capsule = hero.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.8f, 1.8f);
        var controller = hero.AddComponent<PlayerController>();
        controller.killHeight = -4f;
        var visual = Model(HeroModel, start + new Vector3(0f, -0.9f, 0f), Vector3.one * 2f, Both, hero.transform);
        visual.name = "Dibujo";
        controller.visual = visual.transform;
        var animator = visual.GetComponentInChildren<Animator>();
        if (animator == null) animator = visual.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = HeroAnimator();
        controller.animator = animator;
        return body;
    }

    static void BuildCamera(Rigidbody2D target, int width)
    {
        var cameraObject = new GameObject("Camara") { tag = "MainCamera" };
        cameraObject.transform.position = target.transform.position + new Vector3(0f, 1.5f, -18f);
        var cam = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cam.fieldOfView = 35f;
        cam.farClipPlane = 200f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = target;
        // Media pantalla son unas 10 unidades: que no se vea fuera del nivel.
        follow.minX = Mathf.Min(9f, width * 0.5f);
        follow.maxX = Mathf.Max(width - 9f, width * 0.5f);
    }

    static void BuildAudio()
    {
        var audio = new GameObject("Sonido").AddComponent<GameAudio>();
        audio.stepsDay = Clips("footstep_grass_00", 5);
        audio.stepsNight = Clips("footstep_snow_00", 5);
        audio.jump = Clip(Sfx + "drop_002.ogg");
        audio.land = Clips("impactSoft_medium_00", 3);
        audio.die = Clip(Sfx + "error_004.ogg");
        audio.lineGrab = Clip(Sfx + "maximize_003.ogg");
        audio.lineRelease = Clip(Sfx + "minimize_003.ogg");
        audio.lineTick = Clip(Sfx + "switch_005.ogg");
        audio.appearDay = Clip(Sfx + "pluck_001.ogg");
        audio.appearNight = Clip(Sfx + "glass_001.ogg");
        audio.vanish = Clip(Sfx + "glass_004.ogg");
        audio.goal = Clip("Assets/Audio/Jingles/jingles_PIZZI00.ogg");
        audio.collect = Clip(Sfx + "confirmation_002.ogg");
        audio.checkpoint = Clip(Sfx + "switch_002.ogg");
        audio.bounce = Clip(Sfx + "drop_002.ogg");

        foreach (string path in new[] { Music + "dia.wav", Music + "noche.wav" })
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            if (importer == null) continue;
            var settings = importer.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.CompressedInMemory || settings.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.6f;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }
        var music = audio.gameObject.AddComponent<MusicLayers>();
        music.day = Clip(Music + "dia.wav");
        music.night = Clip(Music + "noche.wav");
    }

    static AudioClip Clip(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("No encuentro el sonido " + path);
        return clip;
    }

    static AudioClip[] Clips(string prefix, int count) =>
        Enumerable.Range(0, count).Select(i => Clip($"{Sfx}{prefix}{i}.ogg")).ToArray();

    // ───────────────────────── Interfaz ─────────────────────────

    static Canvas NewCanvas(string name, int order)
    {
        var canvasObject = new GameObject(name);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        outline.effectDistance = new Vector2(3f, -3f);
        return text;
    }

    static void BuildHud(Level level, int number, int fragments)
    {
        var canvas = NewCanvas("Interfaz", 0).transform;
        var hud = canvas.gameObject.AddComponent<Hud>();
        hud.levelName = $"{number}. {level.name}";
        hud.hint = level.hint;
        hud.fragmentsTotal = fragments;
        hud.title = Label(canvas, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(1600f, 140f), 96, TextAnchor.MiddleCenter);
        hud.hintText = Label(canvas, "Pista", new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1500f, 140f), 44, TextAnchor.MiddleCenter);
        hud.corner = Label(canvas, "Esquina", new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(900f, 60f), 38, TextAnchor.UpperLeft);
        hud.counter = Label(canvas, "Contador", new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(500f, 60f), 40, TextAnchor.UpperRight);
        hud.banner = Label(canvas, "Aviso", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 240f), 100, TextAnchor.MiddleCenter);
    }

    /// <summary>Botones de moverse y saltar; solo se ven en móviles (MobileControls).</summary>
    static void BuildMobileUi()
    {
        var events = new GameObject("EventSystem");
        events.AddComponent<UnityEngine.EventSystems.EventSystem>();
        events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var canvas = NewCanvas("ControlesMovil", 1);
        canvas.gameObject.AddComponent<MobileControls>();
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        TouchButton(canvas.transform, "Izquierda", MobileButton.Kind.Left, new Vector2(0f, 0f), new Vector2(190f, 190f), knob, 180f);
        TouchButton(canvas.transform, "Derecha", MobileButton.Kind.Right, new Vector2(0f, 0f), new Vector2(440f, 190f), knob, 0f);
        TouchButton(canvas.transform, "Saltar", MobileButton.Kind.Jump, new Vector2(1f, 0f), new Vector2(-220f, 220f), knob, 90f, 260f);
    }

    /// <summary>Botón redondo con un triángulo dibujado dentro (`arrow` = hacia dónde apunta, en grados).</summary>
    static void TouchButton(Transform parent, string name, MobileButton.Kind kind, Vector2 anchor, Vector2 position, Sprite sprite, float arrow, float size = 210f)
    {
        var button = new GameObject(name, typeof(RectTransform));
        button.transform.SetParent(parent, false);
        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(size, size);
        var image = button.AddComponent<Image>();
        image.sprite = sprite;
        image.color = new Color(1f, 1f, 1f, 0.28f);
        button.AddComponent<MobileButton>().kind = kind;

        var icon = new GameObject("Flecha", typeof(RectTransform));
        icon.transform.SetParent(button.transform, false);
        var iconRect = (RectTransform)icon.transform;
        iconRect.sizeDelta = new Vector2(size * 0.4f, size * 0.4f);
        iconRect.localRotation = Quaternion.Euler(0f, 0f, arrow);
        var triangle = icon.AddComponent<UiTriangle>();
        triangle.color = new Color(1f, 1f, 1f, 0.9f);
        triangle.raycastTarget = false;
    }

    // ───────────────────────── Colisiones ─────────────────────────

    static GameObject Solid(string name, Vector2 center, Vector2 size, World? world, Transform parent)
    {
        var solid = new GameObject(name);
        solid.transform.SetParent(parent);
        solid.transform.position = center;
        solid.AddComponent<BoxCollider2D>().size = size;
        if (world.HasValue) solid.AddComponent<WorldPiece>().world = world.Value;
        return solid;
    }

    static GameObject Trigger(string name, Vector3 center, Vector2 size, Transform parent)
    {
        var trigger = new GameObject(name);
        trigger.transform.SetParent(parent);
        trigger.transform.position = center;
        var box = trigger.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = size;
        return trigger;
    }

    // ───────────────────────── Arte ─────────────────────────

    /// <summary>
    /// Coloca un modelo estirado para ocupar exactamente `size`, con la base centrada en `bottom`.
    /// Así no hace falta conocer el tamaño original de cada modelo.
    /// </summary>
    static GameObject Fit(string path, Vector3 bottom, Vector3 size, int world, Transform parent, Color? tint = null, Color glow = default)
    {
        var model = Model(path, Vector3.zero, Vector3.one, world, parent, tint, glow);
        var bounds = Bounds(model);
        if (bounds.size.x < 1e-4f) return model;
        var s = model.transform.localScale;
        model.transform.localScale = new Vector3(s.x * size.x / bounds.size.x, s.y * size.y / bounds.size.y, s.z * size.z / Mathf.Max(bounds.size.z, 1e-4f));
        bounds = Bounds(model);
        model.transform.position += bottom - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        return model;
    }

    static Bounds Bounds(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(model.transform.position, Vector3.zero);
        var bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        return bounds;
    }

    static GameObject Model(string path, Vector3 position, Vector3 scale, int world, Transform parent, Color? tint = null, Color glow = default)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning("No encuentro el modelo " + path);
            var missing = new GameObject("Falta " + path);
            missing.transform.SetParent(parent);
            return missing;
        }
        var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        model.transform.SetParent(parent, true);
        model.transform.position = position;
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
    static Material Flat(string name, int world, Color glow) => Lit(name, world, Color.black, glow);

    static Material Lit(string name, int world, Color color, Color glow)
    {
        var mat = LoadOrCreate($"{MaterialsFolder}/{name}.mat");
        mat.SetTexture("_BaseMap", null);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Emission", glow);
        mat.SetFloat("_World", world);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material LoadOrCreate(string path)
    {
        string folder = Path.GetDirectoryName(path).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
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

    /// <summary>Animator con idle, walk, jump y fall; PlayerController cambia de estado por nombre.</summary>
    static AnimatorController HeroAnimator()
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(HeroController);
        if (existing != null) return existing;
        var importer = (ModelImporter)AssetImporter.GetAtPath(HeroModel);
        var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
        foreach (var clip in clips)
            clip.loopTime = clip.name == "idle" || clip.name == "walk" || clip.name == "sprint" || clip.name == "fall";
        importer.clipAnimations = clips;
        importer.SaveAndReimport();

        var controller = AnimatorController.CreateAnimatorControllerAtPath(HeroController);
        var stateMachine = controller.layers[0].stateMachine;
        var byName = AssetDatabase.LoadAllAssetsAtPath(HeroModel).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview")).ToDictionary(c => c.name);
        foreach (string state in new[] { "idle", "walk", "jump", "fall" })
        {
            var s = stateMachine.AddState(state);
            if (byName.TryGetValue(state, out var clip)) s.motion = clip;
            if (state == "idle") stateMachine.defaultState = s;
        }
        return controller;
    }
}
