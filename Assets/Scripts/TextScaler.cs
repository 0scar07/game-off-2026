using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Aplica el tamaño de texto de Accesibilidad a un texto de UiKit. Los textos pequeños crecen
    /// del todo; los títulos grandes, poco (ya se leen bien y así no se salen de su sitio).
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextScaler : MonoBehaviour
    {
        Text label;
        int baseSize;

        public void Init(Text text)
        {
            label = text;
            baseSize = text.fontSize;
            Apply();
        }

        void OnEnable() { Settings.Changed += Apply; Apply(); }
        void OnDisable() => Settings.Changed -= Apply;

        /// <summary>Cambia el tamaño de base (si el código pone otro fontSize después de crearlo).</summary>
        public void SetBase(int size) { baseSize = size; Apply(); }

        void Apply()
        {
            if (label == null || baseSize <= 0) return;
            float extra = Settings.TextScale - 1f;
            float k = baseSize <= 40 ? 1f : baseSize <= 60 ? 0.5f : 0.2f;
            label.fontSize = Mathf.RoundToInt(baseSize * (1f + extra * k));
        }
    }
}
