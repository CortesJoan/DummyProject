namespace AnimalMemory.UI
{
    /// <summary>
    /// Armed/confirm state for the destructive "delete save" action.
    /// The first request only arms the button; the save is deleted on a second
    /// request while the arm is still valid. Every transition bumps
    /// <see cref="Version"/> so a stale timeout can never disarm a newer arm.
    /// </summary>
    public sealed class DeleteSaveConfirmation
    {
        public const long ArmTimeoutMilliseconds = 4000;

        public bool IsArmed { get; private set; }

        /// <summary>Bumped on every transition so pending timers can detect they are stale.</summary>
        public int Version { get; private set; }

        /// <summary>
        /// Registers one click. Returns true only for the explicit confirmation
        /// that should actually delete the save.
        /// </summary>
        public bool Request()
        {
            if (!IsArmed)
            {
                IsArmed = true;
                Version++;
                return false;
            }

            IsArmed = false;
            Version++;
            return true;
        }

        /// <summary>
        /// Disarms a pending confirmation when its timeout fires. Returns true
        /// while the timer still owns the current arm, false once the arm was
        /// consumed by a confirmation.
        /// </summary>
        public bool Expire(int armedVersion)
        {
            if (!IsArmed || armedVersion != Version)
                return false;

            IsArmed = false;
            Version++;
            return true;
        }
    }
}
