using NUnit.Framework;
namespace AnimalMemory.Progression.Tests
{
    [TestFixture]
    public sealed class MementoPostgameProgressionTests
    {
        [TestCase(false, false)] [TestCase(false, true)] [TestCase(true, false)]
        public void GatesClosed_NeverEnterOrAdvance(bool released, bool mainComplete)
        {
            var model = new MementoPostgameProgression();
            Assert.That(model.CanEnter(0, released, mainComplete), Is.False);
            Assert.That(model.TryCompleteDuel(0, released, mainComplete), Is.False);
            Assert.That(model.CompletedMask, Is.Zero);
        }

        [TestCase(-1)] [TestCase(6)] [TestCase(32)] [TestCase(int.MaxValue)]
        public void InvalidId_IsRejectedWithoutShiftWrapping(int id)
        {
            var model = new MementoPostgameProgression(63);
            Assert.That(model.CanEnter(id, true, true), Is.False);
            Assert.That(model.IsCompleted(id), Is.False);
            Assert.That(model.TryCompleteDuel(id, true, true), Is.False);
            Assert.That(model.TryCompleteScriptedDefeat(id, true, true), Is.False);
            Assert.That(model.CompletedMask, Is.EqualTo(63));
        }

        [TestCase(-1,0)] [TestCase(0,0)] [TestCase(5,1)] [TestCase(10,0)]
        [TestCase(31,31)] [TestCase(63,63)] [TestCase(127,63)]
        public void LoadMask_KeepsOnlyValidSequentialPrefix(int mask, int expected)
        {
            Assert.That(new MementoPostgameProgression(mask).CompletedMask, Is.EqualTo(expected));
        }

        [Test]
        public void SixEncounters_AdvanceSequentially_WithOnlyReiScriptedDefeat()
        {
            var model = new MementoPostgameProgression();
            for (int id = 0; id < 6; id++)
            {
                Assert.That(model.NextEncounterId(), Is.EqualTo(id));
                Assert.That(model.CanEnter(id, true, true), Is.True);
                if (id < 5) Assert.That(model.CanEnter(id+1, true, true), Is.False);
                Assert.That(model.IsTrueEndingUnlocked(true, true), Is.False);
                int before = model.CompletedMask;
                bool wrongType = id == 4
                    ? model.TryCompleteDuel(id, true, true)
                    : model.TryCompleteScriptedDefeat(id, true, true);
                Assert.That(wrongType, Is.False);
                Assert.That(model.CompletedMask, Is.EqualTo(before));
                bool completed = id == 4
                    ? model.TryCompleteScriptedDefeat(id, true, true)
                    : model.TryCompleteDuel(id, true, true);
                Assert.That(completed, Is.True);
                Assert.That(model.IsCompleted(id), Is.True);
            }
            Assert.That(model.NextEncounterId(), Is.EqualTo(-1));
            Assert.That(model.IsTrueEndingUnlocked(true, true), Is.True);
            Assert.That(model.IsTrueEndingUnlocked(false, true), Is.False);
            Assert.That(model.IsTrueEndingUnlocked(true, false), Is.False);
        }

        [Test]
        public void LockedOrReplayedEncounter_DoesNotGrantProgressTwice()
        {
            var model = new MementoPostgameProgression();
            Assert.That(model.TryCompleteDuel(3, true, true), Is.False);
            Assert.That(model.TryCompleteScriptedDefeat(4, true, true), Is.False);
            Assert.That(model.TryCompleteDuel(0, true, true), Is.True);
            Assert.That(model.CanEnter(0, true, true), Is.True);
            Assert.That(model.TryCompleteDuel(0, true, true), Is.False);
            Assert.That(model.CompletedMask, Is.EqualTo(1));
        }

        [TestCase(0)] [TestCase(-1)] [TestCase(2)]
        public void UnknownSnapshotVersion_DoesNotUnlockContent(int version)
        {
            var model = new MementoPostgameProgression(63);
            model.LoadSnapshot(new MementoPostgameProgression.Snapshot { version=version, completedMask=63 });
            Assert.That(model.CompletedMask, Is.Zero);
        }

        [Test]
        public void Snapshot_RoundTrips_WithoutChangingBaseCampaignCount()
        {
            var source = new MementoPostgameProgression(15);
            var copy = MementoPostgameProgression.FromSnapshot(source.CreateSnapshot());
            Assert.That(copy.CompletedMask, Is.EqualTo(15));
            Assert.That(copy.NextEncounterId(), Is.EqualTo(4));
            Assert.That(MementoMatchCampaignRules.LevelCount, Is.EqualTo(21));
            Assert.That(MementoPostgameRelease.Enabled, Is.False);
        }
    }
}
