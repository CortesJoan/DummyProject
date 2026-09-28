using System;
using System.Collections.Generic;

namespace AnimalMemory.Progression
{
    [Serializable]
    public struct AnimalMemoryProgressionSnapshot
    {
        public int progressionVersion;
        public int pawStars;
        public int unlockedSetMask;
        public int unlockedGuideMask;
        public int selectedSetId;
        public int selectedGuideId;
        public int firstClearDifficultyMask;
        public int defeatedGuideMask;
        public int achievementMask;
        public int campaignClearedMask;
        public int campaignTwoStarMask;
        public int campaignThreeStarMask;
        public int campaignStorySeenMask;
    }

    public sealed class AnimalMemoryProgression
    {
        public const int CurrentVersion = 6;
        private static readonly int[] DifficultyRewards = { 2, 3, 5 };

        public event Action Changed;

        public int PawStars { get; private set; }
        public int UnlockedSetMask { get; private set; }
        public int UnlockedGuideMask { get; private set; }
        public int SelectedSetId { get; private set; }
        public int SelectedGuideId { get; private set; }
        public int FirstClearDifficultyMask { get; private set; }
        public int DefeatedGuideMask { get; private set; }
        public int AchievementMask { get; private set; }
        public int CampaignClearedMask { get; private set; }
        public int CampaignTwoStarMask { get; private set; }
        public int CampaignThreeStarMask { get; private set; }
        public int CampaignStorySeenMask { get; private set; }
        public int CampaignClearedCount => CountBits(CampaignClearedMask);
        public int CampaignTotalStars => MementoMatchCampaignRules.CountTotalStars(
            CampaignClearedMask, CampaignTwoStarMask, CampaignThreeStarMask);
        public bool CampaignEndingUnlocked =>
            CampaignClearedMask == (1 << MementoMatchCampaignRules.LevelCount) - 1;
        public int CampaignRecommendedLevel =>
            MementoMatchCampaignRules.GetRecommendedLevel(CampaignClearedMask);

        public AnimalMemoryProgression()
        {
            ResetToDefaults(false);
        }

        public int AwardVictory(int difficultyId, bool efficiencyBonus)
        {
            if (difficultyId < 0 || difficultyId >= DifficultyRewards.Length)
                throw new ArgumentOutOfRangeException(nameof(difficultyId));

            int difficultyBit = 1 << difficultyId;
            bool firstClear = (FirstClearDifficultyMask & difficultyBit) == 0;
            int reward = DifficultyRewards[difficultyId] + (efficiencyBonus ? 1 : 0);
            if (firstClear)
            {
                reward++;
                FirstClearDifficultyMask |= difficultyBit;
            }

            PawStars = Math.Max(0, PawStars + reward);
            RefreshUnlocksAndSelections();
            Changed?.Invoke();
            return reward;
        }

        public IReadOnlyList<int> RecordResult(MementoMatchResult result)
        {
            int previousMask = AchievementMask;
            AchievementMask = MementoMatchAchievementRules.Evaluate(
                AchievementMask, result, UnlockedSetMask);
            IReadOnlyList<int> unlocked =
                MementoMatchAchievementRules.GetNewAchievements(previousMask, AchievementMask);
            if (unlocked.Count > 0)
                Changed?.Invoke();
            return unlocked;
        }

        public bool CanChallengeGuide(int guideId)
        {
            if (!IsValidGuideId(guideId))
                return false;

            // Guardian duels are unlocked by their campaign boss.
            // The team menu may replay recruited guardians, but it never
            // bypasses the story by spending stars.
            return IsGuideUnlocked(guideId);
        }

        public bool CompleteGuideChallenge(int guideId)
        {
            if (!CanChallengeGuide(guideId))
                return false;

            int previousGuides = UnlockedGuideMask;
            int previousSets = UnlockedSetMask;
            int previousDefeated = DefeatedGuideMask;
            DefeatedGuideMask |= 1 << guideId;
            UnlockedGuideMask |= 1 << guideId;
            UnlockedSetMask |= 1 << guideId;
            RefreshUnlocksAndSelections();
            if (previousGuides != UnlockedGuideMask ||
                previousSets != UnlockedSetMask ||
                previousDefeated != DefeatedGuideMask)
                Changed?.Invoke();
            return true;
        }

