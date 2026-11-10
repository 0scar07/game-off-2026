using System.Collections;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// La Gran Sombra (jefe del final). De noche está despierta: empuja la línea hacia Lumi y
    /// lanza ondas por el suelo que solo existen de noche. Si Lumi consigue que le dé el día, se
    /// vuelve piedra unos segundos y en su cabeza brilla un fragmento de luna: tocarlo es un golpe.
    /// Tras cada golpe ruge y echa la línea atrás. Con tres golpes se deshace y aparece la meta.
    /// </summary>
    public class Boss : MonoBehaviour
    {
        public Transform visual, statue;
        public Animator animator;
        [Tooltip("Fragmento que brilla en su cabeza cuando es de piedra.")]
        public Transform weakPoint;
        [Tooltip("Onda de muestra (desactivada) que se copia en cada ataque.")]
        public GameObject waveTemplate;
        [Tooltip("Meta que aparece al vencerla.")]
        public GameObject goal;
        public int health = 3;
        [Tooltip("Criatura de día (despierta al sol, piedra de noche), como la Garra del Alba.")]
        public bool dayCreature;
        public string bossName = "La Gran Sombra", bossNameEn = "The Great Shadow";
        public string idleState = "Idle", attackState = "Bite_InPlace", hitState = "HitRecieve";
        public float pushSpeed = 1.6f, waveEvery = 2.8f, stoneTime = 6f;

        PlayerController player;
        float waveLeft = 2f, stoneLeft;
        bool stone, busy, defeated;
        int hits;
        UnityEngine.UI.Image bar;
        UnityEngine.UI.Text phaseLabel;

        public bool Defeated => defeated;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            if (goal != null) goal.SetActive(false);
            if (weakPoint != null) weakPoint.gameObject.SetActive(false);
            BuildBar();
        }

        void BuildBar()
        {
            var canvas = UiKit.Canvas("VidaJefe", 6);
            canvas.transform.SetParent(transform, false);
            var back = UiKit.Box(canvas.transform, "Barra", new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(820f, 40f), new Color(0.04f, 0.03f, 0.14f, 0.7f));
            bar = UiKit.Box(back.transform, "Vida", new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(808f, 28f), new Color(0.75f, 0.35f, 1f));
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            UiKit.Text(back.transform, "Nombre", Loc.Pick(bossName, bossNameEn), new Vector2(0.5f, 0f), new Vector2(0f, -36f), new Vector2(820f, 44f), 34);
            phaseLabel = UiKit.Text(back.transform, "Fase", "", new Vector2(1f, 0f), new Vector2(-60f, -36f), new Vector2(200f, 44f), 28, TextAnchor.MiddleRight, UiKit.Gold);
            UpdatePhase();
        }

        /// <summary>Cada golpe abre una fase nueva: empuja más rápido y lanza ondas más seguidas.</summary>
        void UpdatePhase()
        {
            if (phaseLabel != null) phaseLabel.text = Loc.Pick($"Fase {Mathf.Min(hits + 1, health)}/{health}", $"Phase {Mathf.Min(hits + 1, health)}/{health}");
        }

        void Update()
        {
            if (defeated || busy) return;
            bool day = ShiftLine.IsDay(transform.position);
            bool asleep = dayCreature ? !day : day;
            if (asleep != stone) SetStone(asleep);

            if (stone)
            {
                stoneLeft -= Time.deltaTime;
                // Tiembla cuando está a punto de romper la piedra.
                if (statue != null) statue.localPosition = stoneLeft < 1.5f ? (Vector3)Random.insideUnitCircle * 0.08f : Vector3.zero;
                if (stoneLeft <= 0f) StartCoroutine(Roar(false));
                else if (weakPoint != null && player != null && Vector2.Distance(player.transform.position, weakPoint.position) < 1.1f) StartCoroutine(Roar(true));
                return;
            }

            // Despierta: empuja la línea hacia Lumi y lanza ondas.
            var line = ShiftLine.Instance;
            if (line != null && !PauseMenu.Paused)
            {
                // Empuja la línea para que su mundo (noche o día) crezca hacia Lumi.
                float toward = Toward(line);
                line.x = Mathf.Clamp(line.x + toward * pushSpeed * Mathf.Pow(1.3f, hits) * Time.deltaTime, line.minX, line.maxX);
            }
            waveLeft -= Time.deltaTime;
            if (waveLeft <= 0f && waveTemplate != null)
            {
                waveLeft = waveEvery - hits * 0.5f;
                var wave = Instantiate(waveTemplate, waveTemplate.transform.position, Quaternion.identity);
                wave.GetComponent<BossWave>().dayWave = dayCreature;
                wave.SetActive(true);
                if (animator != null) { animator.Play(attackState, 0, 0f); Invoke(nameof(BackToIdle), 0.9f); }
            }
        }

        /// <summary>Hacia dónde mover la línea para que el mundo del jefe (a la derecha) crezca.</summary>
        float Toward(ShiftLine line)
        {
            bool myWorldOnRight = dayCreature ? !line.dayOnLeft : line.dayOnLeft;
            return myWorldOnRight ? -1f : 1f;
        }

        void BackToIdle()
        {
            if (animator != null && !stone) animator.CrossFadeInFixedTime(idleState, 0.2f);
        }

        void SetStone(bool value)
        {
            stone = value;
            stoneLeft = stoneTime;
            if (animator != null) animator.speed = value ? 0f : 1f;
            if (weakPoint != null) weakPoint.gameObject.SetActive(value);
        }

        IEnumerator Roar(bool hit)
        {
            busy = true;
            if (weakPoint != null) weakPoint.gameObject.SetActive(false);
            if (hit)
            {
                hits++;
                GameEvents.RaiseCollected();
                TimeControl.SlowMotion(0.6f, 0.25f);
                Share.Vibrate(80);
                if (bar != null) bar.rectTransform.sizeDelta = new Vector2(808f * (health - hits) / health, 28f);
                UpdatePhase();
                if (hits < health && phaseLabel != null) LumiBubble.Say(player != null ? player.transform : transform, Loc.Pick("¡Se enfada más!", "It's getting angrier!"), 1.6f);

            }
            if (hits >= health)
            {
                yield return Defeat();
                yield break;
            }
            // Rugido: la oscuridad vuelve y echa la línea lejos del jefe.
            yield return new WaitForSeconds(0.4f);
            var line = ShiftLine.Instance;
            if (line != null)
            {
                float target = Toward(line) < 0f ? line.minX + 2f : line.maxX - 2f;
                float from = line.x;
                for (float t = 0f; t < 0.6f; t += Time.deltaTime)
                {
                    line.x = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / 0.6f));
                    yield return null;
                }
            }
            SetStone(false);
            if (animator != null) { animator.Play(hitState, 0, 0f); Invoke(nameof(BackToIdle), 0.9f); }
            waveLeft = 1.2f;
            busy = false;
        }

        IEnumerator Defeat()
        {
            defeated = true;
            if (!dayCreature && health >= 3) Achievements.Unlock("eclipse");
            // Se deshace en luz: se encoge y sube mientras brilla.
            Vector3 scale = transform.localScale;
            for (float t = 0f; t < 1.6f; t += Time.deltaTime)
            {
                float k = t / 1.6f;
                transform.localScale = scale * (1f - k * 0.9f);
                transform.position += Vector3.up * Time.deltaTime * 1.5f;
                yield return null;
            }
            gameObject.SetActive(false);
            if (goal != null) goal.SetActive(true);
        }
    }
}
