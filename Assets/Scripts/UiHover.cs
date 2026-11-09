using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Botón vivo: al pasar el ratón suena un "tin", crece un poco y se inclina hacia el puntero.
    /// UiKit lo pone en todos sus botones; MenuExtras y la pausa, en los que monta el generador.
    /// </summary>
    public class UiHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        bool over;
        float tilt, grow;
        RectTransform rect;
        Selectable selectable;

        void Awake()
        {
            rect = (RectTransform)transform;
            selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData data)
        {
            over = true;
            if (selectable == null || selectable.interactable) Sfx.Play("boton", 0.45f, Random.Range(0.96f, 1.08f));
        }

        public void OnPointerExit(PointerEventData data) => over = false;

        // Solo toca el tamaño y el giro mientras anima: otros scripts (los nodos del mapa) también lo usan.
        bool touched;

        void OnDisable()
        {
            over = false;
            tilt = grow = 0f;
            if (touched && rect != null) { rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one; }
            touched = false;
        }

        void Update()
        {
            float targetTilt = 0f;
            if (over && Pointer.current != null && rect.rect.width > 1f)
            {
                // Hacia el puntero: si está a la derecha del centro, el botón "mira" a la derecha.
                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Pointer.current.position.ReadValue(), null, out local);
                targetTilt = -Mathf.Clamp(local.x / (rect.rect.width * 0.5f), -1f, 1f) * 3f;
            }
            tilt = Mathf.Lerp(tilt, targetTilt, Time.unscaledDeltaTime * 12f);
            grow = Mathf.Lerp(grow, over ? 1f : 0f, Time.unscaledDeltaTime * 14f);
            if (!over && grow < 0.002f && Mathf.Abs(tilt) < 0.02f)
            {
                if (touched) { rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one; touched = false; }
                return;
            }
            touched = true;
            rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
            rect.localScale = Vector3.one * (1f + 0.04f * grow);

        }

        /// <summary>Lo añade a todos los botones bajo `root` que aún no lo tengan.</summary>
        public static void AddTo(Transform root)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
                if (button.GetComponent<UiHover>() == null) button.gameObject.AddComponent<UiHover>();
        }
    }
}
