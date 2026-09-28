using System;
using System.Collections.Generic;

namespace AnimalMemory.Progression
{
    public enum MementoPostgameAbilityId
    {
        Unassigned = 0,
        NewHand = 1,
        Curtain = 2,
        Testimony = 3
    }

    /// <summary>
    /// Oponente postgame. Arte/voz, identidad de combate y AbilityId son contratos separados.
    /// TechniqueGuideId identifica la tecnica heredada que ejecuta el oponente
    /// (AllLegacyTechniques = ciclo completo del Creador; NoLegacyTechnique = sin tecnica,
    /// reservado para D1/D4). La tecnica nunca es la identidad del personaje.
    /// </summary>
    public readonly struct MementoPostgameOpponent
    {
        public int Id { get; }
        public string Name { get; }
        public int TechniqueGuideId { get; }
        public MementoPostgameAbilityId AbilityId { get; }
        public int Health { get; }
        public int MemoryLimit { get; }
        public float ForgetChance { get; }
        public int SkillInterval { get; }

        internal MementoPostgameOpponent(
            int id,
            string name,
            int techniqueGuideId,
            MementoPostgameAbilityId abilityId,
            int health,
            int memoryLimit,
            float forgetChance,
            int skillInterval)
        {
            Id = id;
            Name = name;
            TechniqueGuideId = techniqueGuideId;
            AbilityId = abilityId;
            Health = health;
            MemoryLimit = memoryLimit;
            ForgetChance = forgetChance;
            SkillInterval = skillInterval;
        }
    }

    /// <summary>
    /// Encuentro postgame jugable. PlayerGuideId: -1 protagonista, 1 Mika, 2 Yoru, 3 Hana, 4 Momo, 5 Rei.
    /// </summary>
    public readonly struct MementoPostgameEncounter
    {
        public int Id { get; }
        public string Title { get; }
        public int PlayerGuideId { get; }
        public int OpponentId { get; }
        public int SetId { get; }
        public int Rows { get; }
        public int Columns { get; }
        public bool IsScriptedDefeat { get; }

        internal MementoPostgameEncounter(int id, string title, int playerGuideId, int opponentId,
            int setId, int rows, int columns, bool isScriptedDefeat)
        {
            Id = id;
            Title = title;
            PlayerGuideId = playerGuideId;
            OpponentId = opponentId;
            SetId = setId;
            Rows = rows;
            Columns = columns;
            IsScriptedDefeat = isScriptedDefeat;
        }
    }

    /// <summary>
    /// Catálogo puro del postgame: exactamente 6 encuentros y oponentes 6..10.
    /// Sin UnityEngine; no depende de guardados ni de MementoPostgameRelease.Enabled.
    /// </summary>
    public static class MementoPostgameEncounters
    {
        public const int FirstOpponentId = 6;
        public const int LastOpponentId = 10;
        public const int NoLegacyTechnique = -2;
        public const int AllLegacyTechniques = -1;

        private static readonly MementoPostgameOpponent[] Opponents =
        {
            // D1 remains unresolved: opponent 6 keeps its persisted identity slot and the
            // legacy duel technique revalidated in the handoff; only its final ability is open.
            new MementoPostgameOpponent(6, "Custodia del Cálculo", AnimalMemoryContentIds.AkiGuide, MementoPostgameAbilityId.Unassigned, 6, 4, 0.18f, 3),
            // Azar is decided and can safely own Nueva mano; its duel technique stays Yoru's.
            new MementoPostgameOpponent(7, "Custodia del Azar", AnimalMemoryContentIds.YoruGuide, MementoPostgameAbilityId.NewHand, 7, 5, 0.15f, 5),
            // D4 remains unresolved: do not decide which persisted slot is Actriz or Reportera.
            new MementoPostgameOpponent(8, "Custodia del Vínculo", AnimalMemoryContentIds.HanaGuide, MementoPostgameAbilityId.Unassigned, 7, 4, 0.18f, 4),
            new MementoPostgameOpponent(9, "Custodia del Orgullo", AnimalMemoryContentIds.MomoGuide, MementoPostgameAbilityId.Unassigned, 7, 5, 0.16f, 3),
            // Creador keeps the AllLegacyTechniques marker as its persisted catalog value, but
            // the GOD duel no longer cycles those five techniques: R3 replaced the cycle with
            // the two-phase Juicio/Borrado contract, so CardMatchUI skips the legacy skill turn
            // whenever IsGodDuelConfigured is true.
            new MementoPostgameOpponent(10, "Creador", AllLegacyTechniques, MementoPostgameAbilityId.Unassigned, 15, 6, 0.12f, 3)
        };

        private static readonly MementoPostgameEncounter[] Encounters =
        {
            new MementoPostgameEncounter(0, "El margen de error", 1, 6, 1, 4, 4, false),
            new MementoPostgameEncounter(1, "Una certeza elegida", 2, 7, 2, 4, 5, false),
            new MementoPostgameEncounter(2, "Proteger sin encerrar", 3, 8, 3, 4, 5, false),
            new MementoPostgameEncounter(3, "Pedir ayuda no es perder", 4, 9, 4, 4, 5, false),
            new MementoPostgameEncounter(4, "La decisión de Rei", 5, 10, 5, 5, 6, true),
            new MementoPostgameEncounter(5, "Nuestra siguiente jugada", -1, 10, 5, 6, 5, false)
        };

        private static readonly IReadOnlyList<MementoPostgameEncounter> ReadOnlyEncounters = Array.AsReadOnly(Encounters);

        public static int Count => Encounters.Length; // exactamente 6

        /// <summary>Vista de solo lectura: nunca expone el array interno mutable.</summary>
        public static IReadOnlyList<MementoPostgameEncounter> All => ReadOnlyEncounters;

        public static bool IsPostgameOpponent(int id) =>
            id >= FirstOpponentId && id <= LastOpponentId;

        public static MementoPostgameEncounter Get(int id)
        {
            if (id < 0 || id >= Encounters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id,
                    "Encuentro postgame inexistente.");
            }
            return Encounters[id];
        }

        public static bool TryGetOpponent(int id, out MementoPostgameOpponent opponent)
        {
            if (IsPostgameOpponent(id))
            {
                opponent = Opponents[id - FirstOpponentId];
                return true;
            }
            opponent = default;
            return false;
        }

        public static MementoPostgameAbilityId GetAbilityId(int id)
        {
            if (!TryGetOpponent(id, out MementoPostgameOpponent opponent))
                return MementoPostgameAbilityId.Unassigned;
            return opponent.AbilityId;
        }

        public static string GetOpponentName(int id)
        {
            if (!IsPostgameOpponent(id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id,
                    "Oponente postgame inexistente.");
            }
            return Opponents[id - FirstOpponentId].Name;
        }
    }
}