using System;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Tres retos cada semana (los mismos para todo el mundo esa semana). Al completar los tres
    /// se gana la bufanda de rayas. Escucha los avisos del juego en cualquier escena.
    /// </summary>
    public static class WeeklyChallenges
    {
        public class Challenge
        {
            public string id, es, en;
            public int goal;
            public string Text => Loc.Pick(es, en);
        }

        static readonly Challenge[] Pool =
        {
            new Challenge { id = "sincaer", es = "Termina 3 niveles sin caer", en = "Finish 3 levels without falling", goal = 3 },
            new Challenge { id = "lunas", es = "Coge 10 fragmentos de luna", en = "Collect 10 moon fragments", goal = 10 },
            new Challenge { id = "sombras", es = "Pasa junto a 5 Sombras dormidas", en = "Walk past 5 sleeping Shadows", goal = 5 },
            new Challenge { id = "saltos", es = "Salta 300 veces", en = "Jump 300 times", goal = 300 },
            new Challenge { id = "linea", es = "Mueve la línea 150 veces", en = "Move the line 150 times", goal = 150 },
            new Challenge { id = "cofres", es = "Abre 2 cofres secretos", en = "Open 2 secret chests", goal = 2 },
            new Challenge { id = "niveles", es = "Termina 6 niveles", en = "Finish 6 levels", goal = 6 },
            new Challenge { id = "diario", es = "Termina 2 desafíos diarios", en = "Finish 2 daily challenges", goal = 2 },
        };

        static int Week => (int)((DateTime.Now.Date - new DateTime(2026, 1, 5)).TotalDays / 7);

        /// <summary>Los tres retos de esta semana.</summary>
        public static Challenge[] ThisWeek
        {
            get
            {
                var picked = new Challenge[3];
                int w = Mathf.Abs(Week);
                for (int i = 0; i < 3; i++) picked[i] = Pool[(w * 3 + i * 3 + i) % Pool.Length];
                if (picked[1] == picked[0]) picked[1] = Pool[(Array.IndexOf(Pool, picked[0]) + 1) % Pool.Length];
                if (picked[2] == picked[0] || picked[2] == picked[1]) picked[2] = Pool[(Array.IndexOf(Pool, picked[1]) + 2) % Pool.Length];
                return picked;
            }
        }

        static string Key(string id) => $"fase_semana_{Week}_{id}";

        public static int Progress(Challenge c) => Mathf.Min(c.goal, PlayerPrefs.GetInt(Key(c.id), 0));

        public static bool AllDone { get { foreach (var c in ThisWeek) if (Progress(c) < c.goal) return false; return true; } }

        /// <summary>Alguna semana completa (desbloquea la bufanda de rayas).</summary>
        public static bool EverCompleted => PlayerPrefs.GetInt("fase_semana_completada", 0) == 1;

        static void Add(string id, int amount = 1)
        {
            foreach (var c in ThisWeek)
            {
                if (c.id != id) continue;
                PlayerPrefs.SetInt(Key(id), PlayerPrefs.GetInt(Key(id), 0) + amount);
                if (AllDone && !EverCompleted) { PlayerPrefs.SetInt("fase_semana_completada", 1); PlayerPrefs.Save(); }
            }
        }

        static bool fellThisLevel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Listen()
        {
            GameEvents.Collected += () => Add("lunas");
            GameEvents.Jumped += () => Add("saltos");
            GameEvents.LineGrabbed += () => Add("linea");
            GameEvents.ChestOpened += () => Add("cofres");
            GameEvents.Died += () => fellThisLevel = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) => fellThisLevel = false;
            GameEvents.GoalReached += () =>
            {
                var hud = Hud.Instance;
                if (hud == null || hud.sandbox) return;
                if (hud.Daily) { Add("diario"); return; }
                Add("niveles");
                if (!fellThisLevel) Add("sincaer");
            };
        }

        /// <summary>Una Sombra dormida se quedó atrás (lo avisa Achievements).</summary>
        public static void PassedShadow() => Add("sombras");
    }
}
