using UnityEngine;

namespace Fase
{
    /// <summary>Lumi pasea por el fondo del menú: va y viene, se para a mirar y saluda a veces.</summary>
    public class MenuWalker : MonoBehaviour
    {
        public float minX = 10f, maxX = 30f, speed = 1.6f;
        Animator animator;
        float direction = 1f, pause;
        string state = "";

        float footOffset;   // distancia del origen del modelo a sus pies

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length > 0)
            {
                float feet = renderers[0].bounds.min.y;
                foreach (var r in renderers) feet = Mathf.Min(feet, r.bounds.min.y);
                footOffset = transform.position.y - feet;
            }
            Snap(1f);
        }

        /// <summary>Pone los pies sobre el suelo que hay justo debajo (k = 1 de golpe, menos = suave).</summary>
        void Snap(float k)
        {
            var p = transform.position;
            float feet = p.y - footOffset;
            var hit = Physics2D.Raycast(new Vector2(p.x, feet + 1.2f), Vector2.down, 8f);
            if (hit.collider == null) return;
            p.y = Mathf.Lerp(p.y, hit.point.y + footOffset, k);
            transform.position = p;
        }

        void Update()
        {
            if (pause > 0f)
            {
                pause -= Time.deltaTime;
                Play(pause > 1.6f ? "Wave" : "Idle");
                return;
            }
            var p = transform.position;
            p.x += direction * speed * Time.deltaTime;
            if (p.x > maxX || p.x < minX)
            {
                direction = -direction;
                pause = Random.value < 0.5f ? 3.2f : 1.2f;
            }
            transform.position = p;
            // Los pies siempre sobre el suelo (el del menú no es plano).
            Snap(Time.deltaTime * 12f);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, -direction * 90f, 0f), Time.deltaTime * 6f);
            Play("Run");
        }

        /// <summary>Se para y saluda con la mano unos segundos (lo usa MenuTalk al volver al menú).</summary>
        public void Wave(float seconds) => pause = seconds + 1.6f;

        void Play(string next)

        {
            if (animator == null || next == state) return;
            state = next;
            animator.CrossFadeInFixedTime(next, 0.15f);
        }
    }
}
