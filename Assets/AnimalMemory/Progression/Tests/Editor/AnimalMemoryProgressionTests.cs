using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AnimalMemory.Progression.Tests
{
    public sealed class AnimalMemoryProgressionTests
    {
        [TestCase(0, false, 3)]
        [TestCase(1, false, 4)]
        [TestCase(2, false, 6)]
        [TestCase(0, true, 4)]
        [TestCase(1, true, 5)]
        [TestCase(2, true, 7)]
        public void VictoryAwardsBaseEfficiencyAndFirstClear(
            int difficultyId, bool efficient, int expectedReward)
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            Assert.That(
                progression.AwardVictory(difficultyId, efficient),
                Is.EqualTo(expectedReward));
        }

        [Test]
        public void AkiStartsLockedAndHerCampaignBossRecruitsHer()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();

            Assert.That(progression.SelectedGuideId, Is.EqualTo(AnimalMemoryContentIds.NoGuide));
            Assert.That(progression.IsGuideUnlocked(AnimalMemoryContentIds.AkiGuide), Is.False);
            Assert.That(progression.CanChallengeGuide(AnimalMemoryContentIds.AkiGuide), Is.False);
            Assert.That(progression.CompleteCampaignBoss(AnimalMemoryContentIds.AkiGuide), Is.True);
            Assert.That(progression.IsGuideUnlocked(AnimalMemoryContentIds.AkiGuide), Is.True);
        }

        [Test]
        public void StarsDoNotBypassStoryRecruitment()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            progression.AwardVictory(2, true);
            progression.AwardVictory(2, true);

            Assert.That(progression.PawStars, Is.EqualTo(13));
            Assert.That(progression.CanChallengeGuide(1), Is.False);
            Assert.That(progression.CanChallengeGuide(2), Is.False);
            Assert.That(progression.IsGuideUnlocked(1), Is.False);
            Assert.That(progression.IsSetUnlocked(1), Is.False);
        }

        [Test]
        public void DefeatingGuideUnlocksGuideAndMatchingTheme()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            progression.AwardVictory(2, true);

            Assert.That(progression.CompleteCampaignBoss(1), Is.True);
            Assert.That(progression.IsGuideUnlocked(1), Is.True);
            Assert.That(progression.IsSetUnlocked(1), Is.True);
            Assert.That(progression.TrySelectGuide(1), Is.True);
            Assert.That(progression.TrySelectSet(1), Is.True);
        }

        [Test]
        public void ChallengeCannotCompleteBeforeStarRequirement()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            Assert.That(progression.CompleteGuideChallenge(1), Is.False);
            Assert.That(progression.IsGuideUnlocked(1), Is.False);
        }

        [Test]
        public void SnapshotRoundTripPreservesChallengesAchievementsAndSelection()
        {
            AnimalMemoryProgression original = new AnimalMemoryProgression();
            original.AwardVictory(2, true);
            original.CompleteCampaignBoss(1);
            original.TrySelectGuide(1);
            original.TrySelectSet(1);
            original.RecordResult(new MementoMatchResult
            {
                DifficultyId = 0,
                Turns = 3,
                PairCount = 3,
                Mismatches = 0,
                MaxCombo = 5,
                DefeatedGuideId = 1
            });

            AnimalMemoryProgression restored = new AnimalMemoryProgression();
            restored.LoadSnapshot(original.CreateSnapshot());

            Assert.That(restored.SelectedGuideId, Is.EqualTo(1));
            Assert.That(restored.SelectedSetId, Is.EqualTo(1));
            Assert.That(restored.DefeatedGuideMask, Is.EqualTo(original.DefeatedGuideMask));
            Assert.That(restored.AchievementMask, Is.EqualTo(original.AchievementMask));
        }

        [Test]
        public void VersionOneSaveMigratesExistingGuideUnlocksAsDefeated()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            progression.LoadSnapshot(new AnimalMemoryProgressionSnapshot
            {
                progressionVersion = 1,
                pawStars = 20,
                unlockedSetMask = 0b111,
                unlockedGuideMask = 0b111,
                selectedSetId = 2,
                selectedGuideId = 2
            });

            Assert.That(progression.DefeatedGuideMask & 0b110, Is.EqualTo(0b110));
            Assert.That(progression.SelectedGuideId, Is.EqualTo(2));
            Assert.That(progression.IsSetUnlocked(2), Is.True);
        }

        [Test]
        public void UnsupportedSaveFallsBackToNoGuide()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            progression.LoadSnapshot(new AnimalMemoryProgressionSnapshot
            {
                progressionVersion = 99,
                pawStars = 999,
                unlockedGuideMask = -1
            });

            Assert.That(progression.PawStars, Is.Zero);
            Assert.That(progression.SelectedGuideId, Is.EqualTo(AnimalMemoryContentIds.NoGuide));
            Assert.That(progression.IsGuideUnlocked(1), Is.False);
        }

        [TestCase(2)]
        [TestCase(8)]
        [TestCase(15)]
        public void PairBuilderProducesExactlyTwoOfEveryPair(int pairCount)
        {
            List<int> pairs = AnimalMemoryPairBuilder.BuildPairedIndices(pairCount);
            Assert.That(pairs, Has.Count.EqualTo(pairCount * 2));
            for (int pairId = 0; pairId < pairCount; pairId++)
                Assert.That(pairs.Count(value => value == pairId), Is.EqualTo(2));
        }

        [TestCase(0, 5)]
        [TestCase(1, 6)]
        [TestCase(2, 7)]
        [TestCase(3, 8)]
        [TestCase(4, 7)]
        public void PowerCooldownMatchesGuideContract(int guideId, int cooldown)
        {
            AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
            state.Reset(guideId);
            Assert.That(state.TryActivatePower(), Is.True);
            Assert.That(state.PowerCooldownRemaining, Is.EqualTo(cooldown));
            Assert.That(state.TryActivatePower(), Is.False);
        }

        [Test]
        public void NoGuideRunStateHasNoActivatablePower()
        {
            AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
            state.Reset(AnimalMemoryContentIds.NoGuide);
            Assert.That(state.SelectedGuideId, Is.EqualTo(AnimalMemoryContentIds.NoGuide));
            Assert.That(state.IsPowerReady, Is.False);
            Assert.That(state.TryActivatePower(), Is.False);
        }

        [Test]
        public void CooldownDecreasesOncePerCompletedTurn()
        {
            AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
            state.Reset(AnimalMemoryContentIds.AkiGuide);
            state.TryActivatePower();

            for (int i = 0; i < 5; i++)
                state.RegisterTurnCompleted();

            Assert.That(state.PowerCooldownRemaining, Is.Zero);
            Assert.That(state.IsPowerReady, Is.True);
        }

        [Test]
        public void MikaShieldMustBeActivatedAndProtectsOnlyOneMismatch()
        {
            AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
            state.Reset(AnimalMemoryContentIds.MikaGuide);

            Assert.That(state.TryProtectCombo(3), Is.False);
            Assert.That(state.TryActivatePower(), Is.True);
            Assert.That(state.MikaProtectionArmed, Is.True);
            Assert.That(state.TryProtectCombo(3), Is.True);
            Assert.That(state.MikaProtectionArmed, Is.False);
            Assert.That(state.TryProtectCombo(3), Is.False);
        }

        [Test]
        public void ResetClearsCooldownAndArmedPower()
        {
            AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
            state.Reset(AnimalMemoryContentIds.MikaGuide);
            state.TryActivatePower();
            state.Reset(AnimalMemoryContentIds.MikaGuide);

            Assert.That(state.PowerCooldownRemaining, Is.Zero);
            Assert.That(state.MikaProtectionArmed, Is.False);
        }

        [TestCase(0, 6, 0)]
        [TestCase(1, 4, 1)]
        [TestCase(2, 8, 2)]
        [TestCase(3, 5, 1)]
        [TestCase(4, 6, 2)]
        public void ChallengeUsesExactMissLimitAndDifficulty(
            int guideId, int missLimit, int difficultyId)
        {
            GuideChallengeState state = new GuideChallengeState();
            state.Begin(guideId);

            for (int i = 1; i < missLimit; i++)
                Assert.That(state.RegisterMismatch(), Is.False);
            Assert.That(state.RegisterMismatch(), Is.True);
            Assert.That(state.IsDefeated, Is.True);
            Assert.That(GuideChallengeRules.GetDifficultyId(guideId), Is.EqualTo(difficultyId));
        }

        [Test]
        public void MomoEncoreIsConsumedExactlyOnce()
        {
            AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
            state.Reset(AnimalMemoryContentIds.MomoGuide);
            Assert.That(state.TryActivatePower(), Is.True);
            Assert.That(state.MomoEncoreArmed, Is.True);
            Assert.That(state.TryConsumeEncore(), Is.True);
            Assert.That(state.TryConsumeEncore(), Is.False);
        }

        [Test]
        public void DefeatingReiUnlocksTheExclusiveArchiveCollection()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            for (int levelId = 0; levelId < MementoMatchCampaignRules.FinalBossLevelId; levelId++)
                progression.RecordCampaignVictory(levelId, 999, 1, 999, 0);

            Assert.That(
                progression.IsSetUnlocked(AnimalMemoryContentIds.ClockworkSet),
                Is.False);
            progression.RecordCampaignVictory(
                MementoMatchCampaignRules.FinalBossLevelId, 999, 15, 999, 0);
            Assert.That(
                progression.IsSetUnlocked(AnimalMemoryContentIds.ClockworkSet),
                Is.True);
        }

        [Test]
        public void AchievementRulesUnlockExpectedMilestones()
        {
            int allSets = (1 << AnimalMemoryContentIds.SetCount) - 1;
            int mask = MementoMatchAchievementRules.Evaluate(0, new MementoMatchResult
            {
                DifficultyId = 0,
                Turns = 3,
                PairCount = 3,
                Mismatches = 0,
                MaxCombo = 10,
                DefeatedGuideId = AnimalMemoryContentIds.MikaGuide
            }, allSets);

            Assert.That(mask & (1 << MementoMatchAchievementIds.FirstVictory), Is.Not.Zero);
            Assert.That(mask & (1 << MementoMatchAchievementIds.PerfectEasy), Is.Not.Zero);
            Assert.That(mask & (1 << MementoMatchAchievementIds.ComboTen), Is.Not.Zero);
            Assert.That(mask & (1 << MementoMatchAchievementIds.Flawless), Is.Not.Zero);
            Assert.That(mask & (1 << MementoMatchAchievementIds.DefeatMika), Is.Not.Zero);
            Assert.That(mask & (1 << MementoMatchAchievementIds.CompleteCollection), Is.Not.Zero);
        }

        [Test]
        public void NewAchievementsOnlyReturnsNewBits()
        {
            int previous = 1 << MementoMatchAchievementIds.FirstVictory;
            int next = previous |
                (1 << MementoMatchAchievementIds.ComboFive) |
                (1 << MementoMatchAchievementIds.Flawless);

            IReadOnlyList<int> unlocked =
                MementoMatchAchievementRules.GetNewAchievements(previous, next);

            Assert.That(unlocked, Is.EquivalentTo(new[]
            {
                MementoMatchAchievementIds.ComboFive,
                MementoMatchAchievementIds.Flawless
            }));
        }

        [TestCase(AnimalMemoryContentIds.ForestSet)]
        [TestCase(AnimalMemoryContentIds.CoralSet)]
        [TestCase(AnimalMemoryContentIds.ConstellationSet)]
        [TestCase(AnimalMemoryContentIds.GardenSet)]
        [TestCase(AnimalMemoryContentIds.SweetsSet)]
        [TestCase(AnimalMemoryContentIds.ClockworkSet)]
        public void EveryCardSetDefinesFifteenDistinctPairIdentities(int setId)
        {
            IReadOnlyList<string> identities =
                AnimalMemoryCardIdentityCatalog.GetIdentityKeys(setId);

            Assert.That(
                identities.Count,
                Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
            Assert.That(
                identities.Distinct().Count(),
                Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
            Assert.That(
                identities.All(key => !string.IsNullOrWhiteSpace(key)),
                Is.True);
        }

        [TestCase(AnimalMemoryContentIds.ForestSet)]
        [TestCase(AnimalMemoryContentIds.CoralSet)]
        [TestCase(AnimalMemoryContentIds.ConstellationSet)]
        [TestCase(AnimalMemoryContentIds.GardenSet)]
        [TestCase(AnimalMemoryContentIds.SweetsSet)]
        [TestCase(AnimalMemoryContentIds.ClockworkSet)]
        public void EveryIdentityMapsToExactlyTwoCardsAndNoOtherPair(int setId)
        {
            IReadOnlyList<string> identities =
                AnimalMemoryCardIdentityCatalog.GetIdentityKeys(setId);
            List<int> pairedIds = AnimalMemoryPairBuilder.BuildPairedIndices(
                identities.Count);

            Assert.That(pairedIds, Has.Count.EqualTo(identities.Count * 2));
            for (int pairId = 0; pairId < identities.Count; pairId++)
            {
                Assert.That(
                    pairedIds.Count(candidate => candidate == pairId),
                    Is.EqualTo(2),
                    $"Identity {identities[pairId]} must own exactly two cards.");
            }

            Assert.That(
                pairedIds.Select(pairId => identities[pairId]).Distinct().Count(),
                Is.EqualTo(identities.Count));
        }

        [Test]
        public void CampaignContainsFiveRecruitmentWorldsAndOneUltimateBoss()
        {
            Assert.That(MementoMatchCampaignRules.BaseLevelCount, Is.EqualTo(20));
            Assert.That(MementoMatchCampaignRules.LevelCount, Is.EqualTo(21));
            for (int levelId = 0; levelId < MementoMatchCampaignRules.BaseLevelCount; levelId++)
            {
                MementoMatchCampaignLevel level = MementoMatchCampaignRules.GetLevel(levelId);
                Assert.That(level.WorldId, Is.EqualTo(levelId / 4));
                Assert.That(level.SetId, Is.EqualTo(level.WorldId));
                Assert.That(level.IsBoss, Is.EqualTo(levelId % 4 == 3));
            }

            MementoMatchCampaignLevel final = MementoMatchCampaignRules.GetLevel(
                MementoMatchCampaignRules.FinalBossLevelId);
            Assert.That(final.IsFinalBoss, Is.True);
            Assert.That(final.Rows, Is.EqualTo(5));
            Assert.That(final.Columns, Is.EqualTo(6));
            Assert.That(final.SetId, Is.EqualTo(AnimalMemoryContentIds.ClockworkSet));
            Assert.That(final.OpponentGuideId, Is.EqualTo(AnimalMemoryContentIds.UltimateMasterOpponent));
        }

        [Test]
        public void CampaignProgressionIsSequentialAndNeverStarGated()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            Assert.That(progression.IsCampaignLevelUnlocked(0), Is.True);
            Assert.That(progression.IsCampaignLevelUnlocked(1), Is.False);

            int stars = progression.RecordCampaignVictory(
                0,
                turns: 99,
                pairCount: 3,
                mismatches: 99,
                maxCombo: 0);

            Assert.That(stars, Is.EqualTo(1));
            Assert.That(progression.IsCampaignLevelUnlocked(1), Is.True);
            Assert.That(progression.CampaignClearedCount, Is.EqualTo(1));
        }

        [Test]
        public void CampaignKeepsBestStarRatingAcrossReplays()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            Assert.That(
                progression.RecordCampaignVictory(0, 3, 3, 0, 3),
                Is.EqualTo(3));
            Assert.That(
                progression.RecordCampaignVictory(0, 99, 3, 99, 0),
                Is.EqualTo(3));
            Assert.That(progression.CampaignTotalStars, Is.EqualTo(3));
        }

        [Test]
        public void CampaignSnapshotPreservesLevelsStarsAndStory()
        {
            AnimalMemoryProgression original = new AnimalMemoryProgression();
            original.RecordCampaignVictory(0, 3, 3, 0, 3);
            original.MarkCampaignStorySeen(0);

            AnimalMemoryProgression restored = new AnimalMemoryProgression();
            restored.LoadSnapshot(original.CreateSnapshot());

            Assert.That(restored.GetCampaignStars(0), Is.EqualTo(3));
            Assert.That(restored.IsCampaignLevelUnlocked(1), Is.True);
            Assert.That(restored.IsCampaignStorySeen(0), Is.True);
        }

        [Test]
        public void CompletingAllTwentyLevelsUnlocksEndingWithoutExtraGate()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            for (int levelId = 0;
                 levelId < MementoMatchCampaignRules.LevelCount;
                 levelId++)
            {
                Assert.That(progression.IsCampaignLevelUnlocked(levelId), Is.True);
                progression.RecordCampaignVictory(levelId, 999, 1, 999, 0);
            }

            Assert.That(progression.CampaignEndingUnlocked, Is.True);
            Assert.That(
                progression.CampaignClearedCount,
                Is.EqualTo(MementoMatchCampaignRules.LevelCount));
            Assert.That(
                progression.CampaignTotalStars,
                Is.EqualTo(MementoMatchCampaignRules.LevelCount));
        }

        [Test]
        public void EveryCampaignChapterHasEnoughShortBeatsToExplainItsStory()
        {
            for (int sceneId = 0;
                 sceneId <= MementoMatchCampaignRules.EpilogueSceneId;
                 sceneId++)
            {
                MementoMatchStoryScene scene =
                    MementoMatchCampaignRules.GetStoryScene(sceneId);
                // El tope protege el ritmo del cuerpo de la campana. El tramo final carga la
                // pausa delante del giro y la coda que siembra el postgame, asi que tiene
                // su propio margen; el resto de escenas mantiene el limite de 12.
                bool finale = sceneId >= MementoMatchCampaignRules.FinalBossSceneId;
                Assert.That(scene.Beats.Count, Is.InRange(5, finale ? 17 : 12));
                Assert.That(
                    scene.Beats.All(beat =>
                        !string.IsNullOrWhiteSpace(beat.Speaker) &&
                        !string.IsNullOrWhiteSpace(beat.Text)),
                    Is.True);
            }
        }


        [Test]
        public void CampaignPremiseGivesTheProtagonistAConcreteFinishLine()
        {
            Assert.That(
                MementoMatchCampaignRules.CampaignTitle,
                Is.EqualTo("La Liga de las Seis Zonas"));
            Assert.That(
                MementoMatchCampaignRules.ProtagonistGoal,
                Does.Contain("Zona Cero").And.Contain("alianza"));
        }

        [Test]
        public void EveryGuardianRecruitSceneExplainsTheEquippedAbility()
        {
            for (int world = 0; world < MementoMatchCampaignRules.WorldCount; world++)
            {
                MementoMatchStoryScene scene =
                    MementoMatchCampaignRules.GetStoryScene(world * 3 + 2);
                Assert.That(scene.Phase, Is.EqualTo(MementoMatchStoryPhase.Recruit));
                Assert.That(
                    scene.Beats.Any(beat => beat.Kind == MementoMatchStoryBeatKind.Recruit),
                    Is.True);
                Assert.That(
                    scene.Beats.Any(beat => beat.Kind == MementoMatchStoryBeatKind.Ability),
                    Is.True);
            }
        }

        [Test]
        public void RetryScenesAreRepeatableAndNeverConsumeStorySaveBits()
        {
            for (int world = 0; world < MementoMatchCampaignRules.WorldCount; world++)
            {
                int bossLevel = world * MementoMatchCampaignRules.LevelsPerWorld + 3;
                MementoMatchStoryScene scene =
                    MementoMatchCampaignRules.GetRetryScene(bossLevel);
                Assert.That(scene.Phase, Is.EqualTo(MementoMatchStoryPhase.Retry));
                Assert.That(scene.IsPersistent, Is.False);
                Assert.That(
                    scene.Beats.Any(beat => beat.Kind == MementoMatchStoryBeatKind.Retry),
                    Is.True);
            }
        }

        [Test]
        public void AkiCannotBeEquippedDuringHerOwnRecruitmentDuel()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            const int akisBossLevel = 3;
            Assert.That(
                MementoMatchCampaignRules.GetLevel(akisBossLevel).OpponentGuideId,
                Is.EqualTo(AnimalMemoryContentIds.AkiGuide));
            for (int levelId = 0; levelId < akisBossLevel; levelId++)
                progression.RecordCampaignVictory(levelId, 999, 1, 999, 0);

            // Her duel is reachable with nothing unlocked, so the player has no
            // power to bring into it: Aki is never equipped against herself in
            // the base campaign.
            Assert.That(progression.IsCampaignLevelUnlocked(akisBossLevel), Is.True);
            Assert.That(progression.UnlockedGuideMask, Is.EqualTo(0));
            Assert.That(
                progression.IsGuideUnlocked(AnimalMemoryContentIds.AkiGuide),
                Is.False);
            Assert.That(
                progression.TrySelectGuide(AnimalMemoryContentIds.AkiGuide),
                Is.False);
            Assert.That(
                progression.SelectedGuideId,
                Is.EqualTo(AnimalMemoryContentIds.NoGuide));

            // Winning that duel is exactly what recruits her.
            Assert.That(
                progression.CompleteCampaignBoss(AnimalMemoryContentIds.AkiGuide),
                Is.True);
            Assert.That(
                progression.IsGuideUnlocked(AnimalMemoryContentIds.AkiGuide),
                Is.True);
        }

        [Test]
        public void CampaignSelectorAdvancesToTheNextUnclearedEntry()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            Assert.That(progression.CampaignRecommendedLevel, Is.EqualTo(0));

            progression.RecordCampaignVictory(0, 999, 1, 999, 0);
            Assert.That(progression.CampaignRecommendedLevel, Is.EqualTo(1));

            for (int levelId = 1;
                 levelId < MementoMatchCampaignRules.BaseLevelCount;
                 levelId++)
                progression.RecordCampaignVictory(levelId, 999, 1, 999, 0);

            Assert.That(
                progression.CampaignRecommendedLevel,
                Is.EqualTo(MementoMatchCampaignRules.FinalBossLevelId));

            progression.RecordCampaignVictory(
                MementoMatchCampaignRules.FinalBossLevelId, 999, 15, 999, 0);
            Assert.That(
                progression.CampaignRecommendedLevel,
                Is.EqualTo(MementoMatchCampaignRules.LevelCount - 1));
        }

        [Test]
        public void FinalDuelIsOfferedOnlyOnceTheTwentyBaseLevelsAreCleared()
        {
            AnimalMemoryProgression progression = new AnimalMemoryProgression();
            for (int levelId = 0;
                 levelId < MementoMatchCampaignRules.BaseLevelCount - 1;
                 levelId++)
                progression.RecordCampaignVictory(levelId, 999, 1, 999, 0);

            Assert.That(
                progression.IsCampaignLevelUnlocked(
                    MementoMatchCampaignRules.FinalBossLevelId),
                Is.False);
            Assert.That(
                MementoMatchCampaignRules.IsFinalBossUnlocked(
                    progression.CampaignClearedMask),
                Is.False);

            progression.RecordCampaignVictory(
                MementoMatchCampaignRules.BaseLevelCount - 1, 999, 1, 999, 0);

            Assert.That(
                progression.IsCampaignLevelUnlocked(
                    MementoMatchCampaignRules.FinalBossLevelId),
                Is.True);
            Assert.That(
                MementoMatchCampaignRules.IsFinalBossUnlocked(
                    progression.CampaignClearedMask),
                Is.True);
            // Rei is a boss, never a recruitable guide: "FINAL REI" is a
            // campaign entry, not a loadout choice.
            Assert.That(
                progression.IsGuideUnlocked(
                    AnimalMemoryContentIds.UltimateMasterOpponent),
                Is.False);
            Assert.That(
                progression.TrySelectGuide(
                    AnimalMemoryContentIds.UltimateMasterOpponent),
                Is.False);
        }

        [Test]
        public void CampaignDoesNotReuseMenuSkillOrDuelOutcomeVoiceCues()
        {
            for (int sceneId = 0;
                 sceneId <= MementoMatchCampaignRules.EpilogueSceneId;
                 sceneId++)
            {
                Assert.That(
                    MementoMatchCampaignRules.GetStoryScene(sceneId)
                        .Beats.All(beat => beat.VoiceCue == MementoMatchStoryVoiceCue.None),
                    Is.True);
            }
        }
    }
}
