using NUnit.Framework;
using UnityEngine;

public sealed class ComboPresentationRulesTests
{
    [TestCase(0, false, "tier-0", "")]
    [TestCase(1, true, "tier-1", "CHAIN START")]
    [TestCase(2, true, "tier-2", "NICE!")]
    [TestCase(3, true, "tier-3", "GREAT!")]
    [TestCase(5, true, "tier-4", "AWESOME!")]
    [TestCase(7, true, "tier-5", "BRUTAL!")]
    [TestCase(10, true, "tier-6", "MEMENTO RUSH!")]
    [TestCase(99, true, "tier-6", "MEMENTO RUSH!")]
    public void ForCombo_MapsFightingGameTiers(
        int combo,
        bool visible,
        string tierClass,
        string rank)
    {
        ComboPresentationState state = ComboPresentationRules.ForCombo(combo);

        Assert.That(state.Count, Is.EqualTo(Mathf.Max(0, combo)));
        Assert.That(state.IsVisible, Is.EqualTo(visible));
        Assert.That(state.TierClass, Is.EqualTo(tierClass));
        Assert.That(state.Rank, Is.EqualTo(rank));
        Assert.That(state.IsBroken, Is.False);
    }

    [TestCase(-1, "×0")]
    [TestCase(0, "×0")]
    [TestCase(7, "×7")]
    [TestCase(15, "×15")]
    [TestCase(16, "×15+")]
    public void CounterText_IsCachedAndBounded(int combo, string expected)
    {
        Assert.That(
            ComboPresentationRules.CounterText(combo),
            Is.EqualTo(expected));
    }

    [Test]
    public void ForBreak_PreservesDroppedCountAndUsesBreakStyle()
    {
        ComboPresentationState state = ComboPresentationRules.ForBreak(6);

        Assert.That(state.Count, Is.EqualTo(6));
        Assert.That(state.Heading, Is.EqualTo("COMBO ROTO"));
        Assert.That(state.Rank, Is.EqualTo("¡RECUPÉRATE!"));
        Assert.That(state.TierClass, Is.EqualTo("broken"));
        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.IsBroken, Is.True);
    }
}
