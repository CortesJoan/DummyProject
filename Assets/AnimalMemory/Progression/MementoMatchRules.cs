using System;
using System.Collections.Generic;

namespace AnimalMemory.Progression
{
    public static class MementoMatchAchievementIds
    {
        public const int FirstVictory = 0;
        public const int PerfectEasy = 1;
        public const int ComboFive = 2;
        public const int ComboTen = 3;
        public const int Flawless = 4;
        public const int DefeatMika = 5;
        public const int DefeatYoru = 6;
        public const int CompleteCollection = 7;
        public const int DefeatHana = 8;
        public const int DefeatMomo = 9;
        public const int DefeatRei = 10;
        public const int Count = 11;

        public static string GetName(int id)
        {
            switch (id)
            {
                case FirstVictory: return "Primer vínculo";
                case PerfectEasy: return "Memoria perfecta";
                case ComboFive: return "Racha brillante";
                case ComboTen: return "Mente imparable";
                case Flawless: return "Sin tropiezos";
                case DefeatMika: return "Ingenio superado";
                case DefeatYoru: return "Más allá de las estrellas";
                case CompleteCollection: return "Archivo completo";
                case DefeatHana: return "Jardín en flor";
                case DefeatMomo: return "Encore perfecto";
                case DefeatRei: return "Deseo de las seis zonas";
                default: return string.Empty;
            }
        }

        public static string GetDescription(int id)
        {
            switch (id)
            {
                case FirstVictory: return "Gana tu primera partida.";
                case PerfectEasy: return "Completa Fácil en exactamente 3 turnos.";
                case ComboFive: return "Alcanza un combo de 5.";
                case ComboTen: return "Alcanza un combo de 10.";
                case Flawless: return "Gana sin cometer ningún fallo.";
                case DefeatMika: return "Vence a Mika en su duelo.";
                case DefeatYoru: return "Vence a Yoru en su duelo.";
                case CompleteCollection: return "Desbloquea las seis barajas.";
                case DefeatHana: return "Vence a Hana en su duelo.";
                case DefeatMomo: return "Vence a Momo en su duelo.";
                case DefeatRei: return "Supera a Rei en el Archivo Cero.";
                default: return string.Empty;
            }
        }
    }

    public struct MementoMatchResult
    {
        public int DifficultyId;
        public int Turns;
        public int PairCount;
        public int Mismatches;
        public int MaxCombo;
        public int DefeatedGuideId;
    }

    public static class MementoMatchAchievementRules
    {
        public static int Evaluate(int currentMask, MementoMatchResult result, int unlockedSetMask)
        {
            int mask = currentMask | (1 << MementoMatchAchievementIds.FirstVictory);
            if (result.DifficultyId == 0 && result.Turns == result.PairCount)
                mask |= 1 << MementoMatchAchievementIds.PerfectEasy;
            if (result.MaxCombo >= 5)
                mask |= 1 << MementoMatchAchievementIds.ComboFive;
            if (result.MaxCombo >= 10)
                mask |= 1 << MementoMatchAchievementIds.ComboTen;
            if (result.Mismatches == 0)
                mask |= 1 << MementoMatchAchievementIds.Flawless;
            if (result.DefeatedGuideId == AnimalMemoryContentIds.MikaGuide)
                mask |= 1 << MementoMatchAchievementIds.DefeatMika;
            if (result.DefeatedGuideId == AnimalMemoryContentIds.YoruGuide)
                mask |= 1 << MementoMatchAchievementIds.DefeatYoru;
            if (result.DefeatedGuideId == AnimalMemoryContentIds.HanaGuide)
                mask |= 1 << MementoMatchAchievementIds.DefeatHana;
            if (result.DefeatedGuideId == AnimalMemoryContentIds.MomoGuide)
                mask |= 1 << MementoMatchAchievementIds.DefeatMomo;
            if (result.DefeatedGuideId == AnimalMemoryContentIds.UltimateMasterOpponent)
                mask |= 1 << MementoMatchAchievementIds.DefeatRei;
            int allSets = (1 << AnimalMemoryContentIds.SetCount) - 1;
            if ((unlockedSetMask & allSets) == allSets)
                mask |= 1 << MementoMatchAchievementIds.CompleteCollection;
            return mask;
        }

        public static IReadOnlyList<int> GetNewAchievements(int previousMask, int nextMask)
        {
            List<int> result = new List<int>();
            for (int id = 0; id < MementoMatchAchievementIds.Count; id++)
            {
                int bit = 1 << id;
                if ((previousMask & bit) == 0 && (nextMask & bit) != 0)
                    result.Add(id);
            }

            return result;
        }
    }

    public static class GuideChallengeRules
    {
        public static int GetRequiredStars(int guideId)
        {
            switch (guideId)
            {
                case AnimalMemoryContentIds.MikaGuide: return 4;
                case AnimalMemoryContentIds.YoruGuide: return 12;
                case AnimalMemoryContentIds.HanaGuide: return 18;
                case AnimalMemoryContentIds.MomoGuide: return 24;
                default: return 0;
            }
        }

        public static int GetDifficultyId(int guideId)
        {
            if (guideId == AnimalMemoryContentIds.AkiGuide)
                return 0;
            if (guideId == AnimalMemoryContentIds.YoruGuide ||
                guideId == AnimalMemoryContentIds.MomoGuide ||
                guideId == AnimalMemoryContentIds.UltimateMasterOpponent)
                return 2;
            return 1;
        }

        public static int GetMissLimit(int guideId)
        {
            switch (guideId)
            {
                case AnimalMemoryContentIds.AkiGuide: return 6;
                case AnimalMemoryContentIds.MikaGuide: return 4;
                case AnimalMemoryContentIds.YoruGuide: return 8;
                case AnimalMemoryContentIds.HanaGuide: return 5;
                case AnimalMemoryContentIds.MomoGuide: return 6;
                case AnimalMemoryContentIds.UltimateMasterOpponent: return 10;
                default: return int.MaxValue;
            }
        }
    }

    public sealed class GuideChallengeState
    {
        public int OpponentGuideId { get; private set; } = -1;
        public int Mismatches { get; private set; }
        public int MissLimit { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDefeated => IsActive && Mismatches >= MissLimit;

        public void Begin(int guideId)
        {
            if ((guideId < AnimalMemoryContentIds.AkiGuide ||
                guideId >= AnimalMemoryContentIds.OpponentCount) &&
                !MementoPostgameEncounters.IsPostgameOpponent(guideId))
                throw new ArgumentOutOfRangeException(nameof(guideId));

            OpponentGuideId = guideId;
            Mismatches = 0;
            MissLimit = GuideChallengeRules.GetMissLimit(guideId);
            IsActive = true;
        }

        public bool RegisterMismatch()
        {
            if (!IsActive)
                return false;

            Mismatches++;
            return IsDefeated;
        }

        public void End()
        {
            IsActive = false;
        }

        public void Reset()
        {
            OpponentGuideId = -1;
            Mismatches = 0;
            MissLimit = 0;
            IsActive = false;
        }
    }
}
