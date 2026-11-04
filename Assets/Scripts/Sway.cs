using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Planta pequeña que se dobla hacia la línea al acercarse (la mueve LineReactor). El
    /// generador la deja fuera del "static batching" para que pueda girar.
    /// </summary>
    public class Sway : MonoBehaviour { }
}
