using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// Muestra los botones táctiles solo en móviles (también en el navegador del móvil),
    /// o en el PC si se activa `alwaysVisible` para probarlos.
    /// </summary>
    public class MobileControls : MonoBehaviour
    {
        public bool alwaysVisible;

        void Awake()
        {
            bool touch = Application.isMobilePlatform || Touchscreen.current != null && Keyboard.current == null;
            gameObject.SetActive(alwaysVisible || touch);
        }

        bool shown = true;

        // Con los resultados en pantalla se esconden: tapaban los botones de abajo (Compartir,
        // Código de fantasma, Repetición). Al ocultarse, cada botón se suelta solo (OnDisable).
        void Update()
        {
            bool show = !ResultsScreen.Showing;
            if (show == shown) return;
            shown = show;
            foreach (Transform child in transform) child.gameObject.SetActive(show);
        }
    }
}
