using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>Meta del nivel: al tocarla carga la siguiente escena (o reinicia si no hay).</summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelGoal : MonoBehaviour
    {
        [Tooltip("Escena siguiente. Vacío = reiniciar este nivel.")]
        public string nextScene = "";

        bool reached;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (reached || other.GetComponent<PlayerController>() == null) return;
            reached = true;
            Debug.Log("¡Nivel completado!");
            string scene = string.IsNullOrEmpty(nextScene) ? SceneManager.GetActiveScene().name : nextScene;
            SceneManager.LoadScene(scene);
        }
    }
}
