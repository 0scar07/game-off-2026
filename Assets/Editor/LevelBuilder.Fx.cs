using System.Collections.Generic;
using System.IO;
using Fase;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Aspecto de cada capítulo: cielo, luz, paisaje, clima, partículas y postprocesado.</summary>
public static partial class LevelBuilder
{
    const string Generated = "Assets/Art/Generado/";

    /// <summary>Todo lo que cambia entre capítulos.</summary>
    class Biome
    {
        public string id, name, nameEn;
        public Color skyTop, skyHorizon, nightTop, nightHorizon, sun, haze, hillsNight;
        public Color grass, dirtLight, dirtMid, dirtDark, foliage;
        public string[] dayTrees, dayPlants;
        public string nightTree = Nature + "tree_pineRoundB.fbx";
        public Color nightTint, nightTreeGlow, mushroomGlow, fireflies;
        public int weather; // 0 polen y pétalos, 1 hojas, 2 nieve
        public string musicDay, musicNight, ambienceDay, ambienceNight;
        public float saturation, bloom;

        public Dictionary<string, (Color, Color)> GroundColors => new Dictionary<string, (Color, Color)>
        {
            ["Green_Light"] = (grass, default),
            ["Dirt_1"] = (dirtMid, default),
            ["Dirt_2"] = (dirtDark, default),
            ["Dirt_3"] = (dirtLight, default),
        };

        public Dictionary<string, (Color, Color)> TreeColors => new Dictionary<string, (Color, Color)>
        {
            ["Green"] = (foliage, default),
            ["Green_Light"] = (foliage, default),
            ["BrightGreen"] = (foliage * 1.15f, default),
            ["grass"] = (foliage, default),
        };
    }

    static readonly Biome[] Biomes =
    {
        new Biome
        {
            id = "pradera", name = "La Pradera", nameEn = "The Meadow",
            skyTop = new Color(0.25f, 0.55f, 0.92f), skyHorizon = new Color(1f, 0.93f, 0.78f),
            nightTop = new Color(0.01f, 0.01f, 0.06f), nightHorizon = new Color(0.12f, 0.08f, 0.3f),
            sun = new Color(1f, 0.92f, 0.7f), haze = new Color(0.75f, 0.9f, 0.85f), hillsNight = new Color(0.12f, 0.12f, 0.32f),
            grass = new Color(0.55f, 0.82f, 0.28f), dirtLight = new Color(0.9f, 0.64f, 0.45f), dirtMid = new Color(0.73f, 0.53f, 0.35f), dirtDark = new Color(0.6f, 0.44f, 0.29f),
            foliage = new Color(0.36f, 0.72f, 0.4f),
            dayTrees = new[] { Quat + "Tree.fbx", Quat + "Tree_Fruit.fbx" },
            dayPlants = new[] { Nature + "flower_redA.fbx", Nature + "flower_yellowB.fbx", Nature + "flower_purpleA.fbx", Quat + "Plant_Large.fbx", Quat + "Bush_Fruit.fbx" },
            nightTint = new Color(0.75f, 0.8f, 1.25f), nightTreeGlow = new Color(0.02f, 0.06f, 0.14f), mushroomGlow = new Color(0.15f, 0.25f, 0.7f),
            fireflies = new Color(1f, 0.9f, 0.45f), weather = 0,
            musicDay = "dia", musicNight = "noche", ambienceDay = "amb_pradera_dia", ambienceNight = "amb_pradera_noche",
            saturation = 12f, bloom = 0.9f,
        },
        new Biome
        {
            id = "bosque", name = "El Bosque Otoñal", nameEn = "The Autumn Woods",
            skyTop = new Color(0.95f, 0.55f, 0.3f), skyHorizon = new Color(1f, 0.88f, 0.62f),
            nightTop = new Color(0.03f, 0.0f, 0.04f), nightHorizon = new Color(0.28f, 0.06f, 0.2f),
            sun = new Color(1f, 0.75f, 0.45f), haze = new Color(1f, 0.8f, 0.6f), hillsNight = new Color(0.22f, 0.08f, 0.22f),
            grass = new Color(0.92f, 0.58f, 0.2f), dirtLight = new Color(0.62f, 0.4f, 0.26f), dirtMid = new Color(0.5f, 0.32f, 0.2f), dirtDark = new Color(0.38f, 0.24f, 0.15f),
            foliage = new Color(0.95f, 0.45f, 0.18f),
            dayTrees = new[] { Nature + "tree_oak_fall.fbx", Nature + "tree_default_fall.fbx", Nature + "tree_fat_fall.fbx" },
            dayPlants = new[] { Nature + "mushroom_tanGroup.fbx", Nature + "plant_bushDetailed.fbx", Quat + "Plant_Small.fbx", Nature + "stump_round.fbx" },
            nightTint = new Color(1.1f, 0.6f, 1.0f), nightTreeGlow = new Color(0.12f, 0.02f, 0.1f), mushroomGlow = new Color(0.6f, 0.1f, 0.4f),
            fireflies = new Color(1f, 0.45f, 0.75f), weather = 1,
            musicDay = "otono_dia", musicNight = "otono_noche", ambienceDay = "amb_bosque_dia", ambienceNight = "amb_bosque_noche",
            saturation = 18f, bloom = 1.0f,
        },
        new Biome
        {
            id = "cumbre", name = "La Cumbre Helada", nameEn = "The Frozen Peak",
            skyTop = new Color(0.45f, 0.72f, 0.98f), skyHorizon = new Color(0.92f, 0.97f, 1f),
            nightTop = new Color(0.0f, 0.01f, 0.05f), nightHorizon = new Color(0.03f, 0.14f, 0.3f),
            sun = new Color(0.95f, 0.97f, 1f), haze = new Color(0.88f, 0.94f, 1f), hillsNight = new Color(0.05f, 0.16f, 0.32f),
            grass = new Color(0.96f, 0.98f, 1f), dirtLight = new Color(0.66f, 0.74f, 0.86f), dirtMid = new Color(0.52f, 0.6f, 0.74f), dirtDark = new Color(0.4f, 0.47f, 0.6f),
            foliage = new Color(0.82f, 0.9f, 0.95f),
            dayTrees = new[] { Platformer + "tree-pine-snow.fbx", Platformer + "tree-snow.fbx" },
            dayPlants = new[] { Nature + "rock_smallA.fbx", Nature + "rock_smallC.fbx", Quat + "Rock_1.fbx" },
            nightTint = new Color(0.6f, 1.0f, 1.3f), nightTreeGlow = new Color(0.02f, 0.1f, 0.16f), mushroomGlow = new Color(0.1f, 0.5f, 0.7f),
            fireflies = new Color(0.45f, 0.95f, 1f), weather = 2,
            musicDay = "nieve_dia", musicNight = "nieve_noche", ambienceDay = "amb_cumbre_dia", ambienceNight = "amb_cumbre_noche",
            saturation = 6f, bloom = 1.1f,
        },
    };

