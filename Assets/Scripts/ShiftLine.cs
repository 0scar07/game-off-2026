using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// La línea que parte el mundo: a la izquierda es de día y a la derecha de noche
    /// (o al revés si dayOnLeft = false). Se mueve con Shift izq./der. (o Q/E) y
    /// arrastrándola con el ratón o el dedo.
    /// Los demás scripts leen ShiftLine.X y ShiftLine.IsDay(x).
    /// Los shaders reciben la posición en pantalla (0..1) en la variable global _ShiftScreenX:
    /// el corte es vertical en la pantalla, igual para lo que está cerca y para el fondo.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ShiftLine : MonoBehaviour
    {
        public static ShiftLine Instance { get; private set; }

        /// <summary>Posición X de la línea en el mundo.</summary>
        public static float X => Instance != null ? Instance.x : 0f;

        /// <summary>true si el punto x está en el lado de día.</summary>
        public static bool IsDay(float worldX)
        {
            if (Instance == null) return true;
            return Instance.dayOnLeft ? worldX < Instance.x : worldX >= Instance.x;
        }

        [Tooltip("El día queda a la izquierda de la línea.")]
        public bool dayOnLeft = true;
        [Tooltip("Velocidad de la línea con el teclado (unidades por segundo).")]
        public float keySpeed = 9f;
        [Tooltip("Límites de la línea en este nivel.")]
        public float minX = -20f, maxX = 20f;
        [Tooltip("Distancia (en unidades) a la que se puede agarrar la línea con el dedo o el ratón.")]
        public float grabDistance = 1.2f;
        [Tooltip("Posición inicial.")]
        public float x;

        static readonly int ShiftXId = Shader.PropertyToID("_ShiftX");
        static readonly int ShiftScreenXId = Shader.PropertyToID("_ShiftScreenX");
        static readonly int DayOnLeftId = Shader.PropertyToID("_DayOnLeft");

        Camera cam;
        bool dragging;

        void Awake()
        {
            Instance = this;
            cam = Camera.main;
            Apply();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float move = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftShiftKey.isPressed || kb.qKey.isPressed) move -= 1f;
                if (kb.rightShiftKey.isPressed || kb.eKey.isPressed) move += 1f;
            }
            if (move != 0f) x += move * keySpeed * Time.deltaTime;

            HandleDrag();
            x = Mathf.Clamp(x, minX, maxX);
        }

        // Después de que la cámara se mueva (CameraFollow usa LateUpdate con orden 0).
        void LateUpdate() => Apply();

        /// <summary>Posición de la línea en pantalla (0 = borde izquierdo, 1 = derecho).</summary>
        public float ScreenX()
        {
            if (cam == null) return 0.5f;
            return cam.WorldToViewportPoint(new Vector3(x, cam.transform.position.y, 0f)).x;
        }

        /// <summary>Arrastrar la línea. En el móvil vale cualquier dedo que empiece cerca de ella.</summary>
        void HandleDrag()
        {
            var pointer = Pointer.current;
            if (pointer == null || cam == null) return;

            Vector2 screen = pointer.position.ReadValue();
            float worldX = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z)).x;

            if (pointer.press.wasPressedThisFrame && Mathf.Abs(worldX - x) <= grabDistance && !OverUi())
                dragging = true;
            if (!pointer.press.isPressed)
                dragging = false;
            if (dragging)
                x = worldX;
        }

        /// <summary>Si se toca un botón de la interfaz (saltar, pausa...), no se agarra la línea.</summary>
        static bool OverUi() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        void Apply()
        {
            Shader.SetGlobalFloat(ShiftXId, x);
            Shader.SetGlobalFloat(ShiftScreenXId, ScreenX());
            Shader.SetGlobalFloat(DayOnLeftId, dayOnLeft ? 1f : 0f);
        }
    }
}
