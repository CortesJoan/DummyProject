using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class MementoVoiceVolumeTests
{
    [Test]
    public void SettingsNotificationRefreshesVoiceAndDestroyUnsubscribes()
    {
        // Exercise the notification without modifying any player preferences.
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo changed = typeof(MementoAudioSettings).GetField(
            "Changed", BindingFlags.Static | BindingFlags.NonPublic);
        var host = new GameObject("Voice volume test");
        var player = host.AddComponent<MementoMatchVoicePlayer>();
        try
        {
            FieldInfo sourceField = typeof(MementoMatchVoicePlayer).GetField("voiceSource", instanceFlags);
            if (sourceField.GetValue(player) == null)
                typeof(MementoMatchVoicePlayer).GetMethod("Awake", instanceFlags).Invoke(player, null);
            var source = (AudioSource)sourceField.GetValue(player);
            float expected = 0.92f * MementoAudioSettings.VoiceVolume;
            source.volume = expected > 0.5f ? 0f : 1f;
            var notification = (Action)changed.GetValue(null);
            Assert.That(notification, Is.Not.Null);
            Assert.That(notification.GetInvocationList().Count(d => ReferenceEquals(d.Target, player)),
                Is.EqualTo(1), "Subscribe exactly once.");
            notification.Invoke();
            Assert.That(source.volume, Is.EqualTo(expected).Within(0.0001f));
        }
        finally
        {
            // EditMode does not run the normal PlayMode lifecycle for this component.
            typeof(MementoMatchVoicePlayer).GetMethod("OnDestroy", instanceFlags).Invoke(player, null);
            UnityEngine.Object.DestroyImmediate(host);
        }
        var remaining = (Action)changed.GetValue(null);
        Assert.That(remaining == null ||
            remaining.GetInvocationList().All(d => !ReferenceEquals(d.Target, player)),
            Is.True, "Destroyed players must not remain subscribed.");
    }
}
