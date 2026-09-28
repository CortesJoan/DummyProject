using System;

namespace AnimalMemory.Progression
{
    /// <summary>
    /// Pure postgame progression model for Memento Match (no engine or storage dependencies).
    /// Six encounters: 0 Mika, 1 Yoru, 2 Hana, 3 Momo (duels), 4 Rei (scripted defeat),
    /// 5 protagonist vs Dios (final duel). Completion is sanitized to a strict prefix (no holes);
    /// replays of completed encounters stay allowed while later encounters remain locked.
    /// The main 21-level campaign masks live outside this model and are never touched here.
    /// </summary>
    public sealed class MementoPostgameProgression
    {
        public const int EncounterCount = 6;

        // Encounter 4 (Rei) is a scripted defeat; every other encounter is a duel.
        public const int ScriptedDefeatEncounterId = 4;

        private const int ValidBits = (1 << EncounterCount) - 1;

        /// <summary>Bitmask of completed encounters (bit i = encounter i). Always a strict prefix.</summary>
        public int CompletedMask { get; private set; }

        public MementoPostgameProgression(int loadMask = 0)
        {
            CompletedMask = Sanitize(loadMask);
        }

        /// <summary>Strips negative/invalid bits, then keeps only the leading sequential run from bit 0.</summary>
        private static int Sanitize(int mask)
        {
            if (mask <= 0)
            {
                return 0;
            }

            int sanitized = mask & ValidBits;
            int allowed = 0;
            for (int i = 0; i < EncounterCount; i++)
            {
                int bit = 1 << i;
                if ((sanitized & bit) == 0)
                {
                    break;
                }

                allowed |= bit;
            }

            return allowed;
        }

        /// <summary>Entry gate: valid id, release enabled, base campaign complete, and id at or before the first uncompleted encounter.</summary>
        public bool CanEnter(int encounterId, bool releaseEnabled, bool baseCampaignComplete)
        {
            if (!IsValidId(encounterId) || !releaseEnabled || !baseCampaignComplete)
            {
                return false;
            }

            int next = NextEncounterId();
            if (next < 0)
            {
                return true; // all six complete: replays remain available.
            }

            return encounterId <= next;
        }

        /// <summary>Completes a duel encounter (0 Mika, 1 Yoru, 2 Hana, 3 Momo, 5 final). Encounter 4 is rejected here.</summary>
        public bool TryCompleteDuel(int encounterId, bool releaseEnabled, bool baseCampaignComplete)
        {
            if (!IsValidId(encounterId) || encounterId == ScriptedDefeatEncounterId)
            {
                return false;
            }

            return TryComplete(encounterId, releaseEnabled, baseCampaignComplete);
        }

        /// <summary>Completes ONLY the scripted defeat encounter (4 Rei). Any other id is rejected without state changes.</summary>
        public bool TryCompleteScriptedDefeat(int encounterId, bool releaseEnabled, bool baseCampaignComplete)
        {
            if (encounterId != ScriptedDefeatEncounterId)
            {
                return false;
            }

            return TryComplete(encounterId, releaseEnabled, baseCampaignComplete);
        }

        private bool TryComplete(int encounterId, bool releaseEnabled, bool baseCampaignComplete)
        {
            if (!CanEnter(encounterId, releaseEnabled, baseCampaignComplete) || IsCompleted(encounterId))
            {
                return false; // locked or duplicate: no state change.
            }

            CompletedMask |= 1 << encounterId;
            return true;
        }

        public bool IsCompleted(int encounterId)
        {
            return IsValidId(encounterId) && (CompletedMask & (1 << encounterId)) != 0;
        }

        /// <summary>True Ending requires all six encounters plus both gates.</summary>
        public bool IsTrueEndingUnlocked(bool releaseEnabled, bool baseCampaignComplete)
        {
            return CompletedMask == ValidBits && releaseEnabled && baseCampaignComplete;
        }

        /// <summary>First uncompleted encounter id, or -1 when the postgame is fully cleared.</summary>
        public int NextEncounterId()
        {
            for (int i = 0; i < EncounterCount; i++)
            {
                if ((CompletedMask & (1 << i)) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Versioned save payload. The integrator persists it; this model performs no I/O.</summary>
        [Serializable]
        public struct Snapshot
        {
            public int version;
            public int completedMask;
        }

        /// <summary>Creates the current snapshot (version 1, two fields).</summary>
        public Snapshot CreateSnapshot()
        {
            return new Snapshot { version = 1, completedMask = CompletedMask };
        }

        /// <summary>Loads a snapshot. Version 1 masks are sanitized; unversioned (<= 0) and future versions reset to 0 so nothing unlocks by accident.</summary>
        public void LoadSnapshot(Snapshot snapshot)
        {
            CompletedMask = snapshot.version == 1 ? Sanitize(snapshot.completedMask) : 0;
        }

        /// <summary>Convenience factory mirroring LoadSnapshot.</summary>
        public static MementoPostgameProgression FromSnapshot(Snapshot snapshot)
        {
            return new MementoPostgameProgression(snapshot.version == 1 ? snapshot.completedMask : 0);
        }

        private static bool IsValidId(int encounterId)
        {
            return encounterId >= 0 && encounterId < EncounterCount;
        }
    }

}
