using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Modo espejo (se desbloquea al terminar el juego): la cámara pinta el mundo dado la vuelta
    /// de izquierda a derecha. Los controles siguen la pantalla (derecha = derecha en pantalla) y
    /// los shaders reciben el lado del día invertido para que el corte coincida con lo que se ve.
    /// </summary>
    public static class MirrorMode
    {
        const string Key = "fase_espejo";

        public static bool Unlocked => Progress.Unlocked > LevelCatalog.Get.MainCount;

        /// <summary>Elegido en el menú. Solo cuenta dentro de los niveles.</summary>
        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(Key, 0) == 1 && Unlocked;
            set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool Active { get; private set; }

        /// <summary>-1 si hay que invertir la dirección horizontal de los controles.</summary>
        public static float Sign => Active ? -1f : 1f;

        static Camera mirrored;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            SceneManager.sceneLoaded += (scene, mode) => Apply();
            RenderPipelineManager.beginCameraRendering += (context, cam) => GL.invertCulling = Active && cam == mirrored;
            RenderPipelineManager.endCameraRendering += (context, cam) => GL.invertCulling = false;
            Apply();
        }

        static void Apply()
        {
            // Solo en los niveles (tienen Hud); el menú, la intro y el final se ven normales.
            Active = Enabled && Object.FindAnyObjectByType<Hud>() != null;
            mirrored = Active ? Camera.main : null;
            if (mirrored == null) return;
            mirrored.ResetProjectionMatrix();
            mirrored.projectionMatrix = mirrored.projectionMatrix * Matrix4x4.Scale(new Vector3(-1f, 1f, 1f));
        }
    }
}
