using System;
using NUnit.Framework;
using UnityEngine;

public sealed class MementoStoryVoiceTests
{
    private const string Json = "{\"lines\":[{\"id\":\"story_01_00\",\"guide\":\"aki\",\"es\":\"Hola {PLAYER}.\",\"ja\":\"こんにちは\"}]}";

    [Test]
    public void ExactSourceAndGuide_ResolvesDedicatedPath()
    {
        string path;
        Assert.That(new MementoStoryVoiceCatalog(Json).TryGetResourcePath(0, "Hola {PLAYER}.", out path), Is.True);
        Assert.That(path, Is.EqualTo("Audio/Voices/ja/StoryV3/story_01_00"));
    }

    [TestCase(1, "Hola {PLAYER}.")]
    [TestCase(-1, "Hola {PLAYER}.")]
    [TestCase(6, "Hola {PLAYER}.")]
    [TestCase(0, "Hola Alex.")]
    [TestCase(0, "Hola {PLAYER}. ")]
    [TestCase(0, "hola {PLAYER}.")]
    [TestCase(0, null)]
    public void StaleOrWrongSpeaker_DoesNotResolve(int guide, string text)
    {
        string path;
        Assert.That(new MementoStoryVoiceCatalog(Json).TryGetResourcePath(guide, text, out path), Is.False);
        Assert.That(path, Is.Null);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("{broken")]
    [TestCase("{}")]
    public void MissingOrMalformedCatalog_IsSilent(string json)
    {
        string path;
        Assert.That(new MementoStoryVoiceCatalog(json).TryGetResourcePath(0, "Hola {PLAYER}.", out path), Is.False);
    }

    [Test]
    public void AmbiguousDuplicate_IsSilent()
    {
        string duplicate = Json.Replace("}]}", "},{\"id\":\"story_02_00\",\"guide\":\"aki\",\"es\":\"Hola {PLAYER}.\",\"ja\":\"こんにちは\"}]}");
        string path;
        Assert.That(new MementoStoryVoiceCatalog(duplicate).TryGetResourcePath(0, "Hola {PLAYER}.", out path), Is.False);
    }

    [TestCase("../aki_menu")]
    [TestCase("aki_menu")]
    [TestCase("story_01_00/extra")]
    [TestCase("story_01_00\n")]
    public void InvalidClipId_DoesNotEscapeStoryFolder(string id)
    {
        string path;
        string json = Json.Replace("story_01_00", id.Replace("\n", "\\n"));
        Assert.That(new MementoStoryVoiceCatalog(json).TryGetResourcePath(0, "Hola {PLAYER}.", out path), Is.False);
    }

    [Test]
    public void SameIdAssignedToDifferentLines_BothRemainSilent()
    {
        string json = "{\"lines\":[{\"id\":\"story_01_00\",\"guide\":\"aki\",\"es\":\"First\",\"ja\":\"一\"},{\"id\":\"story_01_00\",\"guide\":\"mika\",\"es\":\"Second\",\"ja\":\"二\"}]}";
        var catalog = new MementoStoryVoiceCatalog(json);
        string path;
        Assert.That(catalog.TryGetResourcePath(0, "First", out path), Is.False);
        Assert.That(path, Is.Null);
        Assert.That(catalog.TryGetResourcePath(1, "Second", out path), Is.False);
        Assert.That(path, Is.Null);
    }

    [Test]
    public void MissingClip_StopsPreviousClipAndNeverFallsBack()
    {
        var owner = new GameObject("Story voice isolated test");
        var oldClip = AudioClip.Create("old", 128, 1, 8000, false);
        int loads = 0;
        try
        {
            using (var player = new MementoStoryVoicePlayer(owner, new MementoStoryVoiceCatalog(Json), path => { loads++; return null; }))
            {
                var source = owner.GetComponent<AudioSource>();
                source.clip = oldClip;
                Assert.That(player.Play(0, "Hola {PLAYER}."), Is.False);
                Assert.That(source.clip, Is.Null);
                Assert.That(loads, Is.EqualTo(1));
                Assert.That(player.Play(1, "Hola {PLAYER}."), Is.False);
                Assert.That(loads, Is.EqualTo(1));
                Assert.That(source.volume, Is.EqualTo(0.92f * MementoAudioSettings.VoiceVolume).Within(0.001f));
            }
            Assert.That(owner.GetComponent<AudioSource>(), Is.Null);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(oldClip);
            UnityEngine.Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void DisposeAndStop_AreIdempotent()
    {
        var owner = new GameObject("Story voice disposal test");
        try
        {
            var player = new MementoStoryVoicePlayer(owner, new MementoStoryVoiceCatalog(null), path => null);
            player.Dispose();
            player.Stop();
            player.Dispose();
            Assert.That(player.Play(0, "anything"), Is.False);
        }
        finally { UnityEngine.Object.DestroyImmediate(owner); }
    }
}
