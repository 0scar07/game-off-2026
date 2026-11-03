using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Diario de Lumi: una página por nivel, escondida en su cofre secreto. Cuenta la historia de
    /// la luna rota. Se lee en el menú (Diario).
    /// </summary>
    public static class Diary
    {
        public static readonly (string es, string en)[] Pages =
        {
            ("La noche que la luna se rompió, el abuelo dijo que cada fragmento guarda un recuerdo del cielo entero.",
             "The night the moon broke, Grandpa said every fragment keeps a memory of the whole sky."),
            ("De noche el hielo brilla como si alguien hubiera dejado encendidas mil linternas pequeñas.",
             "At night the ice glows as if someone left a thousand tiny lanterns on."),
            ("Hoy entendí que no tengo que elegir entre el día y la noche. Puedo tener un poco de cada uno.",
             "Today I understood I don't have to choose between day and night. I can have a bit of both."),
            ("El lago me miró dos veces: de día con ojos de agua, de noche con ojos de cristal.",
             "The lake looked at me twice: by day with water eyes, by night with glass eyes."),
            ("Los relojes de sol del pueblo se pararon cuando el cielo se partió. Al sol, todavía recuerdan cómo andar.",
             "The village sundials stopped when the sky split. In the sun, they still remember how to move."),
            ("Las Sombras no son malas. Solo tienen miedo de la luz, como yo tenía miedo de la oscuridad.",
             "Shadows aren't evil. They're just afraid of light, like I was afraid of the dark."),
            ("Una abeja de la noche se posó en mi farol. Creo que también buscaba la luna.",
             "A night bee landed on my lantern. I think it was looking for the moon too."),
            ("Al revés el mundo no está roto. Solo está mirando hacia el otro lado.",
             "Upside down, the world isn't broken. It's just looking the other way."),
            ("En el bosque las sierras cantan. El abuelo decía que eran los dientes del viento.",
             "In the forest the saws sing. Grandpa said they were the wind's teeth."),
            ("Las luciérnagas me enseñaron caminos que nadie ve. Hay que acercar la luz para creer.",
             "Fireflies showed me paths no one sees. You have to bring the light close to believe."),
            ("Subí la torre entre dos postes rojos. Desde arriba, el día y la noche parecen hermanos.",
             "I climbed the tower between two red posts. From the top, day and night look like siblings."),
            ("El río de noche lleva estrellas. Si las sigues, llegas a la orilla del cielo.",
             "The night river carries stars. Follow them and you reach the shore of the sky."),
            ("Ahora hay dos líneas. Quizá el mundo siempre tuvo más de una forma de partirse... y de unirse.",
             "Now there are two lines. Maybe the world always had more than one way to split... and to heal."),
            ("El eclipse es cuando el sol y la luna se abrazan. Hoy los he visto abrazarse.",
             "An eclipse is when the sun and moon embrace. Today I saw them embrace."),
            ("La Gran Sombra no quería hacerme daño. Quería que alguien le devolviera la luna. Se la devolveré.",
             "The Great Shadow didn't want to hurt me. It wanted someone to give the moon back. I will."),
        };

        public static bool Has(int level) => PlayerPrefs.GetInt("fase_diario_" + level, 0) == 1;

        public static void Find(int level)
        {
            if (level < 1 || level > Pages.Length || Has(level)) return;
            PlayerPrefs.SetInt("fase_diario_" + level, 1);
            PlayerPrefs.Save();
        }

        public static int Found
        {
            get { int n = 0; for (int i = 1; i <= Pages.Length; i++) if (Has(i)) n++; return n; }
        }
    }
}
