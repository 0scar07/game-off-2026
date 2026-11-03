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
    ///
    /// Algunos niveles tienen una segunda línea (hasSecond): entre las dos hay una franja del
    /// otro mundo (día | noche | día). El teclado mueve la línea más cercana a Lumi y el dedo o
    /// el ratón la que agarren. Con energía (LineEnergy) moverlas gasta.
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
            var line = Instance;
            int crossed = (worldX >= line.x ? 1 : 0) + (line.hasSecond && worldX >= line.x2 ? 1 : 0);
            // Cada línea cruzada cambia de mundo.
            return line.dayOnLeft == (crossed % 2 == 0);
        }

        /// <summary>Posiciones de las líneas (una o dos), de izquierda a derecha.</summary>
        /// (Siempre el mismo arreglo: se lee en cada paso de física de cada pieza, sin crear basura.)
        public static float[] Lines
        {
            get
            {
                if (Instance == null) return none;
                var line = Instance;
                if (!line.hasSecond) { one[0] = line.x; return one; }
                two[0] = Mathf.Min(line.x, line.x2);
                two[1] = Mathf.Max(line.x, line.x2);
                return two;
            }
        }

        static readonly float[] none = new float[0], one = new float[1], two = new float[2];

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
        [Tooltip("Segunda línea (opcional).")]
        public bool hasSecond;
        public float x2;

        [Header("Movimiento automático (menú y final)")]
        public bool autoSweep;
        public float sweepCenter, sweepAmplitude = 6f, sweepSpeed = 0.35f;

        static readonly int ShiftXId = Shader.PropertyToID("_ShiftX");
        static readonly int ShiftScreenXId = Shader.PropertyToID("_ShiftScreenX");
        static readonly int DayOnLeftId = Shader.PropertyToID("_DayOnLeft");
        static readonly int WorldDayOnLeftId = Shader.PropertyToID("_WorldDayOnLeft");
        static readonly int ShiftX2Id = Shader.PropertyToID("_ShiftX2");
        static readonly int ShiftScreenX2Id = Shader.PropertyToID("_ShiftScreenX2");
        static readonly int LineCountId = Shader.PropertyToID("_LineCount");

        Camera cam;
        Transform player;
        int dragged;            // 0 = primera línea, 1 = segunda
        int dragTouch = -1;
        bool draggingMouse;
        float grabOffset;
        bool wasHolding;
        float tickDistance;
        const float TickDistance = 0.7f;

        void Awake()
        {
            Instance = this;
            cam = Camera.main;
            Apply();
        }

        void Start()
        {
            var hero = FindAnyObjectByType<PlayerController>();
            if (hero != null) player = hero.transform;
        }

        float Get(int index) => index == 0 ? x : x2;

        void Set(int index, float value)
        {
            value = Mathf.Clamp(value, minX, maxX);
            float from = Get(index);
            if (LineEnergy.Instance != null) value = LineEnergy.Instance.Limit(from, value);
            if (index == 0) x = value; else x2 = value;
        }

        /// <summary>La línea que mueve el teclado: la más cercana a Lumi.</summary>
        int KeyboardLine()
        {
            if (!hasSecond || player == null) return 0;
            return Mathf.Abs(player.position.x - x) <= Mathf.Abs(player.position.x - x2) ? 0 : 1;
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
            if (Settings.Held(Settings.Action.LineLeft)) move -= 1f;
            if (Settings.Held(Settings.Action.LineRight)) move += 1f;
            // Cooperativo con mandos: el segundo mando mueve la línea.
            if (Settings.Coop && Gamepad.all.Count > 1) move += Gamepad.all[1].leftStick.x.ReadValue();
            float before = x + x2;
            int keyLine = KeyboardLine();
            if (move != 0f) Set(keyLine, Get(keyLine) + move * MirrorMode.Sign * keySpeed * Settings.LineSpeedFactor * Time.deltaTime);

            HandleDrag();
            float travelled = Mathf.Abs(x + x2 - before);

            // Avisos para el sonido: agarrar, soltar y un clic por cada tramo recorrido.
            bool holding = move != 0f || dragTouch >= 0 || draggingMouse;
            if (holding && !wasHolding) { GameEvents.RaiseLineGrabbed(); tickDistance = 0f; }
            if (!holding && wasHolding) GameEvents.RaiseLineReleased();
            wasHolding = holding;
            tickDistance += travelled;
            if (tickDistance >= TickDistance)
            {
                tickDistance = 0f;
                GameEvents.RaiseLineTick();
            }
        }

        // Después de que la cámara se mueva (CameraFollow usa LateUpdate con orden 0).
        void LateUpdate() => Apply();

        /// <summary>Posición de la línea en pantalla (0 = borde izquierdo, 1 = derecho).</summary>
        public float ScreenX() => ScreenX(x);

        public float ScreenX(float worldX)
        {
            if (cam == null) return 0.5f;
            return cam.WorldToViewportPoint(new Vector3(worldX, cam.transform.position.y, 0f)).x;
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
                    if (id == dragTouch) Set(dragged, ToWorldX(position) - grabOffset);
                }
                if (dragTouch >= 0 && !TouchHeld(touchscreen, dragTouch)) dragTouch = -1;
                if (anyTouch) return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 screen = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame && TryGrab(screen)) draggingMouse = true;
            if (!mouse.leftButton.isPressed) draggingMouse = false;
            if (draggingMouse) Set(dragged, ToWorldX(screen) - grabOffset);
        }

        bool TryGrab(Vector2 screen)
        {
            if (MobileButton.Contains(screen)) return false;
            float worldX = ToWorldX(screen);
            int nearest = hasSecond && Mathf.Abs(worldX - x2) < Mathf.Abs(worldX - x) ? 1 : 0;
            if (Mathf.Abs(worldX - Get(nearest)) > grabDistance) return false;
            dragged = nearest;
            grabOffset = worldX - Get(nearest);
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
            int count = hasSecond ? 2 : 1;
            Shader.SetGlobalFloat(ShiftXId, x);
            Shader.SetGlobalFloat(ShiftX2Id, hasSecond ? x2 : x);
            Shader.SetGlobalFloat(ShiftScreenXId, ScreenX(x));
            Shader.SetGlobalFloat(ShiftScreenX2Id, ScreenX(hasSecond ? x2 : x));
            Shader.SetGlobalFloat(LineCountId, count);
            // En modo espejo la cámara da la vuelta a la imagen: lo que se ve a la izquierda de la
            // pantalla es el extremo derecho del mundo, que es de día si dayOnLeft y las líneas son pares.
            bool rightmostIsDay = dayOnLeft == (count % 2 == 0);
            Shader.SetGlobalFloat(DayOnLeftId, (MirrorMode.Active ? rightmostIsDay : dayOnLeft) ? 1f : 0f);
            Shader.SetGlobalFloat(WorldDayOnLeftId, dayOnLeft ? 1f : 0f);
        }
    }
}
