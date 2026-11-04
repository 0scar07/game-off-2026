using System.Linq;
using Fase;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Interfaz (HUD, controles táctiles), sonido y música de cada escena.</summary>
public static partial class LevelBuilder
{
    const string FontPath = "Assets/Art/Fonts/FredokaFase.ttf";   // Fredoka con los símbolos de la interfaz (Tools/fuente_simbolos.py)
    const string Own = "Assets/Audio/Sfx/Fase/";
    static readonly Color PillColor = new Color(0.04f, 0.03f, 0.14f, 0.5f);

    // ───────────────────────── Sonido ─────────────────────────

    static void BuildAudio(Biome biome)
    {
        var audio = new GameObject("Sonido").AddComponent<GameAudio>();
        audio.stepsDay = Clips(biome.weather == 2 ? "footstep_snow_00" : "footstep_grass_00", 5);
        audio.stepsNight = Clips("footstep_snow_00", 5);
        // Efectos propios (Tools/efectos.py); los pasos siguen siendo de Kenney.
        audio.jump = Clip(Own + "fase_salto.wav");
        audio.land = new[] { Clip(Own + "fase_aterrizaje.wav") };
        audio.die = Clip(Own + "fase_perder.wav");
        audio.lineGrab = Clip(Own + "fase_linea_agarrar.wav");
        audio.lineRelease = Clip(Own + "fase_linea_soltar.wav");
        audio.lineTick = Clip(Own + "fase_linea_tic.wav");
        audio.appearDay = Clip(Own + "fase_aparece_dia.wav");
        audio.appearNight = Clip(Own + "fase_aparece_noche.wav");
        audio.vanish = Clip(Own + "fase_desaparece.wav");
        audio.goal = Clip(Own + "fase_meta.wav");
        audio.collect = Clip(Own + "fase_fragmento.wav");
        audio.checkpoint = Clip(Own + "fase_control.wav");
        audio.bounce = Clip(Own + "fase_muelle.wav");
        audio.voiceJump = Enumerable.Range(0, 3).Select(i => Clip(Own + $"fase_voz_salto_{i}.wav")).ToArray();
        audio.voiceLand = Clip(Own + "fase_voz_aterrizaje.wav");
        audio.voiceDie = Clip(Own + "fase_voz_perder.wav");
        audio.voiceGoal = Clip(Own + "fase_voz_meta.wav");
        audio.voiceCollect = Clip(Own + "fase_voz_fragmento.wav");

        var music = audio.gameObject.AddComponent<MusicLayers>();
        music.day = MusicClip(biome.musicDay);
        music.night = MusicClip(biome.musicNight);
        music.ambienceDay = MusicClip(biome.ambienceDay);
        music.ambienceNight = MusicClip(biome.ambienceNight);
        // Capas que reaccionan al juego (Tools/musica_extra.py), en la tonalidad de cada capítulo.
        int chapter = System.Array.IndexOf(Biomes, biome);
        music.lineLayer = MusicClip(new[] { "linea_pradera", "linea_bosque", "linea_cumbre" }[Mathf.Clamp(chapter, 0, 2)]);
        music.finale = MusicClip(new[] { "final_pradera", "final_bosque", "final_cumbre" }[Mathf.Clamp(chapter, 0, 2)]);
        music.keyRatio = chapter == 2 ? 1.1225f : 1f;   // la cumbre está en Mi; las otras, en Re
        music.bossLayer = MusicClip(new[] { "jefe_tambores_pradera", "jefe_tambores_bosque", "jefe_tambores" }[Mathf.Clamp(chapter, 0, 2)]);
    }

