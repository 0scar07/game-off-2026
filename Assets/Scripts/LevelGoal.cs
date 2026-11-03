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
        [Tooltip("Meta de la ruta de noche: solo cuenta si se toca de noche.")]
        public bool nightRoute;

        /// <summary>La última meta alcanzada fue la de la ruta de noche.</summary>
        public static bool LastWasNightRoute { get; private set; }

        bool reached;

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerStay2D(Collider2D other)
        {
            if (reached || other.GetComponent<PlayerController>() == null) return;
            if (nightRoute && ShiftLine.IsDay(transform.position)) return;
            reached = true;
            LastWasNightRoute = nightRoute;
            other.GetComponent<PlayerController>().Celebrate();
            var hud = Hud.Instance;
            if (hud != null && !hud.Daily) Progress.Complete(hud.levelNumber, hud.Fragments);
            GameEvents.RaiseGoalReached();
            Share.Vibrate(60);
            ResultsScreen.Show(nextScene, delay);
        }
    }
}
