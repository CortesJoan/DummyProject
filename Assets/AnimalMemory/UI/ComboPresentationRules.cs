public readonly struct ComboPresentationState
{
    public int Count { get; }
    public string Heading { get; }
    public string Rank { get; }
    public string TierClass { get; }
    public bool IsVisible { get; }
    public bool IsBroken { get; }

    public ComboPresentationState(
        int count,
        string heading,
        string rank,
        string tierClass,
        bool isVisible,
        bool isBroken)
    {
        Count = count;
        Heading = heading;
        Rank = rank;
        TierClass = tierClass;
        IsVisible = isVisible;
        IsBroken = isBroken;
    }
}

public static class ComboPresentationRules
{
    public static string CounterText(int combo)
    {
        switch (combo)
        {
            case 0: return "×0";
            case 1: return "×1";
            case 2: return "×2";
            case 3: return "×3";
            case 4: return "×4";
            case 5: return "×5";
            case 6: return "×6";
            case 7: return "×7";
            case 8: return "×8";
            case 9: return "×9";
            case 10: return "×10";
            case 11: return "×11";
            case 12: return "×12";
            case 13: return "×13";
            case 14: return "×14";
            case 15: return "×15";
            default: return combo < 0 ? "×0" : "×15+";
        }
    }

    public static ComboPresentationState ForCombo(int combo)
    {
        if (combo <= 0)
        {
            return new ComboPresentationState(
                0,
                "HIT COMBO",
                string.Empty,
                "tier-0",
                false,
                false);
        }

        if (combo == 1)
            return Active(combo, "CHAIN START", "tier-1");
        if (combo == 2)
            return Active(combo, "NICE!", "tier-2");
        if (combo <= 4)
            return Active(combo, "GREAT!", "tier-3");
        if (combo <= 6)
            return Active(combo, "AWESOME!", "tier-4");
        if (combo <= 9)
            return Active(combo, "BRUTAL!", "tier-5");

        return Active(combo, "MEMENTO RUSH!", "tier-6");
    }

    public static ComboPresentationState ForBreak(int droppedCombo)
    {
        if (droppedCombo <= 0)
            return ForCombo(0);

        return new ComboPresentationState(
            droppedCombo,
            "COMBO ROTO",
            "¡RECUPÉRATE!",
            "broken",
            true,
            true);
    }

    private static ComboPresentationState Active(
        int combo,
        string rank,
        string tierClass)
    {
        return new ComboPresentationState(
            combo,
            combo == 1 ? "CHAIN" : "HIT COMBO",
            rank,
            tierClass,
            true,
            false);
    }
}
