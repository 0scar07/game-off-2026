using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Partes del menú que se montan al arrancar: la barra de iconos de abajo, la pantalla de logros, el armario de Lumi, "Accesibilidad y controles", el modo espejo y los
    /// niveles secretos del mapa de viaje.
    /// </summary>
    [RequireComponent(typeof(MainMenu))]
    public partial class MenuExtras : MonoBehaviour
    {
        MainMenu menu;
        GameObject achievementsPanel, wardrobePanel, accessPanel, diaryPanel, shopPanel, triviaPanel;
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
            diaryPanel = BuildDiary();
            shopPanel = BuildShop();
            triviaPanel = BuildTrivia();
            BuildExtras();
            BuildJourneyExtras();
            BuildJourneyPreview();
            JourneyWalker.Create(menu);
            foreach (var panel in new[] { achievementsPanel, wardrobePanel, accessPanel, diaryPanel, shopPanel, triviaPanel }) menu.AddPanel(panel);
            foreach (var panel in extraPanels) menu.AddPanel(panel);

            BuildGreeting();
            BuildNameOption();
            // Lumi del fondo: saluda al volver y suelta alguna frase mientras se ve el menú principal.
            var walker = FindAnyObjectByType<MenuWalker>();
            if (walker != null)
            {
                var talk = walker.gameObject.AddComponent<MenuTalk>();
                menu.Shown += panel => talk.visible = panel == menu.mainPanel;
            }
            // Todos los botones del menú (también los que monta el generador) suenan y se inclinan.
            UiKit.EnsureEvents();
            UiHover.AddTo(transform);
            menu.Shown += _ => Refresh();
            Loc.Changed += Refresh;
        }

        void OnDestroy() => Loc.Changed -= Refresh;

        void Refresh()
        {
            foreach (var refresh in refreshers) refresh();
        }

        // ───────── Fila del menú principal ─────────

        /// <summary>Barra de abajo con iconos: todo lo que no es jugar o el viaje.</summary>
        void BuildMainRow()
        {
            var dock = UiKit.Box(menu.mainPanel.transform.parent, "Barra", new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(1760f, 150f), new Color(0.04f, 0.03f, 0.14f, 0.8f));
            var dockEdge = dock.gameObject.AddComponent<Outline>();
            dockEdge.effectColor = new Color(1f, 1f, 1f, 0.18f);
            dockEdge.effectDistance = new Vector2(2f, -2f);
            dock.rectTransform.pivot = new Vector2(0.5f, 0f);
            // La barra va con el menú principal: se esconde cuando se abre otro panel.
            menu.Shown += panel => dock.gameObject.SetActive(panel == menu.mainPanel);
            var items = new (string icon, string key, UnityEngine.Events.UnityAction action)[]
            {
                ("☺", "wardrobe", () => menu.Show(wardrobePanel)),
                ("★", "achievements", () => menu.Show(achievementsPanel)),
                ("✎", "diary", () => menu.Show(diaryPanel)),
                ("☀", "daily", () => SceneFader.Load(DailyChallenge.SceneName)),
                ("▦", "workshop", () => SceneFader.Load("Taller")),
                ("∞", "endless", () => SceneFader.Load("SinFin")),
                ("☾", "shop", () => menu.Show(shopPanel)),
                ("?", "trivia", () => menu.Show(triviaPanel)),
                ("✦", "extras", () => menu.Show(extrasPanel)),
                ("⚙", "options", menu.ShowOptions),
            };
            for (int i = 0; i < items.Length; i++)
            {
                var (icon, key, action) = items[i];
                float x = (i - (items.Length - 1) * 0.5f) * 172f;
                var button = UiKit.Button(dock.transform, key, "", new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(164f, 132f), action, 28);
                var image = button.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0.02f);
                var label = button.GetComponentInChildren<Text>();
                label.rectTransform.anchoredPosition = new Vector2(0f, -32f);
                label.fontStyle = FontStyle.Bold;
                // Textos largos ("Curiosidades") se encogen para caber.
                label.rectTransform.sizeDelta = new Vector2(158f, 40f);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 18;
                label.resizeTextMaxSize = 28;
                var glyph = UiKit.Text(button.transform, "Icono", icon, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(164f, 70f), 58, TextAnchor.MiddleCenter, UiKit.Gold);
                glyph.fontStyle = FontStyle.Normal;
                string k = key;
                refreshers.Add(() => label.text = k == "daily" ? Loc.Pick("Desafío", "Daily") : k == "trivia" ? Loc.Pick("Curiosidades", "Trivia") : Loc.Get(k));
                if (key == "daily") StreakBadge(button.transform);
            }
        }

        // ───────── Racha del desafío, saludo y nombre ─────────

        /// <summary>Circulito naranja con los días seguidos sobre el botón del desafío (desde 2 días).</summary>
        void StreakBadge(Transform button)
        {
            var holder = UiKit.Rect(button, "Racha", new Vector2(0.5f, 1f), new Vector2(0f, 6f), new Vector2(200f, 80f));
            var dot = UiKit.Box(holder, "Circulo", new Vector2(0.5f, 0f), Vector2.zero, new Vector2(40f, 40f), new Color(1f, 0.55f, 0.24f));
            dot.type = Image.Type.Simple;
            var number = UiKit.Text(dot.transform, "Dias", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f), 24);
            var caption = UiKit.Text(holder, "Texto", "", new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(220f, 30f), 18, TextAnchor.MiddleCenter, UiKit.Gold);
            refreshers.Add(() =>
            {
                int days = DailyStreak.Current;
                holder.gameObject.SetActive(days >= 2);
                number.text = days.ToString();
                caption.text = Loc.Pick($"{days} días seguidos", $"{days}-day streak", $"{days} dias seguidos", $"{days} jours d'affilée");
            });
        }

        /// <summary>"¡Hola, Oscar!" debajo del lema, si el jugador ha puesto su nombre.</summary>
        void BuildGreeting()
        {
            var column = menu.mainPanel.transform;
            var hello = UiKit.Text(column, "Saludo", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 46f), 36, TextAnchor.MiddleLeft, UiKit.Gold);
            var element = hello.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 760f;
            element.preferredHeight = 46f;
            var tagline = column.Find("Lema");
            if (tagline != null) hello.transform.SetSiblingIndex(tagline.GetSiblingIndex() + 1);
            refreshers.Add(() =>
            {
                hello.gameObject.SetActive(PlayerName.Has);
                hello.text = Loc.Pick($"¡Hola, {PlayerName.Value}!", $"Hi, {PlayerName.Value}!", $"Olá, {PlayerName.Value}!", $"Salut, {PlayerName.Value} !");
            });
        }

        /// <summary>En Opciones: "Tu nombre" abre un cuadro para escribirlo (o borrarlo).</summary>
        void BuildNameOption()
        {
            var options = menu.optionsPanel.transform;
            var overlay = UiKit.Fill(transform, "Nombre").gameObject;
            overlay.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.08f, 0.8f);
            var box = UiKit.Box(overlay.transform, "Caja", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 380f), UiKit.Panel);
            var title = UiKit.Text(box.transform, "Titulo", "", new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(800f, 70f), 50);
            var fieldBack = UiKit.Box(box.transform, "Campo", new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(620f, 90f), new Color(1f, 1f, 1f, 0.95f));
            fieldBack.raycastTarget = true;
            var field = fieldBack.gameObject.AddComponent<InputField>();
            var typed = UiKit.Text(fieldBack.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580f, 80f), 42, TextAnchor.MiddleCenter, new Color(0.2f, 0.14f, 0.36f));
            Destroy(typed.GetComponent<Shadow>());
            typed.supportRichText = false;
            var hint = UiKit.Text(fieldBack.transform, "Ejemplo", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580f, 80f), 36, TextAnchor.MiddleCenter, new Color(0.5f, 0.48f, 0.6f));
            Destroy(hint.GetComponent<Shadow>());
            field.textComponent = typed;
            field.placeholder = hint;
            field.characterLimit = PlayerName.MaxLength;
            var ok = UiKit.Button(box.transform, "Listo", "", new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(320f, 84f), null, 40);
            var okLabel = ok.GetComponentInChildren<Text>();
            System.Action close = () => { PlayerName.Value = field.text; overlay.SetActive(false); Refresh(); };
            ok.onClick.AddListener(() => close());
            field.onSubmit.AddListener(_ => close());
            overlay.SetActive(false);
            refreshers.Add(() =>
            {
                title.text = Loc.Pick("¿Cómo te llamas?", "What's your name?");
                hint.text = Loc.Pick("Escribe tu nombre", "Type your name");
                okLabel.text = Loc.Pick("Listo", "Done");
            });

            var open = UiKit.Button(options, "TuNombre", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 80f), () =>
            {
                field.text = PlayerName.Value;
                overlay.SetActive(true);
                overlay.transform.SetAsLastSibling();
                field.ActivateInputField();
            }, 40);
            var element = open.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 620f;
            element.preferredHeight = 80f;
            open.transform.SetSiblingIndex(Mathf.Max(0, options.childCount - 3));
            var openLabel = open.GetComponentInChildren<Text>();
            refreshers.Add(() => openLabel.text = PlayerName.Has ? Loc.Pick($"Tu nombre: {PlayerName.Value}", $"Your name: {PlayerName.Value}", $"Seu nome: {PlayerName.Value}", $"Ton prénom : {PlayerName.Value}") : Loc.Pick("Tu nombre", "Your name"));
        }

        // ───────── Tienda ─────────


        GameObject BuildShop()
        {
            var panel = Panel("Tienda", "shop", out var content);
            var balance = UiKit.Text(content, "Saldo", "", new Vector2(1f, 1f), new Vector2(-10f, 70f), new Vector2(560f, 50f), 34, TextAnchor.MiddleRight, UiKit.Gold);
            refreshers.Add(() => balance.text = Loc.Pick($"☾ {Shop.Balance} fragmentos para gastar", $"☾ {Shop.Balance} fragments to spend", $"☾ {Shop.Balance} fragmentos para gastar", $"☾ {Shop.Balance} fragments à dépenser"));
            for (int i = 0; i < Shop.Items.Length; i++)
            {
                var item = Shop.Items[i];
                float y = 190f - i * 130f;
                var card = UiKit.Box(content, item.id, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1000f, 110f), new Color(1f, 1f, 1f, 0.07f));
                UiKit.Box(card.transform, "Color", new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(70f, 70f), item.color).rectTransform.pivot = new Vector2(0.5f, 0.5f);
                var itemName = UiKit.Text(card.transform, "Nombre", item.Name, new Vector2(0f, 0.5f), new Vector2(130f, 14f), new Vector2(500f, 50f), 36, TextAnchor.MiddleLeft);
                itemName.rectTransform.pivot = new Vector2(0f, 0.5f);
                refreshers.Add(() => itemName.text = item.Name);
                UiKit.Text(card.transform, "Precio", $"☾ {item.price}", new Vector2(0f, 0.5f), new Vector2(130f, -26f), new Vector2(300f, 36f), 26, TextAnchor.MiddleLeft, UiKit.Gold).rectTransform.pivot = new Vector2(0f, 0.5f);
                var button = UiKit.Button(card.transform, "Accion", "", new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(280f, 80f), null, 30);
                ((RectTransform)button.transform).pivot = new Vector2(1f, 0.5f);
                var label = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => { if (Shop.Owned(item.id)) Shop.Toggle(item); else Shop.Buy(item); Refresh(); });
                refreshers.Add(() =>
                {
                    bool owned = Shop.Owned(item.id);
                    label.text = !owned ? Loc.Pick("Comprar", "Buy") : Shop.Worn(item.id) ? Loc.Pick("Quitar", "Remove") : Loc.Pick("Ponérselo", "Wear");
                    button.interactable = owned || Shop.Balance >= item.price;
                });
            }
            return panel;
        }

        // ───────── Panel base ─────────

        GameObject Panel(string name, string titleKey, out RectTransform content) => Panel(name, titleKey, out content, null);

        /// <summary>Panel base; `back` es adónde lleva "Volver" (por defecto, al menú principal).</summary>
        GameObject Panel(string name, string titleKey, out RectTransform content, UnityEngine.Events.UnityAction back)
        {
            var panel = UiKit.Fill(transform, name).gameObject;
            var background = panel.AddComponent<Image>();
            background.color = new Color(0.02f, 0.02f, 0.08f, 0.55f);
            var box = UiKit.Box(panel.transform, "Caja", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 940f), UiKit.Panel);
            var title = UiKit.Text(box.transform, "Titulo", Loc.Get(titleKey), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1200f, 100f), 76);
            refreshers.Add(() => title.text = Loc.Get(titleKey));
            content = UiKit.Rect(box.transform, "Contenido", new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(1400f, 680f));
            var backButton = UiKit.Button(box.transform, "Volver", Loc.Get("back"), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(380f, 90f), back ?? menu.ShowMain, 42);
            var backLabel = backButton.GetComponentInChildren<Text>();
            refreshers.Add(() => backLabel.text = Loc.Get("back"));
            return panel;
        }

        // ───────── Logros ─────────

        GameObject BuildAchievements()
        {
            var panel = Panel("Logros", "achievements", out var content);
            var count = UiKit.Text(content, "Cuenta", "", new Vector2(1f, 1f), new Vector2(-10f, 70f), new Vector2(300f, 50f), 34, TextAnchor.MiddleRight, UiKit.Gold);
            var all = Achievements.All;
            for (int i = 0; i < all.Length; i++)
            {
                var info = all[i];
                float x = (i % 4 - 1.5f) * 350f, y = 280f - (i / 4) * 78f;
                var card = UiKit.Box(content, info.id, new Vector2(0.5f, 0.5f), new Vector2(x, y - 30f), new Vector2(336f, 72f), Color.white);
                var star = UiKit.Box(card.transform, "Estrella", new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(56f, 56f), Color.white);
                star.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                var icon = UiKit.Text(card.transform, "Icono", "", new Vector2(0f, 0.5f), new Vector2(42f, 2f), new Vector2(56f, 56f), 32);
                icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                // Dibujo propio del logro; la estrella solo queda para los que aún no tienes.
                var art = UiKit.Box(card.transform, "Dibujo", new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(50f, 50f), Color.white, false);
                art.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                art.sprite = AchievementIcons.Get(info.id);
                art.preserveAspect = true;

                var name = UiKit.Text(card.transform, "Nombre", "", new Vector2(0f, 0.5f), new Vector2(80f, 14f), new Vector2(246f, 34f), 24, TextAnchor.MiddleLeft);
                name.rectTransform.pivot = new Vector2(0f, 0.5f);
                var desc = UiKit.Text(card.transform, "Detalle", "", new Vector2(0f, 0.5f), new Vector2(80f, -17f), new Vector2(246f, 28f), 16, TextAnchor.MiddleLeft, UiKit.Soft);
                desc.rectTransform.pivot = new Vector2(0f, 0.5f);
                foreach (var (t, min) in new[] { (name, 13), (desc, 10) })
                {
                    t.horizontalOverflow = HorizontalWrapMode.Wrap;
                    t.resizeTextForBestFit = true;
                    t.resizeTextMinSize = min;
                    t.resizeTextMaxSize = t.fontSize;
                }
                refreshers.Add(() =>
                {
                    bool has = Achievements.Has(info.id);
                    card.color = has ? new Color(1f, 1f, 1f, 0.12f) : new Color(1f, 1f, 1f, 0.04f);
                    star.color = has ? new Color(1f, 0.84f, 0.35f) : new Color(0.25f, 0.24f, 0.4f);
                    bool drawn = has && art.sprite != null;
                    art.enabled = drawn;
                    icon.text = drawn ? "" : has ? "★" : "?";
                    star.color = drawn ? new Color(1f, 1f, 1f, 0.12f) : star.color;

                    icon.color = has ? new Color(0.55f, 0.35f, 0.1f) : new Color(0.6f, 0.6f, 0.75f);
                    bool hidden = info.secret && !has;
                    name.text = hidden ? Loc.Pick("Logro secreto", "Secret achievement") : info.Name;
                    name.color = has ? Color.white : new Color(0.65f, 0.65f, 0.8f);
                    desc.text = hidden ? "???" : info.Description;
                });
            }
            refreshers.Add(() => count.text = $"{Achievements.Count} / {all.Length}");
            return panel;
        }

        // ───────── Armario de Lumi ─────────

        GameObject BuildWardrobe()
        {
            var panel = Panel("Armario", "wardrobetitle", out var content);
            Toggle(content, "character", new Vector2(0f, 262f), () => Wardrobe.IsNox, v => Wardrobe.IsNox = v, () => Wardrobe.NoxUnlocked,
                v => v ? "Nox" : "Lumi");
            BuildItems(content, Wardrobe.Scarves, "scarf", 70f, true);
            BuildItems(content, Wardrobe.Lanterns, "lantern", -235f, false);
            return panel;
        }

        void BuildItems(RectTransform content, Wardrobe.Item[] items, string titleKey, float y, bool scarf)
        {
            var title = UiKit.Text(content, "Titulo" + titleKey, "", new Vector2(0.5f, 0.5f), new Vector2(0f, y + 100f), new Vector2(800f, 50f), 36, TextAnchor.MiddleCenter, UiKit.Gold);
            refreshers.Add(() => title.text = Loc.Get(titleKey));
            float step = Mathf.Min(1380f / items.Length, 330f);
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                float x = (i - (items.Length - 1) * 0.5f) * step;
                var holder = UiKit.Rect(content, item.id, new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(Mathf.Min(step - 6f, 200f), 200f));
                var button = UiKit.Button(holder, "Muestra", "", new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(104f, 104f), () => { Wardrobe.Wear(item); Refresh(); });
                var swatch = UiKit.Box(button.transform, "Color", new Vector2(0.5f, 0.5f), Vector2.zero, scarf ? new Vector2(86f, 40f) : new Vector2(60f, 60f), item.color);
                var swatch2 = scarf ? UiKit.Box(button.transform, "Color2", new Vector2(0.5f, 0.5f), new Vector2(0f, -18f), new Vector2(86f, 20f), item.color2) : null;
                var ring = UiKit.Box(holder, "Marco", new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(120f, 120f), new Color(1f, 0.85f, 0.5f, 0.9f));
                ring.transform.SetSiblingIndex(0);
                float width = holder.sizeDelta.x;
                var name = UiKit.Text(holder, "Nombre", "", new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(width, 30f), 21);
                var how = UiKit.Text(holder, "Como", "", new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(width, 48f), 16, TextAnchor.UpperCenter, UiKit.Soft);
                name.horizontalOverflow = how.horizontalOverflow = HorizontalWrapMode.Wrap;
                foreach (var (t, min) in new[] { (name, 14), (how, 11) })
                {
                    t.resizeTextForBestFit = true;
                    t.resizeTextMinSize = min;
                    t.resizeTextMaxSize = t.fontSize;
                }
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

        // ───────── Diario de Lumi ─────────

        GameObject BuildDiary()
        {
            var panel = Panel("Diario", "diarytitle", out var content);
            var count = UiKit.Text(content, "Cuenta", "", new Vector2(1f, 1f), new Vector2(-10f, 70f), new Vector2(300f, 50f), 30, TextAnchor.MiddleRight, UiKit.Gold);
            var paper = UiKit.Box(content, "Papel", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(1000f, 480f), new Color(0.95f, 0.9f, 0.78f));
            var title = UiKit.Text(paper.transform, "Pagina", "", new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(900f, 50f), 34, TextAnchor.MiddleCenter, new Color(0.42f, 0.29f, 0.16f));
            var body = UiKit.Text(paper.transform, "Texto", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(860f, 300f), 36, TextAnchor.MiddleCenter, new Color(0.35f, 0.23f, 0.12f));
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.fontStyle = FontStyle.Normal;
            foreach (var shadowed in new[] { title, body }) Destroy(shadowed.GetComponent<Shadow>());
            int page = 0;
            System.Action show = () =>
            {
                int level = page + 1;
                count.text = $"{Diary.Found} / {Diary.Pages.Length}";
                title.text = Loc.Pick($"Página {level}", $"Page {level}", $"Página {level}", $"Page {level}");
                body.text = Diary.Has(level) ? Loc.Pick(Diary.Pages[page].es, Diary.Pages[page].en)
                    : Loc.Pick("Esta página está en el cofre secreto del nivel " + level + ".", "This page is in level " + level + "'s secret chest.");
                body.color = Diary.Has(level) ? new Color(0.35f, 0.23f, 0.12f) : new Color(0.55f, 0.45f, 0.35f, 0.7f);
            };
            UiKit.Button(content, "Anterior", "◀", new Vector2(0.5f, 0.5f), new Vector2(-580f, -20f), new Vector2(90f, 90f), () => { page = (page + Diary.Pages.Length - 1) % Diary.Pages.Length; show(); }, 40);
            UiKit.Button(content, "Siguiente", "▶", new Vector2(0.5f, 0.5f), new Vector2(580f, -20f), new Vector2(90f, 90f), () => { page = (page + 1) % Diary.Pages.Length; show(); }, 40);
            refreshers.Add(show);
            return panel;
        }

        // ───────── Accesibilidad y controles ─────────

        GameObject BuildAccess()
        {
            var panel = Panel("Accesibilidad", "controls", out var content);
            // Columna izquierda: nueve ajustes, de 74 en 74.
            float row = 292f;
            Toggle(content, "assist", new Vector2(-350f, row), () => Settings.Assist, v => Settings.Assist = v);
            Toggle(content, "contrast", new Vector2(-350f, row -= 74f), () => Settings.HighContrast, v => Settings.HighContrast = v);
            Toggle(content, "reduceeffects", new Vector2(-350f, row -= 74f), () => Settings.ReduceEffects, v => Settings.ReduceEffects = v);
            // Tamaño del texto: cada clic pasa al siguiente (se ve al momento en todo el menú).
            Toggle(content, "textsize", new Vector2(-350f, row -= 74f), () => Settings.TextSize == 2, _ => Settings.TextSize++, null,
                _ => Loc.Pick(new[] { "Normal", "Grande", "Muy grande" }[Settings.TextSize], new[] { "Normal", "Large", "Extra large" }[Settings.TextSize]));
            // Icono de sol o luna en las piezas, para no depender del color.
            Toggle(content, "pieceicons", new Vector2(-350f, row -= 74f), () => Settings.PieceIcons, v => Settings.PieceIcons = v);
            Toggle(content, "vibration", new Vector2(-350f, row -= 74f), () => Settings.Vibration, v => Settings.Vibration = v);
            Toggle(content, "mirror", new Vector2(-350f, row -= 74f), () => MirrorMode.Enabled, v => MirrorMode.Enabled = v, () => MirrorMode.Unlocked);
            Toggle(content, "coop", new Vector2(-350f, row -= 74f), () => Settings.Coop, v => Settings.Coop = v);
            // Velocidad de la línea: cada clic pasa a la siguiente.
            Toggle(content, "linespeed", new Vector2(-350f, row -= 74f), () => Settings.LineSpeed == 2, _ => Settings.LineSpeed++, null,
                _ => Loc.Pick(new[] { "Lenta", "Normal", "Rápida" }[Settings.LineSpeed], new[] { "Slow", "Normal", "Fast" }[Settings.LineSpeed]));
            // Vista previa del tamaño del texto.
            var sample = UiKit.Text(content, "Muestra", "", new Vector2(0.5f, 0.5f), new Vector2(400f, -330f), new Vector2(560f, 50f), 28, TextAnchor.MiddleCenter, UiKit.Soft);
            // Brillo: cada clic sube un 10 % (de 80 % a 130 % y vuelta a empezar).
            Toggle(content, "brightness", new Vector2(400f, -262f), () => Settings.BrightnessStep == 5, _ => Settings.BrightnessStep++, null,
                _ => $"{Settings.BrightnessPercent} %");
            refreshers.Add(() => sample.text = Loc.Pick("Así se verá el texto", "This is how text will look"));


            var actions = (Settings.Action[])System.Enum.GetValues(typeof(Settings.Action));
            string[] keys = { "left", "right", "jump", "lineleft", "lineright" };
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                string key = keys[i];
                float y = 280f - i * 88f;
                var label = UiKit.Text(content, "Accion" + key, "", new Vector2(0.5f, 0.5f), new Vector2(260f, y), new Vector2(260f, 60f), 32, TextAnchor.MiddleRight);
                var button = UiKit.Button(content, "Tecla" + key, "", new Vector2(0.5f, 0.5f), new Vector2(520f, y), new Vector2(220f, 80f), null, 32);
                var text = button.GetComponentInChildren<Text>();
                button.onClick.AddListener(() => { rebinding = action; rebindLabel = text; text.text = Loc.Get("presskey"); });
                refreshers.Add(() => { label.text = Loc.Get(key); if (rebinding != action) text.text = Settings.KeyName(Settings.GetKey(action)); });
            }
            var reset = UiKit.Button(content, "Reiniciar", Loc.Get("resetkeys"), new Vector2(0.5f, 0.5f), new Vector2(400f, -170f), new Vector2(440f, 76f), () => { Settings.ResetKeys(); Refresh(); }, 30);
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

        void Toggle(RectTransform parent, string key, Vector2 position, System.Func<bool> get, System.Action<bool> set, System.Func<bool> available = null,
            System.Func<bool, string> valueText = null)
        {
            var button = UiKit.Button(parent, key, "", new Vector2(0.5f, 0.5f), position, new Vector2(560f, 68f), null, 28);
            var text = button.GetComponentInChildren<Text>();
            button.onClick.AddListener(() => { set(!get()); Refresh(); });
            refreshers.Add(() =>
            {
                bool can = available == null || available();
                button.interactable = can;
                string value = valueText != null ? valueText(get()) : Loc.Get(get() ? "on" : "off");
                text.text = $"{Loc.Get(key)}:  {(can ? value : "—")}";
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

        // ───────── ¿Sabías qué? ─────────

        GameObject BuildTrivia()
        {
            var panel = Panel("Curiosidades", "trivia", out var content);
            var catalog = LevelCatalog.Get;
            int boss = 0;
            for (int i = 0; i < catalog.levels.Length; i++) if (catalog.levels[i].name.Contains("Sombra")) boss = i + 1;
            System.Func<int> moon = () => { int n = 0; for (int i = 0; i < catalog.levels.Length; i++) n += Progress.Fragments(i + 1); return n; };
            var facts = new (string titleEs, string titleEn, string es, string en, string howEs, string howEn, System.Func<bool> open)[]
            {
                ("La línea", "The line", "FASE nació del tema de la\nGitHub Game Off 2026: SHIFT.", "FASE was born from the GitHub\nGame Off 2026 theme: SHIFT.",
                    "Supera el nivel 1", "Beat level 1", () => Progress.Unlocked > 1),
                ("El farol", "The lantern", "Cerca de un fragmento de luna,\nel farol de Lumi brilla y late\nmás rápido.", "Near a moon fragment, Lumi's\nlantern glows and beats faster.",
                    "Reúne 5 fragmentos", "Collect 5 fragments", () => moon() >= 5),
                ("Las luciérnagas", "The fireflies", "Si te atascas, una luciérnaga\nviene a darte una pista.", "If you get stuck, a firefly\ncomes to give you a hint.",
                    "Llega al nivel 6", "Reach level 6", () => Progress.Unlocked >= 6),
                ("Dos canciones", "Two songs", "Cada capítulo tiene una canción\nde día y otra de noche: suenan a\nla vez y la línea elige cuál oyes.", "Each chapter has a day song and\na night song: both play at once\nand the line picks which you hear.",
                    "Termina el capítulo 1", "Finish chapter 1", () => catalog.ChapterDone(1)),
                ("Nox", "Nox", "Nox salta más alto que Lumi,\npero corre un poco más despacio.", "Nox jumps higher than Lumi,\nbut runs a little slower.",
                    "Termina el capítulo 1", "Finish chapter 1", () => Wardrobe.NoxUnlocked),
                ("El acorde final", "The final chord", "Al llegar a la meta, la música\nacaba en un acorde en el tono\nde su capítulo.", "At the goal, the music ends on\na chord in its chapter's key.",
                    "Termina el capítulo 2", "Finish chapter 2", () => catalog.ChapterDone(2)),
                ("Niveles secretos", "Secret levels", "Cada capítulo esconde un nivel:\nse abre al reunir todos sus\nfragmentos de luna.", "Each chapter hides a level: it\nopens when you collect all of\nits moon fragments.",
                    "Todos los fragmentos\nde un capítulo", "All fragments\nin a chapter", () => catalog.AllFragments(1) || catalog.AllFragments(2) || catalog.AllFragments(3)),
                ("El diario", "The diary", "Hay una página del diario de Lumi\nen el cofre secreto de cada nivel.", "There's a page of Lumi's diary\nin every level's secret chest.",
                    "Encuentra un cofre", "Find a chest", () => Diary.Found >= 1),
                ("La Gran Sombra", "The Great Shadow", "La Gran Sombra se vuelve piedra\ncon la luz del día.", "The Great Shadow turns to stone\nin daylight.",
                    "Vence a la Gran Sombra", "Defeat the Great Shadow", () => boss > 0 && Progress.Unlocked > boss),
                ("Medalla de luna", "Moon medal", "La medalla de luna es más rápida\nque el oro, y su tiempo no se ve\nhasta que la consigues.", "The moon medal beats gold, and\nits time stays hidden until\nyou earn it.",
                    "Consigue 5 logros", "Earn 5 achievements", () => Achievements.Count >= 5),
                ("Modo espejo", "Mirror mode", "Al terminar la historia se abre\nel modo espejo: todos los niveles\ndel revés.", "Finishing the story unlocks\nmirror mode: every level\nflipped around.",
                    "Termina la historia", "Finish the story", () => MirrorMode.Unlocked),
                ("Hecho en un mes", "Made in a month", "FASE se hizo en noviembre de 2026\npara la GitHub Game Off.", "FASE was made in November 2026\nfor the GitHub Game Off.",
                    "Termina la historia", "Finish the story", () => MirrorMode.Unlocked),
            };
            var count = UiKit.Text(content, "Cuenta", "", new Vector2(1f, 1f), new Vector2(-10f, 70f), new Vector2(300f, 50f), 34, TextAnchor.MiddleRight, UiKit.Gold);
            count.rectTransform.pivot = new Vector2(1f, 0.5f);
            for (int i = 0; i < facts.Length; i++)
            {
                var fact = facts[i];
                var card = UiKit.Box(content, "Curiosidad" + i, new Vector2(0.5f, 0.5f), new Vector2((i % 4 - 1.5f) * 345f, 210f - (i / 4) * 220f), new Vector2(330f, 200f), Color.white);
                var title = UiKit.Text(card.transform, "Titulo", "", new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(310f, 40f), 28, TextAnchor.MiddleCenter, UiKit.Gold);
                var body = UiKit.Text(card.transform, "Texto", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(310f, 130f), 21);
                body.horizontalOverflow = HorizontalWrapMode.Wrap;
                body.resizeTextForBestFit = true;
                body.resizeTextMinSize = 14;
                body.resizeTextMaxSize = 21;
                var mark = UiKit.Text(card.transform, "Interrogacion", "?", new Vector2(0.5f, 0.5f), new Vector2(0f, 24f), new Vector2(120f, 100f), 76, TextAnchor.MiddleCenter, new Color(0.4f, 0.37f, 0.65f));
                refreshers.Add(() =>
                {
                    bool open = fact.open();
                    card.color = open ? new Color(1f, 0.94f, 0.78f, 0.12f) : new Color(1f, 1f, 1f, 0.04f);
                    title.text = open ? Loc.Pick(fact.titleEs, fact.titleEn) : "";
                    body.text = (open ? Loc.Pick(fact.es, fact.en) : Loc.Pick(fact.howEs, fact.howEn)).Replace("\n", " ");
                    body.color = open ? Color.white : new Color(0.6f, 0.58f, 0.78f);
                    body.rectTransform.anchoredPosition = new Vector2(0f, open ? -24f : -58f);
                    mark.enabled = !open;
                });
            }
            refreshers.Add(() => { int n = 0; foreach (var f in facts) if (f.open()) n++; count.text = $"{n} / {facts.Length}"; });
            return panel;
        }

        // ───────── Mapa de viaje: vista previa del nivel ─────────

        /// <summary>
        /// La ficha del nivel elegido crece y muestra a la izquierda el plano del nivel (lo dibuja
        /// el generador en Resources/Vistas) recorriéndose despacio, partido en día y noche.
        /// </summary>
        void BuildJourneyPreview()
        {
            if (menu.infoTitle == null) return;
            var info = (RectTransform)menu.infoTitle.transform.parent;
            info.sizeDelta = new Vector2(info.sizeDelta.x, 250f);
            foreach (var label in new[] { menu.infoTitle, menu.infoDetail })
            {
                var r = label.rectTransform;
                r.anchoredPosition += new Vector2(400f, label == menu.infoTitle ? -6f : 6f);
                r.sizeDelta = new Vector2(420f, r.sizeDelta.y);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 18;
            }
            menu.infoTitle.fontSize = menu.infoTitle.resizeTextMaxSize = 44;
            menu.infoDetail.fontSize = menu.infoDetail.resizeTextMaxSize = 28;

            var frame = UiKit.Box(info, "Marco", new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(394f, 210f), new Color(1f, 1f, 1f, 0.25f));
            frame.rectTransform.pivot = new Vector2(0f, 0.5f);
            var window = UiKit.Rect(info, "Vista", new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(390f, 206f));
            window.pivot = new Vector2(0f, 0.5f);
            window.anchoredPosition = new Vector2(24f, 0f);
            window.gameObject.AddComponent<RectMask2D>();
            var day = UiKit.Box(window, "Dia", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(195f, 206f), new Color(0.55f, 0.8f, 0.98f), false);
            day.rectTransform.pivot = new Vector2(0f, 0.5f);
            var night = UiKit.Box(window, "Noche", new Vector2(1f, 0.5f), Vector2.zero, new Vector2(195f, 206f), new Color(0.1f, 0.09f, 0.3f), false);
            night.rectTransform.pivot = new Vector2(1f, 0.5f);
            var map = UiKit.Rect(window, "Plano", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(390f, 206f)).gameObject.AddComponent<RawImage>();
            map.rectTransform.pivot = new Vector2(0f, 0.5f);
            map.raycastTarget = false;
            // Velo de noche sobre la mitad derecha y la línea en medio.
            UiKit.Box(window, "Velo", new Vector2(1f, 0.5f), Vector2.zero, new Vector2(195f, 206f), new Color(0.05f, 0.05f, 0.25f, 0.35f), false).rectTransform.pivot = new Vector2(1f, 0.5f);
            UiKit.Box(window, "Halo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26f, 206f), new Color(1f, 0.95f, 0.8f, 0.25f), false);
            UiKit.Box(window, "Linea", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 206f), Color.white, false);
            var locked = UiKit.Text(window, "Cerrado", "?", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 160f), 110, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.5f));
            var pan = map.gameObject.AddComponent<PreviewPan>();

            menu.Selected += (level, open) =>
            {
                var entry = LevelCatalog.Get.At(level);
                var texture = open && entry != null ? Resources.Load<Texture2D>("Vistas/" + entry.scene) : null;
                map.texture = texture;
                map.enabled = texture != null;
                locked.enabled = !open;
                if (texture != null)
                {
                    float scale = 206f / texture.height;
                    map.rectTransform.sizeDelta = new Vector2(texture.width * scale, 206f);
                    pan.Restart(Mathf.Max(0f, texture.width * scale - 390f));
                }
            };
        }

        // ───────── Mapa de viaje: niveles secretos ─────────


        void BuildJourneyExtras()
        {
            var catalog = LevelCatalog.Get;
            var journey = menu.journeyPanel.transform;
            for (int i = 0; i < catalog.levels.Length; i++)
            {
                var entry = catalog.levels[i];
                if (!entry.secret || entry.practice) continue;

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
