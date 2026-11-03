using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Meta del nivel: al tocarla suena la melodía, Lumi celebra y, tras una pausa, sale la
    /// pantalla de resultados (ResultsScreen), que lleva a la escena siguiente.
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
            other.GetComponent<PlayerController>().Celebrate();
            var hud = Hud.Instance;
            if (hud != null && !hud.Daily) Progress.Complete(hud.levelNumber, hud.Fragments);
            GameEvents.RaiseGoalReached();
            Share.Vibrate(60);
            ResultsScreen.Show(nextScene, delay);
        }
    }
}
