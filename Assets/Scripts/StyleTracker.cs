using UnityEngine;

namespace Fase
{
    /// <summary>Lleva la cuenta de las medallas de estilo durante un nivel de la historia.</summary>
    public class StyleTracker : MonoBehaviour
    {
        PlayerController player;
        Rigidbody2D body;
        bool wasDay, everNight, hardLanding, airborne;
        int crossings;
        float takeoffX;

        void Start()
        {
            var hud = Hud.Instance;
            if (hud == null || hud.Daily || hud.sandbox) { enabled = false; return; }
            player = FindAnyObjectByType<PlayerController>();
            if (player == null) { enabled = false; return; }
            body = player.GetComponent<Rigidbody2D>();
            wasDay = ShiftLine.IsDay(player.transform.position);
            GameEvents.Landed += OnLanded;
            GameEvents.Jumped += OnJumped;
            GameEvents.GoalReached += OnGoal;
        }

        void OnDestroy()
        {
            GameEvents.Landed -= OnLanded;
            GameEvents.Jumped -= OnJumped;
            GameEvents.GoalReached -= OnGoal;
        }

        void OnJumped() { airborne = true; takeoffX = player.transform.position.x; }

        void OnLanded(float speed)
        {
            if (speed > 16f) hardLanding = true;
            if (airborne && Mathf.Abs(player.transform.position.x - takeoffX) >= 10f) StyleMedals.Give("rasante", player.transform);
            airborne = false;
        }

        void Update()
        {
            bool day = ShiftLine.IsDay(player.transform.position);
            if (!day) everNight = true;
            if (day != wasDay && ++crossings == 50) StyleMedals.Give("funambulista", player.transform);
            wasDay = day;
        }

        void OnGoal()
        {
            if (!everNight) StyleMedals.Give("sinnoche", player.transform);
            if (!hardLanding) StyleMedals.Give("pies", player.transform);
        }
    }
}
