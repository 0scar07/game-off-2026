using UnityEngine;
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
        public float grabDistance = 1.8f;
        [Tooltip("Posición inicial.")]
        public float x;

        [Header("Movimiento automático (menú y final)")]
        public bool autoSweep;
        public float sweepCenter, sweepAmplitude = 6f, sweepSpeed = 0.35f;

        static readonly int ShiftXId = Shader.PropertyToID("_ShiftX");
        static readonly int ShiftScreenXId = Shader.PropertyToID("_ShiftScreenX");
        static readonly int DayOnLeftId = Shader.PropertyToID("_DayOnLeft");

        Camera cam;
        int dragTouch = -1;
        bool draggingMouse;
        float grabOffset;
        bool wasHolding;
        float lastTickX;
        const float TickDistance = 0.7f;

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
            if (autoSweep)
            {
                // Menú y final: la línea se mueve sola, sin controles.
                x = Mathf.Clamp(sweepCenter + Mathf.Sin(Time.time * sweepSpeed) * sweepAmplitude, minX, maxX);
                return;
            }
            if (PauseMenu.Paused) return;
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

            // Avisos para el sonido: agarrar, soltar y un clic por cada tramo recorrido.
            bool holding = move != 0f || dragTouch >= 0 || draggingMouse;
            if (holding && !wasHolding) { GameEvents.RaiseLineGrabbed(); lastTickX = x; }
            if (!holding && wasHolding) GameEvents.RaiseLineReleased();
            wasHolding = holding;
            if (Mathf.Abs(x - lastTickX) >= TickDistance)
            {
                lastTickX = x;
                GameEvents.RaiseLineTick();
            }
        }

        // Después de que la cámara se mueva (CameraFollow usa LateUpdate con orden 0).
        void LateUpdate() => Apply();

        /// <summary>Posición de la línea en pantalla (0 = borde izquierdo, 1 = derecho).</summary>
        public float ScreenX()
        {
            if (cam == null) return 0.5f;
            return cam.WorldToViewportPoint(new Vector3(x, cam.transform.position.y, 0f)).x;
        }

        /// <summary>
        /// Arrastrar la línea. En el móvil vale cualquier dedo que empiece cerca de ella, aunque
        /// otro dedo esté pulsando los botones de moverse o saltar.
        /// </summary>
        void HandleDrag()
        {
            if (cam == null) return;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                bool anyTouch = false;
                foreach (var touch in touchscreen.touches)
                {
                    if (!touch.press.isPressed) continue;
                    anyTouch = true;
                    int id = touch.touchId.ReadValue();
                    Vector2 position = touch.position.ReadValue();
                    if (dragTouch < 0 && touch.press.wasPressedThisFrame && TryGrab(position)) dragTouch = id;
                    if (id == dragTouch) x = ToWorldX(position) - grabOffset;
                }
                if (dragTouch >= 0 && !TouchHeld(touchscreen, dragTouch)) dragTouch = -1;
                if (anyTouch) return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 screen = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame && TryGrab(screen)) draggingMouse = true;
            if (!mouse.leftButton.isPressed) draggingMouse = false;
            if (draggingMouse) x = ToWorldX(screen) - grabOffset;
        }

        bool TryGrab(Vector2 screen)
        {
            if (MobileButton.Contains(screen)) return false;
            float worldX = ToWorldX(screen);
            if (Mathf.Abs(worldX - x) > grabDistance) return false;
            grabOffset = worldX - x;
            return true;
        }

        static bool TouchHeld(Touchscreen touchscreen, int id)
        {
            foreach (var touch in touchscreen.touches)
                if (touch.press.isPressed && touch.touchId.ReadValue() == id) return true;
            return false;
        }

        /// <summary>X en el plano del juego (z = 0) bajo un punto de la pantalla.</summary>
        float ToWorldX(Vector2 screen) =>
            cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z)).x;

        void Apply()
        {
            Shader.SetGlobalFloat(ShiftXId, x);
            Shader.SetGlobalFloat(ShiftScreenXId, ScreenX());
            Shader.SetGlobalFloat(DayOnLeftId, dayOnLeft ? 1f : 0f);
        }
    }
}
