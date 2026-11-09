using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Tutorial sin texto del nivel 1: una flecha brillante sobre la línea y una mano fantasma que
    /// la arrastra (y las teclas Q/E en el ordenador). Desaparece en cuanto el jugador mueve la línea.
    /// </summary>
    public class TutorialHint : MonoBehaviour
    {
        const string SeenKey = "fase_tutorial_linea";

        RectTransform root, hand, arrow;
        CanvasGroup group;
        float startX, age, shown;
        bool done;

        void Start()
        {
            var hud = Hud.Instance;
            if (hud == null || hud.levelNumber != 1 || hud.Daily || ShiftLine.Instance == null) { Destroy(this); return; }
            startX = ShiftLine.X;
            var canvas = UiKit.Canvas("Tutorial", 7);
            canvas.transform.SetParent(transform, false);
            group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            root = UiKit.Rect(canvas.transform, "Pista", new Vector2(0f, 0f), Vector2.zero, new Vector2(10f, 10f));

            // Brillo redondo detrás de la flecha (sin 9-slice: un círculo, no un recuadro).
            var halo = UiKit.Box(root, "Halo", new Vector2(0.5f, 0.5f), new Vector2(80f, 60f), new Vector2(240f, 240f), new Color(1f, 0.92f, 0.7f, 0.16f));
            halo.type = Image.Type.Simple;
            arrow = UiKit.Text(root, "Flecha", "→", new Vector2(0.5f, 0.5f), new Vector2(80f, 60f), new Vector2(200f, 160f), 150, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.84f)).rectTransform;

            // Mano fantasma: palma y dedo.
            hand = UiKit.Rect(root, "Mano", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(90f, 130f));
            UiKit.Box(hand, "Palma", new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(80f, 80f), new Color(1f, 1f, 1f, 0.8f));
            UiKit.Box(hand, "Dedo", new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(30f, 70f), new Color(1f, 1f, 1f, 0.8f));

            // En el ordenador, además, las teclas.
            if (Gamepad.current != null)
            {
                string buttons = InputGlyphs.LineButtons.Replace("/", " + ");
                UiKit.Text(root, "Mando", Loc.Pick($"Línea · stick derecho / {buttons}", $"Line · right stick / {buttons}"), new Vector2(0.5f, 0.5f), new Vector2(140f, -165f), new Vector2(640f, 90f), 30);

            }
            else if (Touchscreen.current == null)
            {
                Key(root, Settings.KeyName(Settings.GetKey(Settings.Action.LineLeft)), new Vector2(110f, -165f));
                Key(root, Settings.KeyName(Settings.GetKey(Settings.Action.LineRight)), new Vector2(205f, -165f));
            }
        }

        static void Key(RectTransform parent, string label, Vector2 at)
        {
            var cap = UiKit.Box(parent, "Tecla", new Vector2(0.5f, 0.5f), at, new Vector2(80f, 80f), new Color(0.06f, 0.05f, 0.18f, 0.8f));
            UiKit.Text(cap.transform, "Letra", label, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f), 40);
        }

        void Update()
        {
            if (root == null) return;
            if (PauseMenu.Paused || PhotoMode.Active)
            {
                group.alpha = 0f;
                return;
            }
            age += Time.deltaTime;
            var line = ShiftLine.Instance;
            if (line == null) return;
            if (!done && Mathf.Abs(ShiftLine.X - startX) > 2.5f)
            {
                done = true;
                PlayerPrefs.SetInt(SeenKey, 1);
            }
            // Aparece al segundo de empezar (después del título) y se va al mover la línea.
            shown = Mathf.MoveTowards(shown, done ? 0f : age > 2.5f ? 1f : 0f, Time.unscaledDeltaTime * 2.5f);
            group.alpha = shown;
            if (done && shown <= 0f) { Destroy(group.gameObject); Destroy(this); return; }

            // Sigue a la línea en la pantalla.
            var canvasRect = (RectTransform)group.transform;
            root.anchoredPosition = new Vector2(line.ScreenX() * canvasRect.rect.width, canvasRect.rect.height * 0.55f);
            // La flecha late hacia la derecha; la mano agarra la línea y la arrastra, una y otra vez.
            float beat = Mathf.Repeat(age, 1.6f) / 1.6f;
            arrow.anchoredPosition = new Vector2(80f + Mathf.Sin(age * 5f) * 14f, 60f);
            hand.anchoredPosition = new Vector2(Mathf.SmoothStep(0f, 150f, Mathf.Clamp01((beat - 0.2f) / 0.6f)), -40f);
            hand.localScale = Vector3.one * (beat < 0.2f ? Mathf.Lerp(1.15f, 1f, beat / 0.2f) : 1f);
        }
    }
}
