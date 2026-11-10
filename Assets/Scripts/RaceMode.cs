using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Carrera a dos en la misma pantalla (Extras → Con amigos): el jugador 1 con A/D/Espacio, el
    /// 2 con las flechas (↑ o Ctrl derecho para saltar) o el segundo mando; la línea, con Q/E, es
    /// de los dos. Gana quien llegue antes a la meta. La cámara mira entre los dos.
    /// </summary>
    public static class RaceMode
    {
        public static bool Active { get; private set; }

        public static void Begin(string scene)
        {
            Active = true;
            SceneFader.Load(scene);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Listen() => SceneManager.sceneLoaded += (scene, _) => { if (scene.name == PauseMenu.MenuScene) Active = false; };
    }
}
