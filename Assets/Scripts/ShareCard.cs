using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Tarjeta del final del juego: tiempo jugado, luna reunida, oros y logros, con un botón para
    /// compartirlo (copia o comparte el texto, como en los resultados de cada nivel).
    /// </summary>
    public static class ShareCard
    {
        public static void Show()
        {
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("TarjetaFinal", 30);
            var card = UiKit.Box(canvas.transform, "Tarjeta", new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(620f, 300f), new Color(1f, 1f, 1f, 0.95f));
            card.rectTransform.pivot = new Vector2(1f, 0f);
            var catalog = LevelCatalog.Get;
            int moon = 0, total = 0;
            for (int i = 0; i < catalog.levels.Length; i++) { moon += Progress.Fragments(i + 1); total += catalog.levels[i].fragments; }
            int golds = Medals.CountAtLeast(Medals.Gold, catalog.levels.Length);
            int minutes = PlayToday.TotalMinutes;
            string time = minutes >= 60 ? $"{minutes / 60} h {minutes % 60} min" : $"{minutes} min";
            string who = PlayerName.Has ? PlayerName.Value : "Lumi";
            var dark = new Color(0.16f, 0.12f, 0.36f);
            var title = UiKit.Text(card.transform, "Titulo", Loc.Pick($"FASE · ¡{who} lo ha terminado!", $"FASE · {who} finished it!", $"FASE · {who} terminou!", $"FASE · {who} l'a terminé !"), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(580f, 60f), 36, TextAnchor.MiddleCenter, dark);
            Object.Destroy(title.GetComponent<Shadow>());
            var line = UiKit.Text(card.transform, "Datos", $"⏱ {time}    ☾ {moon}/{total}    ★ {golds} {Loc.Pick("oros", "golds")}", new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(580f, 50f), 30, TextAnchor.MiddleCenter, new Color(0.35f, 0.29f, 0.55f));
            Object.Destroy(line.GetComponent<Shadow>());
            var extra = UiKit.Text(card.transform, "Logros", Loc.Pick($"Logros: {Achievements.Count}/{Achievements.All.Length}    Estilo: {StyleMedals.Count}/{StyleMedals.All.Length}", $"Achievements: {Achievements.Count}/{Achievements.All.Length}    Style: {StyleMedals.Count}/{StyleMedals.All.Length}",
                $"Conquistas: {Achievements.Count}/{Achievements.All.Length}    Estilo: {StyleMedals.Count}/{StyleMedals.All.Length}", $"Succès : {Achievements.Count}/{Achievements.All.Length}    Style : {StyleMedals.Count}/{StyleMedals.All.Length}"),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -28f), new Vector2(580f, 40f), 24, TextAnchor.MiddleCenter, new Color(0.35f, 0.29f, 0.55f));
            Object.Destroy(extra.GetComponent<Shadow>());
            Text label = null;
            var button = UiKit.Button(card.transform, "Compartir", Loc.Pick("Compartir", "Share"), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(320f, 70f), () =>
            {
                string text = Loc.Pick($"¡Terminé FASE! ⏱ {time} · ☾ {moon}/{total} · ★ {golds} oros 🌗 {Share.Url}", $"I finished FASE! ⏱ {time} · ☾ {moon}/{total} · ★ {golds} golds 🌗 {Share.Url}",
                    $"Terminei FASE! ⏱ {time} · ☾ {moon}/{total} · ★ {golds} ouros 🌗 {Share.Url}", $"J'ai fini FASE ! ⏱ {time} · ☾ {moon}/{total} · ★ {golds} ors 🌗 {Share.Url}");
                label.text = Loc.Get(Share.Text(text));
            }, 32);
            button.GetComponent<Image>().color = dark;
            label = button.GetComponentInChildren<Text>();
        }
    }
}
