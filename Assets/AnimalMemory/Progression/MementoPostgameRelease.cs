namespace AnimalMemory.Progression
{
    /// <summary>Single release switch for the future postgame; the base campaign remains complete without it.
    /// Static (not const) so tests can flip it and restore; unlock for the update by setting true here.</summary>
    public static class MementoPostgameRelease
    {
        public static bool Enabled = true;
    }
}
