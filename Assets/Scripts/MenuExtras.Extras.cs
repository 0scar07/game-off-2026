using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Extras del menú: estadísticas y medallas de estilo, banda sonora, galería de modelos,
    /// álbum de fotos, mapa de capítulos con sus plumas, retos de la semana y el botón del
    /// fantasma en El viaje.
    /// </summary>
    public partial class MenuExtras
    {
        GameObject extrasPanel;
        readonly List<GameObject> extraPanels = new List<GameObject>();

        void BuildExtras()
        {
            var stats = BuildStats();
            var music = BuildSoundtrack();
            var gallery = BuildGallery();
            var album = BuildAlbum();
            var map = BuildChapterMap();
            var weekly = BuildWeekly();

            extrasPanel = Panel("Extras", "extras", out var content);
            var help = BuildHelp();
            var extraLevels = BuildExtraLevels();
            var challenges = BuildChallenges();
            var friends = BuildFriends();
            var skills = BuildSkills();
            var weeklyLevel = BuildWeeklyLevel();
            var twitch = BuildTwitch();
            var entries = new (string key, string icon, GameObject panel)[]
            {
                ("stats", "⏱", stats), ("soundtrack", "♪", music), ("gallery", "☺", gallery),
                ("album", "▦", album), ("chaptermap", "☾", map), ("weekly", "★", weekly),
                ("help", "✔", help), ("extralevels", "✦", extraLevels), ("challenges", "⚑", challenges),
                ("friends", "☺", friends), ("skills", "✧", skills), ("weeklylevel", "✎", weeklyLevel), ("twitch", "●", twitch),
            };
            for (int i = 0; i < entries.Length; i++)
            {
                var (key, icon, panel) = entries[i];
                UnityEngine.Events.UnityAction open = () => menu.Show(panel);

                // Rejilla de 3 columnas.
                bool many = entries.Length > 12;
                var button = UiKit.Button(content, key, "", new Vector2(0.5f, 0.5f), new Vector2((i % 3 - 1) * 450f, (many ? 262f : 250f) - (i / 3) * (many ? 114f : 140f)), new Vector2(430f, many ? 100f : 120f), open, 34);
                var label = button.GetComponentInChildren<Text>();
                // El texto empieza después del icono y se encoge si es largo ("Retos de la semana").
                label.rectTransform.anchoredPosition = new Vector2(50f, 0f);
                label.rectTransform.sizeDelta = new Vector2(300f, 100f);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 20;
                label.resizeTextMaxSize = 34;

                UiKit.Text(button.transform, "Icono", icon, new Vector2(0f, 0.5f), new Vector2(55f, 0f), new Vector2(90f, 90f), 50, TextAnchor.MiddleCenter, UiKit.Gold).rectTransform.pivot = new Vector2(0.5f, 0.5f);
                string k = key;
                refreshers.Add(() => label.text = Loc.Get(k));
            }
            extraPanels.Add(extrasPanel);
            foreach (var p in new[] { stats, music, gallery, album, map, weekly, help, extraLevels, challenges, friends, skills, weeklyLevel, twitch }) extraPanels.Add(p);

            BuildGhostToggle();
        }

        void BackToExtras() => menu.Show(extrasPanel);

        // ───────── Niveles extra (práctica y mecánicas nuevas) ─────────

        GameObject BuildExtraLevels()
        {
            var panel = Panel("NivelesExtra", "extralevels", out var content, BackToExtras);
            var catalog = LevelCatalog.Get;
            int n = 0;
            for (int i = 0; i < catalog.levels.Length; i++)
            {
                var entry = catalog.levels[i];
                if (!entry.practice) continue;
                string scene = entry.scene;
                float x = (n % 3 - 1) * 450f, y = 262f - (n / 3) * 100f;
                var button = UiKit.Button(content, scene, "", new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(430f, 86f), () => SceneFader.Load(scene), 30);
                var label = button.GetComponentInChildren<Text>();
                var done = UiKit.Text(button.transform, "Hecho", "✔", new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(50f, 50f), 30, TextAnchor.MiddleCenter, UiKit.Gold);
                int level = i + 1;
                refreshers.Add(() => { label.text = Loc.Pick(entry.name, entry.nameEn); done.enabled = Progress.BestTime(level) > 0f; });
                n++;
            }
            return panel;
        }

        // ───────── Nivel de la semana (descargado del repositorio) ─────────

        GameObject BuildWeeklyLevel()
        {
            var panel = Panel("NivelSemana", "weeklylevel", out var content, BackToExtras);
            var card = UiKit.Box(content, "Tarjeta", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1000f, 300f), new Color(1f, 1f, 1f, 0.08f));
            var name = UiKit.Text(card.transform, "Nombre", "", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(940f, 80f), 56, TextAnchor.MiddleCenter, UiKit.Gold);
            var author = UiKit.Text(card.transform, "Autor", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(940f, 50f), 32, TextAnchor.MiddleCenter, UiKit.Soft);
            var status = UiKit.Text(card.transform, "Estado", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(940f, 50f), 26, TextAnchor.MiddleCenter, UiKit.Soft);
            var play = UiKit.Button(content, "JugarSemana", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(480f, 100f), WeeklyLevel.Play, 42);
            var playLabel = play.GetComponentInChildren<Text>();
            var send = UiKit.Button(content, "EnviarNivel", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -220f), new Vector2(620f, 76f), () => Application.OpenURL(WeeklyLevel.SubmitUrl), 28);
            var sendLabel = send.GetComponentInChildren<Text>();
            refreshers.Add(() =>
            {
                var state = WeeklyLevel.Status;
                bool ready = state == WeeklyLevel.State.Ready;
                name.text = ready ? WeeklyLevel.Name : "✎";
                author.text = ready ? Loc.Pick($"por {WeeklyLevel.Author}", $"by {WeeklyLevel.Author}", $"por {WeeklyLevel.Author}", $"par {WeeklyLevel.Author}") : "";
                status.text = state == WeeklyLevel.State.Failed
                    ? Loc.Pick("No se pudo descargar. ¿Hay conexión?", "Couldn't download it. Are you online?", "Não foi possível baixar. Você está online?", "Impossible de le télécharger. Es-tu en ligne ?")
                    : ready ? Loc.Pick("Un nivel del Taller elegido cada semana.", "A Workshop level picked every week.", "Um nível da Oficina escolhido toda semana.", "Un niveau de l'Atelier choisi chaque semaine.")
                    : Loc.Pick("Descargando…", "Downloading…", "Baixando…", "Téléchargement…");
                play.interactable = ready || state == WeeklyLevel.State.Failed;
                playLabel.text = state == WeeklyLevel.State.Failed ? "↻ " + Loc.Pick("Reintentar", "Retry", "Tentar de novo", "Réessayer") : "▶ " + Loc.Get("play");
                sendLabel.text = Loc.Pick("✎ Envía tu nivel del Taller", "✎ Send your Workshop level", "✎ Envie seu nível da Oficina", "✎ Envoie ton niveau de l'Atelier");
            });
            // Al abrir el panel se descarga (o se reintenta) y se refresca al terminar.
            menu.Shown += shown =>
            {
                if (shown != panel || WeeklyLevel.Status == WeeklyLevel.State.Ready) return;
                StartCoroutine(FetchWeekly());
            };
            play.onClick.AddListener(() => { if (WeeklyLevel.Status == WeeklyLevel.State.Failed) StartCoroutine(FetchWeekly()); });
            return panel;
        }

        System.Collections.IEnumerator FetchWeekly()
        {
            var download = StartCoroutine(WeeklyLevel.Fetch());
            Refresh();
            yield return download;
            Refresh();
        }

        // ───────── Twitch: el chat vota hacia dónde va la línea ─────────

        GameObject BuildTwitch()
        {
            var panel = Panel("Twitch", "twitch", out var content, BackToExtras);
            Note(content, "TwitchAyuda", new Vector2(0f, 260f), 1300f, () => Loc.Pick(
                "Para jugar en directo: el chat de tu canal vota cada 3 segundos hacia dónde va la línea.",
                "For streaming: your channel's chat votes every 3 seconds on where the line goes.",
                "Para jogar ao vivo: o chat do seu canal vota a cada 3 segundos para onde vai a linha.",
                "Pour les lives : le chat de ta chaîne vote toutes les 3 secondes pour diriger la ligne."), 28);
            Note(content, "TwitchVotar", new Vector2(0f, 205f), 1300f, () => Loc.Pick(
                "En el chat: !izquierda o < · !derecha o >  (un voto por persona y ronda). No hace falta cuenta.",
                "In chat: !left or < · !right or >  (one vote per person per round). No account needed.",
                "No chat: !esquerda ou < · !direita ou >  (um voto por pessoa por rodada). Não precisa de conta.",
                "Dans le chat : !gauche ou < · !droite ou >  (un vote par personne et par manche). Pas besoin de compte."), 24);

            var fieldBack = UiKit.Box(content, "Campo", new Vector2(0.5f, 0.5f), new Vector2(-150f, 110f), new Vector2(760f, 84f), new Color(1f, 1f, 1f, 0.95f));
            fieldBack.raycastTarget = true;
            var field = fieldBack.gameObject.AddComponent<InputField>();
            var typed = UiKit.Text(fieldBack.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 76f), 30, TextAnchor.MiddleLeft, new Color(0.2f, 0.14f, 0.36f));
            Destroy(typed.GetComponent<Shadow>());
            typed.supportRichText = false;
            var hint = UiKit.Text(fieldBack.transform, "Ejemplo", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 76f), 30, TextAnchor.MiddleLeft, new Color(0.5f, 0.48f, 0.6f));
            Destroy(hint.GetComponent<Shadow>());
            field.textComponent = typed;
            field.placeholder = hint;
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = 25;
            field.text = TwitchVotes.Channel;
            refreshers.Add(() => hint.text = Loc.Pick("nombre de tu canal", "your channel name", "nome do seu canal", "nom de ta chaîne"));

            Row(content, "TwitchConectar", new Vector2(470f, 110f), new Vector2(300f, 84f),
                () => TwitchVotes.On ? Loc.Pick("Apagar", "Turn off", "Desligar", "Couper") : Loc.Pick("Conectar", "Connect", "Conectar", "Connecter"),
                () => { if (TwitchVotes.On) TwitchVotes.End(); else TwitchVotes.Begin(field.text); Refresh(); },
                () => TwitchVotes.On || field.text.Trim().Length > 2);
            var state = Note(content, "TwitchEstado", new Vector2(0f, 20f), 1300f, () => !TwitchVotes.On
                ? Loc.Pick("Apagado", "Off", "Desligado", "Désactivé")
                : TwitchVotes.Instance.Status == 2 ? Loc.Pick($"✔ Conectado a #{TwitchVotes.Channel}: entra en un nivel y que vote el chat.", $"✔ Connected to #{TwitchVotes.Channel}: start a level and let chat vote.", $"✔ Conectado a #{TwitchVotes.Channel}: entre em um nível e deixe o chat votar.", $"✔ Connecté à #{TwitchVotes.Channel} : lance un niveau et laisse le chat voter.")
                : TwitchVotes.Instance.Status == 3 ? Loc.Pick("No se pudo conectar. Revisa el nombre del canal.", "Couldn't connect. Check the channel name.", "Não foi possível conectar. Confira o nome do canal.", "Connexion impossible. Vérifie le nom de la chaîne.")
                : Loc.Pick("Conectando…", "Connecting…", "Conectando…", "Connexion…"), 28);
            state.color = UiKit.Gold;
            // El estado cambia solo (conectando → conectado): se refresca mientras el panel está abierto.
            StartCoroutine(RefreshWhileOpen(panel));
            return panel;
        }

        System.Collections.IEnumerator RefreshWhileOpen(GameObject panel)
        {
            var wait = new WaitForSecondsRealtime(0.5f);
            while (true)
            {
                if (panel.activeInHierarchy) Refresh();
                yield return wait;
            }
        }

        // ───────── Ayudas ─────────


        GameObject BuildHelp()
        {
            var panel = Panel("Ayudas", "help", out var content, BackToExtras);
            Toggle(content, "linehint", new Vector2(0f, 230f), () => Settings.LineHint, v => Settings.LineHint = v);
            Toggle(content, "relaxed", new Vector2(0f, 130f), () => Settings.Relaxed, v => Settings.Relaxed = v);
            Toggle(content, "slowgame", new Vector2(0f, 30f), () => Settings.SlowGame, v => Settings.SlowGame = v, null, v => v ? "80 %" : "100 %");
            Toggle(content, "onetouch", new Vector2(0f, -70f), () => Settings.OneTouch, v => Settings.OneTouch = v);
            var note = UiKit.Text(content, "Nota", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(1200f, 120f), 26, TextAnchor.MiddleCenter, UiKit.Soft);
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            refreshers.Add(() => note.text = Loc.Pick(
                "Pista: una línea discontinua enseña dónde ponerla. Relajado: al caer al vacío, Lumi vuelve flotando.\nUn toque (móvil): Lumi avanza sola, la línea se coloca sola y tocar es saltar. Los botones táctiles se colocan desde la pausa.",
                "Hint: a dashed line shows where to put it. Relaxed: falling into the void floats Lumi back.\nOne touch (phone): Lumi walks on her own, the line moves itself and tapping jumps. Touch buttons can be moved from the pause menu."));
            return panel;
        }


        // ───────── Estadísticas y medallas de estilo ─────────

        GameObject BuildStats()
        {
            var panel = Panel("Estadisticas", "stats", out var content, BackToExtras);
            var rows = new (string icon, string es, string en, System.Func<string> value)[]
            {
                ("⏱", "Tiempo jugado", "Time played", () => { int m = PlayToday.TotalMinutes; return m >= 60 ? $"{m / 60} h {m % 60} min" : $"{m} min"; }),
                ("☾", "Lunas", "Moons", () => { var c = LevelCatalog.Get; int have = 0, total = 0; for (int i = 0; i < c.levels.Length; i++) { have += Progress.Fragments(i + 1); total += c.levels[i].fragments; } return $"{have} / {total}"; }),
                ("↑", "Saltos", "Jumps", () => Stats.Jumps.ToString("N0")),
                ("|", "Veces que moviste la línea", "Times you moved the line", () => Stats.LineMoves.ToString("N0")),
                ("✖", "Caídas", "Falls", () => Stats.Falls.ToString("N0")),
                ("→", "Recorrido", "Distance", () => Stats.Meters >= 1000f ? $"{Stats.Meters / 1000f:0.0} km" : $"{Stats.Meters:0} m"),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                float y = 270f - i * 92f;
                UiKit.Box(content, "Fila", new Vector2(0.5f, 0.5f), new Vector2(-350f, y), new Vector2(640f, 76f), new Color(1f, 1f, 1f, 0.06f));
                UiKit.Text(content, "Icono", row.icon, new Vector2(0.5f, 0.5f), new Vector2(-620f, y), new Vector2(60f, 60f), 36, TextAnchor.MiddleCenter, UiKit.Gold);
                var name = UiKit.Text(content, "Nombre", "", new Vector2(0.5f, 0.5f), new Vector2(-400f, y), new Vector2(360f, 60f), 26, TextAnchor.MiddleLeft, UiKit.Soft);
                var value = UiKit.Text(content, "Valor", "", new Vector2(0.5f, 0.5f), new Vector2(-130f, y), new Vector2(240f, 60f), 32, TextAnchor.MiddleRight);
                refreshers.Add(() => { name.text = Loc.Pick(row.es, row.en); value.text = row.value(); });
            }
            var styleTitle = UiKit.Text(content, "TituloEstilo", "", new Vector2(0.5f, 0.5f), new Vector2(350f, 300f), new Vector2(640f, 60f), 38, TextAnchor.MiddleCenter, UiKit.Gold);
            refreshers.Add(() => styleTitle.text = Loc.Pick("Medallas de estilo", "Style medals"));
            for (int i = 0; i < StyleMedals.All.Length; i++)
            {
                var medal = StyleMedals.All[i];
                float y = 190f - i * 130f;
                var card = UiKit.Box(content, medal.id, new Vector2(0.5f, 0.5f), new Vector2(350f, y), new Vector2(640f, 112f), Color.white);
                var dot = UiKit.Box(card.transform, "Medalla", new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(70f, 70f), Color.white);
                dot.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                dot.type = Image.Type.Simple;
                var mark = UiKit.Text(dot.transform, "Signo", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 70f), 38);
                var name = UiKit.Text(card.transform, "Nombre", "", new Vector2(0f, 0.5f), new Vector2(120f, 20f), new Vector2(500f, 44f), 30, TextAnchor.MiddleLeft);
                name.rectTransform.pivot = new Vector2(0f, 0.5f);
                var how = UiKit.Text(card.transform, "Como", "", new Vector2(0f, 0.5f), new Vector2(120f, -22f), new Vector2(500f, 36f), 20, TextAnchor.MiddleLeft, UiKit.Soft);
                how.rectTransform.pivot = new Vector2(0f, 0.5f);
                refreshers.Add(() =>
                {
                    bool has = StyleMedals.Has(medal.id);
                    card.color = has ? new Color(1f, 0.94f, 0.78f, 0.12f) : new Color(1f, 1f, 1f, 0.04f);
                    dot.color = has ? new Color(1f, 0.87f, 0.45f) : new Color(0.25f, 0.24f, 0.4f);
                    mark.text = has ? "✦" : "?";
                    mark.color = has ? new Color(0.48f, 0.32f, 0f) : new Color(0.6f, 0.6f, 0.75f);
                    name.text = medal.Name;
                    name.color = has ? Color.white : new Color(0.65f, 0.65f, 0.8f);
                    how.text = medal.How;
                });
            }
            return panel;
        }

        // ───────── Banda sonora ─────────

        GameObject BuildSoundtrack()
        {
            var panel = Panel("BandaSonora", "soundtrack", out var content, BackToExtras);
            var source = panel.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            int playing = -1;
            var clips = menu.soundtrack ?? new AudioClip[0];
            var labels = new List<(Image row, Text mark)>();
            Image bar = null;
            System.Action<int> play = index =>
            {
                if (index == playing && source.isPlaying) { source.Pause(); MusicLayers.Muted = false; }
                else if (index == playing) { source.UnPause(); MusicLayers.Muted = true; }
                else { playing = index; source.clip = clips[index]; source.Play(); MusicLayers.Muted = true; }
                source.volume = Progress.MusicVolume;
                Refresh();
            };
            for (int i = 0; i < clips.Length; i++)
            {
                int index = i;
                float y = 290f - i * 70f;
                var row = UiKit.Button(content, "Pista" + i, "", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1000f, 60f), () => play(index), 28);
                var text = row.GetComponentInChildren<Text>();
                text.alignment = TextAnchor.MiddleLeft;
                text.rectTransform.sizeDelta = new Vector2(880f, 60f);
                text.rectTransform.anchoredPosition = new Vector2(40f, 0f);
                var mark = UiKit.Text(row.transform, "Signo", "♪", new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(60f, 60f), 30, TextAnchor.MiddleCenter, UiKit.Gold);
                var length = UiKit.Text(row.transform, "Duracion", clips[i] != null ? $"{(int)clips[i].length / 60}:{(int)clips[i].length % 60:00}" : "", new Vector2(1f, 0.5f), new Vector2(-60f, 0f), new Vector2(120f, 60f), 22, TextAnchor.MiddleRight, UiKit.Soft);
                labels.Add((row.GetComponent<Image>(), mark));
                refreshers.Add(() =>
                {
                    text.text = menu.soundtrackNames != null && index < menu.soundtrackNames.Length ? Loc.Pick(menu.soundtrackNames[index], menu.soundtrackNamesEn[index]) : "";
                    bool on = index == playing && source.isPlaying;
                    mark.text = on ? "▶" : "♪";
                    row.GetComponent<Image>().color = on ? new Color(1f, 0.85f, 0.45f, 0.35f) : UiKit.ButtonColor;
                });
            }
            UiKit.Box(content, "Barra", new Vector2(0.5f, 0.5f), new Vector2(0f, -290f), new Vector2(1000f, 12f), new Color(1f, 1f, 1f, 0.15f));
            bar = UiKit.Box(content, "Avance", new Vector2(0.5f, 0.5f), new Vector2(-500f, -290f), new Vector2(0f, 12f), new Color(1f, 0.87f, 0.45f));
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            panel.AddComponent<SoundtrackBar>().Init(source, bar);
            // Al salir del panel se para y vuelve la música del menú.
            menu.Shown += shown => { if (shown != panel && source.isPlaying) { source.Stop(); playing = -1; MusicLayers.Muted = false; } };
            return panel;
        }

        // ───────── Galería ─────────

        GameObject BuildGallery()
        {
            var panel = Panel("Galeria", "gallery", out var content, BackToExtras);
            var view = UiKit.Rect(content, "Vista", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(500f, 500f)).gameObject.AddComponent<RawImage>();
            var frame = UiKit.Box(content, "Marco", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(520f, 520f), new Color(1f, 1f, 1f, 0.08f));
            frame.transform.SetSiblingIndex(view.transform.GetSiblingIndex());
            var name = UiKit.Text(content, "Nombre", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(600f, 50f), 38, TextAnchor.MiddleCenter, UiKit.Gold);
            var hint = UiKit.Text(content, "Ayuda", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -294f), new Vector2(600f, 36f), 24, TextAnchor.MiddleCenter, UiKit.Soft);
            var gallery = panel.AddComponent<Gallery>();
            gallery.Init(view, FindAnyObjectByType<MenuWalker>());
            UiKit.Button(content, "Anterior", "◀", new Vector2(0.5f, 0.5f), new Vector2(-420f, 40f), new Vector2(100f, 100f), () => { gallery.Step(-1); Refresh(); }, 44);
            UiKit.Button(content, "Siguiente", "▶", new Vector2(0.5f, 0.5f), new Vector2(420f, 40f), new Vector2(100f, 100f), () => { gallery.Step(1); Refresh(); }, 44);
            refreshers.Add(() => { name.text = gallery.CurrentName; hint.text = Loc.Pick("↻ Arrastra para girarlo", "↻ Drag to turn it"); });
            menu.Shown += shown => gallery.SetVisible(shown == panel);
            return panel;
        }

        // ───────── Álbum ─────────

        GameObject BuildAlbum()
        {
            var panel = Panel("Album", "album", out var content, BackToExtras);
            var photos = new RawImage[Album.Size];
            for (int i = 0; i < Album.Size; i++)
            {
                var back = UiKit.Box(content, "Marco", new Vector2(0.5f, 0.5f), new Vector2((i % 3 - 1) * 400f, 210f - (i / 3) * 215f), new Vector2(370f, 210f), new Color(1f, 1f, 1f, 0.9f), false);
                photos[i] = UiKit.Rect(back.transform, "Foto", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(354f, 194f)).gameObject.AddComponent<RawImage>();
            }
            var empty = UiKit.Text(content, "Vacio", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 120f), 34, TextAnchor.MiddleCenter, UiKit.Soft);
            empty.horizontalOverflow = HorizontalWrapMode.Wrap;
            refreshers.Add(() =>
            {
                if (!panel.activeSelf) return;
                int count = 0;
                for (int i = 0; i < photos.Length; i++)
                {
                    if (photos[i].texture != null) Destroy(photos[i].texture);
                    var tex = Album.Get(i);
                    photos[i].texture = tex;
                    photos[i].transform.parent.gameObject.SetActive(tex != null);
                    if (tex != null) count++;
                }
                empty.text = count == 0 ? Loc.Pick("Aún no hay fotos. En un nivel, pulsa F (o Modo foto en la pausa) y luego Capturar.", "No photos yet. In a level, press F (or Photo mode in the pause menu) and then Capture.") : "";
            });
            return panel;
        }

        // ───────── Mapa de capítulos ─────────

        GameObject BuildChapterMap()
        {
            var panel = Panel("MapaCapitulos", "chaptermap", out var content, BackToExtras);
            var catalog = LevelCatalog.Get;
            string[] names = { "I · La Pradera", "II · El Bosque Otoñal", "III · La Cumbre Helada" };
            string[] namesEn = { "I · The Meadow", "II · The Autumn Forest", "III · The Frozen Summit" };
            for (int chapter = 1; chapter <= 3; chapter++)
            {
                int c = chapter;
                float y = 230f - (chapter - 1) * 225f;
                var title = UiKit.Text(content, "Capitulo", "", new Vector2(0.5f, 0.5f), new Vector2(-330f, y + 82f), new Vector2(700f, 44f), 30, TextAnchor.MiddleLeft, UiKit.Gold);
                title.rectTransform.pivot = new Vector2(0f, 0.5f);
                title.rectTransform.anchoredPosition = new Vector2(-680f, y + 82f);
                var count = UiKit.Text(content, "Plumas", "", new Vector2(0.5f, 0.5f), new Vector2(680f, y + 82f), new Vector2(500f, 44f), 26, TextAnchor.MiddleRight, UiKit.Soft);
                count.rectTransform.pivot = new Vector2(1f, 0.5f);
                var strip = UiKit.Rect(content, "Tira", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1360f, 120f));
                strip.gameObject.AddComponent<RectMask2D>();
                var levels = new List<int>();
                for (int i = 0; i < catalog.levels.Length; i++) if (!catalog.levels[i].secret && catalog.levels[i].chapter == chapter) levels.Add(i + 1);
                float width = 1360f / Mathf.Max(1, levels.Count);
                var covers = new List<(int level, Image cover, Text feather)>();
                for (int k = 0; k < levels.Count; k++)
                {
                    int level = levels[k];
                    var cell = UiKit.Rect(strip, "Nivel" + level, new Vector2(0f, 0.5f), new Vector2(k * width, 0f), new Vector2(width - 6f, 120f));
                    cell.pivot = new Vector2(0f, 0.5f);
                    var sky = UiKit.Box(cell, "Cielo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width - 6f, 120f), new Color(0.55f, 0.8f, 0.98f), false);
                    var preview = UiKit.Rect(cell, "Plano", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width - 6f, 120f)).gameObject.AddComponent<RawImage>();
                    var tex = Resources.Load<Texture2D>("Vistas/" + catalog.levels[level - 1].scene);
                    preview.texture = tex;
                    if (tex != null) preview.uvRect = new Rect(0f, 0f, Mathf.Min(1f, (width - 6f) / 120f * tex.height / tex.width), 1f);
                    var cover = UiKit.Box(cell, "Pergamino", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width - 6f, 120f), new Color(0.91f, 0.86f, 0.76f, 0.96f), false);
                    UiKit.Text(cover.transform, "Interrogacion", "?", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f), 50, TextAnchor.MiddleCenter, new Color(0.6f, 0.5f, 0.4f));
                    var feather = UiKit.Text(cell, "Pluma", "✦", new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(30f, 30f), 26, TextAnchor.MiddleCenter, UiKit.Gold);
                    covers.Add((level, cover, feather));
                }
                refreshers.Add(() =>
                {
                    title.text = Loc.Pick(names[c - 1], namesEn[c - 1]);
                    count.text = $"{Feathers.Name(c, true)}: {Feathers.Count(c)} / {Feathers.Total(c)}";
                    // Se pinta lo que ya has jugado; lo demás sigue en blanco.
                    foreach (var (level, cover, feather) in covers)
                    {
                        cover.gameObject.SetActive(level >= Progress.Unlocked);
                        feather.enabled = Feathers.Has(level);
                    }
                });
            }
            return panel;
        }

        // ───────── Retos de la semana ─────────

        GameObject BuildWeekly()
        {
            var panel = Panel("RetosSemana", "weekly", out var content, BackToExtras);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                float y = 220f - i * 150f;
                UiKit.Box(content, "Reto", new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1000f, 124f), new Color(1f, 1f, 1f, 0.06f));
                var text = UiKit.Text(content, "Texto", "", new Vector2(0.5f, 0.5f), new Vector2(-460f, y + 24f), new Vector2(800f, 50f), 32, TextAnchor.MiddleLeft);
                text.rectTransform.pivot = new Vector2(0f, 0.5f);
                UiKit.Box(content, "Fondo", new Vector2(0.5f, 0.5f), new Vector2(-60f, y - 28f), new Vector2(800f, 20f), new Color(1f, 1f, 1f, 0.15f));
                var fill = UiKit.Box(content, "Lleno", new Vector2(0.5f, 0.5f), new Vector2(-460f, y - 28f), new Vector2(0f, 20f), new Color(1f, 0.87f, 0.45f));
                fill.rectTransform.pivot = new Vector2(0f, 0.5f);
                var amount = UiKit.Text(content, "Cuenta", "", new Vector2(0.5f, 0.5f), new Vector2(440f, y - 28f), new Vector2(160f, 40f), 26, TextAnchor.MiddleRight, UiKit.Soft);
                refreshers.Add(() =>
                {
                    var c = WeeklyChallenges.ThisWeek[index];
                    int p = WeeklyChallenges.Progress(c);
                    text.text = c.Text;
                    fill.rectTransform.sizeDelta = new Vector2(800f * p / c.goal, 20f);
                    fill.color = p >= c.goal ? new Color(0.5f, 0.9f, 0.45f) : new Color(1f, 0.87f, 0.45f);
                    amount.text = $"{p}/{c.goal}";
                });
            }
            var prize = UiKit.Box(content, "Premio", new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(1000f, 100f), new Color(1f, 0.87f, 0.45f, 0.15f));
            var prizeText = UiKit.Text(prize.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 90f), 32, TextAnchor.MiddleCenter, UiKit.Gold);
            refreshers.Add(() => prizeText.text = WeeklyChallenges.EverCompleted
                ? Loc.Pick("✔ Bufanda de rayas conseguida (en el armario de Lumi)", "✔ Striped scarf earned (in Lumi's wardrobe)")
                : Loc.Pick("Premio: bufanda de rayas", "Prize: striped scarf"));
            return panel;
        }

        // ───────── Fantasma en El viaje ─────────

        void BuildGhostToggle()
        {
            var journey = menu.journeyPanel.transform;
            var button = UiKit.Button(journey, "Fantasma", "", new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(520f, 96f), () => { Settings.GhostRace = !Settings.GhostRace; Refresh(); }, 30);
            ((RectTransform)button.transform).pivot = new Vector2(1f, 0f);
            var label = button.GetComponentInChildren<Text>();
            refreshers.Add(() => label.text = Loc.Pick($"Correr contra mi récord: {(Settings.GhostRace ? "Sí" : "No")}", $"Race my best: {(Settings.GhostRace ? "On" : "Off")}", $"Correr contra meu recorde: {(Settings.GhostRace ? "Sim" : "Não")}", $"Courir contre mon record : {(Settings.GhostRace ? "Oui" : "Non")}"));
        }
    }
}
