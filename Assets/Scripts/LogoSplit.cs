using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Logo FASE partido: encima del texto de "día" hay una copia de "noche" dentro de una máscara
    /// que empieza donde está la línea del fondo, así que al moverse la línea las letras cambian
    /// de color. Al empezar, el logo cae con un pequeño rebote.
    /// </summary>
    public class LogoSplit : MonoBehaviour
    {
        public RectMask2D nightMask;

        RectTransform rect;
        float age;

        void Start() => rect = (RectTransform)transform;

        void LateUpdate()
        {
            age += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(age / 0.9f);
            float bounce = 1f - Mathf.Pow(1f - t, 3f) * Mathf.Cos(t * Mathf.PI * 2.5f);
            rect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, bounce);

            if (ShiftLine.Instance == null || nightMask == null) return;
            float screenX = ShiftLine.Instance.ScreenX() * Screen.width;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, new Vector2(screenX, Screen.height * 0.5f), null, out var local);
            float left = Mathf.Clamp(local.x - rect.rect.xMin, 0f, rect.rect.width);
            nightMask.padding = new Vector4(left, 0f, 0f, 0f);
        }
    }
}
