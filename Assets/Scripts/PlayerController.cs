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
        string currentState = "";

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            checkpoint = transform.position;
        }

        public void SetCheckpoint(Vector3 position) => checkpoint = position;

        void Update()
        {
            if (JumpPressed()) bufferLeft = jumpBuffer;
            else bufferLeft -= Time.deltaTime;

            Animate();
        }

        /// <summary>
        /// Elige la animación (idle, walk, jump, fall) y gira el modelo hacia donde camina.
        /// Los estados del Animator se llaman igual que las animaciones del modelo de Kenney.
        /// </summary>
        void Animate()
        {
            Vector2 v = body.linearVelocity;
            if (visual != null && Mathf.Abs(v.x) > 0.3f) facing = Mathf.Sign(v.x);
            if (visual != null)
                visual.localRotation = Quaternion.Slerp(visual.localRotation,
                    Quaternion.Euler(0f, facing * 90f, 0f), Time.deltaTime * 14f);

            if (animator == null || animator.runtimeAnimatorController == null) return;
            string state = !grounded ? (v.y > 0.5f ? "jump" : "fall")
                : Mathf.Abs(v.x) > 0.5f ? "walk" : "idle";
            if (state != currentState)
            {
                animator.CrossFadeInFixedTime(state, 0.1f);
                currentState = state;
            }
        }

        void FixedUpdate()
        {
            grounded = CheckGround();
            coyoteLeft = grounded ? coyoteTime : coyoteLeft - Time.fixedDeltaTime;

            float input = MoveInput();
            Vector2 v = body.linearVelocity;
            float accel = grounded ? acceleration : airAcceleration;
            v.x = Mathf.MoveTowards(v.x, input * speed, accel * Time.fixedDeltaTime);

            if (bufferLeft > 0f && coyoteLeft > 0f)
            {
                v.y = jumpSpeed;
                bufferLeft = 0f;
                coyoteLeft = 0f;
            }

            // Cae más rápido de lo que sube, y el salto se corta si se suelta el botón.
            if (v.y < 0f || !JumpHeld())
                v.y += Physics2D.gravity.y * (fallMultiplier - 1f) * body.gravityScale * Time.fixedDeltaTime;
            v.y = Mathf.Max(v.y, -maxFallSpeed);
            body.linearVelocity = v;

            if (IsCrushed() || transform.position.y < killHeight) Die();
        }

        public void Die()
        {
            body.linearVelocity = Vector2.zero;
            transform.position = checkpoint;
            Died?.Invoke();
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
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) x -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) x += 1f;
            }
            var pad = Gamepad.current;
            if (pad != null) x += pad.leftStick.x.ReadValue();
            return Mathf.Clamp(x, -1f, 1f);
        }

        static bool JumpPressed()
        {
            var kb = Keyboard.current;
            var pad = Gamepad.current;
            return MobileInput.ConsumeJump()
                || (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
                || (pad != null && pad.buttonSouth.wasPressedThisFrame);
        }

        static bool JumpHeld()
        {
            var kb = Keyboard.current;
            var pad = Gamepad.current;
            return MobileInput.JumpHeld
                || (kb != null && (kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed))
                || (pad != null && pad.buttonSouth.isPressed);
        }
    }
}
