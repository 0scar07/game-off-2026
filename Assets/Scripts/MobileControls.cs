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
    }
}
