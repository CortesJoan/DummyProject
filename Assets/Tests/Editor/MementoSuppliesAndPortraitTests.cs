using AnimalMemory.Progression;
using NUnit.Framework;
using UnityEngine;

public sealed class MementoSuppliesAndPortraitTests
{
    [Test]
    public void NewWallet_BuysBothKinds_WithoutNegativeBalance()
    {
        var wallet = new MementoSupplyWallet();
        Assert.That(wallet.TryBuy(MementoSupplyKind.Row), Is.True);
        Assert.That(wallet.TryBuy(MementoSupplyKind.Column), Is.True);
        Assert.That(wallet.Coins, Is.Zero);
        Assert.That(wallet.Rows, Is.EqualTo(1));
        Assert.That(wallet.Columns, Is.EqualTo(1));
        Assert.That(wallet.TryBuy(MementoSupplyKind.Row), Is.False);
    }

    [Test]
    public void RejectedUse_DoesNotConsume_OrCallAnEffectWithoutStock()
    {
        var wallet = new MementoSupplyWallet();
        int called = 0;
        Assert.That(wallet.TryUse(MementoSupplyKind.Row, () => { called++; return true; }), Is.False);
        Assert.That(called, Is.Zero);
        wallet.TryBuy(MementoSupplyKind.Row);
        Assert.That(wallet.TryUse(MementoSupplyKind.Row, () => false), Is.False);
        Assert.That(wallet.Rows, Is.EqualTo(1));
        Assert.That(wallet.TryUse(MementoSupplyKind.Row, () => true), Is.True);
        Assert.That(wallet.Rows, Is.Zero);
    }

    [Test]
    public void OldSave_MigratesWins_AndReloadDoesNotGrantAgain()
    {
        var wallet = new MementoSupplyWallet();
        wallet.Load(0, 0, 0, 0, 20);
        Assert.That(wallet.Coins, Is.EqualTo(90));
        wallet.TryBuy(MementoSupplyKind.Column);
        var reload = new MementoSupplyWallet();
        reload.Load(1, wallet.Coins, wallet.Rows, wallet.Columns, 20);
        Assert.That(reload.Coins, Is.EqualTo(85));
        Assert.That(reload.Columns, Is.EqualTo(1));
    }

    [Test]
    public void CorruptValues_AreClamped_AndInvalidKindsRejected()
    {
        var wallet = new MementoSupplyWallet();
        wallet.Load(1, -12, 999, -5, 0);
        Assert.That(wallet.Coins, Is.Zero);
        Assert.That(wallet.Rows, Is.EqualTo(9));
        Assert.That(wallet.Columns, Is.Zero);
        wallet.AwardVictory();
        Assert.That(wallet.Coins, Is.EqualTo(4));
        Assert.That(wallet.TryBuy((MementoSupplyKind)99), Is.False);
        wallet.Load(1, 100, 9, 0, 0);
        Assert.That(wallet.TryBuy(MementoSupplyKind.Row), Is.False);
    }


    [Test]
    public void GameSave_RoundTripPreservesPurchasesAndExistingProgress()
    {
        var go = new GameObject("Inactive save serializer test");
        go.SetActive(false); // Never runs GameManager.Awake or touches SaveSystem files.
        try
        {
            var manager = go.AddComponent<GameManager>();
            var original = new GameData();
            original.SetData("ProgressionVersion", AnimalMemoryProgression.CurrentVersion);
            original.SetData("PawStars", 90);
            original.SetData("CardMatchUIActualWins", 25);
            original.SetData("CampaignClearedMask", (1 << 21) - 1);
            original.SetData("SelectedSetId", 0);
            original.SetData("SelectedGuideId", 2);
            original.SetData("UnlockedGuideMask", 31);
            original.SetData("DefeatedGuideMask", 31);
            manager.LoadData(original);
            Assert.That(manager.Supplies.Coins, Is.EqualTo(110));
            manager.Supplies.TryBuy(MementoSupplyKind.Row);
            manager.Supplies.TryBuy(MementoSupplyKind.Column);
            var saved = new GameData();
            manager.SaveData(saved);
            manager.LoadData(saved);
            Assert.That(manager.Supplies.Coins, Is.EqualTo(100));
            Assert.That(manager.Supplies.Rows, Is.EqualTo(1));
            Assert.That(manager.Supplies.Columns, Is.EqualTo(1));
            Assert.That(manager.PawStars, Is.EqualTo(90));
            Assert.That(manager.SelectedGuideId, Is.EqualTo(2));
            Assert.That(manager.CampaignEndingUnlocked, Is.True);
        }
        finally { Object.DestroyImmediate(go); }
    }

    [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
    public void FaceCrop_IsSquareInsideTexture_WithPerCharacterFraming(int guide)
    {
        RectInt crop = MementoPortraitFraming.FaceRect(guide, 800, 1200);
        Assert.That(crop.width, Is.EqualTo(crop.height));
        Assert.That(crop.xMin, Is.GreaterThanOrEqualTo(0));
        Assert.That(crop.yMin, Is.GreaterThanOrEqualTo(0));
        Assert.That(crop.xMax, Is.LessThanOrEqualTo(800));
        Assert.That(crop.yMax, Is.LessThanOrEqualTo(1200));
        if (guide == 0 || guide == 1 || guide == 3)
            Assert.That(crop.center.y / 1200f, Is.EqualTo(.70f).Within(.002f));
    }

    [TestCase(1,1)][TestCase(2,3)][TestCase(300,100)]
    public void FaceCrop_TinyAndWideTextures_StayValid(int width, int height)
    {
        var crop = MementoPortraitFraming.FaceRect(-1, width, height);
        Assert.That(crop.width, Is.GreaterThan(0));
        Assert.That(crop.xMax, Is.LessThanOrEqualTo(width));
        Assert.That(crop.yMax, Is.LessThanOrEqualTo(height));
    }
}
