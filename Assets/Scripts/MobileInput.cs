namespace Fase
{
    /// <summary>Estado de los botones táctiles en pantalla. Lo leen PlayerController y otros.</summary>
    public static class MobileInput
    {
        public static float Move;
        public static bool JumpHeld;
        static bool jumpQueued;

        public static void PressJump() { jumpQueued = true; JumpHeld = true; }
        public static void ReleaseJump() => JumpHeld = false;

        /// <summary>Devuelve true una sola vez por cada pulsación.</summary>
        public static bool ConsumeJump()
        {
            bool queued = jumpQueued;
            jumpQueued = false;
            return queued;
        }
    }
}
