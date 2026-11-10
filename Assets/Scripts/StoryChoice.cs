using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Decisiones de la historia: al terminar el capítulo II, Lumi elige a quién ayudar (al día o a
    /// la noche) y el final lo recuerda. Y el final secreto, si se reúne toda la luna y todas las
    /// páginas del diario.
    /// </summary>
    public static class StoryChoice
    {
        const string Key = "fase_decision";

        /// <summary>0 sin elegir, 1 el día, 2 la noche.</summary>
        public static int Decision => PlayerPrefs.GetInt(Key, 0);

        public static string EndingLine => Decision switch
        {
            1 => Loc.Pick("Ayudaste al día: el sol salió un poco antes ese año.", "You helped the day: the sun rose a little earlier that year."),
            2 => Loc.Pick("Ayudaste a la noche: las luciérnagas no olvidaron tu nombre.", "You helped the night: the fireflies never forgot your name."),
            _ => "",
        };

        public static IEnumerator Ask()
        {
            if (Decision != 0) yield break;
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("Decision", 43);
            var shade = UiKit.Fill(canvas.transform, "Fondo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.02f, 0.02f, 0.08f, 0.88f);
            UiKit.Text(canvas.transform, "Pregunta", Loc.Pick("En el bosque, el día y la noche discuten.\n¿A quién ayudas?", "In the forest, day and night are arguing.\nWho do you help?"), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1500f, 200f), 56);
            int chosen = 0;
            var day = UiKit.Button(canvas.transform, "Dia", Loc.Pick("☀ Al día", "☀ The day"), new Vector2(0.5f, 0.5f), new Vector2(-260f, -80f), new Vector2(440f, 130f), () => chosen = 1, 48);
            day.GetComponent<Image>().color = new Color(1f, 0.85f, 0.45f);
            day.GetComponentInChildren<Text>().color = new Color(0.25f, 0.15f, 0.02f);
            var night = UiKit.Button(canvas.transform, "Noche", Loc.Pick("☾ A la noche", "☾ The night"), new Vector2(0.5f, 0.5f), new Vector2(260f, -80f), new Vector2(440f, 130f), () => chosen = 2, 48);
            night.GetComponent<Image>().color = new Color(0.35f, 0.45f, 0.95f);
            while (chosen == 0) yield return null;
            PlayerPrefs.SetInt(Key, chosen);
            PlayerPrefs.Save();
            Object.Destroy(canvas.gameObject);
        }

        /// <summary>¿Se ha ganado el final secreto? (toda la luna y todo el diario)</summary>
        public static bool SecretEarned => LevelCatalog.Get.AllFragments(0) && Diary.Found >= Diary.Pages.Length;

        public static IEnumerator SecretEnding()
        {
            if (!SecretEarned) yield break;
            Achievements.Unlock("finalsecreto");
            var canvas = UiKit.Canvas("FinalSecreto", 44);
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            var shade = UiKit.Fill(canvas.transform, "Fondo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.03f, 0.02f, 0.1f, 0.9f);
            var moon = UiKit.Box(canvas.transform, "Luna", new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(280f, 280f), new Color(1f, 0.97f, 0.88f));
            moon.type = Image.Type.Simple;
            var halo = UiKit.Box(canvas.transform, "Halo", new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(560f, 560f), new Color(1f, 0.95f, 0.75f, 0.2f));
            halo.type = Image.Type.Simple;
            halo.transform.SetSiblingIndex(moon.transform.GetSiblingIndex());
            UiKit.Text(canvas.transform, "Titulo", Loc.Pick("Final secreto", "Secret ending"), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1400f, 90f), 70, TextAnchor.MiddleCenter, UiKit.Gold);
            UiKit.Text(canvas.transform, "Texto", Loc.Pick("Con cada trozo de luna y cada página del diario, Lumi entendió\nque el día y la noche nunca estuvieron peleados: se estaban buscando.",
                "With every piece of moon and every diary page, Lumi understood\nthat day and night had never been fighting: they were looking for each other."),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(1500f, 140f), 36, TextAnchor.MiddleCenter, UiKit.Soft);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime) { group.alpha = t; yield return null; }
            for (float t = 0f; t < 6f; t += Time.unscaledDeltaTime) { halo.rectTransform.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(t * 2f)); yield return null; }
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime) { group.alpha = 1f - t; yield return null; }
            Object.Destroy(canvas.gameObject);
        }
    }
}
