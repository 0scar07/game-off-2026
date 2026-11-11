using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Final: la línea recorre el mundo hasta que amanece del todo, y aparecen el mensaje, la luna
    /// reunida y los créditos. El botón vuelve al menú.
    /// </summary>
    public class Ending : MonoBehaviour
    {
        public ShiftLine line;
        public float from = -2f, to = 42f, duration = 9f;
        public CanvasGroup texts;
        public Text moonLabel;
        public int levels = 11;
        public int fragmentsTotal;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            texts.alpha = 0f;
            texts.interactable = texts.blocksRaycasts = false;
            int moon = 0;
            for (int level = 1; level <= levels; level++) moon += Progress.Fragments(level);
            moonLabel.text = $"{Loc.Get("moontotal")}: {moon}/{fragmentsTotal}";

            // La línea llega a la mitad del mundo y Lumi tiene que elegir.
            line.autoSweep = false;
            float middle = (from + to) * 0.5f;
            for (float t = 0f; t < duration * 0.5f; t += Time.deltaTime)
            {
                line.x = Mathf.Lerp(from, middle, Mathf.SmoothStep(0f, 1f, t / (duration * 0.5f)));
                yield return null;
            }
            int choice = -1;
            yield return Choose(c => choice = c);
            PlayerPrefs.SetInt("fase_final", choice + 1);
            PlayerPrefs.Save();

            if (choice == 0)
            {
                // Reconstruir: los fragmentos vuelan al centro, la luna se recompone y amanece.
                yield return MoonCinematic();
                for (float t = 0f; t < duration * 0.5f; t += Time.deltaTime)
                {
                    line.x = Mathf.Lerp(middle, to, Mathf.SmoothStep(0f, 1f, t / (duration * 0.5f)));
                    yield return null;
                }
                line.x = to;
            }
            else
            {
                // Dejarlo partido: el mundo se queda con sus dos mitades, en paz.
                foreach (var text in texts.GetComponentsInChildren<Text>())
                {
                    var loc = text.GetComponent<LocText>();
                    if (text.text == Loc.Get("dawn")) { if (loc != null) loc.enabled = false; text.text = Loc.Pick("Dos mitades", "Two halves"); }
                    else if (text.text == Loc.Get("onewhole")) { if (loc != null) loc.enabled = false; text.text = Loc.Pick("El mundo siguió partido... y aprendió a quererse así.", "The world stayed split... and learned to love itself that way."); }
                }
                line.autoSweep = true;
                line.sweepCenter = middle;
                line.sweepAmplitude = 3f;
            }

            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                texts.alpha = t / 1.5f;
                yield return null;
            }
            texts.alpha = 1f;
            texts.interactable = texts.blocksRaycasts = true;
            if (StoryChoice.EndingLine.Length > 0) moonLabel.text += "\n" + StoryChoice.EndingLine;
            // Con toda la luna y todo el diario: el final secreto.
            yield return StoryChoice.SecretEnding();
            // Tarjeta para compartir: el resumen de todo el viaje.
            ShareCard.Show();


        }

        public void ToMenu() => SceneFader.Load(PauseMenu.MenuScene);

        /// <summary>Dos botones: reconstruir la luna o dejar el mundo partido.</summary>
        IEnumerator Choose(System.Action<int> chosen)
        {
            UiKit.EnsureEvents();
            var canvas = UiKit.Canvas("Eleccion", 30);
            var group = canvas.gameObject.AddComponent<CanvasGroup>();
            // La pregunta va en una tarjeta oscura: encima del sol y las nubes no se leía.
            var card = UiKit.Box(canvas.transform, "Tarjeta", new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1180f, 130f), new Color(0.07f, 0.05f, 0.2f, 0.82f));
            card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            UiKit.Text(card.transform, "Pregunta", Loc.Pick("¿Qué harás con la luna?", "What will you do with the moon?", "O que você vai fazer com a lua?", "Que vas-tu faire de la lune ?"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 110f), 64);
            int picked = -1;
            var a = UiKit.Button(canvas.transform, "Reconstruir", Loc.Pick("Reconstruirla", "Rebuild it"), new Vector2(0.5f, 0f), new Vector2(-330f, 140f), new Vector2(560f, 120f), () => picked = 0, 46);
            a.GetComponent<Image>().color = new Color(1f, 0.7f, 0.35f, 0.95f);
            // Texto oscuro sobre el botón cálido (en blanco casi no se leía).
            a.GetComponentInChildren<Text>().color = new Color(0.25f, 0.13f, 0.05f);
            var b = UiKit.Button(canvas.transform, "Partido", Loc.Pick("Dejar el mundo partido", "Leave the world split"), new Vector2(0.5f, 0f), new Vector2(330f, 140f), new Vector2(560f, 120f), () => picked = 1, 40);
            b.GetComponent<Image>().color = new Color(0.45f, 0.42f, 1f, 0.95f);
            a.Select();
            for (float t = 0f; t < 0.6f; t += Time.deltaTime) { group.alpha = t / 0.6f; yield return null; }
            while (picked < 0) yield return null;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime) { group.alpha = 1f - t / 0.5f; yield return null; }
            Destroy(canvas.gameObject);
            chosen(picked);
        }

        /// <summary>Doce fragmentos de luz vuelan en espiral hasta el centro y forman la luna.</summary>
        IEnumerator MoonCinematic()
        {
            var canvas = UiKit.Canvas("Luna", 29);
            // Arriba, en el cielo: más abajo tapaba el título y el texto del final.
            var center = new Vector2(0f, 420f);
            var shards = new RectTransform[12];
            for (int i = 0; i < shards.Length; i++)
            {
                shards[i] = UiKit.Box(canvas.transform, "Fragmento", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46f, 46f), new Color(1f, 0.92f, 0.6f)).rectTransform;
                shards[i].localRotation = Quaternion.Euler(0, 0, 45f);
            }
            var moon = UiKit.Box(canvas.transform, "Luna", new Vector2(0.5f, 0.5f), center, new Vector2(10f, 10f), new Color(1f, 0.98f, 0.9f, 0f), false);
            moon.sprite = Circle(false);
            var halo = UiKit.Box(canvas.transform, "Halo", new Vector2(0.5f, 0.5f), center, new Vector2(10f, 10f), new Color(1f, 0.9f, 0.6f, 0f), false);
            halo.sprite = Circle(true);
            moon.rectTransform.pivot = halo.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            halo.transform.SetSiblingIndex(0);
            for (float t = 0f; t < 3f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 2.2f);
                for (int i = 0; i < shards.Length; i++)
                {
                    float angle = i / (float)shards.Length * Mathf.PI * 2f + k * 4f;
                    float radius = Mathf.Lerp(700f, 0f, k);
                    shards[i].anchoredPosition = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * 0.6f);
                    shards[i].localScale = Vector3.one * Mathf.Lerp(1f, 0.3f, k);
                }
                float m = Mathf.Clamp01((t - 2f) / 1f);
                moon.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(10f, 190f, m);
                moon.color = new Color(1f, 0.98f, 0.9f, m);
                halo.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(10f, 520f, m);
                halo.color = new Color(1f, 0.9f, 0.6f, m * 0.45f);
                yield return null;
            }
            foreach (var shard in shards) Destroy(shard.gameObject);
            if (Settings.Vibration) Share.Vibrate(120);
        }

        static Sprite solidCircle, softCircle;

        /// <summary>Círculo liso (la luna) o difuminado (su halo), hecho una vez al vuelo.</summary>
        static Sprite Circle(bool soft)
        {
            ref Sprite cached = ref soft ? ref softCircle : ref solidCircle;
            if (cached != null) return cached;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                    float a = soft ? Mathf.Pow(Mathf.Clamp01(1f - d), 2f) : Mathf.Clamp01((1f - d) * size / 2f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            cached = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cached;
        }
    }
}
