using UnityEngine;

/// <summary>Remembers heard outcome lines without changing campaign progression.</summary>
public static class MementoDefeatPresentation
{
    private const string Key = "MementoMatch.HeardOutcomes.v1";
    public static int OutcomeBit(int guideId, bool playerWon) =>
        guideId >= 0 && guideId < 6 ? 1 << (guideId * 2 + (playerWon ? 1 : 0)) : 0;
    public static bool HasHeard(int guideId, bool playerWon) =>
        (PlayerPrefs.GetInt(Key, 0) & OutcomeBit(guideId, playerWon)) != 0;
    public static void Remember(int guideId, bool playerWon)
    {
        int bit = OutcomeBit(guideId, playerWon);
        if (bit == 0) return;
        PlayerPrefs.SetInt(Key, PlayerPrefs.GetInt(Key, 0) | bit);
        PlayerPrefs.Save();
    }
}
