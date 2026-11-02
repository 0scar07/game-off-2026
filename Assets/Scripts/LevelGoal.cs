using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Meta del nivel: al tocarla suena la melodía y, tras una pausa, carga la siguiente escena
    /// (o reinicia si no hay).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelGoal : MonoBehaviour
    {
        [Tooltip("Escena siguiente. Vacío = reiniciar este nivel.")]
        public string nextScene = "";
        [Tooltip("Segundos de celebración antes de cambiar de nivel.")]
        public float delay = 2f;

        bool reached;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (reached || other.GetComponent<PlayerController>() == null) return;
            reached = true;
            GameEvents.RaiseGoalReached();
            StartCoroutine(Finish());
        }

        IEnumerator Finish()
        {
            yield return new WaitForSeconds(delay);
            string scene = string.IsNullOrEmpty(nextScene) ? SceneManager.GetActiveScene().name : nextScene;
            SceneManager.LoadScene(scene);
        }
    }
}
