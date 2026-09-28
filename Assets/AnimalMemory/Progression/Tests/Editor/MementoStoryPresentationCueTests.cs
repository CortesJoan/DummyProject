using System.Linq;
using NUnit.Framework;

namespace AnimalMemory.Progression.Tests
{
    [TestFixture]
    public sealed class MementoStoryPresentationCueTests
    {
        [Test]
        public void WithPresentation_PreservesDialogueAndPortrait()
        {
            var original = new MementoMatchStoryBeat("Rei", "Tu turno.", 5,
                MementoMatchStoryVoiceCue.StoryChallenge, MementoMatchStoryBeatKind.Challenge, true);
            var directed = original.WithPresentation(MementoStoryCue.ChampionEntrance);
            Assert.That(original.PresentationCue, Is.EqualTo(MementoStoryCue.None));
            Assert.That(directed.PresentationCue, Is.EqualTo(MementoStoryCue.ChampionEntrance));
            Assert.That(directed.Text, Is.EqualTo(original.Text));
            Assert.That(directed.Speaker, Is.EqualTo(original.Speaker));
            Assert.That(directed.GuideId, Is.EqualTo(original.GuideId));
            Assert.That(directed.Kind, Is.EqualTo(original.Kind));
            Assert.That(directed.VoiceCue, Is.EqualTo(original.VoiceCue));
            Assert.That(directed.IsSilhouette, Is.True);
        }

        [Test]
        public void Introduction_DramaticDirectionsAreOrderedAndUnique()
        {
            var cues = MementoMatchCampaignRules.GetStoryScene(0).Beats
                .Select(beat => beat.PresentationCue).Where(cue => cue != MementoStoryCue.None).ToArray();
            Assert.That(cues, Is.EqualTo(new[] {
                MementoStoryCue.Warning, MementoStoryCue.ChampionEntrance,
                MementoStoryCue.OpeningDuel, MementoStoryCue.Launch,
                MementoStoryCue.Impact, MementoStoryCue.Defeat,
                MementoStoryCue.Aftermath, MementoStoryCue.Resolve }));
        }

        [Test]
        public void OtherScenes_DoNotAccidentallyRestartTheOpeningDuel()
        {
            for (int id = 1; id <= MementoMatchCampaignRules.EpilogueSceneId; id++)
                Assert.That(MementoMatchCampaignRules.GetStoryScene(id).Beats
                    .Any(beat => beat.PresentationCue != MementoStoryCue.None), Is.False, "Scene " + id);
        }

        [Test]
        public void Cue_RemainsAttachedWhenDialogueIsInsertedBeforeIt()
        {
            var opening = MementoMatchCampaignRules.GetStoryScene(0).Beats
                .Single(beat => beat.PresentationCue == MementoStoryCue.OpeningDuel);
            var inserted = new MementoMatchStoryBeat("{PLAYER}", "Espera.", -1, MementoMatchStoryVoiceCue.None);
            var reordered = new[] { inserted, inserted, opening };
            Assert.That(reordered[2].PresentationCue, Is.EqualTo(MementoStoryCue.OpeningDuel));
            Assert.That(reordered[0].PresentationCue, Is.EqualTo(MementoStoryCue.None));
        }
    }
}
