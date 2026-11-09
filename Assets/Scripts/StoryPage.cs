using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Narrador de cuento: una frase al empezar cada capítulo (la usa la tarjeta de capítulo) y, al
    /// terminar uno, una página de cuento con dibujo (el plano del primer nivel del siguiente) y
    /// texto antes de seguir.
    /// </summary>
    public static class StoryPage
    {
        /// <summary>Lo que dice el narrador al empezar el capítulo (1-3).</summary>
        public static string Opening(int chapter) => chapter switch
        {
            2 => Loc.Pick("“Las hojas recordaban el verano, y la noche les susurraba que todo cambia.”", "“The leaves remembered summer, and the night whispered that everything changes.”"),
            3 => Loc.Pick("“Arriba, donde la nieve no se derrite, la luna esperaba su último trozo.”", "“Up where the snow never melts, the moon was waiting for its last piece.”"),
            _ => Loc.Pick("“Había una vez una luna partida en dos, y una línea que separaba el día de la noche.”", "“Once upon a time there was a moon split in two, and a line between day and night.”"),
        };

        static (string titleEs, string titleEn, string es, string en) Page(int finished) => finished == 1
            ? ("Lumi dejó atrás la pradera.", "Lumi left the meadow behind.",
               "Los árboles se volvieron rojos\ny el viento olía a otoño.\n\nEn algún lugar del bosque\nbrillaba otro trozo de luna.",
               "The trees turned red\nand the wind smelled of autumn.\n\nSomewhere in the forest\nanother piece of moon was shining.")
            : ("Lumi dejó atrás el bosque.", "Lumi left the forest behind.",
               "Arriba, en la Cumbre Helada,\nel último trozo de luna\nesperaba bajo la nieve.\n\nHacía frío. Mucho frío.",
               "Up on the Frozen Summit,\nthe last piece of moon\nwas waiting under the snow.\n\nIt was cold. Very cold.");

        /// <summary>Página de cuento tras terminar el capítulo `finished` (no hay tras el último).</summary>
        public static IEnumerator Run(int finished)
        {
            if (finished < 1 || finished > 2) yield break;
            var (titleEs, titleEn, es, en) = Page(finished);
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("Cuento", 42);
            var root = canvas.transform;
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            var dark = UiKit.Fill(root, "Fondo").gameObject.AddComponent<Image>();
            dark.color = new Color(0.16f, 0.11f, 0.08f, 0.96f);
            var book = UiKit.Box(root, "Libro", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1560f, 860f), new Color(0.95f, 0.9f, 0.78f));
            UiKit.Box(book.transform, "Lomo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 860f), new Color(0.47f, 0.35f, 0.24f, 0.4f), false);
            // Dibujo: el plano del primer nivel del capítulo que viene, en tonos de cuento.
            var picture = UiKit.Rect(book.transform, "Dibujo", new Vector2(0.25f, 0.5f), new Vector2(0f, 30f), new Vector2(640f, 560f));
            picture.gameObject.AddComponent<RectMask2D>();
            var sky = UiKit.Box(picture, "Cielo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 560f), finished == 1 ? new Color(1f, 0.75f, 0.5f) : new Color(0.75f, 0.85f, 1f), false);
            var catalog = LevelCatalog.Get;
            for (int i = 0; i < catalog.levels.Length; i++)
            {
                if (catalog.levels[i].secret || catalog.levels[i].chapter != finished + 1) continue;
                var tex = Resources.Load<Texture2D>("Vistas/" + catalog.levels[i].scene);
                if (tex == null) break;
                var plan = UiKit.Rect(picture, "Plano", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 560f)).gameObject.AddComponent<RawImage>();
                plan.texture = tex;
                plan.uvRect = new Rect(0f, 0f, Mathf.Min(1f, 640f / 560f * tex.height / tex.width), 1f);
                break;
            }
            UiKit.Box(picture, "Papel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 560f), new Color(0.85f, 0.7f, 0.5f, 0.25f), false);
            var ink = new Color(0.3f, 0.2f, 0.12f);
            var title = UiKit.Text(book.transform, "Titulo", Loc.Pick(titleEs, titleEn), new Vector2(0.75f, 0.5f), new Vector2(0f, 230f), new Vector2(700f, 70f), 48, TextAnchor.MiddleCenter, ink);
            Object.Destroy(title.GetComponent<Shadow>());
            var body = UiKit.Text(book.transform, "Texto", Loc.Pick(es, en), new Vector2(0.75f, 0.5f), new Vector2(0f, -20f), new Vector2(700f, 400f), 36, TextAnchor.MiddleCenter, new Color(0.38f, 0.27f, 0.18f));
            Object.Destroy(body.GetComponent<Shadow>());
            var page = UiKit.Text(book.transform, "Pagina", $"— {finished + 1} —", new Vector2(0.25f, 0f), new Vector2(0f, 70f), new Vector2(200f, 40f), 26, TextAnchor.MiddleCenter, new Color(0.55f, 0.42f, 0.3f));
            Object.Destroy(page.GetComponent<Shadow>());
            bool next = false;
            var button = UiKit.Button(book.transform, "Seguir", Loc.Pick("Seguir ▶", "Continue ▶"), new Vector2(1f, 0f), new Vector2(-60f, 50f), new Vector2(300f, 90f), () => next = true, 40);
            ((RectTransform)button.transform).pivot = new Vector2(1f, 0f);
            button.GetComponent<Image>().color = new Color(0.55f, 0.36f, 0.17f);
            for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime) { group.alpha = t / 0.5f; yield return null; }
            group.alpha = 1f;
            float wait = 0f;
            while (!next)
            {
                wait += Time.unscaledDeltaTime;
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                if (wait > 0.8f && ((kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) || (pad != null && pad.buttonSouth.wasPressedThisFrame))) next = true;
                yield return null;
            }
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime) { group.alpha = 1f - t / 0.4f; yield return null; }
            Object.Destroy(canvas.gameObject);
        }
    }
}
