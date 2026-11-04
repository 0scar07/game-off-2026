using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// La línea que parte el mundo: a un lado es de día y al otro de noche. Se mueve con Shift
    /// izq./der. (o Q/E) y arrastrándola con el ratón o el dedo.
    ///
    /// Normalmente es vertical, pero puede estar inclinada (angle) o tumbada (angle = 90: arriba
    /// y abajo, para los niveles verticales). `x` es su posición medida a lo largo de su normal
    /// (para la vertical, la x del mundo). Algunos niveles tienen una segunda línea paralela
    /// (hasSecond): entre las dos, el otro mundo. El farol de Lumi puede abrir una burbuja de
    /// noche (LanternPower) que manda sobre la línea.
    ///
    /// Los demás scripts preguntan ShiftLine.IsDay(posición). Los shaders reciben la línea ya
    /// proyectada en la pantalla (_LineP, _LineN), así el corte es igual para lo cercano y el fondo.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ShiftLine : MonoBehaviour
    {
        public static ShiftLine Instance { get; private set; }

        /// <summary>Posición de la (primera) línea a lo largo de su normal.</summary>
        public static float X => Instance != null ? Instance.x : 0f;

        /// <summary>Lo rápido que se mueve la línea ahora mismo (unidades por segundo, suavizado).</summary>
        public static float Speed => Instance != null ? Instance.speed : 0f;
        float speed, lastSum;

        /// <summary>Normal de la línea en el mundo: (1, 0) vertical, (0, 1) horizontal.</summary>
        public Vector2 Normal => new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        /// <summary>true si el punto está en el lado de día.</summary>
        public static bool IsDay(Vector2 point)
        {
            if (Instance == null) return true;
            var line = Instance;
            if (LanternPower.Contains(point)) return false;
            float d = Vector2.Dot(point, line.Normal);
            int crossed = (d >= line.x ? 1 : 0) + (line.hasSecond && d >= line.x2 ? 1 : 0);
            // Cada línea cruzada cambia de mundo.
            return line.dayOnLeft == (crossed % 2 == 0);
        }

        public static bool IsDay(Vector3 point) => IsDay((Vector2)point);

        /// <summary>Solo con la x (a la altura media del nivel).</summary>
        public static bool IsDay(float worldX) => IsDay(new Vector2(worldX, Instance != null ? Instance.referenceY : 0f));

        /// <summary>Posiciones de las líneas a lo largo de la normal, de menor a mayor.</summary>
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

        [Tooltip("El día queda a la izquierda (o debajo, si está tumbada) de la línea.")]
        public bool dayOnLeft = true;
        [Tooltip("Grados respecto a la vertical: 0 vertical, 90 horizontal.")]
        public float angle;
        [Tooltip("Altura media del nivel (para quien solo pregunta por la x).")]
        public float referenceY = 5f;
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
        static readonly int ShiftX2Id = Shader.PropertyToID("_ShiftX2");
        static readonly int ShiftScreenXId = Shader.PropertyToID("_ShiftScreenX");
        static readonly int ShiftScreenX2Id = Shader.PropertyToID("_ShiftScreenX2");
        static readonly int LineCountId = Shader.PropertyToID("_LineCount");
        static readonly int DayOnLeftId = Shader.PropertyToID("_DayOnLeft");
        static readonly int LinePId = Shader.PropertyToID("_LineP");
        static readonly int LineNId = Shader.PropertyToID("_LineN");
        static readonly int BubbleId = Shader.PropertyToID("_Bubble");
        static readonly int BubbleWId = Shader.PropertyToID("_BubbleW");

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
            float d = Vector2.Dot(player.position, Normal);
            return Mathf.Abs(d - x) <= Mathf.Abs(d - x2) ? 0 : 1;
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
            if (PauseMenu.Paused || PhotoMode.Active) return;
            float move = 0f;
            if (Settings.Held(Settings.Action.LineLeft)) move -= 1f;
            if (Settings.Held(Settings.Action.LineRight)) move += 1f;
            // Cooperativo con mandos: el segundo mando mueve la línea.
            if (Settings.Coop && Gamepad.all.Count > 1) move += Gamepad.all[1].leftStick.x.ReadValue();
            float before = x + x2;
            int keyLine = KeyboardLine();
            // Tumbada, "derecha" sube la línea; en modo espejo se invierte lo horizontal.
            float sign = angle > 45f ? 1f : MirrorMode.Sign;
            if (move != 0f) Set(keyLine, Get(keyLine) + move * sign * keySpeed * Settings.LineSpeedFactor * Time.deltaTime);

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
        void LateUpdate()
        {
            float sum = x + x2;
            float now = Time.deltaTime > 0f ? Mathf.Abs(sum - lastSum) / Time.deltaTime : 0f;
            lastSum = sum;
            speed = Mathf.Lerp(speed, autoSweep ? 0f : now, Time.deltaTime * 12f);
            Apply();
        }

        /// <summary>Punto de la línea en el mundo más cercano al centro de la cámara.</summary>
        public Vector2 PointOnLine(float offset)
        {
            var n = Normal;
            var tangent = new Vector2(-n.y, n.x);
            Vector2 center = cam != null ? (Vector2)cam.transform.position : Vector2.zero;
            return n * offset + tangent * Vector2.Dot(center, tangent);
        }

        /// <summary>Posición de la línea en pantalla (0 = borde izquierdo, 1 = derecho).</summary>
        public float ScreenX() => ScreenX(x);

        public float ScreenX(float offset)
        {
            if (cam == null) return 0.5f;
            return cam.WorldToViewportPoint(PointOnLine(offset)).x;
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
                    if (id == dragTouch) Set(dragged, ToLine(position) - grabOffset);
                }
                if (dragTouch >= 0 && !TouchHeld(touchscreen, dragTouch)) dragTouch = -1;
                if (anyTouch) return;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 screen = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame && TryGrab(screen)) draggingMouse = true;
            if (!mouse.leftButton.isPressed) draggingMouse = false;
            if (draggingMouse) Set(dragged, ToLine(screen) - grabOffset);
        }

        bool TryGrab(Vector2 screen)
        {
            if (MobileButton.Contains(screen)) return false;
            float d = ToLine(screen);
            int nearest = hasSecond && Mathf.Abs(d - x2) < Mathf.Abs(d - x) ? 1 : 0;
            if (Mathf.Abs(d - Get(nearest)) > grabDistance) return false;
            dragged = nearest;
            grabOffset = d - Get(nearest);
            return true;
        }

        static bool TouchHeld(Touchscreen touchscreen, int id)
        {
            foreach (var touch in touchscreen.touches)
                if (touch.press.isPressed && touch.touchId.ReadValue() == id) return true;
            return false;
        }

        /// <summary>Posición a lo largo de la normal del punto del plano del juego (z = 0) bajo la pantalla.</summary>
        float ToLine(Vector2 screen)
        {
            Vector2 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            return Vector2.Dot(world, Normal);
        }

        /// <summary>Punto del mundo en la pantalla, con la x corregida por el ancho (unidades iguales en x e y).</summary>
        Vector2 ToScreenSpace(Vector2 world)
        {
            var v = cam.WorldToViewportPoint(world);
            return new Vector2(v.x * cam.aspect, v.y);
        }

        void Apply()
        {
            int count = hasSecond ? 2 : 1;
            Shader.SetGlobalFloat(ShiftXId, x);
            Shader.SetGlobalFloat(ShiftX2Id, hasSecond ? x2 : x);
            Shader.SetGlobalFloat(LineCountId, count);
            Shader.SetGlobalFloat(DayOnLeftId, dayOnLeft ? 1f : 0f);
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            Shader.SetGlobalFloat(ShiftScreenXId, ScreenX(x));
            Shader.SetGlobalFloat(ShiftScreenX2Id, ScreenX(hasSecond ? x2 : x));

            // La línea vista en pantalla: un punto de cada línea y la normal (proyectando la del
            // mundo, así el modo espejo y la perspectiva salen solos).
            var n = Normal;
            Vector2 p1 = ToScreenSpace(PointOnLine(x)), p2 = ToScreenSpace(PointOnLine(hasSecond ? x2 : x));
            Vector2 screenN = (ToScreenSpace(PointOnLine(x) + n) - p1).normalized;
            Shader.SetGlobalVector(LinePId, new Vector4(p1.x, p1.y, p2.x, p2.y));
            Shader.SetGlobalVector(LineNId, new Vector4(screenN.x, screenN.y, n.x, n.y));

            // Burbuja de noche del farol.
            if (LanternPower.Radius > 0.01f)
            {
                Vector2 c = LanternPower.Center;
                Vector2 sc = ToScreenSpace(c), edge = ToScreenSpace(c + Vector2.right * LanternPower.Radius);
                Shader.SetGlobalVector(BubbleId, new Vector4(sc.x, sc.y, (edge - sc).magnitude, 1f));
                Shader.SetGlobalVector(BubbleWId, new Vector4(c.x, c.y, LanternPower.Radius, 1f));
            }
            else
            {
                Shader.SetGlobalVector(BubbleId, Vector4.zero);
                Shader.SetGlobalVector(BubbleWId, Vector4.zero);
            }
        }
    }
}
