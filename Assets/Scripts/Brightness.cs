using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Fase
{
    /// <summary>
    /// Brillo de Opciones: sube o baja la exposición del posprocesado de cada escena (sobre la que
    /// ya trae cada capítulo). Se aplica al cargar escena y al cambiar el ajuste.
    /// </summary>
    public static class Brightness
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            SceneManager.sceneLoaded += (_, _) => Apply();
            Settings.Changed += Apply;
            Apply();
        }

        static readonly System.Collections.Generic.Dictionary<VolumeProfile, float> baseExposure = new System.Collections.Generic.Dictionary<VolumeProfile, float>();

        static void Apply()
        {
            float stops = Mathf.Log(Settings.BrightnessFactor, 2f);
            foreach (var volume in Object.FindObjectsByType<Volume>())
            {
                if (!volume.isGlobal) continue;
                // .profile crea una copia para esta escena: el archivo del perfil no se toca.
                var profile = volume.profile;
                if (profile == null) continue;
                if (!profile.TryGet(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
                if (!baseExposure.TryGetValue(profile, out float baseValue))
                {
                    baseValue = color.postExposure.overrideState ? color.postExposure.value : 0f;
                    baseExposure[profile] = baseValue;
                }
                color.postExposure.overrideState = true;
                color.postExposure.value = baseValue + stops;
                break;
            }
        }
    }
}
