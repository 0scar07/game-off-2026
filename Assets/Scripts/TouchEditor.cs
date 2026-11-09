using UnityEngine;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// Editar los botones táctiles (desde la pausa, en el móvil): se arrastran a donde quieras, se
    /// toca uno y se agranda o achica, y se guarda al pulsar Listo.
    /// </summary>
    public static class TouchEditor
    {
        public static void Open(GameObject pausePanel)
        {
            var buttons = Object.FindObjectsByType<MobileButton>();
            if (buttons.Length == 0) return;
            MobileButton.Editing = true;
            MobileButton.Selected = buttons[0];
            if (pausePanel != null) pausePanel.SetActive(false);
            var canvas = UiKit.Canvas("EditarTactiles", 60);
            var root = canvas.transform;
            var top = UiKit.Box(root, "Aviso", new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1300f, 90f), new Color(0.03f, 0.03f, 0.1f, 0.85f));
            top.rectTransform.pivot = new Vector2(0.5f, 1f);
            UiKit.Text(top.transform, "Texto", Loc.Pick("Arrastra los botones. Toca uno y cambia su tamaño.", "Drag the buttons. Tap one and change its size."), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1260f, 80f), 34);
            float x = -480f;
            foreach (var (label, action) in new (string, System.Action)[]
            {
                ("−", () => MobileButton.Selected?.Resize(0.85f)),
                ("+", () => MobileButton.Selected?.Resize(1.18f)),
                (Loc.Pick("Restablecer", "Reset"), () => { foreach (var b in buttons) b.ResetLayout(); }),
                (Loc.Pick("Listo", "Done"), () =>
                {
                    foreach (var b in buttons) b.SaveLayout();
                    PlayerPrefs.Save();
                    MobileButton.Editing = false;
                    Object.Destroy(canvas.gameObject);
                    if (pausePanel != null) pausePanel.SetActive(true);
                }),
            })
            {
                var act = action;
                UiKit.Button(root, label, label, new Vector2(0.5f, 1f), new Vector2(x, -180f), new Vector2(label.Length > 2 ? 300f : 120f, 90f), () => act(), 40);
                x += label.Length > 2 ? 340f : 160f;
            }
        }
    }
}
