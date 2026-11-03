using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Cofre de la ruta secreta: al tocarlo se abre y da el fragmento de luna escondido.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ChestPickup : MonoBehaviour
    {
        public Animator animator;
        public Transform reward;

        bool opened;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (opened || other.GetComponent<PlayerController>() == null) return;
            opened = true;
            if (animator != null) animator.Play("Chest_Open");
            GameEvents.RaiseCollected();
            GameEvents.RaiseChestOpened();
            if (reward != null) StartCoroutine(RiseAndVanish());
        }

        System.Collections.IEnumerator RiseAndVanish()
        {
            Vector3 from = reward.position;
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                reward.position = from + Vector3.up * Mathf.SmoothStep(0f, 1.6f, t / 1.2f);
                reward.Rotate(0f, 360f * Time.deltaTime, 0f, Space.World);
                yield return null;
            }
            reward.gameObject.SetActive(false);
        }
    }
}
