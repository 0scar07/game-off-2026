using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Al terminar el último nivel de un capítulo: el cielo se oscurece, Lumi lanza su fragmento
    /// y encaja en su hueco de la luna. Sale antes de la pantalla de resultados.
    /// </summary>
    public static class ChapterCelebration
    {
        static readonly Vector2[] Slots = { new Vector2(-48f, 62f), new Vector2(66f, 12f), new Vector2(-22f, -64f) };
        static readonly float[] SlotAngles = { 115f, -20f, 250f };

        /// <summary>¿Es el último nivel (de la historia) de su capítulo?</summary>
        public static bool IsChapterEnd(int level)
        {
            var catalog = LevelCatalog.Get;
            var entry = catalog.At(level);
            if (entry == null || entry.secret) return false;
            var next = catalog.At(level + 1);
            return next == null || next.secret || next.chapter != entry.chapter;
        }

        public static IEnumerator Run(int level)
        {
            if (!IsChapterEnd(level)) yield break;
            int chapter = Mathf.Clamp(LevelCatalog.Get.At(level).chapter, 1, 3);
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("Capitulo completado", 41);
            var root = (RectTransform)canvas.transform;
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            var shade = UiKit.Fill(root, "Cielo").gameObject.AddComponent<Image>();
            shade.color = new Color(0.02f, 0.02f, 0.09f, 0.82f);

            // Luna con sus tres huecos; los de capítulos anteriores ya están llenos.
            var moon = UiKit.Rect(root, "Luna", new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(300f, 300f));
            var halo = UiKit.Box(moon, "Halo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 560f), new Color(1f, 0.93f, 0.75f, 0.12f));
            halo.type = Image.Type.Simple;
            var disc = UiKit.Box(moon, "Disco", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 300f), new Color(1f, 0.96f, 0.85f));
            disc.type = Image.Type.Simple;
            var slots = new Image[3];
            for (int i = 0; i < 3; i++) slots[i] = Shard(moon, Slots[i], SlotAngles[i], i < chapter - 1 ? Gold : Hole);

            UiKit.Text(root, "Titulo", Loc.Pick($"Capítulo {chapter} completado", $"Chapter {chapter} complete"), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1600f, 110f), 84);
            var subtitle = UiKit.Text(root, "Subtitulo", Loc.Pick(new[] { "La luna recupera su primer trozo", "La luna recupera su segundo trozo", "La luna vuelve a estar entera" }[chapter - 1],
                new[] { "The moon gets its first piece back", "The moon gets its second piece back", "The moon is whole again" }[chapter - 1]),
                new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(1400f, 60f), 40, TextAnchor.MiddleCenter, UiKit.Soft);
            var subGroup = subtitle.gameObject.AddComponent<CanvasGroup>();
            subGroup.alpha = 0f;

            for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime) { group.alpha = t / 0.5f; yield return null; }
            group.alpha = 1f;

            // El fragmento sale de Lumi (en pantalla) y vuela en curva hasta su hueco.
            Vector2 from = new Vector2(0f, -330f);
            var player = Object.FindAnyObjectByType<PlayerController>();
            var cam = Camera.main;
            if (player != null && cam != null)
            {
                var screen = cam.WorldToScreenPoint(player.transform.position + Vector3.up * 1.2f);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out var local)) from = local;
            }
            Vector2 to = moon.anchoredPosition + Slots[chapter - 1];
            var flying = (RectTransform)Shard(root, from, SlotAngles[chapter - 1] - 200f, Gold).transform.parent;
            var glow = UiKit.Box(flying, "Brillo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150f, 150f), new Color(1f, 0.85f, 0.4f, 0.35f));
            glow.type = Image.Type.Simple;
            Vector2 control = new Vector2((from.x + to.x) * 0.5f - 380f, (from.y + to.y) * 0.5f + 120f);
            float spark = 0f;
            for (float t = 0f; t < 1.7f; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 1.7f);
                Vector2 p = Vector2.Lerp(Vector2.Lerp(from, control, k), Vector2.Lerp(control, to, k), k);
                flying.anchoredPosition = p;
                flying.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(SlotAngles[chapter - 1] - 200f, SlotAngles[chapter - 1], k));
                flying.localScale = new Vector3(1f, 0.62f, 1f) * Mathf.Lerp(1.3f, 1f, k);
                spark -= Time.unscaledDeltaTime;
                if (spark <= 0f) { spark = 0.04f; Trail(root, p); }
                yield return null;
            }
            Object.Destroy(flying.gameObject);
            slots[chapter - 1].color = Gold;
            Share.Vibrate(40);
            // Destello de la luna al encajar.
            for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.6f;
                halo.color = new Color(1f, 0.93f, 0.75f, Mathf.Lerp(0.55f, 0.15f, k));
                halo.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(420f, 640f, k);
                subGroup.alpha = k;
                yield return null;
            }
            subGroup.alpha = 1f;

            bool next = false;
            var button = UiKit.Button(root, "Seguir", Loc.Pick("Seguir ▶", "Continue ▶"), new Vector2(1f, 0f), new Vector2(-60f, 60f), new Vector2(340f, 100f), () => next = true, 44);
            ((RectTransform)button.transform).pivot = new Vector2(1f, 0f);
            button.GetComponent<Image>().color = new Color(1f, 0.87f, 0.45f);
            button.GetComponentInChildren<Text>().color = new Color(0.25f, 0.15f, 0.02f);
            float wait = 0f;
            while (!next)
            {
                wait += Time.unscaledDeltaTime;
                halo.color = new Color(1f, 0.93f, 0.75f, 0.15f + 0.05f * Mathf.Sin(wait * 3f));
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                if (wait > 0.6f && ((kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) || (pad != null && pad.buttonSouth.wasPressedThisFrame))) next = true;
                if (wait > 12f) next = true;
                yield return null;
            }
            for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime) { group.alpha = 1f - t / 0.35f; yield return null; }
            Object.Destroy(canvas.gameObject);
        }

        static readonly Color Gold = new Color(1f, 0.84f, 0.4f);
        static readonly Color Hole = new Color(0.17f, 0.15f, 0.36f);

        /// <summary>Trozo de luna: un rombo alargado (cuadrado girado 45° dentro de un padre aplastado).</summary>
        static Image Shard(Transform parent, Vector2 position, float angle, Color color)
        {
            var holder = UiKit.Rect(parent, "Trozo", new Vector2(0.5f, 0.5f), position, new Vector2(100f, 100f));
            holder.localRotation = Quaternion.Euler(0f, 0f, angle);
            holder.localScale = new Vector3(1f, 0.62f, 1f);
            var image = UiKit.Box(holder, "Cristal", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f), color, false);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            return image;
        }

        static void Trail(Transform parent, Vector2 at)
        {
            var dot = UiKit.Box(parent, "Chispa", new Vector2(0.5f, 0.5f), at + Random.insideUnitCircle * 14f, Vector2.one * Random.Range(8f, 16f), new Color(1f, 0.9f, 0.55f, 0.9f));
            dot.type = Image.Type.Simple;
            dot.gameObject.AddComponent<UiFadeOut>();
        }
    }
}
