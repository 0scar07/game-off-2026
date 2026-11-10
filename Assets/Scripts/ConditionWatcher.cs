using UnityEngine;

namespace Fase
{
    /// <summary>Vigila la condición armada durante el nivel (lo añade LevelLife).</summary>
    public class ConditionWatcher : MonoBehaviour
    {
        int condition = -1, grabs;
        float lastX;
        bool broken;

        void Start()
        {
            var hud = Hud.Instance;
            if (Conditions.Armed < 0 || hud == null || hud.Daily || hud.sandbox) { enabled = false; return; }
            condition = Conditions.Armed;
            Conditions.Armed = -1;
            lastX = ShiftLine.X;
            GameEvents.LineGrabbed += OnGrab;
            GameEvents.Died += OnDied;
            GameEvents.GoalReached += OnGoal;
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) LumiBubble.Say(player.transform, "✦ " + Loc.Pick(Conditions.Names[condition].es, Conditions.Names[condition].en), 3f);
        }

        void OnDestroy()
        {
            GameEvents.LineGrabbed -= OnGrab;
            GameEvents.Died -= OnDied;
            GameEvents.GoalReached -= OnGoal;
        }

        void OnGrab() { if (condition == 1 && ++grabs > 5) Break(); }
        void OnDied() { if (condition == 2) Break(); }

        void Update()
        {
            if (condition != 0) return;
            float x = ShiftLine.X;
            if (x < lastX - 0.05f) Break();
            lastX = Mathf.Max(lastX, x);
        }

        void OnGoal()
        {
            if (broken || Hud.Instance == null) return;
            Conditions.MarkDone(condition, Hud.Instance.levelNumber);
            enabled = false;
        }

        void Break()
        {
            if (broken) return;
            broken = true;
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) LumiBubble.Say(player.transform, Loc.Pick("¡Condición rota! Otra vez...", "Condition broken! Again..."), 1.6f);
            // El mismo nivel desde el principio, con la misma condición.
            Conditions.Armed = condition;
            Invoke(nameof(Restart), 1.5f);
        }

        void Restart()
        {
            if (Hud.Instance != null) SaveGuard.ClearCheckpoint(Hud.Instance.levelNumber);
            SceneFader.Load(gameObject.scene.name);
        }
    }
}
