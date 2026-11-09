using System.Collections.Generic;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Sonidos sueltos de Resources/Sonidos (los genera Tools/sonidos_ui.py): botones, farol,
    /// estornudo... Un solo altavoz que sobrevive a los cambios de escena.
    /// </summary>
    public static class Sfx
    {
        static AudioSource source;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public static void Play(string name, float volume = 1f, float pitch = 1f)
        {
            if (!clips.TryGetValue(name, out var clip))
            {
                clip = Resources.Load<AudioClip>("Sonidos/" + name);
                clips[name] = clip;
            }
            if (clip == null) return;
            if (source == null)
            {
                var go = new GameObject("SonidosSueltos");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
            }
            source.pitch = pitch;
            source.PlayOneShot(clip, volume * Progress.SfxVolume);
        }
    }
}
