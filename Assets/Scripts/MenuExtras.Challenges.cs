using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Extras → Retos (contrarreloj por capítulo, condiciones y mutadores), Con amigos (fantasma
    /// de un amigo y carrera a dos) y Habilidades (activar o apagar las que ya tienes).
    /// </summary>
    public partial class MenuExtras
    {
        /// <summary>Botón de texto libre que se rehace al refrescar el menú.</summary>
        Button Row(RectTransform parent, string name, Vector2 position, Vector2 size, System.Func<string> label, UnityEngine.Events.UnityAction click, System.Func<bool> available = null, int font = 28)
        {
            var button = UiKit.Button(parent, name, "", new Vector2(0.5f, 0.5f), position, size, click, font);
            var text = button.GetComponentInChildren<Text>();
            refreshers.Add(() => { button.interactable = available == null || available(); text.text = label(); });
            return button;
        }

        Text Note(RectTransform parent, string name, Vector2 position, float width, System.Func<string> text, int font = 26)
        {
            var label = UiKit.Text(parent, name, "", new Vector2(0.5f, 0.5f), position, new Vector2(width, 50f), font, TextAnchor.MiddleCenter, UiKit.Soft);
            refreshers.Add(() => label.text = text());
            return label;
        }

        // ───────── Retos ─────────

        GameObject BuildChallenges()
        {
            var panel = Panel("Retos", "challenges", out var content, BackToExtras);
            Note(content, "Crono", new Vector2(-450f, 300f), 420f, () => Loc.Pick("Contrarreloj de capítulo", "Chapter time trial"), 30).color = UiKit.Gold;
            for (int c = 1; c <= 3; c++)
            {
                int chapter = c;
                string roman = new[] { "I", "II", "III" }[c - 1];
                Row(content, "Capitulo" + c, new Vector2(-450f, 220f - (c - 1) * 95f), new Vector2(420f, 82f), () =>
                {
                    float best = ChapterRun.Best(chapter);
                    string time = best > 0f ? Hud.FormatTime(best) : "—";
                    return LevelCatalog.Get.ChapterDone(chapter) ? $"{Loc.Pick("Capítulo", "Chapter")} {roman}   ⏱ {time}" : $"{Loc.Pick("Capítulo", "Chapter")} {roman}   —";
                }, () => ChapterRun.Begin(chapter), () => LevelCatalog.Get.ChapterDone(chapter));
            }

            Note(content, "Cond", new Vector2(0f, 300f), 420f, () => Loc.Pick("Condición del próximo nivel", "Next level's condition"), 30).color = UiKit.Gold;
            for (int i = 0; i < Conditions.Names.Length; i++)
            {
                int index = i;
                Row(content, "Condicion" + i, new Vector2(0f, 220f - i * 95f), new Vector2(420f, 82f), () =>
                {
                    var (es, en) = Conditions.Names[index];
                    string mark = Conditions.Armed == index ? "● " : "○ ";
                    return $"{mark}{Loc.Pick(es, en)}  ({Conditions.Count(index)})";
                }, () => { Conditions.Armed = Conditions.Armed == index ? -1 : index; Refresh(); }, null, 24);
            }

            Note(content, "Mut", new Vector2(450f, 300f), 420f, () => Loc.Pick("Mutadores (sin medallas)", "Mutators (no medals)"), 30).color = UiKit.Gold;
            for (int i = 0; i < Mutators.All.Length; i++)
            {
                var m = Mutators.All[i];
                Row(content, "Mutador" + m.id, new Vector2(450f, 220f - i * 95f), new Vector2(420f, 82f), () =>
                    Mutators.Unlocked ? $"{Loc.Pick(m.es, m.en)}:  {Loc.Get(Mutators.On(m.id) ? "on" : "off")}" : $"{Loc.Pick(m.es, m.en)}  —",
                    () => { Mutators.Set(m.id, !Mutators.On(m.id)); Refresh(); }, () => Mutators.Unlocked);
            }
            Note(content, "MutAyuda", new Vector2(450f, 220f - Mutators.All.Length * 95f), 460f,
                () => Mutators.Unlocked ? "" : Loc.Pick("Termina la historia para abrirlos", "Finish the story to unlock them"), 22);
            Note(content, "Ayuda", new Vector2(0f, -250f), 1300f, () => Loc.Pick(
                "La condición se aplica al siguiente nivel que juegues. El contrarreloj encadena todo el capítulo.",
                "The condition applies to the next level you play. The time trial chains the whole chapter."), 24);
            return panel;
        }

        // ───────── Con amigos ─────────

        static string RaceScene()
        {
            var catalog = LevelCatalog.Get;
            int level = Mathf.Clamp(Progress.Unlocked, 1, Mathf.Max(1, catalog.MainCount));
            return catalog.levels[level - 1].scene;
        }

        GameObject BuildFriends()
        {
            var panel = Panel("ConAmigos", "friends", out var content, BackToExtras);
            Note(content, "FantTitulo", new Vector2(0f, 300f), 1200f, () => Loc.Pick("Fantasma de un amigo", "A friend's ghost"), 32).color = UiKit.Gold;
            Note(content, "FantAyuda", new Vector2(0f, 245f), 1300f, () => Loc.Pick(
                "Pega aquí el código FANT- que te pasaron (al terminar un nivel, «Código de fantasma»).",
                "Paste the FANT- code you were sent here (after a level, \"Ghost code\")."), 24);

            var fieldBack = UiKit.Box(content, "Campo", new Vector2(0.5f, 0.5f), new Vector2(-150f, 160f), new Vector2(760f, 84f), new Color(1f, 1f, 1f, 0.95f));
            fieldBack.raycastTarget = true;
            var field = fieldBack.gameObject.AddComponent<InputField>();
            var typed = UiKit.Text(fieldBack.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 76f), 28, TextAnchor.MiddleLeft, new Color(0.2f, 0.14f, 0.36f));
            Destroy(typed.GetComponent<Shadow>());
            typed.supportRichText = false;
            var hint = UiKit.Text(fieldBack.transform, "Ejemplo", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 76f), 28, TextAnchor.MiddleLeft, new Color(0.5f, 0.48f, 0.6f));
            Destroy(hint.GetComponent<Shadow>());
            field.textComponent = typed;
            field.placeholder = hint;
            field.lineType = InputField.LineType.SingleLine;
            refreshers.Add(() => hint.text = "FANT-3-...");

            int imported = 0;
            string status = "";
            Row(content, "Cargar", new Vector2(470f, 160f), new Vector2(300f, 84f), () => Loc.Pick("Cargar", "Load"), () =>
            {
                imported = FriendGhost.Import(field.text);
                var catalog = LevelCatalog.Get;
                status = imported > 0 && imported <= catalog.levels.Length
                    ? Loc.Pick($"¡Listo! Su fantasma te espera en el nivel {imported}: {catalog.levels[imported - 1].name}", $"Done! Their ghost is waiting in level {imported}: {catalog.levels[imported - 1].nameEn}",
                        $"Pronto! O fantasma te espera no nível {imported}: {Loc.Pick(catalog.levels[imported - 1].name, catalog.levels[imported - 1].nameEn)}", $"C'est prêt ! Son fantôme t'attend au niveau {imported} : {Loc.Pick(catalog.levels[imported - 1].name, catalog.levels[imported - 1].nameEn)}")
                    : Loc.Pick("Ese código no vale. Cópialo entero.", "That code doesn't work. Copy all of it.");
                if (imported > catalog.levels.Length) imported = 0;
                Refresh();
            });
            Note(content, "Estado", new Vector2(0f, 80f), 1300f, () => status, 26);
            Row(content, "JugarFantasma", new Vector2(0f, 0f), new Vector2(520f, 84f), () => Loc.Pick("▶ Correr contra su fantasma", "▶ Race their ghost"),
                () => SceneFader.Load(LevelCatalog.Get.levels[imported - 1].scene), () => imported > 0);

            Note(content, "CarreraTitulo", new Vector2(0f, -110f), 1200f, () => Loc.Pick("Carrera a dos (mismo teclado o dos mandos)", "Two-player race (same keyboard or two pads)"), 32).color = UiKit.Gold;
            Note(content, "CarreraAyuda", new Vector2(0f, -165f), 1300f, () => Loc.Pick(
                "Jugador 1: A/D y espacio · Jugador 2: flechas y ↑ (o el segundo mando). Gana quien llegue antes a la meta.",
                "Player 1: A/D and space · Player 2: arrows and ↑ (or the second pad). First to the goal wins."), 24);
            Row(content, "Carrera", new Vector2(0f, -250f), new Vector2(620f, 90f), () =>
            {
                var catalog = LevelCatalog.Get;
                int level = Mathf.Clamp(Progress.Unlocked, 1, Mathf.Max(1, catalog.MainCount));
                return Loc.Pick($"▶ Carrera en el nivel {level}", $"▶ Race on level {level}", $"▶ Corrida no nível {level}", $"▶ Course au niveau {level}");
            }, () => RaceMode.Begin(RaceScene()), null, 34);
            return panel;
        }

        // ───────── Habilidades ─────────

        GameObject BuildSkills()
        {
            var panel = Panel("Habilidades", "skills", out var content, BackToExtras);
            for (int i = 0; i < Skills.All.Length; i++)
            {
                var s = Skills.All[i];
                float y = 220f - i * 170f;
                Row(content, "Habilidad" + s.id, new Vector2(0f, y), new Vector2(720f, 90f), () =>
                    Skills.Unlocked(s.id) ? $"{Loc.Pick(s.es, s.en)}:  {Loc.Get(Skills.Enabled(s.id) ? "on" : "off")}" : $"{Loc.Pick(s.es, s.en)}  —",
                    () => { Skills.SetEnabled(s.id, !Skills.Enabled(s.id)); Refresh(); }, () => Skills.Unlocked(s.id), 34);
                Note(content, "Como" + s.id, new Vector2(0f, y - 70f), 900f, () => Skills.Unlocked(s.id) ? "" : Loc.Pick(s.howEs, s.howEn), 24);
            }
            Note(content, "Plumas", new Vector2(0f, -300f), 1000f, () => Loc.Pick($"Plumas, hojas y copos: {Feathers.Count(0)}", $"Feathers, leaves and snowflakes: {Feathers.Count(0)}", $"Penas, folhas e flocos: {Feathers.Count(0)}", $"Plumes, feuilles et flocons : {Feathers.Count(0)}"), 26);
            return panel;
        }
    }
}
