using UnityEngine;

namespace Fase
{
    /// <summary>En la carrera a dos: crea al segundo jugador (en lila) y anuncia quién gana.</summary>
    public class RaceSetup : MonoBehaviour
    {
        PlayerController one, two;
        bool decided;

        void Start()
        {
            if (!RaceMode.Active) { enabled = false; return; }
            one = FindAnyObjectByType<PlayerController>();
            if (one == null) return;
            // Copia bajo un padre apagado para quitarle lo que no debe ir repetido antes de que arranque.
            var holder = new GameObject("Jugador2");
            holder.SetActive(false);
            var copy = Instantiate(one.gameObject, one.transform.position + Vector3.right * 1.2f, one.transform.rotation, holder.transform);
            foreach (var extra in new System.Type[] { typeof(LumiSleep), typeof(LanternPower), typeof(Ghost), typeof(Footprints) })
            {
                var c = copy.GetComponent(extra);
                if (c != null) DestroyImmediate(c);
            }
            two = copy.GetComponent<PlayerController>();
            two.playerIndex = 1;
            copy.transform.SetParent(null);
            Destroy(holder);
            copy.SetActive(true);
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", new Color(0.75f, 0.6f, 1f));
            foreach (var r in copy.GetComponentsInChildren<SkinnedMeshRenderer>()) r.SetPropertyBlock(block);
            var cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null) cam.second = copy.GetComponent<Rigidbody2D>();
            LumiBubble.Say(one.transform, Loc.Pick("¡Carrera! J1: A/D · J2: flechas", "Race! P1: A/D · P2: arrows"), 3f);
            GameEvents.GoalReached += OnGoal;
        }

        void OnDestroy() => GameEvents.GoalReached -= OnGoal;

        void OnGoal()
        {
            if (decided || one == null || two == null) return;
            decided = true;
            var goal = FindAnyObjectByType<LevelGoal>();
            Vector3 g = goal != null ? goal.transform.position : Vector3.zero;
            bool firstWins = Vector2.Distance(one.transform.position, g) <= Vector2.Distance(two.transform.position, g);
            var winner = firstWins ? one : two;
            LumiBubble.Say(winner.transform, Loc.Pick(firstWins ? "¡Gana el jugador 1!" : "¡Gana el jugador 2!", firstWins ? "Player 1 wins!" : "Player 2 wins!"), 3f);
        }
    }
}
