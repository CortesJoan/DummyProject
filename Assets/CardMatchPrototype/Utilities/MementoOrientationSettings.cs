using System;
using UnityEngine;

/// <summary>The player's preferred mobile screen orientation.</summary>
public enum MementoOrientationMode { Automatic = 0, Portrait = 1, Landscape = 2 }

/// <summary>Screen policy independent of menu and gameplay state.</summary>
public struct MementoOrientationPolicy
{
    public ScreenOrientation Orientation;
    public bool Portrait;
    public bool PortraitUpsideDown;
    public bool LandscapeLeft;
    public bool LandscapeRight;
}

/// <summary>Persistence boundary, isolated from campaign save data.</summary>
public interface IMementoOrientationStore
{
    int Read();
    void Write(int value);
}

/// <summary>Loads and applies one preference without changing game state.</summary>
public sealed class MementoOrientationPreferences
{
    private readonly IMementoOrientationStore store;
    private readonly Action<MementoOrientationPolicy> apply;
    public MementoOrientationMode Current { get; private set; }

    public MementoOrientationPreferences(IMementoOrientationStore store, Action<MementoOrientationPolicy> apply)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
        Current = Normalize(store.Read());
    }

    public void Apply() { apply(PolicyFor(Current)); }

    public void Select(MementoOrientationMode mode)
    {
        Current = Normalize((int)mode);
        store.Write((int)Current);
        Apply();
    }

    public static MementoOrientationMode Normalize(int value)
    {
        return value >= 0 && value <= 2 ? (MementoOrientationMode)value : MementoOrientationMode.Automatic;
    }

    public static MementoOrientationPolicy PolicyFor(MementoOrientationMode mode)
    {
        mode = Normalize((int)mode);
        bool automatic = mode == MementoOrientationMode.Automatic;
        return new MementoOrientationPolicy
        {
            Orientation = mode == MementoOrientationMode.Portrait
                ? ScreenOrientation.Portrait : ScreenOrientation.AutoRotation,
            Portrait = automatic || mode == MementoOrientationMode.Portrait,
            PortraitUpsideDown = automatic,
            LandscapeLeft = automatic || mode == MementoOrientationMode.Landscape,
            LandscapeRight = automatic || mode == MementoOrientationMode.Landscape
        };
    }
}

/// <summary>Unity adapter. Restores the preference before any scene presents its UI.</summary>
public static class MementoOrientationSettings
{
    public const string PreferenceKey = "MementoMatch.Orientation.v1";
    private static MementoOrientationPreferences preferences;
    private static MementoOrientationPreferences Preferences
    {
        get
        {
            if (preferences == null)
                preferences = new MementoOrientationPreferences(new PreferenceStore(), ApplyScreen);
            return preferences;
        }
    }

    public static MementoOrientationMode Current { get { return Preferences.Current; } }
    public static void SetMode(MementoOrientationMode mode) { Preferences.Select(mode); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Reload even when Enter Play Mode has domain reload disabled.
        preferences = null;
        Preferences.Apply();
    }

    private static void ApplyScreen(MementoOrientationPolicy policy)
    {
        // Set the allowed rotations first, then request the actual orientation.
        Screen.autorotateToPortrait = policy.Portrait;
        Screen.autorotateToPortraitUpsideDown = policy.PortraitUpsideDown;
        Screen.autorotateToLandscapeLeft = policy.LandscapeLeft;
        Screen.autorotateToLandscapeRight = policy.LandscapeRight;
        Screen.orientation = policy.Orientation;
    }

    private sealed class PreferenceStore : IMementoOrientationStore
    {
        public int Read() { return PlayerPrefs.GetInt(PreferenceKey, 0); }
        public void Write(int value)
        {
            PlayerPrefs.SetInt(PreferenceKey, value);
            PlayerPrefs.Save();
        }
    }
}
