using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>Pone en un Text el texto de `key` (ver Loc) y lo cambia si se cambia el idioma.</summary>
    [RequireComponent(typeof(Text))]
    public class LocText : MonoBehaviour
    {
        public string key;

        void OnEnable()
        {
            Loc.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => Loc.Changed -= Refresh;

        public void Refresh() => GetComponent<Text>().text = Loc.Get(key);
    }
}
