using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Energía de la línea (solo en algunos niveles): cada unidad que se mueve gasta energía y,
    /// sin energía, la línea se queda quieta. Los puntos de control la rellenan.
    /// </summary>
    public class LineEnergy : MonoBehaviour
    {
        public static LineEnergy Instance { get; private set; }

        public float max = 30f;
        public float Left { get; private set; }
        public bool Empty => Left <= 0.01f;

        Image fill;
        Text label;
        float lastX, lastX2;

        void Awake()
        {
            Instance = this;
            Left = max;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void OnEnable() => GameEvents.CheckpointReached += Refill;
        void OnDisable() => GameEvents.CheckpointReached -= Refill;

        void Start()
        {
            if (ShiftLine.Instance != null) { lastX = ShiftLine.Instance.x; lastX2 = ShiftLine.Instance.x2; }
            var canvas = UiKit.Canvas("Energia", 5);
            canvas.transform.SetParent(transform, false);
            var back = UiKit.Box(canvas.transform, "Barra", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(520f, 36f), new Color(0.04f, 0.03f, 0.14f, 0.6f));
            fill = UiKit.Box(back.transform, "Relleno", new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(508f, 24f), new Color(1f, 0.85f, 0.5f));
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            label = UiKit.Text(back.transform, "Texto", Loc.Get("energy"), new Vector2(0.5f, 0f), new Vector2(0f, -30f), new Vector2(520f, 36f), 26, TextAnchor.MiddleCenter, UiKit.Soft);
        }

        void Refill() => Left = max;

        /// <summary>Lo que la línea puede moverse desde `from` hacia `to` con la energía que queda.</summary>
        public float Limit(float from, float to)
        {
            float wanted = to - from;
            float allowed = Mathf.Min(Mathf.Abs(wanted), Left);
            return from + Mathf.Sign(wanted) * allowed;
        }

        void LateUpdate()
        {
            var line = ShiftLine.Instance;
            if (line == null) return;
            float moved = Mathf.Abs(line.x - lastX) + Mathf.Abs(line.x2 - lastX2);
            lastX = line.x;
            lastX2 = line.x2;
            Left = Mathf.Max(0f, Left - moved);
            if (fill != null)
            {
                fill.rectTransform.sizeDelta = new Vector2(508f * Left / max, 24f);
                fill.color = Empty ? new Color(1f, 0.35f, 0.35f) : Color.Lerp(new Color(1f, 0.5f, 0.3f), new Color(1f, 0.85f, 0.5f), Left / max);
            }
            if (label != null) label.text = Loc.Get("energy");
        }
    }
}
