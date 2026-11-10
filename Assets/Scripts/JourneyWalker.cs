using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Lumi camina por el mapa del viaje: ← y → la llevan de nivel en nivel (solo hasta el último
    /// abierto) y Enter entra en el nivel en el que está.
    /// </summary>
    public class JourneyWalker : MonoBehaviour
    {
        MainMenu menu;
        RectTransform figure, body;
        int level = 1;
        bool open = true;
        float facing = 1f;

        public static JourneyWalker Create(MainMenu menu)
        {
            var root = new GameObject("LumiViaje", typeof(RectTransform));
            root.transform.SetParent(menu.journeyPanel.transform, false);
            var walker = root.AddComponent<JourneyWalker>();
            walker.menu = menu;
            walker.figure = (RectTransform)root.transform;
            walker.figure.anchorMin = walker.figure.anchorMax = new Vector2(0.5f, 0.5f);
            walker.figure.sizeDelta = new Vector2(60f, 70f);
            var halo = UiKit.Box(root.transform, "Halo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90f, 90f), new Color(1f, 0.9f, 0.6f, 0.25f));
            halo.raycastTarget = false;
            var body = UiKit.Box(root.transform, "Cuerpo", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46f, 52f), new Color(1f, 0.93f, 0.78f));
            body.raycastTarget = false;
            walker.body = body.rectTransform;
            foreach (float x in new[] { -9f, 9f })
            {
                var eye = UiKit.Box(body.transform, "Ojo", new Vector2(0.5f, 0.5f), new Vector2(x, 6f), new Vector2(7f, 10f), new Color(0.12f, 0.1f, 0.2f));
                eye.raycastTarget = false;
            }
            menu.Selected += walker.OnSelected;
            // Lumi hace de marcador: el triángulo que botaba encima ya no hace falta.
            if (menu.marker != null) { menu.marker.gameObject.SetActive(false); menu.marker = null; }
            return walker;
        }

        void OnDestroy() { if (menu != null) menu.Selected -= OnSelected; }

        void OnSelected(int value, bool isOpen) { level = value; open = isOpen; }

        Vector2 Target => menu.nodes.Length == 0 ? Vector2.zero
            : ((RectTransform)menu.nodes[Mathf.Clamp(level, 1, menu.nodes.Length) - 1].transform).anchoredPosition + new Vector2(0f, 82f);


        void OnEnable() { if (menu != null && figure != null) figure.anchoredPosition = Target; }

        void Update()
        {
            if (menu == null || !menu.journeyPanel.activeInHierarchy) return;
            HandleKeys();
            var target = Target;
            var now = figure.anchoredPosition;
            float dx = target.x - now.x;
            if (Mathf.Abs(dx) > 2f) facing = Mathf.Sign(dx);
            bool walking = (target - now).sqrMagnitude > 16f;
            figure.anchoredPosition = Vector2.MoveTowards(now, target, Time.unscaledDeltaTime * 900f);
            float t = Time.unscaledTime;
            body.anchoredPosition = new Vector2(0f, walking ? Mathf.Abs(Mathf.Sin(t * 14f)) * 10f : Mathf.Sin(t * 2.5f) * 3f);
            body.localScale = new Vector3(facing, 1f, 1f);
            body.localRotation = Quaternion.Euler(0f, 0f, walking ? Mathf.Sin(t * 14f) * 6f : 0f);
        }

        void HandleKeys()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            int count = menu.nodes.Length;
            int last = Mathf.Clamp(Progress.Unlocked, 1, Mathf.Max(1, count));
            int step = (kb.rightArrowKey.wasPressedThisFrame ? 1 : 0) - (kb.leftArrowKey.wasPressedThisFrame ? 1 : 0);
            if (step != 0)
            {
                int next = Mathf.Clamp(level + step, 1, last);
                if (next != level)
                {
                    menu.SelectLevel(next);
                    Sfx.Play("boton", 0.4f, 1.3f);
                }
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(menu.nodes[next - 1].gameObject);
            }
            if ((kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) && open)
            {
                var focus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                bool onNode = focus == null || System.Array.Exists(menu.nodes, n => n != null && n.gameObject == focus);
                if (onNode) menu.PlaySelected();
            }
        }
    }
}
