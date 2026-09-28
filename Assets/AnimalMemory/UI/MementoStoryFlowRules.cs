using System;
using AnimalMemory.Progression;

namespace AnimalMemory.UI
{
    public static class MementoStoryFlowRules
    {
        public static bool ShouldOfferGuideEquip(
            MementoMatchStoryScene completedScene)
        {
            return completedScene != null &&
                completedScene.Phase == MementoMatchStoryPhase.Recruit &&
                completedScene.WorldId >= 0 &&
                completedScene.WorldId < MementoMatchCampaignRules.WorldCount;
        }

        public static bool ShouldPresentFinalCelebration(
            bool endingUnlocked,
            bool lastResultWasCampaign,
            int lastCampaignLevel)
        {
            return endingUnlocked &&
                lastResultWasCampaign &&
                lastCampaignLevel == MementoMatchCampaignRules.LevelCount - 1;
        }

        public static int GetReplaySceneId(int worldId, int phaseIndex)
        {
            if (worldId < 0 || worldId >= MementoMatchCampaignRules.WorldCount)
                throw new ArgumentOutOfRangeException(nameof(worldId));
            if (phaseIndex < 0 || phaseIndex > 2)
                throw new ArgumentOutOfRangeException(nameof(phaseIndex));
            return worldId * 3 + phaseIndex;
        }

        public static bool IsReplaySceneAvailable(
            int sceneId,
            Func<int, bool> isStorySeen)
        {
            return isStorySeen != null &&
                sceneId >= 0 &&
                sceneId <= MementoMatchCampaignRules.EpilogueSceneId &&
                isStorySeen(sceneId);
        }

        public static bool IsReplayWorldAvailable(
            int worldId,
            Func<int, bool> isStorySeen)
        {
            if (isStorySeen == null ||
                worldId < 0 ||
                worldId >= MementoMatchCampaignRules.WorldCount)
                return false;

            for (int phase = 0; phase < 3; phase++)
            {
                if (isStorySeen(GetReplaySceneId(worldId, phase)))
                    return true;
            }

            return false;
        }
    }
}
