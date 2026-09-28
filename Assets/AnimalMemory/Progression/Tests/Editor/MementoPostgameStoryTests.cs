using System.Linq;
using NUnit.Framework;

namespace AnimalMemory.Progression.Tests
{
    [TestFixture]
    public sealed class MementoPostgameStoryTests
    {
        private const int EncounterCount = 6;

        [Test]
        public void EveryEncounterExceptScriptedRei_HasIntroAndOutcome()
        {
            for (int id = 0; id < EncounterCount; id++)
            {
                bool scripted = MementoPostgameEncounters.Get(id).IsScriptedDefeat;
                Assert.That(MementoPostgameStory.HasIntro(id), Is.EqualTo(!scripted));
                Assert.That(MementoPostgameStory.HasOutcome(id), Is.EqualTo(!scripted));
            }
        }

        [Test]
        public void IntroScenes_AreWellFormedAndNeverPersistent()
        {
            for (int id = 0; id < EncounterCount; id++)
            {
                if (!MementoPostgameStory.HasIntro(id)) continue;
                var scene = MementoPostgameStory.BuildIntro(id);
                AssertSceneWellFormed(scene);
                Assert.That(scene.Beats.Count, Is.InRange(1, 8));
            }
        }

        [Test]
        public void OutcomeScenes_AreBriefAndWellFormed()
        {
            for (int id = 0; id < EncounterCount; id++)
            {
                if (!MementoPostgameStory.HasOutcome(id)) continue;
                AssertSceneWellFormed(MementoPostgameStory.BuildVictory(id));
                Assert.That(MementoPostgameStory.BuildVictory(id).Beats.Count, Is.InRange(1, 5));
                AssertSceneWellFormed(MementoPostgameStory.BuildDefeat(id));
                Assert.That(MementoPostgameStory.BuildDefeat(id).Beats.Count, Is.InRange(1, 4));
            }
        }

        [Test]
        public void OpeningScene_ExistsIsBriefAndNeverPersists()
        {
            var scene = MementoPostgameStory.BuildOpeningScene();
            AssertSceneWellFormed(scene);
            Assert.That(scene.Beats.Count, Is.InRange(2, 6));
            Assert.That(scene.Beats.Any(b => b.GuideId == 5), Is.True,
                "Rei presenta la puerta en la apertura.");
        }

        [Test]
        public void ScriptedReiScene_ExistsReiSpeaksAndNeverPersists()
        {
            var scene = MementoPostgameStory.BuildScriptedScene();
            AssertSceneWellFormed(scene);
            Assert.That(scene.Beats.Count, Is.InRange(8, 16));
            Assert.That(scene.Beats.Any(b => b.GuideId == 5), Is.True,
                "Rei debe hablar en su propio checkpoint.");
        }

        [Test]
        public void Epilogue_ExistsIsDefinitiveAndNeverPersists()
        {
            var scene = MementoPostgameStory.BuildEpilogue();
            AssertSceneWellFormed(scene);
            Assert.That(scene.Beats.Count, Is.InRange(6, 14));
        }

        [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)]
        public void Custodias_HaveOwnIdentity(int opponentId)
        {
            Assert.That(MementoPostgameStory.HasCustodiaProfile(opponentId), Is.True);
            Assert.That(MementoPostgameStory.GetCustodiaIdentity(opponentId), Is.Not.Empty);
            Assert.That(MementoPostgameStory.GetCustodiaIdentity(opponentId).Length,
                Is.InRange(10, 220));
        }

        [TestCase(-1)] [TestCase(5)] [TestCase(11)] [TestCase(int.MaxValue)]
        public void CustodiaLookups_RejectNonPostgameOpponents(int opponentId)
        {
            Assert.That(MementoPostgameStory.HasCustodiaProfile(opponentId), Is.False);
            Assert.That(() => MementoPostgameStory.GetCustodiaIdentity(opponentId),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [TestCase(-1)] [TestCase(6)] [TestCase(int.MaxValue)]
        public void SceneBuilders_RejectInvalidEncounters(int encounterId)
        {
            Assert.That(() => MementoPostgameStory.BuildIntro(encounterId),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => MementoPostgameStory.BuildVictory(encounterId),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => MementoPostgameStory.BuildDefeat(encounterId),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void CustodiaAndCreatorBeats_NeverBorrowGuidePortraitOrVoice()
        {
            // GuideId -1: sin retrato y sin intento de voz. Sus identidades no
            // tienen arte aún y el catálogo de voces exige texto exacto por guía.
            var scenes = new[]
            {
                MementoPostgameStory.BuildScriptedScene(),
                MementoPostgameStory.BuildEpilogue(),
                MementoPostgameStory.BuildIntro(5)
            };
            foreach (var scene in scenes)
            foreach (var beat in scene.Beats)
            {
                if (beat.Speaker.StartsWith("CUSTODIA") || beat.Speaker == "CREADOR")
                {
                    Assert.That(beat.GuideId, Is.EqualTo(-1));
                    Assert.That(beat.IsSilhouette, Is.False);
                }
            }
        }

        [Test]
        public void AllBeats_AreShortAndNonEmpty()
        {
            var builders = new System.Collections.Generic.List<
                System.Func<MementoMatchStoryScene>>
            {
                MementoPostgameStory.BuildScriptedScene,
                MementoPostgameStory.BuildEpilogue
            };
            for (int id = 0; id < EncounterCount; id++)
            {
                if (MementoPostgameStory.HasIntro(id))
                {
                    int capture = id;
                    builders.Add(() => MementoPostgameStory.BuildIntro(capture));
                    builders.Add(() => MementoPostgameStory.BuildVictory(capture));
                    builders.Add(() => MementoPostgameStory.BuildDefeat(capture));
                }
            }

            foreach (var build in builders)
            foreach (var beat in build().Beats)
            {
                Assert.That(beat.Text, Is.Not.Null);
                Assert.That(beat.Text.Trim(), Is.Not.Empty);
                Assert.That(beat.Text.Length, Is.AtMost(240),
                    $"Línea demasiado larga: {beat.Text}");
            }
        }

        [Test]
        public void FinalEncounterIntro_AnnouncesRulesAndCounterplay()
        {
            // El duelo final debe anunciar sus reglas: sin sorpresas que
            // invaliden arbitrariamente la memoria del jugador.
            var intro = MementoPostgameStory.BuildIntro(
                MementoPostgameEncounters.Count - 1);
            Assert.That(intro.Beats.Any(b =>
                b.Speaker == "CREADOR" &&
                b.Text.ToLowerInvariant().Contains("reglas")), Is.True);
        }

        private static void AssertSceneWellFormed(MementoMatchStoryScene scene)
        {
            Assert.That(scene, Is.Not.Null);
            Assert.That(scene.Id, Is.Negative, "Nunca se registran como vistas de campaña.");
            Assert.That(scene.IsPersistent, Is.False);
            Assert.That(scene.Beats, Is.Not.Empty);
            Assert.That(scene.Kicker, Is.Not.Empty);
            Assert.That(scene.Chapter, Is.Not.Empty);
            Assert.That(scene.WorldId, Is.EqualTo(5));
            foreach (var beat in scene.Beats)
            {
                Assert.That(beat.Speaker, Is.Not.Null);
                Assert.That(beat.Speaker, Is.Not.Empty);
                Assert.That(beat.PresentationCue, Is.EqualTo(MementoStoryCue.None),
                    "El postgame no reutiliza los cues del minijuego de la intro.");
            }
        }
    }
}
