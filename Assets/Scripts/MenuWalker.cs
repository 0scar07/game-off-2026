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

        void Start() => animator = GetComponentInChildren<Animator>();

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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, -direction * 90f, 0f), Time.deltaTime * 6f);
            Play("Run");
        }

        void Play(string next)
        {
            if (animator == null || next == state) return;
            state = next;
            animator.CrossFadeInFixedTime(next, 0.15f);
        }
    }
}
