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
/// (Assets/Scenes/Niveles) con arte (Quaternius y Kenney, CC0), colisiones, sonido, música,
/// postprocesado, partículas, interfaz y controles táctiles, y genera también el menú y el final.
///
/// Formato de un nivel: ver Assets/Levels/LEEME.md.
/// </summary>
public static partial class LevelBuilder
{
    const string LevelsFolder = "Assets/Levels";
    const string ScenesFolder = "Assets/Scenes/Niveles";
    const string MaterialsFolder = "Assets/Materials";
    const string Platformer = "Assets/Art/Platformer/";
    const string Nature = "Assets/Art/Nature/";
    const string Quat = "Assets/Art/Quaternius/";
    const string Sfx = "Assets/Audio/Sfx/";
    const string Music = "Assets/Audio/Music/";
    const string HeroModel = Quat + "Character.fbx";
    const string HeroController = "Assets/Art/Lumi.controller";
    const string MonsterController = "Assets/Art/Sombra.controller";
    const string BeeController = "Assets/Art/Abeja.controller";

    const int Day = 0, Night = 1, Both = 2;
    const float Depth = 2.4f;

    static readonly Color IceGlow = new Color(0.08f, 0.16f, 0.45f);
    static readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    class Level
    {
        public string file, name = "", hint = "", nameEn = "", hintEn = "";
        public float line;
        public int chapter = 1;
        public bool dayOnLeft = true;
        public float minLine = float.NaN, maxLine = float.NaN;
        public string[] rows;
        public int Width => rows.Max(r => r.Length);
        public int Height => rows.Length;
        public Biome Biome => Biomes[Mathf.Clamp(chapter, 1, Biomes.Length) - 1];
        public int Fragments => rows.Sum(r => r.Count(c => c == '*' || c == 'K'));

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
        PrepareRendering();

        // Orden del juego: Menu, Intro, niveles..., Final.
        var levels = files.Select(Parse).ToList();
        var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(BuildMenu(levels), true) };
        string intro = BuildIntro();
        if (intro != null) scenes.Add(new EditorBuildSettingsScene(intro, true));
        for (int i = 0; i < levels.Count; i++)
        {
            string next = i + 1 < levels.Count ? Path.GetFileNameWithoutExtension(levels[i + 1].file) : FinalScene;
            string path = Build(levels[i], i + 1, next);
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        scenes.Add(new EditorBuildSettingsScene(BuildFinal(levels), true));
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
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        for (int i = 0; i < separator; i++)
        {
            int colon = lines[i].IndexOf(':');
            if (colon < 0) continue;
            string key = lines[i].Substring(0, colon).Trim().ToLowerInvariant();
            string value = lines[i].Substring(colon + 1).Trim();
            switch (key)
            {
                case "nombre": level.name = value; break;
                case "name": level.nameEn = value; break;
                case "pista": level.hint = value.Replace("\\n", "\n"); break;
                case "hint": level.hintEn = value.Replace("\\n", "\n"); break;
                case "linea": level.line = float.Parse(value, inv); break;
                case "capitulo": level.chapter = int.Parse(value, inv); break;
                case "dia": level.dayOnLeft = value.ToLowerInvariant() != "derecha"; break;
                case "limites":
                    var parts = value.Split(new[] { ' ', '-', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    level.minLine = float.Parse(parts[0], inv);
                    level.maxLine = float.Parse(parts[1], inv);
                    break;
            }
        }
        if (level.nameEn == "") level.nameEn = level.name;
        if (level.hintEn == "") level.hintEn = level.hint;
        // Las filas vacías también cuentan (son aire); solo se quitan las del final del archivo.
        var rows = lines.Skip(separator + 1).Select(l => l.TrimEnd()).ToList();
        while (rows.Count > 0 && rows[rows.Count - 1].Length == 0) rows.RemoveAt(rows.Count - 1);
        level.rows = rows.ToArray();
        return level;
    }

    // ───────────────────────── Escena de nivel ─────────────────────────

    static string Build(Level level, int number, string nextScene)
    {
        materialCache.Clear();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (start, fragments, _) = BuildWorld(level, nextScene);

        var body = BuildHero(start);
        BuildCamera(body, level.Width, level.Biome);
        BuildAudio(level.Biome);
        BuildFx();
        BuildHud(level, number, fragments);
        BuildMobileUi();
        BuildPause();
        BuildTransition();

        string path = $"{ScenesFolder}/{Path.GetFileNameWithoutExtension(level.file)}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    /// <summary>Luz, línea, cielo, paisaje y todo lo del mapa. Devuelve la salida, los fragmentos y la línea.</summary>
    static (Vector3 start, int fragments, ShiftLine line) BuildWorld(Level level, string nextScene)
    {
        int width = level.Width;
        var biome = level.Biome;
        var random = new System.Random(level.name.GetHashCode());

        BuildLight(biome);
        var line = new GameObject("Linea").AddComponent<ShiftLine>();
        line.x = level.line;
        line.dayOnLeft = level.dayOnLeft;
        line.minX = float.IsNaN(level.minLine) ? 0.5f : level.minLine;
        line.maxX = float.IsNaN(level.maxLine) ? width - 0.5f : level.maxLine;
        if (!float.IsNaN(level.minLine)) BuildLimits(line.minX, line.maxX);
        BuildLineVisual();
        BuildSky(width, random, biome);
        BuildWeather(width, level.Height, biome);

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
                    case 'M': BuildMonster(cell, root, false); break;
                    case 'B': BuildMonster(cell, root, true); break;
                    case 'X': BuildSaw(cell, root); break;
                    case 'F': BuildSpring(cell, root); break;
                    case '*': BuildFragment(cell, root); fragments++; break;
                    case 'K': BuildChest(cell, root); fragments++; break;
                    case 'C': BuildCheckpoint(cell, root); break;
                }
                if (c == '#' && level.At(col, row - 1) == ' ' && level.At(col, row - 2) == ' ')
                    Decorate(cell + Vector3.up, random, decor, biome);
            }
        }
        MarkStatic(root);
        MarkStatic(decor);
        return (start, fragments, line);
    }

