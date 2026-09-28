using System;

namespace AnimalMemory.Progression
{
    public sealed class AnimalMemoryGuideRunState
    {
        public const float AkiRevealBonusSeconds = 0.75f;
        public const int AkiCooldownTurns = 5;
        public const int MikaCooldownTurns = 6;
        public const int YoruCooldownTurns = 7;
        public const int HanaCooldownTurns = 8;
        public const int MomoCooldownTurns = 7;

        private readonly int[] allianceCooldowns = new int[AnimalMemoryContentIds.GuideCount];
        public int AllianceMask { get; private set; }
        public bool IsAllianceActive => AllianceMask != 0;
        public bool AlliancePowerUsedThisTurn { get; private set; }

        /// <summary>Only the explicitly present companions join; Aki has not reconciled.</summary>
        public void EnableAlliance(int presentMask)
        {
            if (IsAllianceActive) return;
            AllianceMask = presentMask & 0x1E;
            if (!IsAllianceActive) return;
            if (SelectedGuideId >= 0)
                allianceCooldowns[SelectedGuideId] = PowerCooldownRemaining;
            if ((AllianceMask & (1 << SelectedGuideId)) == 0)
                for (int id = 1; id < AnimalMemoryContentIds.GuideCount; id++)
                    if ((AllianceMask & (1 << id)) != 0) { SelectAllianceGuide(id); break; }
        }

        public int GetAllianceCooldown(int id) =>
            id >= 0 && id < allianceCooldowns.Length ? allianceCooldowns[id] : 0;

        public bool IsAlliancePowerReady(int id) =>
            IsAllianceActive && id >= 1 && id < allianceCooldowns.Length &&
            (AllianceMask & (1 << id)) != 0 && !AlliancePowerUsedThisTurn &&
            allianceCooldowns[id] == 0 &&
            !(id == AnimalMemoryContentIds.MikaGuide && MikaProtectionArmed) &&
            !(id == AnimalMemoryContentIds.MomoGuide && MomoEncoreArmed);

        public bool SelectAllianceGuide(int id)
        {
            if (id < 1 || id >= allianceCooldowns.Length || (AllianceMask & (1 << id)) == 0)
                return false;
            SelectedGuideId = id;
            PowerCooldownRemaining = allianceCooldowns[id];
            return true;
        }

        public int SelectedGuideId { get; private set; }
        public int ConsecutiveFailures { get; private set; }
        public int PowerCooldownRemaining { get; private set; }
        public bool MikaProtectionArmed { get; private set; }
        public bool MomoEncoreArmed { get; private set; }
        public bool IsPowerReady => IsAllianceActive ? IsAlliancePowerReady(SelectedGuideId) : SelectedGuideId >= 0 && PowerCooldownRemaining <= 0;
        public bool HasPendingPower => MikaProtectionArmed || MomoEncoreArmed;

        // false = CAST (the player armed/used it), true = PROC (its
        // gameplay effect has actually resolved). Views may animate each phase
        // independently without owning any gameplay state.
        public event Action<int, bool> PowerPresentationRequested;
        public float InitialRevealBonusSeconds =>
            SelectedGuideId == AnimalMemoryContentIds.AkiGuide ? AkiRevealBonusSeconds : 0f;

        public int PowerCooldownTurns
        {
            get
            {
                switch (SelectedGuideId)
                {
                    case AnimalMemoryContentIds.MikaGuide: return MikaCooldownTurns;
                    case AnimalMemoryContentIds.YoruGuide: return YoruCooldownTurns;
                    case AnimalMemoryContentIds.HanaGuide: return HanaCooldownTurns;
                    case AnimalMemoryContentIds.MomoGuide: return MomoCooldownTurns;
                    case AnimalMemoryContentIds.AkiGuide: return AkiCooldownTurns;
                    default: return 0;
                }
            }
        }

        public string PowerName
        {
            get
            {
                switch (SelectedGuideId)
                {
                    case AnimalMemoryContentIds.MikaGuide: return "Escudo de combo";
                    case AnimalMemoryContentIds.YoruGuide: return "Visión estelar";
                    case AnimalMemoryContentIds.HanaGuide: return "Floración";
                    case AnimalMemoryContentIds.MomoGuide: return "Encore dulce";
                    case AnimalMemoryContentIds.AkiGuide: return "Deshacer";
                    default: return "Sin guía";
                }
            }
        }

        public AnimalMemoryGuideRunState()
        {
            Reset(AnimalMemoryContentIds.NoGuide);
        }

        public void Reset(int selectedGuideId)
        {
            SelectedGuideId =
                selectedGuideId >= 0 && selectedGuideId < AnimalMemoryContentIds.GuideCount
                    ? selectedGuideId
                    : AnimalMemoryContentIds.NoGuide;
            AllianceMask = 0;
            AlliancePowerUsedThisTurn = false;
            Array.Clear(allianceCooldowns, 0, allianceCooldowns.Length);
            ConsecutiveFailures = 0;
            PowerCooldownRemaining = 0;
            MikaProtectionArmed = false;
            MomoEncoreArmed = false;
        }

        public bool TryActivatePower()
        {
            if (SelectedGuideId < 0 ||
                !IsPowerReady || (!IsAllianceActive && (MikaProtectionArmed || MomoEncoreArmed)))
                return false;

            PowerCooldownRemaining = PowerCooldownTurns;
            if (IsAllianceActive)
            {
                allianceCooldowns[SelectedGuideId] = PowerCooldownRemaining;
                AlliancePowerUsedThisTurn = true;
            }
            if (SelectedGuideId == AnimalMemoryContentIds.MikaGuide)
                MikaProtectionArmed = true;
            else if (SelectedGuideId == AnimalMemoryContentIds.MomoGuide)
                MomoEncoreArmed = true;

            PowerPresentationRequested?.Invoke(SelectedGuideId, false);
            return true;
        }

        public void RegisterTurnCompleted()
        {
            if (IsAllianceActive)
            {
                for (int id = 0; id < allianceCooldowns.Length; id++)
                    if (allianceCooldowns[id] > 0) allianceCooldowns[id]--;
                PowerCooldownRemaining = allianceCooldowns[SelectedGuideId];
                AlliancePowerUsedThisTurn = false;
            }
            else if (PowerCooldownRemaining > 0)
                PowerCooldownRemaining--;
        }

        public bool TryProtectCombo(int currentCombo)
        {
            // Encore resolves after the match event. Protect this attempt without
            // consuming it here: TryConsumeEncore owns the single PROC and extra turn.
            if ((IsAllianceActive || SelectedGuideId == AnimalMemoryContentIds.MomoGuide) &&
                MomoEncoreArmed && currentCombo > 0)
                return true;

            if ((!IsAllianceActive && SelectedGuideId != AnimalMemoryContentIds.MikaGuide) ||
                !MikaProtectionArmed ||
                currentCombo < 1)
                return false;

            MikaProtectionArmed = false;
            PowerPresentationRequested?.Invoke(AnimalMemoryContentIds.MikaGuide, true);
            return true;
        }

        public bool TryConsumeEncore()
        {
            if ((!IsAllianceActive && SelectedGuideId != AnimalMemoryContentIds.MomoGuide) ||
                !MomoEncoreArmed)
                return false;

            MomoEncoreArmed = false;
            PowerPresentationRequested?.Invoke(AnimalMemoryContentIds.MomoGuide, true);
            return true;
        }

        public bool RegisterMismatch()
        {
            ConsecutiveFailures++;
            return false;
        }

        public void RegisterMatch()
        {
            ConsecutiveFailures = 0;
        }
    }
}
