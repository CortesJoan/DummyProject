namespace AnimalMemory.UI
{
    public enum MementoTutorialStage
    {
        Inactive,
        Introduction,
        FindFirstPair,
        ComboLesson,
        FindSecondPair,
        GuideLesson,
        Complete
    }

    /// <summary>
    /// Tracks contextual lessons without pausing play or requiring a modal dismissal.
    /// Basic matching and the first equipped ability have independent save flags.
    /// </summary>
    public sealed class MementoTutorialFlow
    {
        public MementoTutorialStage Stage { get; private set; } =
            MementoTutorialStage.Inactive;

        public bool IsOverlayVisible =>
            Stage == MementoTutorialStage.Introduction ||
            Stage == MementoTutorialStage.ComboLesson ||
            Stage == MementoTutorialStage.GuideLesson;

        public bool IsWaitingForMove =>
            Stage == MementoTutorialStage.Introduction ||
            Stage == MementoTutorialStage.FindFirstPair ||
            Stage == MementoTutorialStage.ComboLesson ||
            Stage == MementoTutorialStage.FindSecondPair;

        public bool TryBegin(bool alreadyCompleted, int campaignLevel)
        {
            if (alreadyCompleted || campaignLevel != 0 ||
                (Stage != MementoTutorialStage.Inactive &&
                 Stage != MementoTutorialStage.Complete))
                return false;

            Stage = MementoTutorialStage.Introduction;
            return true;
        }

        /// <summary>Starts only when the equipped, recruited guide can actually be used.</summary>
        public bool TryBeginGuideLesson(
            bool alreadyCompleted,
            int equippedGuideId,
            bool guideUnlocked,
            bool powerUsable)
        {
            if (alreadyCompleted || equippedGuideId < 0 ||
                !guideUnlocked || !powerUsable ||
                (Stage != MementoTutorialStage.Inactive &&
                 Stage != MementoTutorialStage.Complete))
                return false;

            Stage = MementoTutorialStage.GuideLesson;
            return true;
        }

        // Dismissing a basic hint never gates the next card touch.
        public bool Continue()
        {
            switch (Stage)
            {
                case MementoTutorialStage.Introduction:
                    Stage = MementoTutorialStage.FindFirstPair;
                    return false;
                case MementoTutorialStage.ComboLesson:
                    Stage = MementoTutorialStage.FindSecondPair;
                    return false;
                case MementoTutorialStage.GuideLesson:
                    Stage = MementoTutorialStage.Complete;
                    return true;
                default:
                    return false;
            }
        }

        public bool ObservePlayerMove(bool matched)
        {
            if (!matched)
                return false;

            if (Stage == MementoTutorialStage.Introduction ||
                Stage == MementoTutorialStage.FindFirstPair)
            {
                Stage = MementoTutorialStage.ComboLesson;
                return true;
            }

            if (Stage == MementoTutorialStage.ComboLesson ||
                Stage == MementoTutorialStage.FindSecondPair)
            {
                Stage = MementoTutorialStage.Complete;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            Stage = MementoTutorialStage.Inactive;
        }
    }
}
