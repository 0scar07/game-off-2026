using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fase;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Menú principal con "El viaje", pausa, historia (intro) y final.</summary>
public static partial class LevelBuilder
{
    const string MenuSceneName = PauseMenu.MenuScene;
    const string FinalScene = "Final";
    const string IntroScene = "Intro";
    const string IntroArt = "Assets/Art/Intro/";

    static readonly Color PanelColor = new Color(0.05f, 0.04f, 0.15f, 0.88f);
    static readonly Color ButtonColor = new Color(0.16f, 0.13f, 0.36f, 0.95f);
    static readonly Color DayText = new Color(1f, 0.95f, 0.82f);
    static readonly Color NightText = new Color(0.62f, 0.75f, 1f);

    [MenuItem("Fase/Abrir menú del juego")]
    static void OpenMenu() => OpenScene($"Assets/Scenes/{MenuSceneName}.unity");

    /// <summary>Pantalla de inicio: el logo partido por la línea antes del menú (SplashScreen).</summary>
    static string BuildSplash()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("Camara") { tag = "MainCamera" }.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.gameObject.AddComponent<AudioListener>();
        var splash = new GameObject("Inicio").AddComponent<SplashScreen>();
        splash.nextScene = MenuSceneName;
        splash.chord = MusicClip("final_pradera");
        string path = "Assets/Scenes/Inicio.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    /// <summary>Solo el menú (mucho más rápido que regenerar todos los niveles).</summary>
    /// <summary>En modo Play: lanza la celebración del primer capítulo para verla sin jugar hasta el final.</summary>
    [MenuItem("Fase/Probar celebración de capítulo")]
    static void TestChapterCelebration()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Pulsa Play primero."); return; }
        var catalog = LevelCatalog.Get;
        for (int level = 1; level <= catalog.levels.Length; level++)
            if (ChapterCelebration.IsChapterEnd(level))
            {
                new GameObject("PruebaCelebracion").AddComponent<UiRunner>().StartCoroutine(ChapterCelebration.Run(level));
                return;
            }
    }

    [MenuItem("Fase/Generar solo el menú")]
    static void BuildMenuOnly()
    {
        var levels = Directory.GetFiles(LevelsFolder, "*.txt").OrderBy(f => f, System.StringComparer.Ordinal).Select(Parse).OrderBy(l => l.secret).ToList();
        PrepareRendering();
        BuildMenu(levels);
        WritePreviews(levels);
        AssetDatabase.SaveAssets();
        Debug.Log("Menú generado.");
    }

    [MenuItem("Fase/Abrir inicio")]
    static void OpenSplash() => OpenScene("Assets/Scenes/Inicio.unity");

    [MenuItem("Fase/Abrir jefe")]
    static void OpenBoss() => OpenScene($"{ScenesFolder}/12-la-gran-sombra.unity");

    [MenuItem("Fase/Abrir final")]
    static void OpenFinal() => OpenScene($"Assets/Scenes/{FinalScene}.unity");

    [MenuItem("Fase/Desbloquear todo (pruebas)")]
    static void UnlockAll()
    {
        PlayerPrefs.SetInt("fase_desbloqueado", 99);
        PlayerPrefs.Save();
        Debug.Log("Todos los niveles desbloqueados (solo en este ordenador).");
    }

    static void OpenScene(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(path);
    }

    /// <summary>Paisaje del menú y del final: suelo con piezas de día y de noche para lucir el corte.</summary>
    static Level Backdrop(string name) => new Level
    {
        file = name,
        name = name,
        line = 20f,
        // Suelo continuo y plano de punta a punta (sin huecos que aparezcan y desaparezcan);
        // las piezas de día y de noche flotan encima para que la línea siga mostrando el cambio.
        rows = new[]
        {
            "", "", "", "", "",
            "                        nnn",
            "                  DD            nn",
            "",
            "",
            "",
            "##################################################",
            "##################################################",
            "##################################################",
        },
    };

    // ───────────────────────── Menú principal ─────────────────────────

    static string BuildMenu(List<Level> levels)
    {
        materialCache.Clear();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, line) = BuildWorld(Backdrop("Menu"), "");
        line.autoSweep = true;
        line.sweepCenter = 20f;
        line.sweepAmplitude = 11f;
        StaticCamera(new Vector3(20f, 5f, -19f));
        BuildAudio(Biomes[0]);
        // Lumi pasea por el fondo del menú.
        // En el mismo plano que en los niveles (z = 0): más al fondo quedaba tapada por el borde del suelo.
        var walkerModel = BuildLumi(new Vector3(25f, 3f, 0f), null);
        walkerModel.transform.localScale *= 1.15f;
        var walker = walkerModel.AddComponent<MenuWalker>();
        walker.minX = 21.5f;
        walker.maxX = 29f;
        // Música del capítulo por el que vas y día o noche según la hora real.
        var mood = Object.FindAnyObjectByType<MusicLayers>().gameObject.AddComponent<MenuMood>();
        mood.day = Biomes.Select(b => MusicClip(b.musicDay)).ToArray();
        mood.night = Biomes.Select(b => MusicClip(b.musicNight)).ToArray();
        mood.ambienceDay = Biomes.Select(b => MusicClip(b.ambienceDay)).ToArray();
        mood.ambienceNight = Biomes.Select(b => MusicClip(b.ambienceNight)).ToArray();

        var canvas = NewCanvas("Menu", 0).transform;
        var menu = canvas.gameObject.AddComponent<MainMenu>();
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        menu.introScene = IntroScene;

        // Sombra suave a la izquierda para que se lea el texto sobre el paisaje.
        var shade = new GameObject("SombraIzquierda", typeof(RectTransform));
        shade.transform.SetParent(canvas, false);
        var shadeRect = (RectTransform)shade.transform;
        shadeRect.anchorMin = new Vector2(0f, 0f);
        shadeRect.anchorMax = new Vector2(0.62f, 1f);
        shadeRect.offsetMin = shadeRect.offsetMax = Vector2.zero;
        var shadeImage = shade.AddComponent<RawImage>();
        shadeImage.texture = GeneratedTexture("sombra_menu", 256, 4, (u, v) => new Color(0.02f, 0.02f, 0.09f, 0.78f * Mathf.Pow(1f - u, 1.6f)));
        shadeImage.raycastTarget = false;

        // Principal, a la izquierda: logo partido, lema, Continuar (con dónde vas) y El viaje.
        menu.mainPanel = Column(canvas, "Principal", Vector2.zero, 14f);
        var mainRect = (RectTransform)menu.mainPanel.transform;
        mainRect.anchorMin = mainRect.anchorMax = mainRect.pivot = new Vector2(0f, 0.5f);
        mainRect.anchoredPosition = new Vector2(70f, 70f);
        var mainLayout = menu.mainPanel.GetComponent<VerticalLayoutGroup>();
        mainLayout.childAlignment = TextAnchor.MiddleLeft;
        mainLayout.padding = new RectOffset(40, 40, 40, 40);
        BuildLogo(menu.mainPanel.transform);
        var tagline = Label(menu.mainPanel.transform, "Lema", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 60f), 40, TextAnchor.MiddleLeft);
        Localize(tagline, "tagline");
        var taglineElement = tagline.gameObject.AddComponent<LayoutElement>();
        taglineElement.preferredHeight = 80f;
        taglineElement.preferredWidth = 760f;
        var play = UiButton(menu.mainPanel.transform, "play", new Vector2(560f, 116f), out menu.playLabel, 56);
        play.GetComponent<Image>().color = new Color(1f, 0.74f, 0.35f, 0.97f);
        menu.playLabel.color = new Color(0.17f, 0.1f, 0f);
        menu.playLabel.GetComponent<Shadow>().enabled = false;
        // El texto lo pone MainMenu (lleva el ▶ delante), no el traductor automático.
        Object.DestroyImmediate(menu.playLabel.GetComponent<LocText>());
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
        menu.playInfo = Label(menu.mainPanel.transform, "DondeVas", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 40f), 28, TextAnchor.MiddleCenter);
        menu.playInfo.color = new Color(0.8f, 0.84f, 1f);
        var infoElement = menu.playInfo.gameObject.AddComponent<LayoutElement>();
        infoElement.preferredHeight = 44f;
        infoElement.preferredWidth = 560f;
        UnityEventTools.AddPersistentListener(UiButton(menu.mainPanel.transform, "journey", new Vector2(560f, 92f), out _, 44).onClick, menu.ShowJourney);

        BuildJourney(canvas, menu, levels);
        // Barra de abajo (Lumi, Logros, Diario, Desafío, Taller, Sin fin, Tienda, Opciones),
        // controles y niveles secretos: se montan al arrancar (MenuExtras).
        canvas.gameObject.AddComponent<MenuExtras>();

        // Opciones.
        menu.optionsPanel = Column(canvas, "Opciones", Vector2.zero, 24f, PanelColor);
        Heading(menu.optionsPanel.transform, "options");
        menu.music = LabeledSlider(menu.optionsPanel.transform, "music");
        UnityEventTools.AddPersistentListener(menu.music.onValueChanged, menu.OnMusicChanged);
        menu.sfx = LabeledSlider(menu.optionsPanel.transform, "sfx");
        UnityEventTools.AddPersistentListener(menu.sfx.onValueChanged, menu.OnSfxChanged);
        UnityEventTools.AddPersistentListener(UiButton(menu.optionsPanel.transform, "language", new Vector2(620f, 96f), out _, 40).onClick, menu.ToggleLanguage);
        UnityEventTools.AddPersistentListener(UiButton(menu.optionsPanel.transform, "intro", new Vector2(620f, 96f), out _, 40).onClick, menu.WatchIntro);
        UnityEventTools.AddPersistentListener(UiButton(menu.optionsPanel.transform, "reset", new Vector2(620f, 86f), out _, 34).onClick, menu.ResetProgress);
        UnityEventTools.AddPersistentListener(UiButton(menu.optionsPanel.transform, "back", new Vector2(420f, 100f), out _).onClick, menu.ShowMain);

        var credit = Label(canvas, "Credito", new Vector2(1f, 1f), new Vector2(-30f, -24f), new Vector2(1000f, 40f), 24, TextAnchor.UpperRight);
        credit.color = new Color(0.85f, 0.87f, 1f, 0.85f);
        Localize(credit, "credit");

        string path = $"Assets/Scenes/{MenuSceneName}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    /// <summary>"FASE" en dos copias: la de noche, enmascarada a partir de la línea.</summary>
    static void BuildLogo(Transform parent)
    {
        var logo = new GameObject("Logo", typeof(RectTransform));
        logo.transform.SetParent(parent, false);
        ((RectTransform)logo.transform).sizeDelta = new Vector2(760f, 210f);
        var element = logo.AddComponent<LayoutElement>();
        element.preferredWidth = 760f;
        element.preferredHeight = 210f;
        var day = Label(logo.transform, "Dia", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 250f), 230, TextAnchor.MiddleLeft);
        day.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        day.text = "FASE";
        day.color = DayText;
        var maskObject = new GameObject("MascaraNoche", typeof(RectTransform));
        maskObject.transform.SetParent(logo.transform, false);
        var maskRect = (RectTransform)maskObject.transform;
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = maskRect.offsetMax = Vector2.zero;
        var mask = maskObject.AddComponent<RectMask2D>();
        var night = Label(maskObject.transform, "Noche", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 250f), 230, TextAnchor.MiddleLeft);
        night.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        night.text = "FASE";
        night.color = NightText;
        logo.AddComponent<LogoSplit>().nightMask = mask;
    }

    /// <summary>El viaje: once nodos en un camino que ondula por los tres capítulos.</summary>
    static void BuildJourney(Transform canvas, MainMenu menu, List<Level> levels)
    {
        var panel = new GameObject("ElViaje", typeof(RectTransform));
        panel.transform.SetParent(canvas, false);
        var rect = (RectTransform)panel.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.08f, 0.45f);
        menu.journeyPanel = panel;
        menu.scenes = levels.Select(l => Path.GetFileNameWithoutExtension(l.file)).ToArray();
        menu.names = levels.Select(l => l.name).ToArray();
        menu.namesEn = levels.Select(l => l.nameEn).ToArray();
        menu.fragmentsTotal = levels.Select(l => l.Fragments).ToArray();

        var title = Label(panel.transform, "Titulo", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 110f), 86, TextAnchor.UpperCenter);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        Localize(title, "journey");

        // Nodos solo para la historia; los secretos (al final de la lista) los pinta MenuExtras.
        int count = levels.Count(l => !l.secret);
        var positions = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float x = Mathf.Lerp(-780f, 780f, count == 1 ? 0.5f : i / (float)(count - 1));
            positions[i] = new Vector2(x, 40f + Mathf.Sin(i * 1.05f) * 120f);
        }
        // Camino punteado entre nodos.
        var dot = RoundedSprite();
        for (int i = 0; i + 1 < count; i++)
            for (int k = 1; k < 7; k++)
            {
                var p = Vector2.Lerp(positions[i], positions[i + 1], k / 7f);
                var d = new GameObject("Punto", typeof(RectTransform), typeof(Image));
                d.transform.SetParent(panel.transform, false);
                var dr = (RectTransform)d.transform;
                dr.anchoredPosition = p;
                dr.sizeDelta = new Vector2(12f, 12f);
                var image = d.GetComponent<Image>();
                image.sprite = dot;
                image.color = new Color(1f, 0.95f, 0.8f, 0.6f);
                image.raycastTarget = false;
            }
        // Nombres de los capítulos encima de su grupo de nodos.
        for (int chapter = 1; chapter <= Biomes.Length; chapter++)
        {
            var indexes = Enumerable.Range(0, count).Where(i => levels[i].chapter == chapter).ToList();
            if (indexes.Count == 0) continue;
            float cx = indexes.Average(i => positions[i].x);
            var biome = Biomes[chapter - 1];
            var label = Label(panel.transform, "Capitulo", new Vector2(0.5f, 0.5f), new Vector2(cx, 270f), new Vector2(560f, 60f), 34, TextAnchor.MiddleCenter);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.text = $"{new[] { "I", "II", "III" }[chapter - 1]} · {biome.name}";
            label.color = new Color(1f, 0.85f, 0.55f);
            label.gameObject.AddComponent<ChapterLabel>().Set(biome.name, biome.nameEn, new[] { "I", "II", "III" }[chapter - 1]);
        }
        menu.nodes = new Button[count];
        menu.nodeImages = new Image[count];
        for (int i = 0; i < count; i++)
        {
            var node = new GameObject($"Nivel{i + 1}", typeof(RectTransform));
            node.transform.SetParent(panel.transform, false);
            var nr = (RectTransform)node.transform;
            nr.anchoredPosition = positions[i];
            float nodeSize = count > 12 ? 88f : 104f;
            nr.sizeDelta = new Vector2(nodeSize, nodeSize);
            var image = node.AddComponent<Image>();
            image.sprite = RoundedSprite();
            var button = node.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            UnityEventTools.AddIntPersistentListener(button.onClick, menu.SelectLevel, i + 1);
            var number = Label(node.transform, "Numero", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(nodeSize, nodeSize), count > 12 ? 44 : 52, TextAnchor.MiddleCenter);
            number.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            number.text = (i + 1).ToString();
            number.color = new Color(0.12f, 0.08f, 0.3f);
            number.GetComponent<Shadow>().enabled = false;
            menu.nodes[i] = button;
            menu.nodeImages[i] = image;
        }
        // Marcador sobre el nivel elegido: un triángulo que apunta hacia abajo.
        var marker = new GameObject("Marcador", typeof(RectTransform));
        marker.transform.SetParent(panel.transform, false);
        ((RectTransform)marker.transform).sizeDelta = new Vector2(54f, 54f);
        marker.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
        var triangle = marker.AddComponent<UiTriangle>();
        triangle.color = Color.white;
        marker.AddComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
        triangle.raycastTarget = false;
        menu.marker = (RectTransform)marker.transform;

        // Ficha del nivel elegido.
        var info = Pill(panel.transform, "Ficha", new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1240f, 190f));
        info.color = PanelColor;
        menu.infoTitle = Label(info.transform, "Nombre", new Vector2(0f, 1f), new Vector2(60f, -30f), new Vector2(800f, 70f), 54, TextAnchor.UpperLeft);
        menu.infoDetail = Label(info.transform, "Detalle", new Vector2(0f, 0f), new Vector2(60f, 40f), new Vector2(820f, 50f), 34, TextAnchor.LowerLeft);
        menu.infoDetail.color = new Color(0.8f, 0.84f, 1f);
        menu.infoPlay = UiButton(info.transform, "play", new Vector2(300f, 110f), out _, 50);
        var playRect = (RectTransform)menu.infoPlay.transform;
        playRect.anchorMin = playRect.anchorMax = playRect.pivot = new Vector2(1f, 0.5f);
        playRect.anchoredPosition = new Vector2(-40f, 0f);
        UnityEventTools.AddPersistentListener(menu.infoPlay.onClick, menu.PlaySelected);

        var back = UiButton(panel.transform, "back", new Vector2(300f, 96f), out _);
        var backRect = (RectTransform)back.transform;
        backRect.anchorMin = backRect.anchorMax = backRect.pivot = new Vector2(0f, 0f);
        backRect.anchoredPosition = new Vector2(40f, 40f);
        UnityEventTools.AddPersistentListener(back.onClick, menu.ShowMain);
    }

    // ───────────────────────── Historia ─────────────────────────

    static readonly string[] IntroEs =
    {
        "Hubo un tiempo en que el día y la noche compartían el mismo cielo.",
        "Pero una noche, la luna se rompió en mil pedazos…",
        "…y una grieta de luz partió el mundo en dos.",
        "Lumi decidió reunir los fragmentos. Solo tenía un farol… y la línea.",
    };

    static readonly string[] IntroEn =
    {
        "Once, day and night shared the same sky.",
        "But one night, the moon shattered into a thousand pieces…",
        "…and a crack of light split the world in two.",
        "Lumi set out to gather the pieces, with only a lantern… and the line.",
    };

    static string BuildIntro()
    {
        var slides = Enumerable.Range(1, 4).Select(i => IntroSprite($"{IntroArt}vineta-{i}.png")).ToArray();
        if (slides.Any(s => s == null)) { Debug.LogWarning("Faltan las viñetas de la intro en " + IntroArt); return null; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("Camara") { tag = "MainCamera" }.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.gameObject.AddComponent<AudioListener>();
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        var music = new GameObject("Musica").AddComponent<AudioSource>();
        music.clip = MusicClip("noche");
        music.loop = true;
        music.volume = 0.6f;
        music.playOnAwake = true;

        var canvas = NewCanvas("Historia", 0).transform;
        var pictureObject = new GameObject("Vineta", typeof(RectTransform));
        pictureObject.transform.SetParent(canvas, false);
        var pr = (RectTransform)pictureObject.transform;
        pr.anchorMin = Vector2.zero;
        pr.anchorMax = Vector2.one;
        pr.offsetMin = pr.offsetMax = Vector2.zero;
        var picture = pictureObject.AddComponent<Image>();
        picture.preserveAspect = false;
        var group = pictureObject.AddComponent<CanvasGroup>();
        // Franjas negras de cine.
        foreach (var (anchor, y) in new[] { (new Vector2(0.5f, 1f), 0f), (new Vector2(0.5f, 0f), 0f) })
        {
            var bar = new GameObject("Franja", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(canvas, false);
            var br = (RectTransform)bar.transform;
            br.anchorMin = new Vector2(0f, anchor.y);
            br.anchorMax = new Vector2(1f, anchor.y);
            br.pivot = anchor;
            br.sizeDelta = new Vector2(0f, anchor.y > 0.5f ? 90f : 170f);
            bar.GetComponent<Image>().color = Color.black;
        }
        var subtitle = Label(canvas, "Subtitulo", new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(1700f, 110f), 48, TextAnchor.MiddleCenter);
        subtitle.rectTransform.pivot = new Vector2(0.5f, 0f);
        var skip = UiButton(canvas, "skip", new Vector2(240f, 70f), out _, 32);
        var sr = (RectTransform)skip.transform;
        sr.anchorMin = sr.anchorMax = sr.pivot = new Vector2(1f, 1f);
        sr.anchoredPosition = new Vector2(-30f, -10f);

        var intro = canvas.gameObject.AddComponent<Intro>();
        intro.picture = picture;
        intro.pictureGroup = group;
        intro.subtitle = subtitle;
        intro.slides = slides;
        intro.linesEs = IntroEs;
        intro.voiceEs = Enumerable.Range(0, 4).Select(i => Clip($"Assets/Audio/Voz/intro_es_{i}.wav")).ToArray();
        intro.voiceEn = Enumerable.Range(0, 4).Select(i => Clip($"Assets/Audio/Voz/intro_en_{i}.wav")).ToArray();
        intro.linesEn = IntroEn;
        intro.nextScene = Path.GetFileNameWithoutExtension(Directory.GetFiles(LevelsFolder, "*.txt").OrderBy(f => f, System.StringComparer.Ordinal).First());
        UnityEventTools.AddPersistentListener(skip.onClick, intro.Skip);

        string path = $"Assets/Scenes/{IntroScene}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    static Sprite IntroSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;
        if (importer.textureType != TextureImporterType.Sprite || importer.maxTextureSize < 2048)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ───────────────────────── Final ─────────────────────────

    static string BuildFinal(List<Level> levels)
    {
        materialCache.Clear();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var (_, _, line) = BuildWorld(Backdrop("Final"), "");
        line.minX = -5f;
        line.maxX = 45f;
        line.x = -2f;
        StaticCamera(new Vector3(20f, 5f, -19f));
        BuildAudio(Biomes[0]);

        // Lumi mira el amanecer en mitad del paisaje.
        // Mirando a la cámara (giro 0 en el modelo de Quaternius).
        BuildLumi(new Vector3(20f, 3f, 0f), null);

        var canvas = NewCanvas("Final", 0).transform;
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        var column = Column(canvas, "Textos", new Vector2(0f, 60f), 20f, PanelColor);
        var group = column.AddComponent<CanvasGroup>();
        Heading(column.transform, "dawn");
        var message = Label(column.transform, "Mensaje", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 70f), 44, TextAnchor.MiddleCenter);
        Localize(message, "onewhole");
        message.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;
        var moon = Label(column.transform, "Luna", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 60f), 46, TextAnchor.MiddleCenter);
        moon.color = new Color(1f, 0.9f, 0.55f);
        moon.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;

        var ending = canvas.gameObject.AddComponent<Ending>();
        ending.line = line;
        ending.texts = group;
        ending.moonLabel = moon;
        ending.levels = levels.Count(l => !l.secret);
        ending.fragmentsTotal = levels.Where(l => !l.secret).Sum(l => l.Fragments);
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "tomenu", new Vector2(560f, 110f), out _).onClick, ending.ToMenu);

        string path = $"Assets/Scenes/{FinalScene}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    // ───────────────────────── Pausa ─────────────────────────

    static void BuildPause()
    {
        var canvas = NewCanvas("Pausa", 5).transform;
        var pause = canvas.gameObject.AddComponent<PauseMenu>();

        var button = UiButton(canvas, "II", new Vector2(110f, 110f), out var label, 48, localize: false);
        var rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-30f, -24f);
        UnityEventTools.AddPersistentListener(button.onClick, pause.Toggle);

        var dim = new GameObject("Fondo", typeof(RectTransform));
        dim.transform.SetParent(canvas, false);
        var dimRect = (RectTransform)dim.transform;
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0.05f, 0.7f);
        var column = Column(dim.transform, "Botones", Vector2.zero, 26f, PanelColor);
        Heading(column.transform, "pause");
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "resume", new Vector2(520f, 110f), out _).onClick, pause.Resume);
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "restart", new Vector2(520f, 110f), out _).onClick, pause.Restart);
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "menu", new Vector2(520f, 110f), out _).onClick, pause.ToMenu);
        pause.panel = dim;
        canvas.gameObject.AddComponent<PauseMap>();
    }

    // ───────────────────────── Piezas de interfaz ─────────────────────────

    static void StaticCamera(Vector3 position)
    {
        var cameraObject = new GameObject("Camara") { tag = "MainCamera" };
        cameraObject.transform.position = position;
        var cam = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cam.fieldOfView = 35f;
        cam.farClipPlane = 200f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        EnablePostProcessing(cam, Biomes[0]);
    }

    static void Localize(Text text, string key)
    {
        text.gameObject.AddComponent<LocText>().key = key;
        text.text = key;
    }

    /// <summary>Columna centrada que coloca a sus hijos uno debajo de otro.</summary>
    static GameObject Column(Transform parent, string name, Vector2 position, float spacing, Color? background = null)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        if (background.HasValue)
        {
            var image = go.AddComponent<Image>();
            image.sprite = PanelSprite();
            image.type = Image.Type.Sliced;
            image.color = background.Value;
        }
        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = new RectOffset(70, 70, 50, 50);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }

    static void Heading(Transform parent, string key)
    {
        var heading = Label(parent, "Encabezado", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 110f), 84, TextAnchor.MiddleCenter);
        Localize(heading, key);
        var element = heading.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 120f;
        element.preferredWidth = 900f;
    }

    /// <summary>Botón redondeado. `key` es la clave del texto (Loc) salvo con localize = false.</summary>
    static Button UiButton(Transform parent, string key, Vector2 size, out Text label, int fontSize = 48, bool localize = true)
    {
        var go = new GameObject(key, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = size;
        var element = go.AddComponent<LayoutElement>();
        element.preferredWidth = size.x;
        element.preferredHeight = size.y;
        var image = go.AddComponent<Image>();
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = ButtonColor;
        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(1.25f, 1.1f, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.9f, 1.2f);
        colors.selectedColor = new Color(1.15f, 1.05f, 0.9f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.colorMultiplier = 1.4f;
        button.colors = colors;
        label = Label(go.transform, "Texto", new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize, TextAnchor.MiddleCenter);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        if (localize) Localize(label, key); else label.text = key;
        return button;
    }

    static Slider LabeledSlider(Transform parent, string key)
    {
        var row = new GameObject(key, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var element = row.AddComponent<LayoutElement>();
        element.preferredWidth = 900f;
        element.preferredHeight = 90f;
        var label = Label(row.transform, "Texto", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(300f, 80f), 44, TextAnchor.MiddleLeft);
        label.rectTransform.pivot = new Vector2(0f, 0.5f);
        Localize(label, key);

        var resources = new DefaultControls.Resources
        {
            standard = RoundedSprite(),
            background = RoundedSprite(),
            knob = RoundedSprite(),
        };
        var sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.transform.SetParent(row.transform, false);
        var rect = (RectTransform)sliderObject.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(560f, 36f);
        var handle = sliderObject.transform.Find("Handle Slide Area/Handle") as RectTransform;
        if (handle != null) handle.sizeDelta = new Vector2(56f, 20f);
        foreach (var image in sliderObject.GetComponentsInChildren<Image>())
            image.color = image.name == "Fill" ? new Color(1f, 0.6f, 0.3f) : image.name == "Handle" ? Color.white : new Color(0.2f, 0.18f, 0.4f);
        var slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
        return slider;
    }
}
