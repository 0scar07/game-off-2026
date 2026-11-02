using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fase;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Menú principal, pausa y final (parte del menú Fase → Generar niveles).</summary>
public static partial class LevelBuilder
{
    const string MenuSceneName = PauseMenu.MenuScene;
    const string FinalScene = "Final";

    static readonly Color PanelColor = new Color(0.07f, 0.06f, 0.16f, 0.95f);
    static readonly Color ButtonColor = new Color(0.16f, 0.14f, 0.32f, 0.92f);

    /// <summary>Paisaje del menú y del final: suelo con piezas de día y de noche para lucir el corte.</summary>
    static Level Backdrop(string name) => new Level
    {
        file = name,
        name = name,
        line = 20f,
        rows = new[]
        {
            "", "", "", "", "", "",
            "                          nn",
            "            DD       nn",
            "                                 ",
            "########dddddd##########nnnnnn##########",
            "########################################",
            "########################################",
        },
    };

    [MenuItem("Fase/Abrir menú del juego")]
    static void OpenMenu() => OpenScene($"Assets/Scenes/{MenuSceneName}.unity");

    [MenuItem("Fase/Abrir final")]
    static void OpenFinal() => OpenScene($"Assets/Scenes/{FinalScene}.unity");

    static void OpenScene(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(path);
    }

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
        BuildAudio();

        var canvas = NewCanvas("Menu", 0).transform;
        var menu = canvas.gameObject.AddComponent<MainMenu>();
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

