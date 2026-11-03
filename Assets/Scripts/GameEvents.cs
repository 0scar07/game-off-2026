using System;
using UnityEngine;

namespace Fase
{
    /// <summary>
    /// Lo que pasa en el juego, para que el sonido (y luego efectos, cámara...) reaccione
    /// sin que cada script tenga que conocer a los demás.
    /// </summary>
    public static class GameEvents
    {
        public static event Action Jumped;
        public static event Action<float> Landed;          // velocidad de caída
        public static event Action<bool> Step;             // true = paso en el lado de noche
        public static event Action Died;
        public static event Action LineGrabbed;
        public static event Action LineReleased;
        public static event Action LineTick;               // la línea avanzó un tramo
        public static event Action<Vector3, World> PieceAppeared;
        public static event Action<Vector3, World> PieceVanished;
        public static event Action GoalReached;
        public static event Action Collected;
        public static event Action CheckpointReached;
        public static event Action Bounced;
        public static event Action ChestOpened;
        public static event Action<string> Achievement;     // id del logro conseguido

        public static void RaiseJumped() => Jumped?.Invoke();
        public static void RaiseLanded(float fallSpeed) => Landed?.Invoke(fallSpeed);
        public static void RaiseStep(bool night) => Step?.Invoke(night);
        public static void RaiseDied() => Died?.Invoke();
        public static void RaiseLineGrabbed() => LineGrabbed?.Invoke();
        public static void RaiseLineReleased() => LineReleased?.Invoke();
        public static void RaiseLineTick() => LineTick?.Invoke();
        public static void RaisePieceAppeared(Vector3 at, World world) => PieceAppeared?.Invoke(at, world);
        public static void RaisePieceVanished(Vector3 at, World world) => PieceVanished?.Invoke(at, world);
        public static void RaiseGoalReached() => GoalReached?.Invoke();
        public static void RaiseCollected() => Collected?.Invoke();
        public static void RaiseCheckpoint() => CheckpointReached?.Invoke();
        public static void RaiseBounced() => Bounced?.Invoke();
        public static void RaiseChestOpened() => ChestOpened?.Invoke();
        public static void RaiseAchievement(string id) => Achievement?.Invoke(id);
    }
}
