using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fase
{
    /// <summary>
    /// La interfaz está hecha a 1920×1080 y escala por la altura. En pantallas más estrechas que
    /// 16:9 (4:3, tabletas, móvil en vertical) eso cortaba los lados (la barra del menú, el mapa),
    /// así que ahí pasa a escalar por la anchura y todo cabe. En pantallas más anchas no cambia.
    /// Con el móvil en vertical, dentro de un nivel se ve muy poco camino: un aviso pide girarlo.
    /// </summary>
    public class AspectFit : MonoBehaviour
    {
        int width, height;
        bool dirty = true;
        GameObject turnHint;
        UnityEngine.UI.Text turnText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            var go = new GameObject("AjusteDePantalla");
            go.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(go);
            go.AddComponent<AspectFit>();
        }

        void OnEnable() => SceneManager.sceneLoaded += OnLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnLoaded;
        void OnLoaded(Scene scene, LoadSceneMode mode) => dirty = true;

        void LateUpdate()
        {
            // Los lienzos que se crean después de cargar (pausa, resultados...) también se ajustan.
            if (!dirty && Screen.width == width && Screen.height == height && Time.frameCount % 30 != 0) return;
            dirty = false;
            width = Screen.width;
            height = Screen.height;
            UpdateTurnHint();
            foreach (var scaler in FindObjectsByType<CanvasScaler>())
            {
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) continue;
                var reference = scaler.referenceResolution;
                if (reference.x < 1000f) continue; // el fundido usa la de por defecto y no lo necesita
                float wanted = (float)width / Mathf.Max(1, height) < reference.x / reference.y - 0.01f ? 0f : 1f;
                if (!Mathf.Approximately(scaler.matchWidthOrHeight, wanted)) scaler.matchWidthOrHeight = wanted;
            }
        }

        void UpdateTurnHint()
        {
            bool show = height > width * 1.1f && FindAnyObjectByType<PlayerController>() != null;
            if (show && turnHint == null)
            {
                var canvas = UiKit.Canvas("AvisoGirar", 900);
                canvas.transform.SetParent(transform, false);
                var box = UiKit.Box(canvas.transform, "Aviso", new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1500f, 150f), new Color(0.07f, 0.05f, 0.2f, 0.9f));
                box.rectTransform.pivot = new Vector2(0.5f, 1f);
                turnText = UiKit.Text(box.transform, "Texto", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1440f, 140f), 52, TextAnchor.MiddleCenter, UiKit.Gold);
                turnHint = canvas.gameObject;
            }
            if (turnHint != null)
            {
                turnHint.SetActive(show);
                turnText.text = Loc.Pick("↻ Gira el móvil para ver más camino", "↻ Turn your phone to see more of the path",
                    "↻ Gire o celular para ver mais do caminho", "↻ Tourne ton téléphone pour voir plus de chemin");
            }
        }
    }
}