    static bool IsRunChar(char c) => c == '#' || c == 'D' || c == 'N' || c == 'd' || c == 'n' || c == '~';

    static void BuildRun(Level level, char c, int from, int to, int row, Transform parent)
    {
        var biome = level.Biome;
        float bottom = level.Bottom(row);
        float left = from, right = to + 1, center = (left + right) * 0.5f, width = right - left;
        switch (c)
        {
            case '#':
            {
                Solid("Suelo", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), null, parent);
                var ground = biome.GroundColors;
                for (int col = from; col <= to; col++)
                {
                    bool top = level.At(col, row - 1) != '#';
                    bool leftEnd = level.At(col - 1, row) != '#', rightEnd = level.At(col + 1, row) != '#';
                    string kind = top ? "Grass" : "Dirt";
                    string piece = leftEnd || rightEnd ? $"Cube_{kind}_1x1End" : $"Cube_{kind}_1x1Center";
                    if (leftEnd && rightEnd) piece = top ? "Cube_Grass_Single" : "Cube_Dirt_Single";
                    float yaw = leftEnd && !rightEnd ? 180f : 0f;
                    Fit(Quat + piece + ".fbx", new Vector3(col + 0.5f, bottom, 0f), new Vector3(1.02f, 1f, Depth), Both, parent,
                        recolor: ground, yaw: yaw);
                }
                break;
            }
            case 'D':
            {
                var solid = Solid("BloqueDia", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), World.Day, parent);
                for (int col = from; col <= to; col++)
                    Fit(Quat + "Cube_Crate.fbx", new Vector3(col + 0.5f, bottom, 0f), new Vector3(1f, 1f, 1.3f), Day, solid.transform);
                break;
            }
            case 'N':
            {
                var solid = Solid("BloqueNoche", new Vector2(center, bottom + 0.5f), new Vector2(width, 1f), World.Night, parent);
                for (int col = from; col <= to; col++)
                    Fit(Platformer + "block-snow.fbx", new Vector3(col + 0.5f, bottom, 0f), new Vector3(1f, 1f, 1.3f), Night, solid.transform, biome.nightTint, IceGlow);
                break;
            }
            case 'd':
            {
                // Puente de madera de Quaternius: piezas modulares, con barandilla.
                var solid = Solid("TablaDia", new Vector2(center, bottom + 0.85f), new Vector2(width, 0.3f), World.Day, parent);
                for (int col = from; col <= to; col++)
                    Fit(Quat + "Bridge_Modular_Center.fbx", new Vector3(col + 0.5f, bottom + 0.55f, 0f), new Vector3(1.02f, 0.75f, 1.6f), Day, solid.transform);
                break;
            }
            case 'n':
            {
                var solid = Solid("TablaNoche", new Vector2(center, bottom + 0.85f), new Vector2(width, 0.3f), World.Night, parent);
                for (int col = from; col <= to; col++)
                    Fit(Platformer + "block-snow-low.fbx", new Vector3(col + 0.5f, bottom + 0.7f, 0f), new Vector3(1f, 0.3f, 1.3f), Night, solid.transform, biome.nightTint, IceGlow);
                var glow = Glow("BrilloHielo", new Vector3(center, bottom + 0.85f, -0.2f), width * 0.9f + 1f, new Color(0.4f, 0.6f, 1.6f, 0.5f), Night, solid.transform);
                glow.transform.localScale = new Vector3(width + 1.2f, 1.2f, 1f);
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
                    Lit("Agua", Day, new Color(0.2f, 0.55f, 0.95f), new Color(0.05f, 0.15f, 0.3f)));
                water.transform.SetParent(parent, true);
                var danger = Trigger("PeligroAgua", new Vector3(center, bottom + 0.35f, 0f), new Vector2(width - 0.1f, 0.6f), parent);
                danger.AddComponent<Hazard>().active = Hazard.When.DayOnly;
                break;
            }
        }
    }

    static void BuildSpikes(Vector3 cell, Transform parent)
    {
        var spikes = Trigger("Pinchos", cell + new Vector3(0f, 0.25f, 0f), new Vector2(0.8f, 0.45f), parent);
        spikes.AddComponent<Hazard>();
        Fit(Quat + "Spikes.fbx", cell, new Vector3(0.9f, 0.7f, 0.9f), Both, spikes.transform);
    }

    static void BuildSaw(Vector3 cell, Transform parent)
    {
        var saw = new GameObject("Sierra");
        saw.transform.SetParent(parent);
        saw.transform.position = cell + new Vector3(0f, 0.5f, 0f);
        var hit = saw.AddComponent<CircleCollider2D>();
        hit.isTrigger = true;
        hit.radius = 0.6f;
        saw.AddComponent<Hazard>();
        var visual = Fit(Quat + "Hazard_Saw.fbx", cell - new Vector3(0f, 0.25f, 0.3f), new Vector3(1.5f, 1.5f, 0.12f), Both, saw.transform);
        var pivot = new GameObject("Giro").transform;
        pivot.SetParent(saw.transform, false);
        visual.transform.SetParent(pivot, true);
        var spinner = saw.AddComponent<Spinner>();
        spinner.spinning = pivot;
        spinner.travel = new Vector3(0f, 2.5f, 0f);
        spinner.travelSpeed = 0.45f;
    }

    static readonly Dictionary<string, (Color, Color)> ShadowNight = new Dictionary<string, (Color, Color)>
    {
        ["Main"] = (new Color(0.24f, 0.17f, 0.52f), new Color(0.06f, 0.03f, 0.16f)),
        ["Main_Dark"] = (new Color(0.13f, 0.09f, 0.33f), default),
        ["Main_2"] = (new Color(0.13f, 0.09f, 0.33f), default),
        ["Horns"] = (new Color(0.42f, 0.32f, 0.85f), new Color(0.1f, 0.06f, 0.25f)),
        ["Wings"] = (new Color(0.5f, 0.4f, 1f), new Color(0.25f, 0.2f, 0.6f)),
        ["Eyes"] = (new Color(1f, 0.3f, 0.55f), new Color(1.6f, 0.25f, 0.7f)),
        ["Tongue"] = (new Color(1f, 0.3f, 0.55f), new Color(0.5f, 0.05f, 0.2f)),
    };

    static readonly Dictionary<string, (Color, Color)> ShadowStone = new Dictionary<string, (Color, Color)>
    {
        ["Main"] = (new Color(0.6f, 0.58f, 0.55f), default),
        ["Main_Dark"] = (new Color(0.45f, 0.43f, 0.41f), default),
        ["Main_2"] = (new Color(0.45f, 0.43f, 0.41f), default),
        ["Horns"] = (new Color(0.45f, 0.62f, 0.38f), default),
        ["Wings"] = (new Color(0.55f, 0.53f, 0.5f), default),
        ["Eyes"] = (new Color(0.3f, 0.29f, 0.28f), default),
        ["Teeth"] = (new Color(0.5f, 0.48f, 0.46f), default),
        ["Tongue"] = (new Color(0.4f, 0.38f, 0.36f), default),
    };

    /// <summary>Sombra de suelo (M) o voladora (B): animada y oscura de noche, estatua de piedra de día.</summary>
    static void BuildMonster(Vector3 cell, Transform parent, bool flying)
    {
        string path = Quat + (flying ? "Bee.fbx" : "Enemy.fbx");
        var monster = new GameObject(flying ? "SombraVoladora" : "Sombra");
        monster.transform.SetParent(parent);
        monster.transform.position = cell + (flying ? Vector3.up * 1.2f : Vector3.zero);
        float height = flying ? 0.9f : 1.15f;
        var size = flying ? new Vector3(1.3f, height, 0.9f) : new Vector3(1.2f, height, 1f);
        var night = Fit(path, monster.transform.position, size, Night, monster.transform, recolor: ShadowNight);
        var statue = Fit(path, monster.transform.position, size, Day, monster.transform, recolor: ShadowStone);
        var animator = night.GetComponentInChildren<Animator>();
        if (animator == null) animator = night.AddComponent<Animator>();
        animator.runtimeAnimatorController = ClipController(path, flying ? BeeController : MonsterController,
            flying ? new[] { "Flying" } : new[] { "Walk", "Idle" });
        var statueAnimator = statue.GetComponentInChildren<Animator>();
        if (statueAnimator != null) Object.DestroyImmediate(statueAnimator);
        Glow("Aura", monster.transform.position + new Vector3(0f, height * 0.5f, -0.3f), 2.6f, new Color(0.6f, 0.25f, 1f, 0.55f), Night, night.transform);

        var m = monster.AddComponent<Monster>();
        m.visual = night.transform;
        m.statue = statue.transform;
        m.animator = animator;
        m.height = height;
        m.flying = flying;
        m.speed = flying ? 2.6f : 2.2f;
    }

    static void BuildSpring(Vector3 cell, Transform parent)
    {
        var spring = Trigger("FlorMuelle", cell + new Vector3(0f, 0.4f, 0f), new Vector2(0.9f, 0.6f), parent);
        var visual = Fit(Quat + "Bouncer.fbx", cell, new Vector3(0.95f, 0.75f, 0.95f), Day, spring.transform);
        Fit(Nature + "flower_redA.fbx", cell + new Vector3(0.45f, 0f, 0.4f), new Vector3(0.4f, 0.9f, 0.4f), Day, spring.transform);
        spring.AddComponent<Spring>().visual = visual.transform;
    }

    static void BuildFragment(Vector3 cell, Transform parent)
    {
        var fragment = Trigger("FragmentoLuna", cell + new Vector3(0f, 0.5f, 0f), new Vector2(0.7f, 0.7f), parent);
        Fit(Quat + "Star.fbx", cell + new Vector3(0f, 0.15f, 0f), new Vector3(0.75f, 0.72f, 0.35f), Both, fragment.transform,
            recolor: new Dictionary<string, (Color, Color)> { ["Gold"] = (new Color(1f, 0.92f, 0.6f), new Color(0.9f, 0.7f, 0.2f)) });
        Glow("Brillo", cell + new Vector3(0f, 0.5f, -0.3f), 2.2f, new Color(1.4f, 1.1f, 0.5f, 0.8f), Both, fragment.transform).GetComponent<GlowSprite>().dayFactor = 0.8f;
        fragment.AddComponent<Collectible>();
    }

    static void BuildChest(Vector3 cell, Transform parent)
    {
        var chest = Trigger("CofreSecreto", cell + new Vector3(0f, 0.5f, 0f), new Vector2(1f, 1f), parent);
        var visual = Fit(Quat + "Chest.fbx", cell, new Vector3(1f, 0.8f, 0.8f), Both, chest.transform);
        var reward = Fit(Quat + "Star.fbx", cell + new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.58f, 0.3f), Both, chest.transform,
            recolor: new Dictionary<string, (Color, Color)> { ["Gold"] = (new Color(1f, 0.92f, 0.6f), new Color(0.9f, 0.7f, 0.2f)) });
        var pickup = chest.AddComponent<ChestPickup>();
        pickup.animator = visual.GetComponentInChildren<Animator>();
        if (pickup.animator != null)
            pickup.animator.runtimeAnimatorController = ClipController(Quat + "Chest.fbx", "Assets/Art/Cofre.controller", new[] { "Chest_Close", "Chest_Open" }, loop: false);
        pickup.reward = reward.transform;
        Glow("Brillo", cell + new Vector3(0f, 0.6f, -0.4f), 2.4f, new Color(1.4f, 1.1f, 0.5f, 0.6f), Both, chest.transform);
    }

    static void BuildCheckpoint(Vector3 cell, Transform parent)
    {
        var checkpoint = Trigger("PuntoControl", cell + new Vector3(0f, 1f, 0f), new Vector2(0.8f, 2f), parent);
        var visual = Fit(Quat + "Goal_Flag.fbx", cell, new Vector3(0.7f, 1.4f, 0.25f), Both, checkpoint.transform,
            recolor: new Dictionary<string, (Color, Color)> { ["Flag"] = (new Color(0.35f, 0.75f, 1f), new Color(0.05f, 0.15f, 0.3f)) });
        checkpoint.AddComponent<Checkpoint>().visual = visual.transform;
    }

    static void BuildGoal(Vector3 cell, string nextScene, Transform parent)
    {
        var goal = Trigger("Meta", cell + new Vector3(0f, 1f, 0f), new Vector2(1f, 2f), parent);
        goal.AddComponent<LevelGoal>().nextScene = nextScene;
        Fit(Quat + "Goal_Flag.fbx", cell, new Vector3(1.2f, 2.6f, 0.35f), Both, goal.transform,
            recolor: new Dictionary<string, (Color, Color)> { ["Flag"] = (new Color(1f, 0.8f, 0.3f), new Color(0.6f, 0.4f, 0.05f)) });
        Glow("Brillo", cell + new Vector3(0f, 1.5f, -0.3f), 3.5f, new Color(1.3f, 1f, 0.5f, 0.5f), Both, goal.transform);
    }

    static void Decorate(Vector3 top, System.Random random, Transform parent, Biome biome)
    {
        double roll = random.NextDouble();
        if (roll < 0.2)
        {
            var spot = top + new Vector3(0f, 0f, 0.8f + (float)random.NextDouble() * 0.4f);
            float s = 1.6f + (float)random.NextDouble() * 0.9f;
            Fit(biome.dayTrees[random.Next(biome.dayTrees.Length)], spot, new Vector3(s * 0.85f, s * 1.3f, s * 0.85f), Day, parent, recolor: biome.TreeColors);
            Fit(biome.nightTree, spot, new Vector3(s * 0.75f, s * 1.4f, s * 0.75f), Night, parent, biome.nightTint, biome.nightTreeGlow);
        }
        else if (roll < 0.48)
        {
            var spot = top + new Vector3((float)random.NextDouble() * 0.6f - 0.3f, 0f, -0.7f);
            string plant = biome.dayPlants[random.Next(biome.dayPlants.Length)];
            Fit(plant, spot, new Vector3(0.5f, 0.45f, 0.5f), Day, parent, recolor: biome.TreeColors);
            Fit(Nature + "mushroom_redGroup.fbx", spot, new Vector3(0.5f, 0.5f, 0.5f), Night, parent, biome.nightTint * 0.9f, biome.mushroomGlow);
            if (random.NextDouble() < 0.5)
                Glow("BrilloSeta", spot + new Vector3(0f, 0.3f, -0.2f), 1.4f, biome.mushroomGlow * 2f + new Color(0, 0, 0, 0.6f), Night, parent);
        }
        else if (roll < 0.56)
        {
            var spot = top + new Vector3(0f, 0f, 0.9f);
            Fit(Quat + (random.NextDouble() < 0.5 ? "Rock_1.fbx" : "Bush.fbx"), spot, new Vector3(1.1f, 0.7f, 0.9f), Both, parent, recolor: biome.TreeColors);
        }
    }

    // ───────────────────────── Personaje ─────────────────────────

    static readonly Dictionary<string, (Color, Color)> LumiColors = new Dictionary<string, (Color, Color)>
    {
        ["Main"] = (new Color(0.96f, 0.95f, 1f), new Color(0.05f, 0.05f, 0.08f)),
        ["Main2"] = (new Color(0.78f, 0.76f, 0.92f), default),
        ["Main_Light"] = (new Color(1f, 0.62f, 0.38f), new Color(0.08f, 0.03f, 0f)),
        ["EyeColor"] = (new Color(0.35f, 0.8f, 1f), new Color(0.2f, 0.6f, 0.9f)),
    };

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
        var visual = BuildLumi(start + new Vector3(0f, -0.9f, 0f), hero.transform);
        controller.visual = visual.transform;
        controller.animator = visual.GetComponentInChildren<Animator>();
        return body;
    }

    /// <summary>Lumi: el personaje de Quaternius en blanco y naranja, con bufanda que ondea y farol.</summary>
    static GameObject BuildLumi(Vector3 feet, Transform parent)
    {
        var visual = Fit(HeroModel, feet, new Vector3(1.35f, 1.8f, 0.85f), Both, parent, recolor: LumiColors, uniform: true);
        visual.name = "Lumi";
        var animator = visual.GetComponentInChildren<Animator>();
        if (animator == null) animator = visual.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.runtimeAnimatorController = ClipController(HeroModel, HeroController,
            new[] { "Idle", "Run", "Jump", "Jump_Idle", "Jump_Land", "Wave", "Death" });

        var neck = visual.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Neck");
        var hand = visual.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Fist.R");
        if (neck != null)
        {
            var scarfObject = new GameObject("Bufanda");
            scarfObject.transform.SetParent(visual.transform, false);
            var line = scarfObject.AddComponent<LineRenderer>();
            line.sharedMaterial = ParticleMaterial("Bufanda", SoftBand(), new Color(1f, 0.45f, 0.18f), Both, false);
            line.widthCurve = new AnimationCurve(new Keyframe(0f, 0.32f), new Keyframe(1f, 0.16f));
            line.numCapVertices = 4;
            line.textureMode = LineTextureMode.Stretch;
            var scarf = scarfObject.AddComponent<Scarf>();
            scarf.anchor = neck;
        }
        if (hand != null)
        {
            var lantern = new GameObject("Farol").transform;
            lantern.SetParent(hand, false);
            lantern.localPosition = Vector3.zero;
            var lanternWorld = hand.position + new Vector3(0f, -0.15f, -0.1f);
            var frame = Primitive(PrimitiveType.Cube, "Marco", lanternWorld, new Vector3(0.2f, 0.26f, 0.2f), Lit("FarolMarco", Both, new Color(0.25f, 0.2f, 0.3f), default));
            frame.transform.SetParent(lantern, true);
            var flame = Primitive(PrimitiveType.Cube, "Llama", lanternWorld, new Vector3(0.14f, 0.18f, 0.22f), Flat("FarolLuz", Both, new Color(2.2f, 1.6f, 0.7f)));
            flame.transform.SetParent(lantern, true);
            var glow = Glow("LuzFarol", lanternWorld + new Vector3(0f, 0f, -0.3f), 4.5f, new Color(1.2f, 0.8f, 0.35f, 0.75f), Both, lantern);
            glow.GetComponent<GlowSprite>().dayFactor = 0.25f;
        }
        return visual;
    }

    static void BuildCamera(Rigidbody2D target, int width, Biome biome)
    {
        var cameraObject = new GameObject("Camara") { tag = "MainCamera" };
        cameraObject.transform.position = target.transform.position + new Vector3(0f, 1.5f, -18f);
        var cam = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cam.fieldOfView = 35f;
        cam.farClipPlane = 200f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        EnablePostProcessing(cam, biome);
        var follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = target;
        // Media pantalla son unas 10 unidades: que no se vea fuera del nivel.
        follow.minX = Mathf.Min(9f, width * 0.5f);
        follow.maxX = Mathf.Max(width - 9f, width * 0.5f);
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

    // ───────────────────────── Modelos y materiales ─────────────────────────

    /// <summary>
    /// Coloca un modelo estirado para ocupar `size`, con la base centrada en `bottom`.
    /// `uniform` mantiene la proporción (personajes). `recolor` cambia colores por nombre de material.
    /// </summary>
    static GameObject Fit(string path, Vector3 bottom, Vector3 size, int world, Transform parent, Color? tint = null, Color glow = default,
        Dictionary<string, (Color, Color)> recolor = null, float yaw = 0f, bool uniform = false)
    {
        var model = Model(path, Vector3.zero, Vector3.one, world, parent, tint, glow, recolor);
        if (yaw != 0f) model.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * model.transform.rotation;
        var bounds = Bounds(model);
        if (bounds.size.x < 1e-4f) return model;
        var s = model.transform.localScale;
        var factor = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / Mathf.Max(bounds.size.z, 1e-4f));
        if (uniform) factor = Vector3.one * factor.y;
        // La escala es local (puede estar girada): se aplica según los ejes del mundo de forma aproximada.
        model.transform.localScale = Vector3.Scale(s, AxisScale(model.transform, factor));
        bounds = Bounds(model);
        model.transform.position += bottom - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        return model;
    }

    /// <summary>Traduce un factor de escala en ejes del mundo a los ejes locales del modelo (rotaciones de 90°).</summary>
    static Vector3 AxisScale(Transform t, Vector3 world)
    {
        Vector3 Pick(Vector3 axis)
        {
            axis = t.rotation * axis;
            axis = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
            return axis;
        }
        float Component(Vector3 localAxis)
        {
            var a = Pick(localAxis);
            return a.x * world.x + a.y * world.y + a.z * world.z;
        }
        return new Vector3(Component(Vector3.right), Component(Vector3.up), Component(Vector3.forward));
    }

    static Bounds Bounds(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(model.transform.position, Vector3.zero);
        var bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        return bounds;
    }

    static GameObject Model(string path, Vector3 position, Vector3 scale, int world, Transform parent, Color? tint = null, Color glow = default,
        Dictionary<string, (Color, Color)> recolor = null)
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
        if (parent != null) model.transform.SetParent(parent, true);
        model.transform.position = position;
        model.transform.localScale = Vector3.Scale(asset.transform.localScale, scale);
        foreach (var collider in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
        foreach (var renderer in model.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var (color, extraGlow) = recolor != null && materials[i] != null && recolor.TryGetValue(materials[i].name, out var c) ? c : ((Color?)null, default(Color));
                materials[i] = WorldMaterial(materials[i], world, tint ?? Color.white, glow + extraGlow, color);
            }
            renderer.sharedMaterials = materials;
            if (world == Night && glow != default) renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        return model;
    }

    static Material WorldMaterial(Material source, int world, Color tint, Color glow, Color? overrideColor = null)
    {
        string sourceName = source != null ? source.name : "sin_material";
        string colorKey = overrideColor.HasValue ? ColorUtility.ToHtmlStringRGB(overrideColor.Value) : "o";
        string key = $"{sourceName}_{world}_{ColorUtility.ToHtmlStringRGB(tint)}_{ColorUtility.ToHtmlStringRGB(glow)}_{colorKey}";
        if (materialCache.TryGetValue(key, out var cached)) return cached;
        var mat = LoadOrCreate($"{MaterialsFolder}/Arte/{key}.mat");
        var texture = source != null ? source.mainTexture : null;
        var color = overrideColor ?? (source != null && source.HasProperty("_Color") ? source.color : Color.white);
        if (texture != null && !overrideColor.HasValue) color = Color.white;
        mat.SetTexture("_BaseMap", texture);
        mat.SetColor("_BaseColor", color * tint);
        mat.SetColor("_Emission", glow);
        mat.SetFloat("_World", world);
        mat.enableInstancing = true;
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

    static Material LoadOrCreate(string path, string shader = "Fase/World")
    {
        string folder = Path.GetDirectoryName(path).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, path);
        }
        else if (mat.shader.name != shader) mat.shader = Shader.Find(shader);
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

    static void MarkStatic(Transform root)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>())
        {
            // Lo que se mueve o se anima no puede ser estático.
            if (t.GetComponentInParent<Monster>() || t.GetComponentInParent<Spinner>() || t.GetComponentInParent<Collectible>()
                || t.GetComponentInParent<ChestPickup>() || t.GetComponentInParent<Spring>() || t.GetComponentInParent<Checkpoint>()
                || t.GetComponentInParent<GlowSprite>() || t.GetComponent<SkinnedMeshRenderer>()) continue;
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);
        }
    }

    /// <summary>
    /// Animator con un estado por animación (mismo nombre que la animación, sin el prefijo
    /// "Armature|"). Los scripts cambian de estado por nombre, sin transiciones.
    /// </summary>
    static AnimatorController ClipController(string modelPath, string controllerPath, string[] states, bool loop = true)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
        bool changed = importer.clipAnimations.Length == 0;
        foreach (var clip in clips)
        {
            string shortName = clip.name.Contains("|") ? clip.name.Substring(clip.name.LastIndexOf('|') + 1) : clip.name;
            bool shouldLoop = loop && (shortName == "Idle" || shortName == "Run" || shortName == "Walk" || shortName == "Flying" || shortName == "Jump_Idle");
            if (clip.loopTime != shouldLoop) { clip.loopTime = shouldLoop; changed = true; }
        }
        if (changed)
        {
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var machine = controller.layers[0].stateMachine;
        var byName = new Dictionary<string, AnimationClip>();
        foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>())
        {
            if (clip.name.StartsWith("__preview")) continue;
            string shortName = clip.name.Contains("|") ? clip.name.Substring(clip.name.LastIndexOf('|') + 1) : clip.name;
            byName[shortName] = clip;
        }
        for (int i = 0; i < states.Length; i++)
        {
            var state = machine.AddState(states[i]);
            if (byName.TryGetValue(states[i], out var clip)) state.motion = clip;
            if (i == 0) machine.defaultState = state;
        }
        return controller;
    }
}
