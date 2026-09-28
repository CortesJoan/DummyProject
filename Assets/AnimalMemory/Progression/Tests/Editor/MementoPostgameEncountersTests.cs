using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AnimalMemory.Progression.Tests
{
    /// <summary>
    /// Regresion del catalogo puro del postgame: 6 encuentros, oponentes 6..10,
    /// identidad separada de AbilityId, tableros de parejas completas,
    /// exposicion de solo lectura y rechazo de ids invalidos.
    /// </summary>
    [TestFixture]
    public class MementoPostgameEncountersTests
    {
        private const int ExpectedEncounters = 6;
        private const int PlayerGuideMin = -1;    // -1 = protagonista
        private const int PlayerGuideMax = 5;     // 5 = Rei
        private const int SetIdMin = 0;           // 6 mazos: 0..5
        private const int SetIdMax = 5;

        [Test]
        public void Catalog_ExposesExactlySixEncounters()
        {
            Assert.That(MementoPostgameEncounters.Count, Is.EqualTo(ExpectedEncounters));
            Assert.That(MementoPostgameEncounters.All.Count, Is.EqualTo(MementoPostgameEncounters.Count));
        }

        [Test]
        public void All_IsStableReadOnlyView_WithSequentialUniqueIds()
        {
            var all = MementoPostgameEncounters.All;

            Assert.That(all, Is.InstanceOf<IReadOnlyList<MementoPostgameEncounter>>());
            Assert.That(all, Is.Not.InstanceOf<MementoPostgameEncounter[]>(),
                "All nunca debe exponer el array interno mutable.");
            Assert.That(ReferenceEquals(MementoPostgameEncounters.All, all), Is.True,
                "All debe devolver siempre la misma vista de solo lectura.");

            var mutableInterface = all as IList<MementoPostgameEncounter>;
            Assert.That(mutableInterface, Is.Not.Null);
            Assert.That(() => mutableInterface[0] = default(MementoPostgameEncounter), Throws.TypeOf<NotSupportedException>());

            var seen = new HashSet<int>();
            for (int i = 0; i < MementoPostgameEncounters.Count; i++)
            {
                Assert.That(all[i].Id, Is.EqualTo(i), "El id del encuentro debe coincidir con su indice.");
                Assert.That(seen.Add(all[i].Id), Is.True, "Los ids de encuentro deben ser unicos.");
            }
        }

        [Test]
        public void Get_ReturnsTheEncounterWithMatchingId()
        {
            for (int i = 0; i < MementoPostgameEncounters.Count; i++)
            {
                Assert.That(MementoPostgameEncounters.Get(i).Id, Is.EqualTo(i));
            }
        }

        [Test]
        public void Encounters_HaveValidShape_WholePairs_AndResolvableOpponents()
        {
            int scriptedDefeats = 0;

            foreach (MementoPostgameEncounter e in MementoPostgameEncounters.All)
            {
                Assert.That(e.Title, Is.Not.Null.And.Not.Empty, "Titulo del encuentro " + e.Id);
                Assert.That(e.Rows, Is.GreaterThan(0), "Filas del encuentro " + e.Id);
                Assert.That(e.Columns, Is.GreaterThan(0), "Columnas del encuentro " + e.Id);

                int cards = e.Rows * e.Columns;
                Assert.That(cards, Is.GreaterThan(0));
                Assert.That(cards % 2, Is.EqualTo(0),
                    "El tablero " + e.Rows + "x" + e.Columns + " debe repartir parejas completas.");

                Assert.That(e.PlayerGuideId, Is.InRange(PlayerGuideMin, PlayerGuideMax), "Guia jugador " + e.Id);
                Assert.That(e.SetId, Is.InRange(SetIdMin, SetIdMax), "Mazo del encuentro " + e.Id);

                Assert.That(MementoPostgameEncounters.TryGetOpponent(e.OpponentId, out MementoPostgameOpponent o), Is.True,
                    "El encuentro " + e.Id + " debe referenciar un oponente postgame existente.");
                Assert.That(o.Id, Is.EqualTo(e.OpponentId));

                if (e.IsScriptedDefeat)
                {
                    scriptedDefeats++;
                }
            }

            Assert.That(scriptedDefeats, Is.EqualTo(1), "Exactamente un encuentro es derrota guionizada.");
        }

        [Test]
        public void PostgameAbilities_AreSeparateFromIdentity_WithoutResolvingD1OrD4()
        {
            var names = new HashSet<string>();

            for (int id = MementoPostgameEncounters.FirstOpponentId;
                 id <= MementoPostgameEncounters.LastOpponentId; id++)
            {
                Assert.That(MementoPostgameEncounters.IsPostgameOpponent(id), Is.True, "id " + id);
                Assert.That(MementoPostgameEncounters.TryGetOpponent(id, out MementoPostgameOpponent o), Is.True, "id " + id);
                Assert.That(o.Id, Is.EqualTo(id));
                Assert.That(o.Name, Is.Not.Null.And.Not.Empty);
                Assert.That(names.Add(o.Name), Is.True, "Los nombres de oponente deben ser unicos.");
                Assert.That(MementoPostgameEncounters.GetOpponentName(id), Is.EqualTo(o.Name));
            }

            Assert.That(
                MementoPostgameEncounters.GetAbilityId(7),
                Is.EqualTo(MementoPostgameAbilityId.NewHand),
                "Azar es la unica asignacion de habilidad postgame ya decidida.");

            foreach (int unresolvedId in new[] { 6, 8, 9 })
            {
                Assert.That(
                    MementoPostgameEncounters.GetAbilityId(unresolvedId),
                    Is.EqualTo(MementoPostgameAbilityId.Unassigned),
                    "D1/D4 no deben resolverse accidentalmente en el catalogo.");
            }

            // El handoff revalida la tecnica heredada de cada Custodia: la tecnica
            // es un contrato aparte y nunca puede ser la identidad del personaje.
            var legacyTechniques = new Dictionary<int, int>
            {
                { 6, AnimalMemoryContentIds.AkiGuide },
                { 7, AnimalMemoryContentIds.YoruGuide },
                { 8, AnimalMemoryContentIds.HanaGuide },
                { 9, AnimalMemoryContentIds.MomoGuide }
            };
            foreach (KeyValuePair<int, int> entry in legacyTechniques)
            {
                Assert.That(
                    MementoPostgameEncounters.TryGetOpponent(entry.Key, out MementoPostgameOpponent custodian),
                    Is.True,
                    "id " + entry.Key);
                Assert.That(
                    custodian.TechniqueGuideId,
                    Is.EqualTo(entry.Value),
                    "Tecnica heredada del oponente " + entry.Key);
                Assert.That(
                    custodian.TechniqueGuideId,
                    Is.Not.EqualTo(entry.Key),
                    "La tecnica no puede reutilizar el id del personaje.");
            }

            Assert.That(
                MementoPostgameEncounters.GetAbilityId(10),
                Is.EqualTo(MementoPostgameAbilityId.Unassigned),
                "GOD se implementa en R3, no en el bloque de habilidades puras.");
            Assert.That(
                MementoPostgameEncounters.GetAbilityId(AnimalMemoryContentIds.MikaGuide),
                Is.EqualTo(MementoPostgameAbilityId.Unassigned));
        }

        [Test]
        public void Creator_UsesTheAllTechniquesMarker_ExactlyOnce()
        {
            int allMarkerOwners = 0;

            for (int id = MementoPostgameEncounters.FirstOpponentId;
                 id <= MementoPostgameEncounters.LastOpponentId; id++)
            {
                Assert.That(MementoPostgameEncounters.TryGetOpponent(id, out MementoPostgameOpponent o), Is.True);
                if (o.TechniqueGuideId == MementoPostgameEncounters.AllLegacyTechniques)
                {
                    allMarkerOwners++;
                    Assert.That(id, Is.EqualTo(MementoPostgameEncounters.LastOpponentId),
                        "Solo el Creador cierra el catalogo con el marcador de todas las tecnicas.");
                }
            }

            Assert.That(allMarkerOwners, Is.EqualTo(1), "Exactamente un oponente cicla las cinco tecnicas.");
        }

        [Test]
        public void OpponentProfiles_ArePlayable()
        {
            for (int id = MementoPostgameEncounters.FirstOpponentId;
                 id <= MementoPostgameEncounters.LastOpponentId; id++)
            {
                Assert.That(MementoPostgameEncounters.TryGetOpponent(id, out MementoPostgameOpponent o), Is.True, "id " + id);
                Assert.That(o.Health, Is.GreaterThan(0), "Vida del oponente " + id);
                Assert.That(o.MemoryLimit, Is.GreaterThan(0), "Limite de memoria del oponente " + id);
                Assert.That(o.ForgetChance, Is.GreaterThan(0f).And.LessThan(1f), "Olvido del oponente " + id);
                Assert.That(o.SkillInterval, Is.GreaterThan(0), "Intervalo de habilidad del oponente " + id);
            }
        }

        [Test]
        public void InvalidIds_AreRejected()
        {
            TestDelegate getBelow = () => MementoPostgameEncounters.Get(-1);
            TestDelegate getAbove = () => MementoPostgameEncounters.Get(MementoPostgameEncounters.Count);
            Assert.That(getBelow, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(getAbove, Throws.TypeOf<ArgumentOutOfRangeException>());

            // Rei (id 5) pertenece al reparto principal, no al rango de nuevas oponentes.
            int guideBoundary = MementoPostgameEncounters.FirstOpponentId - 1;
            Assert.That(MementoPostgameEncounters.IsPostgameOpponent(guideBoundary), Is.False);
            Assert.That(MementoPostgameEncounters.IsPostgameOpponent(MementoPostgameEncounters.LastOpponentId + 1), Is.False);

            Assert.That(MementoPostgameEncounters.TryGetOpponent(guideBoundary, out MementoPostgameOpponent missed), Is.False);
            Assert.That(missed, Is.EqualTo(default(MementoPostgameOpponent)), "El fallo debe dejar el out en default.");
            Assert.That(MementoPostgameEncounters.TryGetOpponent(MementoPostgameEncounters.LastOpponentId + 1, out _), Is.False);

            TestDelegate nameBelow = () => MementoPostgameEncounters.GetOpponentName(guideBoundary);
            TestDelegate nameAbove = () => MementoPostgameEncounters.GetOpponentName(MementoPostgameEncounters.LastOpponentId + 1);
            Assert.That(nameBelow, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(nameAbove, Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