    // ───────────────────────── Render ─────────────────────────

    /// <summary>HDR en URP (para que brille el bloom) y sin pantalla de Unity al empezar.</summary>
    static void PrepareRendering()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset == null) continue;
            asset.supportsHDR = true;
            asset.msaaSampleCount = 4;
            EditorUtility.SetDirty(asset);
        }
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        Directory.CreateDirectory(Generated);
    }

    static void EnablePostProcessing(Camera cam, Biome biome)
    {
        var data = cam.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = true;
        data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        var volume = new GameObject("Postprocesado").AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = PostProfile(biome);
    }

    static VolumeProfile PostProfile(Biome biome)
    {
        string path = $"Assets/Settings/Post_{biome.id}.asset";
        AssetDatabase.DeleteAsset(path);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(biome.bloom);
        bloom.scatter.Override(0.72f);
        var tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.Neutral);
        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.15f);
        color.contrast.Override(14f);
        color.saturation.Override(biome.saturation);
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.3f);
        vignette.smoothness.Override(0.45f);
        foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
        EditorUtility.SetDirty(profile);
        return profile;
    }

    static void BuildLight(Biome biome)
    {
        var sun = new GameObject("Luz").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.3f;
        sun.color = Color.Lerp(Color.white, biome.sun, 0.5f);
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.75f;
        sun.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(new Color(0.5f, 0.52f, 0.6f), biome.haze, 0.3f);
        RenderSettings.fog = false;
    }

    // ───────────────────────── Cielo y paisaje ─────────────────────────

    static void BuildSky(int width, System.Random random, Biome biome)
    {
        float mid = width * 0.5f;
        var sky = new GameObject("Cielo").transform;
        var dayGradient = Gradient($"cielo_{biome.id}_dia", biome.skyTop, biome.skyHorizon);
        var nightGradient = Gradient($"cielo_{biome.id}_noche", biome.nightTop, biome.nightHorizon);
        Quad("CieloDia", new Vector3(mid, 8f, 34f), new Vector2(width + 260f, 70f), ParticleMaterial($"Cielo_{biome.id}_Dia", dayGradient, Color.white, Day, false), sky);
        Quad("CieloNoche", new Vector3(mid, 8f, 34f), new Vector2(width + 260f, 70f), ParticleMaterial($"Cielo_{biome.id}_Noche", nightGradient, Color.white, Night, false), sky);
        rand = random;

        // Sol con halo y rayos.
        var sunPos = new Vector3(mid - 6f, 15f, 32f);
        Quad("Sol", sunPos, new Vector2(6f, 6f), ParticleMaterial("SolDisco", SoftDisc(), biome.sun * 2.4f, Day, true), sky);
        Quad("SolHalo", sunPos, new Vector2(34f, 34f), ParticleMaterial("SolHalo", SoftDot(), new Color(biome.sun.r, biome.sun.g, biome.sun.b, 0.55f), Day, true), sky);
        for (int i = 0; i < 7; i++)
        {
            var ray = Quad("Rayo", sunPos + new Vector3(0f, 0f, -2f), new Vector2(3.2f + R(0f, 2.5f), 70f), ParticleMaterial("Rayo", Ray(), new Color(biome.sun.r, biome.sun.g, biome.sun.b, 0.12f), Day, true), sky);
            ray.transform.rotation = Quaternion.Euler(0f, 0f, -20f + i * 7f + R(-2f, 2f));
            ray.transform.position = sunPos + ray.transform.up * -33f + new Vector3(0f, 0f, -2f);
        }

        // Luna, aurora y estrellas.
        var moonPos = new Vector3(mid + 8f, 15f, 32f);
        Quad("Luna", moonPos, new Vector2(4.6f, 4.6f), ParticleMaterial("LunaDisco", SoftDisc(), new Color(1.6f, 1.7f, 2f), Night, true), sky);
        Quad("LunaHalo", moonPos, new Vector2(24f, 24f), ParticleMaterial("LunaHalo", SoftDot(), new Color(0.6f, 0.7f, 1f, 0.45f), Night, true), sky);
        var aurora = Quad("Aurora", new Vector3(mid, 14f, 31f), new Vector2(width + 120f, 14f), ParticleMaterial($"Aurora_{biome.id}", Band(), new Color(biome.fireflies.r * 0.8f, 0.5f + biome.fireflies.g * 0.4f, 1f, 0.32f), Night, true), sky);
        aurora.transform.rotation = Quaternion.Euler(0f, 0f, 4f);
        var starMat = ParticleMaterial("Estrella", SoftDot(), new Color(1.4f, 1.4f, 1.7f, 1f), Night, true);
        for (int i = 0; i < 70 + width * 2; i++)
            Quad("Estrella", new Vector3(R(-40f, width + 40f), R(-2f, 30f), 30f), Vector2.one * R(0.25f, 0.7f), starMat, sky);

        // Colinas onduladas en tres capas, cada vez más difuminadas por la bruma, y nubes de día.
        var hills = new GameObject("Colinas").transform;
        for (int layer = 0; layer < 3; layer++)
        {
            float z = 12f + layer * 7f, baseY = 1.5f + layer * 2.2f, amplitude = 2.2f + layer * 1.2f;
            var dayTop = Color.Lerp(biome.foliage * 1.05f, biome.haze, 0.12f + layer * 0.22f);
            var dayBottom = Color.Lerp(biome.foliage * 0.7f, biome.haze, 0.25f + layer * 0.2f);
            var nightTop = Color.Lerp(biome.hillsNight, biome.nightHorizon, layer * 0.3f) * 1.4f;
            var nightBottom = Color.Lerp(nightTop, biome.nightTop, 0.6f);
            var (dayMesh, nightMesh) = HillMeshes($"{biome.id}_{width}_{layer}", width, baseY, amplitude, dayTop, dayBottom, nightTop, nightBottom);
            MeshObject("ColinaDia", dayMesh, new Vector3(0f, 0f, z), ParticleMaterial("ColinaDia", null, Color.white, Day, false), hills);
            MeshObject("ColinaNoche", nightMesh, new Vector3(0f, 0f, z), ParticleMaterial("ColinaNoche", null, Color.white, Night, false), hills);
        }
        for (float x = -30f; x < width + 30f; x += R(10f, 18f))
            Fit(Quat + $"Cloud_{1 + (int)R(0f, 2.99f)}.fbx", new Vector3(x, R(10f, 16f), R(28f, 31f)), new Vector3(R(4f, 7f), R(2f, 3f), 2f), Day, sky,
                new Color(1f, 1f, 1f), new Color(0.35f, 0.35f, 0.35f));
    }

    /// <summary>Silueta de colinas (suma de ondas) con degradado de color de la cima a la base.</summary>
    static (Mesh day, Mesh night) HillMeshes(string name, int width, float baseY, float amplitude, Color dayTop, Color dayBottom, Color nightTop, Color nightBottom)
    {
        float phase1 = R(0f, 10f), phase2 = R(0f, 10f), phase3 = R(0f, 10f);
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        float x0 = -60f, x1 = width + 60f, bottom = -8f;
        int steps = Mathf.CeilToInt((x1 - x0) / 0.75f);
        for (int i = 0; i <= steps; i++)
        {
            float x = x0 + i * 0.75f;
            float y = baseY + amplitude * (0.55f * Mathf.Sin(x * 0.11f + phase1) + 0.3f * Mathf.Sin(x * 0.27f + phase2) + 0.15f * Mathf.Sin(x * 0.61f + phase3));
            vertices.Add(new Vector3(x, y, 0f));
            vertices.Add(new Vector3(x, bottom, 0f));
            if (i > 0)
            {
                int a = (i - 1) * 2, b = i * 2;
                triangles.AddRange(new[] { a, b, a + 1, b, b + 1, a + 1 });
            }
        }
        Mesh Make(string suffix, Color top, Color low)
        {
            var mesh = new Mesh { name = "colinas_" + name + suffix };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            var colors = new List<Color>();
            // Los colores de vértice van en espacio lineal: sin convertir saldrían lavados.
            for (int i = 0; i < vertices.Count; i++) colors.Add((i % 2 == 0 ? top : low).linear);
            mesh.SetColors(colors);
            mesh.RecalculateBounds();
            string path = Generated + mesh.name + ".asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
        return (Make("_dia", dayTop, dayBottom), Make("_noche", nightTop, nightBottom));
    }

    static GameObject MeshObject(string name, Mesh mesh, Vector3 position, Material material, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return go;
    }

    static System.Random rand = new System.Random(1);
    static float R(float a, float b) => a + (float)rand.NextDouble() * (b - a);

    static void BuildLineVisual(int index = 0)
    {
        var line = new GameObject(index == 0 ? "DibujoLinea" : "DibujoLinea2");
        line.transform.position = new Vector3(0f, 5f, -1.6f);
        line.AddComponent<LineVisual>().index = index;
        var core = Primitive(PrimitiveType.Cube, "Nucleo", new Vector3(0f, 5f, -1.6f), new Vector3(0.07f, 80f, 0.07f), Flat("Linea", Both, new Color(2.6f, 2.4f, 2f)));
        core.transform.SetParent(line.transform, true);
        var glow = Quad("Halo", new Vector3(0f, 5f, -1.65f), new Vector2(2.2f, 80f), ParticleMaterial("LineaHalo", Ray(true), new Color(1f, 0.95f, 0.85f, 0.35f), Both, true), line.transform);
        // Chispas: cálidas hacia el día y frías hacia la noche.
        var sparks = Particles("Chispas", line.transform, new Vector3(0f, 5f, -1.7f), ParticleMaterial("Chispa", SoftDot(), Color.white, Both, true));
        var main = sparks.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.5f), new Color(0.6f, 0.75f, 1f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = sparks.emission;
        emission.rateOverTime = 40f;
        var shape = sparks.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.2f, 16f, 0.1f);
        var noise = sparks.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        FadeOut(sparks);
    }

    static void BuildLimits(float min, float max)
    {
        var mat = ParticleMaterial("Limite", SoftDot(), new Color(1.6f, 0.4f, 0.3f, 0.9f), Both, true);
        var root = new GameObject("Limites").transform;
        foreach (float x in new[] { min, max })
            for (int i = -2; i < 16; i++)
                Quad("Limite", new Vector3(x, i + 0.25f, -1.3f), new Vector2(0.35f, 0.35f), mat, root);
    }

    // ───────────────────────── Clima ─────────────────────────

    static void BuildWeather(int width, int height, Biome biome)
    {
        var root = new GameObject("Clima").transform;
        var area = new Vector3(width + 20f, height + 8f, 6f);
        var center = new Vector3(width * 0.5f, height * 0.5f + 2f, 0f);

        // Luciérnagas de noche (en todos los capítulos, del color de cada uno).
        var fireflies = Particles("Luciernagas", root, center, ParticleMaterial($"Luciernaga_{biome.id}", SoftDot(), biome.fireflies * 2.2f, Night, true));
        Ambient(fireflies, area, rate: width * 0.6f, life: 6f, size: (0.12f, 0.3f), noise: 0.5f);

        if (biome.weather == 0)
        {
            var pollen = Particles("Polen", root, center, ParticleMaterial("Polen", SoftDot(), new Color(1.4f, 1.35f, 1f, 0.8f), Day, true));
            Ambient(pollen, area, rate: width * 0.8f, life: 8f, size: (0.05f, 0.12f), noise: 0.3f);
            var petals = Particles("Petalos", root, center, ParticleMaterial("Petalo", SoftDot(), new Color(1f, 0.6f, 0.75f, 0.9f), Day, false));
            Ambient(petals, area, rate: width * 0.08f, life: 9f, size: (0.12f, 0.2f), noise: 1.2f, stretch: true);
        }
        else if (biome.weather == 1)
        {
            var leaves = Particles("Hojas", root, center + Vector3.up * 4f, ParticleMaterial("Hoja", SoftDot(), new Color(1f, 0.55f, 0.2f, 0.95f), Both, false));
            Ambient(leaves, area, rate: width * 0.35f, life: 9f, size: (0.15f, 0.28f), noise: 0.9f, gravity: 0.06f, stretch: true);
        }
        else
        {
            var snow = Particles("Nieve", root, center + Vector3.up * 4f, ParticleMaterial("Copo", SoftDot(), new Color(1f, 1f, 1f, 0.95f), Both, false));
            Ambient(snow, area, rate: width * 1.4f, life: 10f, size: (0.06f, 0.16f), noise: 0.4f, gravity: 0.05f);
        }
    }

    static void Ambient(ParticleSystem system, Vector3 area, float rate, float life, (float, float) size, float noise, float gravity = 0f, bool stretch = false)
    {
        var main = system.main;
        main.startLifetime = life;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(size.Item1, size.Item2);
        main.maxParticles = 3000;
        main.prewarm = true;
        main.loop = true;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        if (stretch)
        {
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(size.Item1 * 1.6f, size.Item2 * 1.6f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(size.Item1 * 0.8f, size.Item2 * 0.8f);
            main.startSizeZ = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
        }
        var emission = system.emission;
        emission.rateOverTime = rate / life * 6f;
        var shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = area;
        var n = system.noise;
        n.enabled = true;
        n.strength = noise;
        n.frequency = 0.25f;
        FadeOut(system, fadeIn: true);
    }

    static void FadeOut(ParticleSystem system, bool fadeIn = false)
    {
        var color = system.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            fadeIn
                ? new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
                : new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = gradient;
    }

    static ParticleSystem Particles(string name, Transform parent, Vector3 position, Material material)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        var system = go.AddComponent<ParticleSystem>();
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        var main = system.main;
        main.playOnAwake = true;
        return system;
    }

    // ───────────────────────── Efectos de juego ─────────────────────────

    static void BuildFx()
    {
        var fx = new GameObject("Efectos").AddComponent<FxManager>();
        fx.dust = Burst("Polvo", fx.transform, ParticleMaterial("Polvo", SoftDot(), new Color(1f, 0.95f, 0.85f, 0.7f), Both, false), new Color(1f, 0.95f, 0.9f), 1.6f, 0.45f, (0.15f, 0.35f), 0.4f, up: true);
        fx.appearDay = Burst("ApareceDia", fx.transform, ParticleMaterial("ChispaDia", SoftDot(), new Color(1.6f, 1.2f, 0.6f, 1f), Day, true), Color.white, 2.5f, 0.7f, (0.08f, 0.2f), 0f);
        fx.appearNight = Burst("ApareceNoche", fx.transform, ParticleMaterial("ChispaNoche", SoftDot(), new Color(0.6f, 0.9f, 1.8f, 1f), Night, true), Color.white, 2.5f, 0.7f, (0.08f, 0.2f), 0f);
        fx.collect = Burst("Luna", fx.transform, ParticleMaterial("ChispaLuna", SoftDot(), new Color(2f, 1.6f, 0.6f, 1f), Both, true), Color.white, 6f, 0.9f, (0.1f, 0.25f), 0f);
        fx.poof = Burst("Nube", fx.transform, ParticleMaterial("NubeCaida", SoftDot(), new Color(1.2f, 0.9f, 1.4f, 0.9f), Both, true), Color.white, 4f, 0.6f, (0.2f, 0.45f), 0f);
    }

    static ParticleSystem Burst(string name, Transform parent, Material material, Color color, float speed, float life, (float, float) size, float gravity, bool up = false)
    {
        var system = Particles(name, parent, Vector3.zero, material);
        var main = system.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.6f, life);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size.Item1, size.Item2);
        main.startColor = color;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = system.emission;
        emission.rateOverTime = 0f;
        var shape = system.shape;
        shape.shapeType = up ? ParticleSystemShapeType.Hemisphere : ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        if (up) shape.rotation = new Vector3(-90f, 0f, 0f);
        var sizeOverLife = system.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f));
        FadeOut(system);
        system.Stop();
        return system;
    }

    /// <summary>Halo de luz que mira a la cámara (GlowSprite).</summary>
    static GameObject Glow(string name, Vector3 position, float size, Color color, int world, Transform parent)
    {
        var quad = Quad(name, position, Vector2.one, ParticleMaterial($"Halo_{ColorUtility.ToHtmlStringRGBA(color)}_{world}", SoftDot(), color, world, true), parent);
        var glow = quad.AddComponent<GlowSprite>();
        glow.size = size;
        glow.dayFactor = 1f;
        return quad;
    }

    static GameObject Quad(string name, Vector3 position, Vector2 size, Material material, Transform parent)
    {
        var quad = Primitive(PrimitiveType.Quad, name, position, new Vector3(size.x, size.y, 1f), material);
        if (parent != null) quad.transform.SetParent(parent, true);
        return quad;
    }

    // ───────────────────────── Materiales y texturas generadas ─────────────────────────

    static Material ParticleMaterial(string name, Texture2D texture, Color color, int world, bool additive)
    {
        var mat = LoadOrCreate($"{MaterialsFolder}/Fx/{name}.mat", "Fase/Particle");
        mat.SetTexture("_MainTex", texture);
        mat.SetColor("_Color", color);
        mat.SetFloat("_World", world);
        mat.SetFloat("_Additive", additive ? 1f : 0f);
        mat.SetFloat("_SrcBlend", additive ? (float)BlendMode.One : (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
        mat.renderQueue = 3000;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Texture2D SoftDot() => GeneratedTexture("punto_suave", 128, 128, (u, v) =>
    {
        float d = Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f);
        float a = Mathf.Pow(1f - d, 2.2f);
        return new Color(1f, 1f, 1f, a);
    });

    static Texture2D SoftDisc() => GeneratedTexture("disco", 128, 128, (u, v) =>
    {
        float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
        return new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 18f));
    });

    static Texture2D SoftBand() => GeneratedTexture("banda", 16, 64, (u, v) => new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Sin(v * Mathf.PI) * 1.8f)));

    /// <summary>Rayo de luz: vertical, suave por los lados y desvanecido hacia un extremo.</summary>
    static Texture2D Ray(bool symmetric = false) => GeneratedTexture(symmetric ? "rayo_simetrico" : "rayo", 64, 256, (u, v) =>
    {
        float across = Mathf.Pow(Mathf.Sin(u * Mathf.PI), 2.5f);
        float along = symmetric ? Mathf.Sin(v * Mathf.PI) : v;
        return new Color(1f, 1f, 1f, across * along);
    });

    static Texture2D Band() => GeneratedTexture("aurora", 256, 64, (u, v) =>
    {
        float wave = Mathf.Sin(u * Mathf.PI * 3f) * 0.12f;
        float a = Mathf.Exp(-Mathf.Pow((v - 0.5f - wave) * 4.5f, 2f)) * Mathf.Sin(u * Mathf.PI);
        return new Color(0.7f + 0.3f * u, 1f, 0.8f + 0.2f * (1f - u), a);
    });

    static Texture2D Gradient(string name, Color top, Color horizon) => GeneratedTexture(name, 4, 256, (u, v) =>
        Color.Lerp(horizon, top, Mathf.SmoothStep(0.25f, 1f, v)));

    /// <summary>Crea (si no existe) una textura PNG dibujada por una función y la importa sin compresión.</summary>
    static Texture2D GeneratedTexture(string name, int width, int height, System.Func<float, float, Color> paint)
    {
        string path = Generated + name + ".png";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, paint((x + 0.5f) / width, (y + 0.5f) / height));
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
