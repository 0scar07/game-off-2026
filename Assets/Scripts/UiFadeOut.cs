using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>Chispa de interfaz: se apaga, se encoge y se borra sola.</summary>
    [RequireComponent(typeof(Image))]
    public class UiFadeOut : MonoBehaviour
    {
        public float duration = 0.6f;
        float life = -1f;
        Image image;
        float alpha;

        void Awake() { image = GetComponent<Image>(); alpha = image.color.a; }

        void Update()
        {
            if (life < 0f) life = duration;
            life -= Time.unscaledDeltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            float k = life / duration;
            var c = image.color;
            c.a = alpha * k;
            image.color = c;
            transform.localScale = Vector3.one * k;
        }
    }
}