        // Principal.
        menu.mainPanel = Column(canvas, "Principal", new Vector2(0f, -60f), 26f);
        var title = Label(menu.mainPanel.transform, "Titulo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 230f), 200, TextAnchor.MiddleCenter);
        title.text = "FASE";
        title.gameObject.AddComponent<LayoutElement>().preferredHeight = 230f;
        var subtitle = Label(menu.mainPanel.transform, "Subtitulo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 60f), 40, TextAnchor.MiddleCenter);
        subtitle.text = "Mueve la línea. Cambia el mundo.";
        subtitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 90f;
        var play = UiButton(menu.mainPanel.transform, "Jugar", new Vector2(560f, 120f), out menu.playLabel);
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
        UnityEventTools.AddPersistentListener(UiButton(menu.mainPanel.transform, "Niveles", new Vector2(560f, 110f), out _).onClick, menu.ShowLevels);
        UnityEventTools.AddPersistentListener(UiButton(menu.mainPanel.transform, "Opciones", new Vector2(560f, 110f), out _).onClick, menu.ShowOptions);

        // Niveles.
        menu.levelsPanel = Column(canvas, "Niveles", Vector2.zero, 30f, PanelColor);
        Heading(menu.levelsPanel.transform, "Niveles");
        var grid = new GameObject("Rejilla", typeof(RectTransform)).transform;
        grid.SetParent(menu.levelsPanel.transform, false);
        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(400f, 140f);
        layout.spacing = new Vector2(22f, 22f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 4;
        layout.childAlignment = TextAnchor.MiddleCenter;
        grid.gameObject.AddComponent<LayoutElement>().preferredHeight = Mathf.Ceil(levels.Count / 4f) * 162f;
        ((RectTransform)grid).sizeDelta = new Vector2(4 * 422f, 0f);
        menu.scenes = levels.Select(l => Path.GetFileNameWithoutExtension(l.file)).ToArray();
        menu.names = levels.Select(l => l.name).ToArray();
        menu.fragmentsTotal = levels.Select(l => l.rows.Sum(r => r.Count(c => c == '*'))).ToArray();
        menu.levelButtons = new Button[levels.Count];
        menu.levelLabels = new Text[levels.Count];
        for (int i = 0; i < levels.Count; i++)
        {
            menu.levelButtons[i] = UiButton(grid, $"{i + 1}. {levels[i].name}", new Vector2(400f, 140f), out menu.levelLabels[i], 34);
            UnityEventTools.AddIntPersistentListener(menu.levelButtons[i].onClick, menu.LoadLevel, i + 1);
        }
        UnityEventTools.AddPersistentListener(UiButton(menu.levelsPanel.transform, "Volver", new Vector2(420f, 100f), out _).onClick, menu.ShowMain);

        // Opciones.
        menu.optionsPanel = Column(canvas, "Opciones", Vector2.zero, 28f, PanelColor);
        Heading(menu.optionsPanel.transform, "Opciones");
        menu.music = LabeledSlider(menu.optionsPanel.transform, "Música");
        UnityEventTools.AddPersistentListener(menu.music.onValueChanged, menu.OnMusicChanged);
        menu.sfx = LabeledSlider(menu.optionsPanel.transform, "Efectos");
        UnityEventTools.AddPersistentListener(menu.sfx.onValueChanged, menu.OnSfxChanged);
        UnityEventTools.AddPersistentListener(UiButton(menu.optionsPanel.transform, "Borrar progreso", new Vector2(520f, 90f), out _, 34).onClick, menu.ResetProgress);
        UnityEventTools.AddPersistentListener(UiButton(menu.optionsPanel.transform, "Volver", new Vector2(420f, 100f), out _).onClick, menu.ShowMain);

        var credit = Label(canvas, "Credito", new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1800f, 50f), 28, TextAnchor.LowerCenter);
        credit.text = "GitHub Game Off 2026 · tema SHIFT · por 0scar07";

        string path = $"Assets/Scenes/{MenuSceneName}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
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
        BuildAudio();

        // El personaje mira el amanecer en mitad del paisaje.
        var hero = Model(HeroModel, new Vector3(20f, 3f, 0f), Vector3.one * 2f, Both, null);
        hero.name = "Jugador";
        var animator = hero.GetComponentInChildren<Animator>();
        if (animator == null) animator = hero.AddComponent<Animator>();
        animator.runtimeAnimatorController = HeroAnimator();
        hero.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        var canvas = NewCanvas("Final", 0).transform;
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        var column = Column(canvas, "Textos", new Vector2(0f, 40f), 20f, PanelColor);
        var group = column.AddComponent<CanvasGroup>();
        Heading(column.transform, "Amaneció");
        var message = Label(column.transform, "Mensaje", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 70f), 44, TextAnchor.MiddleCenter);
        message.text = "Las dos mitades vuelven a ser una.";
        message.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;
        var moon = Label(column.transform, "Luna", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 60f), 46, TextAnchor.MiddleCenter);
        moon.color = new Color(1f, 0.9f, 0.55f);
        moon.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;
        var credits = Label(column.transform, "Creditos", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 200f), 32, TextAnchor.MiddleCenter);
        credits.text = "Un juego de 0scar07 para el GitHub Game Off 2026 (tema: SHIFT)\n" +
                       "Modelos y efectos de sonido: Kenney (kenney.nl, CC0)\n" +
                       "Música original · Hecho con Unity";
        credits.gameObject.AddComponent<LayoutElement>().preferredHeight = 200f;

        var ending = canvas.gameObject.AddComponent<Ending>();
        ending.line = line;
        ending.texts = group;
        ending.moonLabel = moon;
        ending.levels = levels.Count;
        ending.fragmentsTotal = levels.Sum(l => l.rows.Sum(r => r.Count(c => c == '*')));
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "Volver al menú", new Vector2(520f, 110f), out _).onClick, ending.ToMenu);

        string path = $"Assets/Scenes/{FinalScene}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    // ───────────────────────── Pausa ─────────────────────────

    static void BuildPause()
    {
        var canvas = NewCanvas("Pausa", 5).transform;
        var pause = canvas.gameObject.AddComponent<PauseMenu>();

        var button = UiButton(canvas, "II", new Vector2(110f, 110f), out var label, 48);
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
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0.05f, 0.6f);
        var column = Column(dim.transform, "Botones", Vector2.zero, 26f, PanelColor);
        Heading(column.transform, "Pausa");
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "Continuar", new Vector2(520f, 110f), out _).onClick, pause.Resume);
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "Reiniciar nivel", new Vector2(520f, 110f), out _).onClick, pause.Restart);
        UnityEventTools.AddPersistentListener(UiButton(column.transform, "Menú", new Vector2(520f, 110f), out _).onClick, pause.ToMenu);
        pause.panel = dim;
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
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = background.Value;
        }
        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = new RectOffset(60, 60, 50, 50);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }

    static void Heading(Transform parent, string text)
    {
        var heading = Label(parent, "Encabezado", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 110f), 80, TextAnchor.MiddleCenter);
        heading.text = text;
        var element = heading.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 120f;
        element.preferredWidth = 900f;
    }

    static Button UiButton(Transform parent, string text, Vector2 size, out Text label, int fontSize = 48)
    {
        var go = new GameObject(text, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = size;
        var element = go.AddComponent<LayoutElement>();
        element.preferredWidth = size.x;
        element.preferredHeight = size.y;
        var image = go.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = ButtonColor;
        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.85f, 0.6f);
        colors.pressedColor = new Color(0.7f, 0.8f, 1f);
        colors.selectedColor = new Color(1f, 0.9f, 0.7f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;
        label = Label(go.transform, "Texto", new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize, TextAnchor.MiddleCenter);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        label.text = text;
        return button;
    }

    static Slider LabeledSlider(Transform parent, string text)
    {
        var row = new GameObject(text, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var element = row.AddComponent<LayoutElement>();
        element.preferredWidth = 900f;
        element.preferredHeight = 90f;
        var label = Label(row.transform, "Texto", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(300f, 80f), 44, TextAnchor.MiddleLeft);
        label.rectTransform.pivot = new Vector2(0f, 0.5f);
        label.text = text;

        var resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
        };
        var sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.transform.SetParent(row.transform, false);
        var rect = (RectTransform)sliderObject.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(560f, 44f);
        var handle = sliderObject.transform.Find("Handle Slide Area/Handle") as RectTransform;
        if (handle != null) handle.sizeDelta = new Vector2(56f, 0f);
        var slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
        return slider;
    }
}