    /// <summary>Música o ambiente de Assets/Audio/Music, comprimido en memoria (lo que mejor va en la web).</summary>
    static AudioClip MusicClip(string name)
    {
        string path = Music + name + ".wav";
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (importer == null) { Debug.LogWarning("Falta la música " + path); return null; }
        var settings = importer.defaultSampleSettings;
        // Calidad 0,45: casi igual al oído y bastante más ligera para la web.
        if (settings.loadType != AudioClipLoadType.CompressedInMemory || settings.compressionFormat != AudioCompressionFormat.Vorbis || settings.quality > 0.46f)
        {
            settings.loadType = AudioClipLoadType.CompressedInMemory;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.45f;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    /// <summary>
    /// Efecto o voz: en mono y comprimido (Vorbis) para que la web pese menos; se descomprime al
    /// cargar porque son cortos y suenan muchas veces.
    /// </summary>
    static AudioClip Clip(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (importer != null)
        {
            var settings = importer.defaultSampleSettings;
            if (!importer.forceToMono || settings.compressionFormat != AudioCompressionFormat.Vorbis || settings.quality > 0.46f)
            {
                importer.forceToMono = true;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.45f;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("No encuentro el sonido " + path);
        return clip;
    }

    static AudioClip[] Clips(string prefix, int count) =>
        Enumerable.Range(0, count).Select(i => Clip($"{Sfx}{prefix}{i}.ogg")).ToArray();

    // ───────────────────────── Interfaz ─────────────────────────

    static Font UiFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    /// <summary>Rectángulo con esquinas muy redondeadas (píldora), estirable sin deformar las esquinas.</summary>
    static Sprite RoundedSprite()
    {
        string path = Generated + "redondeado.png";
        var tex = GeneratedTexture("redondeado", 96, 96, (u, v) =>
        {
            float x = Mathf.Abs(u - 0.5f) * 96f, y = Mathf.Abs(v - 0.5f) * 96f, r = 46f;
            float dx = Mathf.Max(x - (48f - r), 0f), dy = Mathf.Max(y - (48f - r), 0f);
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return new Color(1f, 1f, 1f, Mathf.Clamp01(r - d + 0.5f));
        });
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(46f, 46f, 46f, 46f);
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// <summary>Rectángulo con esquinas suaves (paneles grandes).</summary>
    static Sprite PanelSprite()
    {
        string path = Generated + "panel.png";
        GeneratedTexture("panel", 96, 96, (u, v) =>
        {
            float x = Mathf.Abs(u - 0.5f) * 96f, y = Mathf.Abs(v - 0.5f) * 96f, r = 28f;
            float dx = Mathf.Max(x - (48f - r), 0f), dy = Mathf.Max(y - (48f - r), 0f);
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return new Color(1f, 1f, 1f, Mathf.Clamp01(r - d + 0.5f));
        });
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(32f, 32f, 32f, 32f);
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

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
        text.font = UiFont();
        text.fontSize = fontSize;
        // La fuente variable (Fredoka) es fina por defecto: los textos grandes, en negrita.
        if (fontSize >= 40) text.fontStyle = FontStyle.Bold;
        text.alignment = align;
        text.color = Color.white;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0.1f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -4f);
        return text;
    }

    /// <summary>Píldora semitransparente detrás de un texto del HUD.</summary>
    static Image Pill(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = PillColor;
        image.raycastTarget = false;
        return image;
    }

    static void BuildHud(Level level, int number, int fragments)
    {
        var canvas = NewCanvas("Interfaz", 0).transform;
        var hud = canvas.gameObject.AddComponent<Hud>();
        hud.levelNumber = number;
        hud.levelName = level.name;
        hud.levelNameEn = level.nameEn;
        hud.hint = level.hint;
        hud.hintEn = level.hintEn;
        hud.fragmentsTotal = fragments;
        hud.medalTimes = level.MedalTimes;
        hud.daily = level.daily || level.sandbox;
        hud.sandbox = level.sandbox;
        hud.title = Label(canvas, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(1700f, 150f), 104, TextAnchor.MiddleCenter);
        hud.hintText = Label(canvas, "Pista", new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1500f, 150f), 46, TextAnchor.MiddleCenter);
        Pill(canvas, "FondoEsquina", new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(620f, 76f));
        hud.corner = Label(canvas, "Esquina", new Vector2(0f, 1f), new Vector2(60f, -36f), new Vector2(580f, 60f), 40, TextAnchor.UpperLeft);
        if (fragments > 0) Pill(canvas, "FondoLuna", new Vector2(1f, 1f), new Vector2(-160f, -24f), new Vector2(300f, 76f));
        hud.counter = Label(canvas, "Contador", new Vector2(1f, 1f), new Vector2(-190f, -36f), new Vector2(260f, 60f), 40, TextAnchor.UpperRight);
        hud.counter.color = new Color(1f, 0.9f, 0.6f);
        Pill(canvas, "FondoTiempo", new Vector2(0f, 0f), new Vector2(28f, 24f), new Vector2(260f, 64f));
        hud.timer = Label(canvas, "Tiempo", new Vector2(0f, 0f), new Vector2(60f, 36f), new Vector2(220f, 50f), 36, TextAnchor.LowerLeft);
        hud.timer.color = new Color(0.82f, 0.88f, 1f);
        hud.banner = Label(canvas, "Aviso", new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(1700f, 320f), 96, TextAnchor.MiddleCenter);
        hud.banner.color = new Color(1f, 0.95f, 0.8f);
    }

    /// <summary>Botones de moverse y saltar; solo se ven en móviles (MobileControls).</summary>
    static void BuildMobileUi()
    {
        var events = new GameObject("EventSystem");
        events.AddComponent<UnityEngine.EventSystems.EventSystem>();
        events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var canvas = NewCanvas("ControlesMovil", 1);
        canvas.gameObject.AddComponent<MobileControls>();
        TouchButton(canvas.transform, "Izquierda", MobileButton.Kind.Left, new Vector2(0f, 0f), new Vector2(190f, 190f), 180f);
        TouchButton(canvas.transform, "Derecha", MobileButton.Kind.Right, new Vector2(0f, 0f), new Vector2(440f, 190f), 0f);
        TouchButton(canvas.transform, "Saltar", MobileButton.Kind.Jump, new Vector2(1f, 0f), new Vector2(-220f, 220f), 90f, 260f);
    }

    /// <summary>Botón redondo con un triángulo dibujado dentro (`arrow` = hacia dónde apunta, en grados).</summary>
    static void TouchButton(Transform parent, string name, MobileButton.Kind kind, Vector2 anchor, Vector2 position, float arrow, float size = 210f)
    {
        var button = new GameObject(name, typeof(RectTransform));
        button.transform.SetParent(parent, false);
        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(size, size);
        var image = button.AddComponent<Image>();
        image.sprite = RoundedSprite();
        image.color = new Color(1f, 1f, 1f, 0.22f);
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

    /// <summary>Las transiciones las crea SceneFader al usarse; aquí no hace falta nada en la escena.</summary>
    static void BuildTransition() { }
}