        public bool IsCampaignLevelUnlocked(int levelId)
        {
            return MementoMatchCampaignRules.IsLevelUnlocked(
                levelId, CampaignClearedMask);
        }

        public int GetCampaignStars(int levelId)
        {
            if (levelId < 0 || levelId >= MementoMatchCampaignRules.LevelCount)
                return 0;
            return MementoMatchCampaignRules.GetStoredStars(
                levelId,
                CampaignClearedMask,
                CampaignTwoStarMask,
                CampaignThreeStarMask);
        }

        public int RecordCampaignVictory(
            int levelId,
            int turns,
            int pairCount,
            int mismatches,
            int maxCombo)
        {
            if (!IsCampaignLevelUnlocked(levelId))
                return 0;

            int stars = MementoMatchCampaignRules.EvaluateStars(
                levelId, turns, pairCount, mismatches, maxCombo);
            int bit = 1 << levelId;
            int previousStars = GetCampaignStars(levelId);
            CampaignClearedMask |= bit;
            if (stars >= 2)
                CampaignTwoStarMask |= bit;
            if (stars >= 3)
                CampaignThreeStarMask |= bit;
            RefreshUnlocksAndSelections();
            if (GetCampaignStars(levelId) != previousStars || previousStars == 0)
                Changed?.Invoke();
            return GetCampaignStars(levelId);
        }

        public bool IsCampaignStorySeen(int sceneId)
        {
            return sceneId >= 0 &&
                   sceneId <= MementoMatchCampaignRules.EpilogueSceneId &&
                   (CampaignStorySeenMask & (1 << sceneId)) != 0;
        }

        public bool MarkCampaignStorySeen(int sceneId)
        {
            if (sceneId < 0 || sceneId > MementoMatchCampaignRules.EpilogueSceneId)
                return false;
            int bit = 1 << sceneId;
            if ((CampaignStorySeenMask & bit) != 0)
                return false;
            CampaignStorySeenMask |= bit;
            Changed?.Invoke();
            return true;
        }

        public bool CompleteCampaignBoss(int guideId)
        {
            if (!IsValidGuideId(guideId))
                return false;
            int previousGuides = UnlockedGuideMask;
            int previousSets = UnlockedSetMask;
            int previousDefeated = DefeatedGuideMask;
            DefeatedGuideMask |= 1 << guideId;
            UnlockedGuideMask |= 1 << guideId;
            UnlockedSetMask |= 1 << guideId;
            RefreshUnlocksAndSelections();
            if (previousGuides != UnlockedGuideMask ||
                previousSets != UnlockedSetMask ||
                previousDefeated != DefeatedGuideMask)
                Changed?.Invoke();
            return true;
        }

        public bool IsSetUnlocked(int setId)
        {
            return IsValidSetId(setId) && (UnlockedSetMask & (1 << setId)) != 0;
        }

        public bool IsGuideUnlocked(int guideId)
        {
            return IsValidGuideId(guideId) && (UnlockedGuideMask & (1 << guideId)) != 0;
        }

        public bool IsAchievementUnlocked(int achievementId)
        {
            return achievementId >= 0 &&
                   achievementId < MementoMatchAchievementIds.Count &&
                   (AchievementMask & (1 << achievementId)) != 0;
        }

        public int GetSetUnlockRequirement(int setId)
        {
            if (setId == AnimalMemoryContentIds.ClockworkSet)
                return MementoMatchCampaignRules.LevelCount;
            return setId < AnimalMemoryContentIds.GuideCount
                ? GetGuideUnlockRequirement(setId)
                : 0;
        }

        public int GetGuideUnlockRequirement(int guideId)
        {
            return GuideChallengeRules.GetRequiredStars(guideId);
        }

        public string GetSetName(int setId)
        {
            switch (setId)
            {
                case AnimalMemoryContentIds.ForestSet: return "Aki · Criaturas del Bosque";
                case AnimalMemoryContentIds.CoralSet: return "Mika · Reliquias Coral";
                case AnimalMemoryContentIds.ConstellationSet: return "Yoru · Cielo de Tinta";
                case AnimalMemoryContentIds.GardenSet: return "Hana · Jardín Errante";
                case AnimalMemoryContentIds.SweetsSet: return "Momo · Atelier de Dulces";
                case AnimalMemoryContentIds.ClockworkSet: return "Archivo · Relojería Antigua";
                default: return string.Empty;
            }
        }

