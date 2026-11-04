using UnityEngine;
using UnityEngine.InputSystem;

namespace Fase
{
    /// <summary>
    /// Personaje de plataformas con física 2D. Salto cómodo: tiempo de coyote (saltar
    /// justo después de salir del borde), salto guardado (pulsar un poco antes de tocar
    /// el suelo) y salto más corto si se suelta pronto.
    /// Si un trozo de mundo aparece encima del personaje (lo aplasta), vuelve al último punto de control.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        public float speed = 7f;
        public float acceleration = 70f;
        public float airAcceleration = 45f;

        [Header("Salto")]
        public float jumpSpeed = 13f;
        public float coyoteTime = 0.1f;
        public float jumpBuffer = 0.12f;
        [Tooltip("Gravedad extra al caer o al soltar el salto (más control).")]
        public float fallMultiplier = 2.2f;
        public float maxFallSpeed = 22f;

        [Header("Suelo")]
        public LayerMask groundMask = ~0;
        [Tooltip("Si cae por debajo de esta altura, vuelve al punto de control.")]
        public float killHeight = -12f;

        [Header("Dibujo")]
        [Tooltip("Modelo que se gira hacia donde camina.")]
        public Transform visual;
        public Animator animator;

        public System.Action Died;

        Rigidbody2D body;
        CapsuleCollider2D capsule;
        Vector3 checkpoint;
        float coyoteLeft, bufferLeft;
        bool grounded;
        float facing = 1f;
        float lastFallSpeed, stepLeft, landLeft, carried;
        bool braking;
        bool celebrating;
        float idleTime, boredLeft;
        string boredState = "Yes";
        const float stepInterval = 0.3f;
        string currentState = "";

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            checkpoint = transform.position;
            if (Wardrobe.IsNox)
            {
                // Nox: salta más alto, corre más despacio.
                jumpSpeed *= 1.12f;
                speed *= 0.88f;
            }
            if (Settings.Assist)
            {
                jumpSpeed *= 1.1f;
                coyoteTime *= 2f;
                jumpBuffer *= 1.6f;
            }
        }

        public void SetCheckpoint(Vector3 position) => checkpoint = position;

        // Estirar y aplastar: (ancho, alto) que vuelve poco a poco a (1, 1).
        Vector2 squash = Vector2.one;
        float lean, lastVx;
        Vector3 visualScale = Vector3.one;
        bool scaleKnown;

        /// <summary>Se estira al saltar y se aplasta al aterrizar (más cuanto más fuerte cae).</summary>
        void OnJumpedSquash() => squash = new Vector2(0.84f, 1.2f);

        void OnLandedSquash(float speed)
        {
            float k = Mathf.Clamp01(speed / 20f);
            squash = new Vector2(1f + 0.3f * k, 1f - 0.28f * k);
        }

        void OnEnable()
        {
            GameEvents.Jumped += OnJumpedSquash;
            GameEvents.Landed += OnLandedSquash;
        }

        void OnDisable()
        {
            GameEvents.Jumped -= OnJumpedSquash;
            GameEvents.Landed -= OnLandedSquash;
        }

        /// <summary>En la meta: deja de responder a los controles, mira a la cámara y saluda.</summary>
        public void Celebrate() => celebrating = true;

        void Update()
        {
            if (JumpPressed()) bufferLeft = jumpBuffer;
            else bufferLeft -= Time.deltaTime;

            Animate();
        }

        /// <summary>
        /// Elige la animación y gira el modelo hacia donde camina. Estados del Animator (los del
        /// personaje de Quaternius): Idle, Run, Jump (despegue), Jump_Idle (en el aire),
        /// Jump_Land (aterrizaje) y Wave (saluda en la meta). Además se estira, se aplasta y se ladea.
        /// </summary>
        void Animate()
        {
            Vector2 v = body.linearVelocity;
            if (visual != null && Mathf.Abs(v.x) > 0.3f && !celebrating) facing = Mathf.Sign(v.x);
            // El modelo de Quaternius mira hacia la cámara con giro 0: -90 es la derecha y +90 la izquierda.
            float yaw = celebrating ? 0f : -facing * 90f;
            if (visual != null)
            {
                if (!scaleKnown) { visualScale = visual.localScale; scaleKnown = true; }
                // Se ladea al acelerar o girar, como si tuviera inercia.
                float accel = (v.x - lastVx) / Mathf.Max(Time.deltaTime, 1e-4f);
                lastVx = v.x;
                float targetLean = grounded ? Mathf.Clamp(-accel * 0.35f, -10f, 10f) : Mathf.Clamp(-v.x * 0.8f, -6f, 6f);
                lean = Mathf.Lerp(lean, targetLean, Time.deltaTime * 10f);
                visual.localRotation = Quaternion.Slerp(visual.localRotation,
                    Quaternion.Euler(0f, 0f, lean) * Quaternion.Euler(0f, yaw, 0f), Time.deltaTime * 14f);
                squash = Vector2.Lerp(squash, Vector2.one, Time.deltaTime * 10f);
                visual.localScale = new Vector3(visualScale.x * squash.x, visualScale.y * squash.y, visualScale.z * squash.x);
            }

            if (animator == null || animator.runtimeAnimatorController == null) return;
            landLeft -= Time.deltaTime;
            bool still = grounded && Mathf.Abs(v.x) < 0.5f && landLeft <= 0f && !celebrating;
            idleTime = still ? idleTime + Time.deltaTime : 0f;
            // Parado mucho rato: mira alrededor (asiente o niega) cada pocos segundos.
            if (idleTime > 7f) { idleTime = 2f; boredLeft = 1.6f; boredState = Random.value < 0.5f ? "Yes" : "No"; }
            boredLeft = still ? boredLeft - Time.deltaTime : 0f;
            string state = celebrating ? "Wave"
                : !grounded ? (v.y > 2f ? "Jump" : "Jump_Idle")
                : landLeft > 0f && Mathf.Abs(v.x) < 0.5f ? "Jump_Land"
                : Mathf.Abs(v.x) > 0.5f ? "Run"
                : still && AtEdge() ? "HitReact"
                : boredLeft > 0f ? boredState : "Idle";
            if (state != currentState)
            {
                animator.CrossFadeInFixedTime(state, 0.1f);
                currentState = state;
            }
        }

        void FixedUpdate()
        {
            bool wasGrounded = grounded;
            grounded = CheckGround();
            coyoteLeft = grounded ? coyoteTime : coyoteLeft - Time.fixedDeltaTime;
            if (grounded && !wasGrounded)
            {
                GameEvents.RaiseLanded(lastFallSpeed);
                if (lastFallSpeed > 6f) landLeft = 0.18f;
            }

            float input = celebrating ? 0f : MoveInput();
            Vector2 v = body.linearVelocity;
            if (!grounded) lastFallSpeed = Mathf.Max(0f, -v.y);

            // Pasos: hierba en el lado de día, nieve en el de noche.
            stepLeft -= Time.fixedDeltaTime;
            if (grounded && Mathf.Abs(v.x) > 1f && stepLeft <= 0f)
            {
                GameEvents.RaiseStep(!ShiftLine.IsDay(transform.position));
                stepLeft = stepInterval;
            }
            // Frenazo en el suelo: avisa para levantar polvo.
            if (grounded && Mathf.Abs(v.x) > 5f && Mathf.Abs(input) < 0.1f) braking = true;
            if (braking && Mathf.Abs(v.x) < 1.5f)
            {
                braking = false;
                if (grounded) GameEvents.RaiseBraked(transform.position, speed);
            }
            float accel = grounded ? acceleration : airAcceleration;
            v.x = Mathf.MoveTowards(v.x - carried, input * speed, accel * Time.fixedDeltaTime);
            // Encima de una plataforma que se mueve: Lumi viaja con ella.
            carried = grounded ? PlatformSpeed() : Mathf.MoveTowards(carried, 0f, 20f * Time.fixedDeltaTime);
            v.x += carried;
            // Corrientes de aire de día: empujan hacia arriba.
            v.y += AirCurrent.Lift(body.position) * Time.fixedDeltaTime;

            if (bufferLeft > 0f && coyoteLeft > 0f)
            {
                v.y = jumpSpeed;
                bufferLeft = 0f;
                coyoteLeft = 0f;
                GameEvents.RaiseJumped();
            }

            // Cae más rápido de lo que sube, y el salto se corta si se suelta el botón.
            if (v.y < 0f || !JumpHeld())
                v.y += Physics2D.gravity.y * (fallMultiplier - 1f) * body.gravityScale * Time.fixedDeltaTime;
            v.y = Mathf.Max(v.y, -maxFallSpeed);
            body.linearVelocity = v;

            if (grounded) Stats.AddMeters(Mathf.Abs(v.x) * Time.fixedDeltaTime);
            if (IsCrushed() || transform.position.y < killHeight) Die();
        }

        /// <summary>Impulso hacia arriba (flor-muelle). Cuenta como salto para el coyote y el búfer.</summary>
        public void Bounce(float upSpeed)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, upSpeed);
            coyoteLeft = 0f;
            bufferLeft = 0f;
        }

        public void Die()
        {
            body.linearVelocity = Vector2.zero;
            transform.position = checkpoint;
            Died?.Invoke();
            GameEvents.RaiseDied();
        }

        /// <summary>Velocidad horizontal de la plataforma móvil que hay bajo los pies (0 si no hay).</summary>
        float PlatformSpeed()
        {
            Bounds b = capsule.bounds;
            foreach (var hit in Physics2D.OverlapBoxAll(new Vector2(b.center.x, b.min.y), new Vector2(b.size.x * 0.85f, 0.15f), 0f, groundMask))
            {
                var platform = hit.GetComponentInParent<SunPlatform>();
                if (platform != null) return platform.Velocity.x;
            }
            return 0f;
        }

        /// <summary>Hay vacío justo delante de los pies: Lumi se tambalea.</summary>
        bool AtEdge()
        {
            Bounds b = capsule.bounds;
            Vector2 front = new Vector2(b.center.x + facing * (b.extents.x + 0.15f), b.min.y + 0.05f);
            return !Solid(Physics2D.RaycastAll(front, Vector2.down, 1.2f, groundMask));
        }

        bool Solid(RaycastHit2D[] hits)
        {
            foreach (var hit in hits)
                if (!hit.collider.isTrigger && hit.rigidbody != body) return true;
            return false;
        }

        bool CheckGround()
        {
            Bounds b = capsule.bounds;
            Vector2 feet = new Vector2(b.center.x, b.min.y);
            return Solid(Physics2D.OverlapBoxAll(feet, new Vector2(b.size.x * 0.85f, 0.1f), 0f, groundMask));
        }

        /// <summary>Hay algo sólido en el centro del personaje: un trozo de mundo apareció encima.</summary>
        bool IsCrushed()
        {
            Bounds b = capsule.bounds;
            return Solid(Physics2D.OverlapBoxAll(b.center, b.size * 0.5f, 0f, groundMask));
        }

        /// <summary>Algún collider sólido que no sea del propio personaje.</summary>
        bool Solid(Collider2D[] hits)
        {
            foreach (var hit in hits)
                if (!hit.isTrigger && hit.attachedRigidbody != body) return true;
            return false;
        }

        static float MoveInput()
        {
            float x = MobileInput.Move;
            if (Settings.Held(Settings.Action.Left)) x -= 1f;
            if (Settings.Held(Settings.Action.Right)) x += 1f;
            var pad = Settings.Coop && Gamepad.all.Count > 0 ? Gamepad.all[0] : Gamepad.current;
            if (pad != null) x += pad.leftStick.x.ReadValue();
            // En modo espejo "derecha" es la derecha de la pantalla, que en el mundo es la izquierda.
            return Mathf.Clamp(x, -1f, 1f) * MirrorMode.Sign;
        }

        static bool JumpPressed()
        {
            var pad = Gamepad.current;
            return MobileInput.ConsumeJump()
                || Settings.Pressed(Settings.Action.Jump)
                || (pad != null && pad.buttonSouth.wasPressedThisFrame);
        }

        static bool JumpHeld()
        {
            var pad = Gamepad.current;
            return MobileInput.JumpHeld
                || Settings.Held(Settings.Action.Jump)
                || (pad != null && pad.buttonSouth.isPressed);
        }
    }
}
