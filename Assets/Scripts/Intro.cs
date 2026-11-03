using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// La historia: viñetas ilustradas con zoom lento (efecto Ken Burns), fundidos y subtítulos
    /// en el idioma del jugador. Tocar o pulsar una tecla pasa a la siguiente; "Saltar" va directo
    /// al primer nivel.
    /// </summary>
    public class Intro : MonoBehaviour
    {
        public const string SeenKey = "fase_intro_vista";

        public Image picture;
        public CanvasGroup pictureGroup;
        public Text subtitle;
        public Sprite[] slides;
        [TextArea] public string[] linesEs;
        [TextArea] public string[] linesEn;
        public float secondsPerSlide = 5.5f;
        [Tooltip("Narración de cada viñeta (voz suave), en español y en inglés.")]
        public AudioClip[] voiceEs, voiceEn;
        AudioSource voice;
        public string nextScene;

        bool advance, leaving;

        IEnumerator Start()
        {
            voice = gameObject.AddComponent<AudioSource>();
            for (int i = 0; i < slides.Length && !leaving; i++)
            {
                picture.sprite = slides[i];
                subtitle.text = Loc.Pick(linesEs[i], linesEn[i]);
                advance = false;
                var clips = Loc.English ? voiceEn : voiceEs;
                float slideTime = secondsPerSlide;
                if (clips != null && i < clips.Length && clips[i] != null)
                {
                    voice.Stop();
                    voice.PlayOneShot(clips[i], Progress.SfxVolume);
                    slideTime = Mathf.Max(secondsPerSlide, clips[i].length + 1.2f);
                }
                float panX = i % 2 == 0 ? -1f : 1f;
                for (float t = 0f; t < slideTime && !advance && !leaving; t += Time.deltaTime)
                {
                    float k = t / slideTime;
                    picture.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.04f, 1.14f, k);
                    picture.rectTransform.anchoredPosition = new Vector2(panX * Mathf.Lerp(-30f, 30f, k), 0f);
                    pictureGroup.alpha = Mathf.Clamp01(Mathf.Min(t / 0.8f, (slideTime - t) / 0.6f));
                    subtitle.color = new Color(1f, 1f, 1f, Mathf.Clamp01((t - 0.4f) / 0.6f));
                    yield return null;
                }
            }
            Finish();
        }

        void Update()
        {
            var kb = Keyboard.current;
            var pointer = Pointer.current;
            if ((kb != null && kb.anyKey.wasPressedThisFrame) || (pointer != null && pointer.press.wasPressedThisFrame))
                advance = true;
        }

        public void Skip() => Finish();

        void Finish()
        {
            if (leaving) return;
            leaving = true;
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
            SceneFader.Load(nextScene);
        }
    }
}
