using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Pantalla de inicio: la línea entra barriendo la pantalla y parte el logo en dos (de día a
    /// la izquierda, de noche a la derecha), suena un acorde y aparece "0scar07 presenta". Con
    /// cualquier tecla o toque (o a los pocos segundos) pasa al menú.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        public string nextScene = "Menu";
        public AudioClip chord;

        RectTransform line, dayMask, nightMask;
        CanvasGroup credit, prompt;
        bool leaving;

        IEnumerator Start()
        {
            var canvas = UiKit.Canvas("Inicio", 10).transform;
            var black = UiKit.Fill(canvas, "Noche").gameObject.AddComponent<Image>();
            black.color = new Color(0.02f, 0.016f, 0.055f);
            var rnd = new System.Random(7);
            for (int i = 0; i < 140; i++)
            {
                var star = UiKit.Box(canvas, "Estrella", new Vector2((float)rnd.NextDouble(), (float)rnd.NextDouble()), Vector2.zero, Vector2.one * (2f + (float)rnd.NextDouble() * 3f), new Color(1f, 1f, 1f, 0.3f + (float)rnd.NextDouble() * 0.6f));
                star.type = Image.Type.Simple;
            }

            // El logo dos veces: claro sobre la noche (debajo) y oscuro sobre el día (dentro del panel, que lo tapa).
            var nightLogo = UiKit.Text(canvas, "LogoNoche", "FASE", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1400f, 300f), 260, TextAnchor.MiddleCenter, new Color(0.8f, 0.88f, 1f));
            nightLogo.gameObject.AddComponent<Outline>().effectColor = new Color(0.5f, 0.6f, 1f, 0.35f);
            // Lado de día: un panel cálido que crece desde la izquierda hasta la línea.
            dayMask = UiKit.Rect(canvas, "Dia", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(0f, 1080f));
            dayMask.anchorMin = new Vector2(0f, 0f);
            dayMask.anchorMax = new Vector2(0f, 1f);
            dayMask.pivot = new Vector2(0f, 0.5f);
            dayMask.gameObject.AddComponent<Image>().color = new Color(1f, 0.93f, 0.78f);
            dayMask.gameObject.AddComponent<RectMask2D>();

            var dayLogo = UiKit.Text(dayMask, "LogoDia", "FASE", new Vector2(0f, 0.5f), new Vector2(960f, 60f), new Vector2(1400f, 300f), 260, TextAnchor.MiddleCenter, new Color(0.23f, 0.14f, 0.06f));
            dayLogo.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Destroy(dayLogo.GetComponent<Shadow>());

            // La línea con su halo.
            line = UiKit.Rect(canvas, "Linea", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(10f, 1200f));
            var glow = UiKit.Box(line, "Halo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 1200f), new Color(1f, 0.92f, 0.7f, 0.25f), false);
            UiKit.Box(line, "Nucleo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 1200f), Color.white, false);

            credit = Group(UiKit.Text(canvas, "Presenta", Loc.Pick("0scar07 presenta", "0scar07 presents"), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(1000f, 60f), 40, TextAnchor.MiddleCenter, new Color(0.55f, 0.5f, 0.75f)).gameObject);
            prompt = Group(UiKit.Text(canvas, "Pulsa", Loc.Pick("pulsa cualquier tecla", "press any key"), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(1000f, 50f), 28, TextAnchor.MiddleCenter, new Color(0.5f, 0.47f, 0.68f)).gameObject);

            // La línea barre de la izquierda al centro y el día la sigue.
            // El ancho real del lienzo solo se sabe tras el primer frame (cuando el escalador ya actuó).
            yield return null;
            Canvas.ForceUpdateCanvases();
            float width = ((RectTransform)canvas).rect.width;
            // El logo de día, centrado igual que el de noche (el panel crece desde la izquierda).
            dayLogo.rectTransform.anchoredPosition = new Vector2(width * 0.5f, 60f);
            for (float t = 0f; t < 1.4f && !leaving; t += Time.unscaledDeltaTime)
            {
                float x = Mathf.SmoothStep(0f, width * 0.5f, t / 1.4f);
                Place(x);
                glow.color = new Color(1f, 0.97f, 0.9f, 0.25f + 0.15f * Mathf.Sin(t * 10f));
                yield return null;
            }
            Place(width * 0.5f);
            if (chord != null) gameObject.AddComponent<AudioSource>().PlayOneShot(chord, 0.8f * Progress.MusicVolume);
            for (float t = 0f; t < 0.8f && !leaving; t += Time.unscaledDeltaTime) { credit.alpha = t / 0.8f; yield return null; }
            credit.alpha = 1f;
            for (float t = 0f; t < 5f && !leaving; t += Time.unscaledDeltaTime)
            {
                prompt.alpha = 0.5f + 0.5f * Mathf.Sin(t * 3f);
                yield return null;
            }
            Leave();
        }

        static CanvasGroup Group(GameObject go)
        {
            var g = go.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            return g;
        }

        void Place(float x)
        {
            line.anchoredPosition = new Vector2(x, 0f);
            dayMask.sizeDelta = new Vector2(x, 0f);
        }

        void Update()
        {
            // Las primeras décimas no cuentan: la tecla que abrió el juego no debe saltarse la intro.
            if (Time.realtimeSinceStartup < 0.6f || Time.timeSinceLevelLoad < 0.6f) return;
            var kb = Keyboard.current;
            var pointer = Pointer.current;
            var pad = Gamepad.current;
            if ((kb != null && kb.anyKey.wasPressedThisFrame) || (pointer != null && pointer.press.wasPressedThisFrame)
                || (pad != null && pad.buttonSouth.wasPressedThisFrame))
                Leave();
        }

        void Leave()
        {
            if (leaving) return;
            leaving = true;
            SceneFader.Load(nextScene);
        }
    }
}
