using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>Barra de avance de la canción que suena en la banda sonora.</summary>
    public class SoundtrackBar : MonoBehaviour
    {
        AudioSource source;
        Image bar;

        public void Init(AudioSource s, Image b) { source = s; bar = b; }

        void Update()
        {
            if (source == null || bar == null) return;
            float k = source.clip != null && source.clip.length > 0f ? source.time / source.clip.length : 0f;
            bar.rectTransform.sizeDelta = new Vector2(1000f * k, 12f);
            source.volume = Progress.MusicVolume;
        }
    }
}