        public string GetGuideName(int guideId)
        {
            switch (guideId)
            {
                case AnimalMemoryContentIds.AkiGuide: return "Aki";
                case AnimalMemoryContentIds.MikaGuide: return "Mika";
                case AnimalMemoryContentIds.YoruGuide: return "Yoru";
                case AnimalMemoryContentIds.HanaGuide: return "Hana";
                case AnimalMemoryContentIds.MomoGuide: return "Momo";
                case AnimalMemoryContentIds.UltimateMasterOpponent: return "Rei";
                default: return string.Empty;
            }
        }

        public string GetGuideSkillDescription(int guideId)
        {
            switch (guideId)
            {
                case AnimalMemoryContentIds.AkiGuide:
                    return $"Deshacer: revierte la última jugada y restaura el combo anterior, incluso si acertaste. Recarga: {AnimalMemoryGuideRunState.AkiCooldownTurns} turnos.";
                case AnimalMemoryContentIds.MikaGuide:
                    return $"Escudo de combo: conserva tu racha ante el próximo fallo que fuera a romperla. Recarga: {AnimalMemoryGuideRunState.MikaCooldownTurns} turnos.";
                case AnimalMemoryContentIds.YoruGuide:
                    return $"Visión estelar: muestra brevemente una pareja oculta sin completarla. Recarga: {AnimalMemoryGuideRunState.YoruCooldownTurns} turnos.";
                case AnimalMemoryContentIds.HanaGuide:
                    return $"Floración: completa una pareja oculta y hace daño en duelo. Recarga: {AnimalMemoryGuideRunState.HanaCooldownTurns} turnos.";
                case AnimalMemoryContentIds.MomoGuide:
                    return $"Encore dulce: la próxima jugada no consume turno ni cede el control, aunque falles. Recarga: {AnimalMemoryGuideRunState.MomoCooldownTurns} turnos.";
                default:
                    return string.Empty;
            }
        }

        public string GetAchievementName(int achievementId)
        {
            return MementoMatchAchievementIds.GetName(achievementId);
        }

        public string GetAchievementDescription(int achievementId)
        {
            return MementoMatchAchievementIds.GetDescription(achievementId);
        }

        public bool TrySelectSet(int setId)
        {
            if (!IsSetUnlocked(setId))
                return false;

            if (SelectedSetId != setId)
            {
                SelectedSetId = setId;
                Changed?.Invoke();
            }

            return true;
        }

        public bool TrySelectGuide(int guideId)
        {
            if (!IsGuideUnlocked(guideId))
                return false;

            if (SelectedGuideId != guideId)
            {
                SelectedGuideId = guideId;
                Changed?.Invoke();
            }

            return true;
        }

        public AnimalMemoryProgressionSnapshot CreateSnapshot()
        {
            return new AnimalMemoryProgressionSnapshot
            {
                progressionVersion = CurrentVersion,
                pawStars = PawStars,
                unlockedSetMask = UnlockedSetMask,
                unlockedGuideMask = UnlockedGuideMask,
                selectedSetId = SelectedSetId,
                selectedGuideId = SelectedGuideId,
                firstClearDifficultyMask = FirstClearDifficultyMask,
                defeatedGuideMask = DefeatedGuideMask,
                achievementMask = AchievementMask,
                campaignClearedMask = CampaignClearedMask,
                campaignTwoStarMask = CampaignTwoStarMask,
                campaignThreeStarMask = CampaignThreeStarMask,
                campaignStorySeenMask = CampaignStorySeenMask
            };
        }

