using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Lumi en el menú: al volver saluda con la mano y dice algo; luego, de vez en cuando, suelta
    /// una frase. Solo mientras se ve el menú principal.
    /// </summary>
    [RequireComponent(typeof(MenuWalker))]
    public class MenuTalk : MonoBehaviour
    {
        const string SeenKey = "fase_menu_visto";

        static readonly (string es, string en)[] Lines =
        {
            ("¿Seguimos?", "Shall we go on?"),
            ("Esta noche hay luna llena.", "There's a full moon tonight."),
            ("Me encanta cuando cruzas la línea.", "I love it when you cross the line."),
            ("¿Has visto las luciérnagas?", "Have you seen the fireflies?"),
            ("Tengo frío de este lado...", "I'm cold on this side..."),
            ("¿Día o noche? ¡Las dos!", "Day or night? Both!"),
        };

        MenuWalker walker;
        float next = 14f;
        public bool visible = true;

        void Start()
        {
            walker = GetComponent<MenuWalker>();
            bool back = PlayerPrefs.GetInt(SeenKey, 0) == 1;
            PlayerPrefs.SetInt(SeenKey, 1);
            string name = PlayerName.Has ? ", " + PlayerName.Value : "";
            string hello = back ? Loc.Pick($"¡Has vuelto{name}! ¿Seguimos?", $"You're back{name}! Shall we go on?")
                                : Loc.Pick($"¡Hola{name}! Soy Lumi.", $"Hi{name}! I'm Lumi.");
            walker.Wave(3.2f);
            Invoke(nameof(SayHello), 0.8f);
            helloText = hello;
        }

        string helloText;
        void SayHello() { if (visible) LumiBubble.Say(transform, helloText, 3.5f, 2.6f); }

        void Update()
        {
            if (!visible) { LumiBubble.Hide(); return; }
            next -= Time.deltaTime;
            if (next > 0f) return;
            next = Random.Range(14f, 22f);
            var line = Lines[Random.Range(0, Lines.Length)];
            LumiBubble.Say(transform, Loc.Pick(line.es, line.en), 3.2f, 2.6f);
        }
    }
}
