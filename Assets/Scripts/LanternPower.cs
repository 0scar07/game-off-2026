using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Poder del farol (se gana al terminar el capítulo II): con L, el botón Y del mando o el
    /// botón del farol en el móvil, Lumi abre una burbuja de noche a su alrededor durante unos
    /// segundos. Dentro de la burbuja es de noche aunque la línea diga otra cosa.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class LanternPower : MonoBehaviour
    {
        public float maxRadius = 3.2f, duration = 4f, cooldown = 10f;

        public static Vector2 Center { get; private set; }
        public static float Radius { get; private set; }

        public static bool Contains(Vector2 point) => Radius > 0.01f && (point - Center).sqrMagnitude < Radius * Radius;

        public static bool Unlocked => LevelCatalog.Get.ChapterDone(2);

        float active, wait;
        Image icon, fill;
        static bool touchPressed;

        void Start()
        {
            Radius = 0f;
            if (!Unlocked) { enabled = false; return; }
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("Farol", 6);
            canvas.transform.SetParent(transform, false);
            icon = UiKit.Box(canvas.transform, "Icono", new Vector2(1f, 0f), new Vector2(-60f, 300f), new Vector2(130f, 130f), new Color(0.12f, 0.1f, 0.3f, 0.75f));
            icon.raycastTarget = true;
            var button = icon.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => touchPressed = true);
            fill = UiKit.Box(icon.transform, "Carga", new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(100f, 0f), new Color(0.55f, 0.6f, 1f, 0.6f));
            fill.rectTransform.pivot = new Vector2(0.5f, 0f);
            UiKit.Text(icon.transform, "Simbolo", "☾", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130f, 130f), 64);
            UiKit.Text(icon.transform, "Tecla", "L", new Vector2(0.5f, 0f), new Vector2(0f, -22f), new Vector2(130f, 40f), 24, TextAnchor.MiddleCenter, UiKit.Soft);
        }

        void OnDisable() => Radius = 0f;

        void Update()
        {
            Center = (Vector2)transform.position + Vector2.up * 0.8f;
            var kb = Keyboard.current;
            var pad = Settings.PlayerGamepad;
            bool pressed = touchPressed || (kb != null && kb.lKey.wasPressedThisFrame && !UiKit.Typing) || (pad != null && pad.buttonNorth.wasPressedThisFrame);
            touchPressed = false;
            if (PauseMenu.Paused || PhotoMode.Active || UiKit.Typing) return;
            if (pressed && wait <= 0f && active <= 0f)
            {
                active = duration;
                wait = cooldown;
                GameEvents.RaiseLineGrabbed();
                Share.Vibrate(40);
            }
            if (active > 0f)
            {
                active -= Time.deltaTime;
                // Se abre rápido, aguanta y se cierra en el último segundo.
                float open = Mathf.Clamp01((duration - active) / 0.25f), close = Mathf.Clamp01(active / 0.8f);
                Radius = maxRadius * Mathf.Min(open, close);
            }
            else
            {
                Radius = 0f;
                wait -= Time.deltaTime;
            }
            if (fill != null) fill.rectTransform.sizeDelta = new Vector2(100f, 110f * (1f - Mathf.Clamp01(wait / cooldown)));
            if (icon != null) icon.color = wait <= 0f ? new Color(0.25f, 0.22f, 0.6f, 0.9f) : new Color(0.12f, 0.1f, 0.3f, 0.75f);
        }
    }
}
