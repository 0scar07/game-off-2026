using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Fase
{
    /// <summary>
    /// Calidad automática: si el juego va a menos de 40 imágenes por segundo durante un rato,
    /// baja la resolución interna un poco (sin que se note la interfaz), y si va sobrado la sube.
    /// </summary>
    public class QualityGuard : MonoBehaviour
    {
        float window, frames;
        float cooldown = 5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (Application.isEditor) return;
            DontDestroyOnLoad(new GameObject("CalidadAutomatica").AddComponent<QualityGuard>().gameObject);
        }

        void Update()
        {
            if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)) return;
            window += Time.unscaledDeltaTime;
            frames++;
            cooldown -= Time.unscaledDeltaTime;
            if (window < 2f) return;
            float fps = frames / window;
            window = frames = 0f;
            if (cooldown > 0f || PhotoMode.Active) return;
            if (fps < 40f && urp.renderScale > 0.55f) { urp.renderScale -= 0.1f; cooldown = 4f; }
            else if (fps > 58f && urp.renderScale < 1f) { urp.renderScale = Mathf.Min(1f, urp.renderScale + 0.05f); cooldown = 8f; }
        }
    }
}
