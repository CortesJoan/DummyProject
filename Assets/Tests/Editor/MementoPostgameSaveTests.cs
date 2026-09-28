using AnimalMemory.Progression;
using NUnit.Framework;
using UnityEngine;

public sealed class MementoPostgameSaveTests
{
    [TestCase(0, 0)] [TestCase(15, 15)] [TestCase(63, 63)] [TestCase(5, 1)]
    public void Save_RoundTripsPostgameWithoutChangingBaseEnding(int mask, int sanitized)
    {
        var go = new GameObject("Inactive postgame save test");
        go.SetActive(false);
        try
        {
            var manager = go.AddComponent<GameManager>();
            var data = new GameData();
            data.SetData("ProgressionVersion", AnimalMemoryProgression.CurrentVersion);
            data.SetData("CampaignClearedMask", (1 << 21) - 1);
            data.SetData("PawStars", 90);
            data.SetData("UnlockedGuideMask", 31);
            data.SetData("DefeatedGuideMask", 31);
            data.SetData("PostgameVersion", 1);
            data.SetData("PostgameCompletedMask", mask);
            manager.LoadData(data);
            var saved = new GameData();
            manager.SaveData(saved);
            manager.LoadData(saved);
            Assert.That(saved.GetData<int>("PostgameVersion"), Is.EqualTo(1));
            Assert.That(manager.PostgameCompletedMask, Is.EqualTo(sanitized));
            Assert.That(manager.PawStars, Is.EqualTo(90));
            Assert.That(manager.CampaignEndingUnlocked, Is.True);
            Assert.That(manager.IsPostgameAvailable, Is.False);
            Assert.That(manager.IsTrueEndingUnlocked, Is.False);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(-1));
            for (int id = 0; id < 6; id++)
                Assert.That(manager.IsPostgameEncounterUnlocked(id), Is.False);
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void OldSaveAndFreshProfile_StartWithoutPostgameCompletion()
    {
        var go = new GameObject("Inactive old save test");
        go.SetActive(false);
        try
        {
            var manager = go.AddComponent<GameManager>();
            var existing = new GameData();
            existing.SetData("PostgameVersion", 1);
            existing.SetData("PostgameCompletedMask", 63);
            manager.LoadData(existing);
            Assert.That(manager.PostgameCompletedMask, Is.EqualTo(63));
            manager.LoadData(new GameData());
            Assert.That(manager.PostgameCompletedMask, Is.Zero);
            Assert.That(manager.CampaignEndingUnlocked, Is.False);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
