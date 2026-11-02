using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>Nombre de un capítulo en el mapa de El viaje, en el idioma elegido.</summary>
    [RequireComponent(typeof(Text))]
    public class ChapterLabel : MonoBehaviour
    {
        [SerializeField] string es, en, roman;

        public void Set(string spanish, string english, string number)
        {
            es = spanish;
            en = english;
            roman = number;
        }

        void OnEnable()
        {
            Loc.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => Loc.Changed -= Refresh;

        void Refresh() => GetComponent<Text>().text = $"{roman} · {Loc.Pick(es, en)}";
    }
}