        public void LoadSnapshot(AnimalMemoryProgressionSnapshot snapshot)
        {
            if (snapshot.progressionVersion <= 0 ||
                snapshot.progressionVersion > CurrentVersion)
            {
                ResetToDefaults(true);
                return;
            }

            PawStars = Math.Max(0, snapshot.pawStars);
            UnlockedSetMask = snapshot.unlockedSetMask | (1 << AnimalMemoryContentIds.ForestSet);
            UnlockedGuideMask = snapshot.unlockedGuideMask;
            SelectedSetId = snapshot.selectedSetId;
            SelectedGuideId = snapshot.selectedGuideId;
            FirstClearDifficultyMask = snapshot.firstClearDifficultyMask & 0b111;
            AchievementMask = snapshot.achievementMask;
            int campaignMask = (1 << MementoMatchCampaignRules.LevelCount) - 1;
            CampaignClearedMask = snapshot.progressionVersion >= 4
                ? snapshot.campaignClearedMask & campaignMask
                : 0;
            CampaignTwoStarMask = snapshot.progressionVersion >= 4
                ? snapshot.campaignTwoStarMask & CampaignClearedMask
                : 0;
            CampaignThreeStarMask = snapshot.progressionVersion >= 4
                ? snapshot.campaignThreeStarMask & CampaignTwoStarMask
                : 0;
            CampaignStorySeenMask = snapshot.progressionVersion >= 4
                ? snapshot.campaignStorySeenMask & ((1 << (MementoMatchCampaignRules.EpilogueSceneId + 1)) - 1)
                : 0;
            // Scene 15 used to be the old epilogue. In v6 it is Rei's reveal,
            // so returning players must see it before the new final duel.
            if (snapshot.progressionVersion < 6)
                CampaignStorySeenMask &= ~(1 << MementoMatchCampaignRules.FinalBossSceneId);

            if (snapshot.progressionVersion == 1)
            {
                DefeatedGuideMask = UnlockedGuideMask &
                    ((1 << AnimalMemoryContentIds.MikaGuide) |
                     (1 << AnimalMemoryContentIds.YoruGuide));
            }
            else
            {
                DefeatedGuideMask = snapshot.defeatedGuideMask;
            }

            // Saves before v5 always granted Aki for free. Preserve a genuine
            // boss clear, but remove the legacy default unlock otherwise.
            if (snapshot.progressionVersion < 5 &&
                (DefeatedGuideMask & (1 << AnimalMemoryContentIds.AkiGuide)) == 0)
            {
                UnlockedGuideMask &= ~(1 << AnimalMemoryContentIds.AkiGuide);
                if (SelectedGuideId == AnimalMemoryContentIds.AkiGuide)
                    SelectedGuideId = AnimalMemoryContentIds.NoGuide;
            }

            RefreshUnlocksAndSelections();
            Changed?.Invoke();
        }

        public void ResetToDefaults()
        {
            ResetToDefaults(true);
        }

        private void ResetToDefaults(bool notify)
        {
            PawStars = 0;
            UnlockedSetMask = 1 << AnimalMemoryContentIds.ForestSet;
            UnlockedGuideMask = 0;
            SelectedSetId = AnimalMemoryContentIds.ForestSet;
            SelectedGuideId = AnimalMemoryContentIds.NoGuide;
            FirstClearDifficultyMask = 0;
            DefeatedGuideMask = 0;
            AchievementMask = 0;
            CampaignClearedMask = 0;
            CampaignTwoStarMask = 0;
            CampaignThreeStarMask = 0;
            CampaignStorySeenMask = 0;
            if (notify)
                Changed?.Invoke();
        }

        private void RefreshUnlocksAndSelections()
        {
            UnlockedSetMask |= 1 << AnimalMemoryContentIds.ForestSet;

            for (int guideId = AnimalMemoryContentIds.AkiGuide;
                 guideId < AnimalMemoryContentIds.GuideCount;
                 guideId++)
            {
                if ((DefeatedGuideMask & (1 << guideId)) != 0)
                {
                    UnlockedGuideMask |= 1 << guideId;
                    UnlockedSetMask |= 1 << guideId;
                }
            }

            // The Archive deck is the tangible reward for defeating Rei,
            // not a premature unlock for recruiting the last two guardians.
            if ((CampaignClearedMask & (1 << MementoMatchCampaignRules.FinalBossLevelId)) != 0)
                UnlockedSetMask |= 1 << AnimalMemoryContentIds.ClockworkSet;

            if (!IsSetUnlocked(SelectedSetId))
                SelectedSetId = AnimalMemoryContentIds.ForestSet;
            if (!IsGuideUnlocked(SelectedGuideId))
                SelectedGuideId = AnimalMemoryContentIds.NoGuide;
        }

        private static int CountBits(int value)
        {
            int count = 0;
            uint remaining = unchecked((uint)value);
            while (remaining != 0)
            {
                remaining &= remaining - 1;
                count++;
            }
            return count;
        }

        private static bool IsValidSetId(int setId)
        {
            return setId >= 0 && setId < AnimalMemoryContentIds.SetCount;
        }

        private static bool IsValidGuideId(int guideId)
        {
            return guideId >= 0 && guideId < AnimalMemoryContentIds.GuideCount;
        }
    }
}
