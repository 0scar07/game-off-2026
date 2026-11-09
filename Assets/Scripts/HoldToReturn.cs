using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Mantener R (o el botón Select del mando) para volver al último punto de control: un círculo
    /// que se llena arriba a la derecha. No cuenta como caída. Reiniciar el nivel entero sigue en
    /// el menú de pausa (antes R lo hacía al instante y era fácil pulsarla sin querer).
    /// </summary>
    public class HoldToReturn : MonoBehaviour
    {
        const float Hold = 0.7f;

        PlayerController player;
        RectTransform ring;
        Image fill;
        Text hint;
        float held;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            var canvas = UiKit.Canvas("MantenerR", 25);
            canvas.transform.SetParent(transform, false);
            ring = UiKit.Rect(canvas.transform, "Anillo", new Vector2(1f, 1f), new Vector2(-150f, -230f), new Vector2(110f, 110f));
            var back = UiKit.Box(ring, "Fondo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f), new Color(0f, 0f, 0.1f, 0.55f));
            back.type = Image.Type.Simple;
            fill = UiKit.Box(ring, "Lleno", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f), new Color(1f, 0.87f, 0.45f, 0.9f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillClockwise = true;
            var center = UiKit.Box(ring, "Centro", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f), new Color(0.06f, 0.05f, 0.18f, 0.95f));
            center.type = Image.Type.Simple;
            UiKit.Text(ring, "R", "R", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f), 44);
            hint = UiKit.Text(ring, "Texto", "", new Vector2(0.5f, 0f), new Vector2(0f, -30f), new Vector2(360f, 40f), 22, TextAnchor.MiddleCenter, UiKit.Soft);
            ring.gameObject.SetActive(false);
        }

        void Update()
        {
            if (player == null) return;
            var kb = Keyboard.current;
            var pad = Settings.PlayerGamepad;
            bool busy = PauseMenu.Paused || PhotoMode.Active || UiKit.Typing || FindAnyObjectByType<ResultsScreen>() != null;
            bool pressing = !busy && ((kb != null && kb.rKey.isPressed) || (pad != null && pad.selectButton.isPressed));
            held = pressing ? held + Time.unscaledDeltaTime : 0f;
            ring.gameObject.SetActive(held > 0.05f);
            if (!ring.gameObject.activeSelf) return;
            fill.fillAmount = Mathf.Clamp01(held / Hold);
            hint.text = Loc.Pick("al punto de control", "to checkpoint");
            if (held >= Hold)
            {
                player.ReturnToCheckpoint();
                held = -10f;   // hasta que se suelte
            }
        }
    }
}
