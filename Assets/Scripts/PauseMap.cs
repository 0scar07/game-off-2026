using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Mapa del nivel en la pausa: el suelo visto desde lejos, dónde está Lumi y qué fragmentos
    /// de luna faltan. Se dibuja una vez con las colisiones del nivel y se actualiza al pausar.
    /// </summary>
    [RequireComponent(typeof(PauseMenu))]
    public class PauseMap : MonoBehaviour
    {
        const float MapW = 760f, MapH = 300f;

        RectTransform map, lumiDot;
        Text missing;
        Rect world;
        readonly List<(Transform target, Text dot)> fragments = new List<(Transform, Text)>();
        Transform player;
        bool built;

        void Update()
        {
            var pause = GetComponent<PauseMenu>();
            if (pause.panel == null || !pause.panel.activeSelf) return;
            if (!built) Build(pause.panel.transform);
            Refresh();
        }

        void Build(Transform panel)
        {
            built = true;
            // Los botones a la izquierda; el mapa a la derecha.
            var column = panel.Find("Botones") as RectTransform;
            if (column != null) column.anchoredPosition = new Vector2(-430f, 0f);
            UiKit.Button(panel, "Foto", Loc.Pick("Modo foto (F)", "Photo mode (F)"), new Vector2(0.5f, 0.5f), new Vector2(330f, -190f), new Vector2(420f, 90f), PhotoMode.Open, 36);

            var colliders = new List<BoxCollider2D>();
            foreach (var box in FindObjectsByType<BoxCollider2D>())
                if (!box.isTrigger && box.attachedRigidbody == null) colliders.Add(box);
            if (colliders.Count == 0) return;
            var b = colliders[0].bounds;
            foreach (var c in colliders) b.Encapsulate(c.GetComponent<WorldPiece>() != null ? FullBounds(c) : c.bounds);
            world = new Rect(b.min.x, b.min.y, Mathf.Max(1f, b.size.x), Mathf.Max(1f, b.size.y + 3f));

            var back = UiKit.Box(panel, "Mapa", new Vector2(0.5f, 0.5f), new Vector2(330f, 80f), new Vector2(MapW + 40f, MapH + 40f), new Color(1f, 1f, 1f, 0.08f));
            map = UiKit.Rect(back.transform, "Dentro", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(MapW, MapH));
            foreach (var c in colliders)
            {
                var piece = c.GetComponent<WorldPiece>();
                var color = piece == null ? new Color(0.45f, 0.4f, 0.65f) : piece.world == World.Day ? new Color(0.85f, 0.6f, 0.35f) : new Color(0.55f, 0.75f, 1f);
                var cb = piece != null ? FullBounds(c) : c.bounds;
                var r = UiKit.Box(map, "Pieza", new Vector2(0f, 0f), ToMap(cb.min), Vector2.Scale(cb.size, Scale), color, false).rectTransform;
                r.pivot = Vector2.zero;
            }
            foreach (var f in FindObjectsByType<Collectible>()) AddFragment(f.transform);
            foreach (var chest in FindObjectsByType<ChestPickup>()) AddFragment(chest.transform);
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
            lumiDot = UiKit.Box(map, "Lumi", new Vector2(0f, 0f), Vector2.zero, new Vector2(18f, 18f), Color.white).rectTransform;
            missing = UiKit.Text(panel, "Faltan", "", new Vector2(0.5f, 0.5f), new Vector2(330f, -110f), new Vector2(760f, 40f), 30, TextAnchor.MiddleCenter, UiKit.Gold);
        }

        /// <summary>Tamaño completo de una pieza de un mundo (su colisión puede estar recortada).</summary>
        static Bounds FullBounds(BoxCollider2D box)
        {
            var renderers = box.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return box.bounds;
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        void AddFragment(Transform target)
        {
            var dot = UiKit.Text(map, "Fragmento", "★", new Vector2(0f, 0f), ToMap(target.position), new Vector2(30f, 30f), 28, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f));
            fragments.Add((target, dot));
        }

        Vector2 Scale => new Vector2(MapW / world.width, MapH / world.height);
        Vector2 ToMap(Vector2 p) => Vector2.Scale(p - world.min, Scale);

        void Refresh()
        {
            if (map == null) return;
            if (player != null) lumiDot.anchoredPosition = ToMap(player.position);
            int left = 0;
            foreach (var (target, dot) in fragments)
            {
                var chest = target != null ? target.GetComponent<ChestPickup>() : null;
                bool taken = target == null || (chest != null && chest.Opened);
                dot.color = taken ? new Color(0.5f, 0.5f, 0.6f, 0.5f) : new Color(1f, 0.85f, 0.35f);
                if (!taken) left++;
            }
            missing.text = left == 0 ? Loc.Pick("¡Tienes toda la luna!", "You have the whole moon!")
                : Loc.English ? $"{left} fragment{(left > 1 ? "s" : "")} left" : $"Falta{(left > 1 ? "n" : "")} {left} fragmento{(left > 1 ? "s" : "")}";
        }
    }
}
