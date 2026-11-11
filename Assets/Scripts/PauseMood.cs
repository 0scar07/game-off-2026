using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Al pausar, el mundo se queda en gris y solo la línea conserva su luz (se vuelve a dibujar
    /// encima, en la interfaz). Arriba a la derecha, un reloj con lo que llevas jugando hoy; si
    /// pasa de una hora, sugiere un descanso.
    /// </summary>
    [RequireComponent(typeof(PauseMenu))]
    public class PauseMood : MonoBehaviour
    {
        RectTransform canvas, line;
        Text today, rest;
        bool wasPaused;

        void Start()
        {
            var pause = GetComponent<PauseMenu>();
            if (pause.panel == null) { enabled = false; return; }
            canvas = (RectTransform)pause.panel.transform;
            // El fondo de la pausa, más claro: se ve el mundo en gris detrás.
            var dim = pause.panel.GetComponent<Image>();
            if (dim != null) dim.color = new Color(0f, 0f, 0.05f, 0.45f);
            // La línea, primera hija del panel: encima del fondo y debajo de los botones y el mapa.
            line = UiKit.Rect(canvas, "Linea", new Vector2(0f, 0f), Vector2.zero, new Vector2(10f, 3000f));
            line.SetSiblingIndex(0);
            UiKit.Box(line, "Halo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90f, 3000f), new Color(1f, 0.92f, 0.7f, 0.25f), false);
            UiKit.Box(line, "Nucleo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 3000f), Color.white, false);
            var clock = UiKit.Rect(canvas, "Reloj", new Vector2(1f, 1f), new Vector2(-330f, -90f), new Vector2(300f, 140f));
            UiKit.Text(clock, "Icono", "⏱", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(120f, 70f), 60, TextAnchor.MiddleCenter, UiKit.Gold);
            today = UiKit.Text(clock, "Hoy", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(300f, 44f), 32);
            rest = UiKit.Text(clock, "Descanso", "", new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(300f, 32f), 22, TextAnchor.MiddleCenter, UiKit.Soft);
            // En el móvil: editar dónde están los botones táctiles y su tamaño.
            if (FindObjectsByType<MobileButton>().Length > 0)
                UiKit.Button(canvas, "EditarTactiles", Loc.Pick("Editar controles táctiles", "Edit touch controls"), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(560f, 90f), () => TouchEditor.Open(pause.panel), 32);
            // Los botones de la pausa también suenan y se inclinan.
            UiHover.AddTo(transform);

        }

        void Update()
        {
            bool paused = PauseMenu.Paused && !PhotoMode.Active;
            if (paused != wasPaused)
            {
                wasPaused = paused;
                Brightness.SetGray(paused);
            }
            if (!paused) return;
            var shift = ShiftLine.Instance;
            var cam = Camera.main;
            if (shift != null && cam != null)
            {
                var screen = cam.WorldToScreenPoint(shift.PointOnLine(shift.x));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out var local);
                line.anchoredPosition = local + canvas.rect.size * 0.5f;
                line.localRotation = Quaternion.Euler(0f, 0f, shift.angle);
            }
            int minutes = PlayToday.Minutes;
            today.text = Loc.Pick($"Hoy: {minutes} min", $"Today: {minutes} min", $"Hoje: {minutes} min", $"Aujourd'hui : {minutes} min");
            rest.text = minutes >= 60 ? Loc.Pick("¿Un descanso?", "Time for a break?") : "";
        }

        void OnDestroy() => Brightness.SetGray(false);
    }
}
