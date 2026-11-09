using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>Texto que sube en diagonal y se apaga (las Z de Lumi dormida).</summary>
    public class FloatAway : MonoBehaviour
    {
        float life;
        Text text;
        RectTransform rect;

        void Awake() { text = GetComponent<Text>(); rect = (RectTransform)transform; }

        void Update()
        {
            life += Time.deltaTime;
            rect.anchoredPosition += new Vector2(30f, 70f) * Time.deltaTime;
            var c = text.color;
            c.a = Mathf.Clamp01(1f - life / 2f) * Mathf.Clamp01(life * 4f);
            text.color = c;
            if (life > 2f) Destroy(gameObject);
        }
    }
}
