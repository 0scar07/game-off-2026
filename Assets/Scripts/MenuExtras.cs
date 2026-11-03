using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Partes del menú que se montan al arrancar: la fila Lumi · Logros · Desafío diario, la
    /// pantalla de logros, el armario de Lumi, "Accesibilidad y controles", el modo espejo y los
    /// niveles secretos del mapa de viaje.
    /// </summary>
    [RequireComponent(typeof(MainMenu))]
    public class MenuExtras : MonoBehaviour
    {
        MainMenu menu;
        GameObject achievementsPanel, wardrobePanel, accessPanel;
        readonly List<System.Action> refreshers = new List<System.Action>();
        Settings.Action? rebinding;
        Text rebindLabel;

        void Start()
        {
            menu = GetComponent<MainMenu>();
            BuildMainRow();
            achievementsPanel = BuildAchievements();
            wardrobePanel = BuildWardrobe();
            accessPanel = BuildAccess();
            BuildJourneyExtras();
            foreach (var panel in new[] { achievementsPanel, wardrobePanel, accessPanel }) menu.AddPanel(panel);
            menu.Shown += _ => Refresh();
            Loc.Changed += Refresh;
        }

        void OnDestroy() => Loc.Changed -= Refresh;

        void Refresh()
        {
            foreach (var refresh in refreshers) refresh();
        }

        // ───────── Fila del menú principal ─────────

        void BuildMainRow()
        {
            var row = new GameObject("FilaExtra", typeof(RectTransform));
            row.transform.SetParent(menu.mainPanel.transform, false);
            // Justo debajo de "El viaje", antes de Opciones.
            row.transform.SetSiblingIndex(Mathf.Max(0, menu.mainPanel.transform.childCount - 2));
            var element = row.AddComponent<LayoutElement>();
            element.preferredWidth = 560f;
            element.preferredHeight = 96f;
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddRowButton(row.transform, "wardrobe", () => menu.Show(wardrobePanel));
            AddRowButton(row.transform, "achievements", () => menu.Show(achievementsPanel));
            AddRowButton(row.transform, "daily", () => SceneFader.Load(DailyChallenge.SceneName));
        }

        void AddRowButton(Transform parent, string key, UnityEngine.Events.UnityAction action)
        {
            var button = UiKit.Button(parent, key, Loc.Get(key), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(176f, 96f), action, 30);
            var label = button.GetComponentInChildren<Text>();
            refreshers.Add(() => label.text = Loc.Get(key));
        }

        // ───────── Panel base ─────────

        GameObject Panel(string name, string titleKey, out RectTransform content)
        {
            var panel = UiKit.Fill(transform, name).gameObject;
            var background = panel.AddComponent<Image>();
            background.color = new Color(0.02f, 0.02f, 0.08f, 0.55f);
            var box = UiKit.Box(panel.transform, "Caja", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 940f), UiKit.Panel);
            var title = UiKit.Text(box.transform, "Titulo", Loc.Get(titleKey), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1200f, 100f), 76);
            refreshers.Add(() => title.text = Loc.Get(titleKey));
            content = UiKit.Rect(box.transform, "Contenido", new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(1400f, 680f));
            var back = UiKit.Button(box.transform, "Volver", Loc.Get("back"), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(380f, 90f), menu.ShowMain, 42);
            var backLabel = back.GetComponentInChildren<Text>();
            refreshers.Add(() => backLabel.text = Loc.Get("back"));
            return panel;
        }

        // ───────── Logros ─────────

        GameObject BuildAchievements()
        {
            var panel = Panel("Logros", "achievements", out var content);
            var count = UiKit.Text(content, "Cuenta", "", new Vector2(0.5f, 1f), new Vector2(0f, 20f), new Vector2(600f, 50f), 34, TextAnchor.MiddleCenter, UiKit.Gold);
            var all = Achievements.All;
            for (int i = 0; i < all.Length; i++)
            {
                var info = all[i];
                float x = (i % 3 - 1) * 460f, y = 250f - (i / 3) * 98f;
                var card = UiKit.Box(content, info.id, new Vector2(0.5f, 0.5f), new Vector2(x, y - 30f), new Vector2(440f, 84f), Color.white);
                var star = UiKit.Box(card.transform, "Estrella", new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(60f, 60f), Color.white);
                var icon = UiKit.Text(card.transform, "Icono", "", new Vector2(0f, 0.5f), new Vector2(46f, 2f), new Vector2(60f, 60f), 34);
                var name = UiKit.Text(card.transform, "Nombre", "", new Vector2(0f, 0.5f), new Vector2(90f, 16f), new Vector2(340f, 40f), 28, TextAnchor.MiddleLeft);
                name.rectTransform.pivot = new Vector2(0f, 0.5f);
                var desc = UiKit.Text(card.transform, "Detalle", "", new Vector2(0f, 0.5f), new Vector2(90f, -18f), new Vector2(340f, 30f), 20, TextAnchor.MiddleLeft, UiKit.Soft);
                desc.rectTransform.pivot = new Vector2(0f, 0.5f);
                refreshers.Add(() =>
                {
                    bool has = Achievements.Has(info.id);
                    card.color = has ? new Color(1f, 1f, 1f, 0.12f) : new Color(1f, 1f, 1f, 0.04f);
                    star.color = has ? new Color(1f, 0.84f, 0.35f) : new Color(0.25f, 0.24f, 0.4f);
                    icon.text = has ? "★" : "?";
                    icon.color = has ? new Color(0.55f, 0.35f, 0.1f) : new Color(0.6f, 0.6f, 0.75f);
                    name.text = info.Name;
                    name.color = has ? Color.white : new Color(0.65f, 0.65f, 0.8f);
                    desc.text = info.Description;
                });
            }
            refreshers.Add(() => count.text = $"{Achievements.Count} / {all.Length}");
            return panel;
        }

        // ───────── Armario de Lumi ─────────

        GameObject BuildWardrobe()
        {
            var panel = Panel("Armario", "wardrobetitle", out var content);
            BuildItems(content, Wardrobe.Scarves, "scarf", 140f, true);
            BuildItems(content, Wardrobe.Lanterns, "lantern", -200f, false);
            return panel;
        }

        void BuildItems(RectTransform content, Wardrobe.Item[] items, string titleKey, float y, bool scarf)
        {
            var title = UiKit.Text(content, "Titulo" + titleKey, "", new Vector2(0.5f, 0.5f), new Vector2(0f, y + 110f), new Vector2(800f, 50f), 38, TextAnchor.MiddleCenter, UiKit.Gold);
            refreshers.Add(() => title.text = Loc.Get(titleKey));
            float step = 1320f / items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                float x = (i - (items.Length - 1) * 0.5f) * Mathf.Min(step, 330f);
                var holder = UiKit.Rect(content, item.id, new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(150f, 220f));
                var button = UiKit.Button(holder, "Muestra", "", new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(120f, 120f), () => { Wardrobe.Wear(item); Refresh(); });
                var swatch = UiKit.Box(button.transform, "Color", new Vector2(0.5f, 0.5f), Vector2.zero, scarf ? new Vector2(86f, 40f) : new Vector2(60f, 60f), item.color);
                var swatch2 = scarf ? UiKit.Box(button.transform, "Color2", new Vector2(0.5f, 0.5f), new Vector2(0f, -18f), new Vector2(86f, 20f), item.color2) : null;
                var ring = UiKit.Box(holder, "Marco", new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(136f, 136f), new Color(1f, 0.85f, 0.5f, 0.9f));
                ring.transform.SetSiblingIndex(0);
                var name = UiKit.Text(holder, "Nombre", "", new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(150f, 36f), 21);
                var how = UiKit.Text(holder, "Como", "", new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(150f, 40f), 15, TextAnchor.UpperCenter, UiKit.Soft);
                name.horizontalOverflow = how.horizontalOverflow = HorizontalWrapMode.Wrap;
                refreshers.Add(() =>
                {
                    bool open = item.Unlocked;
                    bool worn = (scarf ? Wardrobe.Scarf : Wardrobe.Lantern) == item;
                    button.interactable = open;
                    swatch.color = open ? new Color(Mathf.Min(item.color.r, 1f), Mathf.Min(item.color.g, 1f), Mathf.Min(item.color.b, 1f)) : new Color(0.25f, 0.24f, 0.4f);
                    if (swatch2 != null) swatch2.color = open ? item.color2 : new Color(0.25f, 0.24f, 0.4f);
                    ring.enabled = worn;
                    name.text = open ? item.Name : "???";
                    how.text = worn ? Loc.Get("equipped") : open ? "" : item.How;
                });
            }
        }

        // ───────── Accesibilidad y controles ─────────

        GameObject BuildAccess()
        {
            var panel = Panel("Accesibilidad", "controls", out var content);
            Toggle(content, "assist", new Vector2(-350f, 260f), () => Settings.Assist, v => Settings.Assist = v);
            Toggle(content, "contrast", new Vector2(-350f, 160f), () => Settings.HighContrast, v => Settings.HighContrast = v);
            Toggle(content, "vibration", new Vector2(-350f, 60f), () => Settings.Vibration, v => Settings.Vibration = v);
            Toggle(content, "mirror", new Vector2(-350f, -40f), () => MirrorMode.Enabled, v => MirrorMode.Enabled = v, () => MirrorMode.Unlocked);

            var actions = (Settings.Action[])System.Enum.GetValues(typeof(Settings.Action));
            string[] keys = { "left", "right", "jump", "lineleft", "lineright" };
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                string key = keys[i];
                float y = 260f - i * 100f;
                var label = UiKit.Text(content, "Accion" + key, "", new Vector2(0.5f, 0.5f), new Vector2(260f, y), new Vector2(260f, 60f), 32, TextAnchor.MiddleRight);
                var button = UiKit.Button(content, "Tecla" + key, "", new Vector2(0.5f, 0.5f), new Vector2(520f, y), new Vector2(220f, 80f), null, 32);
                var text = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => { rebinding = action; rebindLabel = text; text.text = Loc.Get("presskey"); });
                refreshers.Add(() => { label.text = Loc.Get(key); if (rebinding != action) text.text = Settings.KeyName(Settings.GetKey(action)); });
            }
            var reset = UiKit.Button(content, "Reiniciar", Loc.Get("resetkeys"), new Vector2(0.5f, 0.5f), new Vector2(400f, -250f), new Vector2(440f, 80f), () => { Settings.ResetKeys(); Refresh(); }, 30);
            var resetLabel = reset.GetComponentInChildren<Text>();
            refreshers.Add(() => resetLabel.text = Loc.Get("resetkeys"));

            // Botón para llegar aquí desde Opciones (encima de "Volver").
            var options = menu.optionsPanel.transform;
            var open = UiKit.Button(options, "Controles", Loc.Get("controls"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 96f), () => menu.Show(panel), 40);
            var element = open.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 620f;
            element.preferredHeight = 96f;
            open.transform.SetSiblingIndex(Mathf.Max(0, options.childCount - 2));
            var openLabel = open.GetComponentInChildren<Text>();
            refreshers.Add(() => openLabel.text = Loc.Get("controls"));
            return panel;
        }

        void Toggle(RectTransform parent, string key, Vector2 position, System.Func<bool> get, System.Action<bool> set, System.Func<bool> available = null)
        {
            var button = UiKit.Button(parent, key, "", new Vector2(0.5f, 0.5f), position, new Vector2(560f, 80f), null, 32);
            var text = button.GetComponentInChildren<Text>();
            button.onClick.AddListener(() => { set(!get()); Refresh(); });
            refreshers.Add(() =>
            {
                bool can = available == null || available();
                button.interactable = can;
                text.text = $"{Loc.Get(key)}:  {(can ? Loc.Get(get() ? "on" : "off") : "🔒")}";
            });
        }

        void Update()
        {
            if (rebinding == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            foreach (var control in kb.allKeys)
            {
                if (control == null || !control.wasPressedThisFrame) continue;
                if (control.keyCode != Key.Escape) Settings.SetKey(rebinding.Value, control.keyCode);
                rebinding = null;
                Refresh();
                break;
            }
        }

        // ───────── Mapa de viaje: niveles secretos ─────────

        void BuildJourneyExtras()
        {
            var catalog = LevelCatalog.Get;
            var journey = menu.journeyPanel.transform;
            for (int i = 0; i < catalog.levels.Length; i++)
            {
                var entry = catalog.levels[i];
                if (!entry.secret) continue;
                int level = i + 1;
                int chapter = entry.chapter;
                // Estrella sobre el último nivel de su capítulo.
                int anchorIndex = -1;
                for (int k = 0; k < menu.nodes.Length; k++) if (catalog.levels[k].chapter == chapter) anchorIndex = k;
                if (anchorIndex < 0) continue;
                var node = (RectTransform)menu.nodes[anchorIndex].transform;
                var button = UiKit.Button(journey, "Secreto" + level, "★", new Vector2(0.5f, 0.5f), node.anchoredPosition + new Vector2(70f, -110f), new Vector2(84f, 84f), () => menu.LoadLevel(level), 44);
                var image = button.GetComponent<Image>();
                image.color = new Color(0.35f, 0.2f, 0.6f, 0.95f);
                refreshers.Add(() => button.gameObject.SetActive(catalog.AllFragments(chapter) && catalog.ChapterDone(chapter)));
            }
        }
    }
}
