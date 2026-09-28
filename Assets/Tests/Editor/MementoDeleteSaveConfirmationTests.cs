using AnimalMemory.UI;
using NUnit.Framework;

public sealed class MementoDeleteSaveConfirmationTests
{
    [Test]
    public void FirstRequestOnlyArmsAndNeverDeletes()
    {
        DeleteSaveConfirmation confirmation = new DeleteSaveConfirmation();

        Assert.That(confirmation.Request(), Is.False);
        Assert.That(confirmation.IsArmed, Is.True);
    }

    [Test]
    public void SecondRequestConfirmsThePendingArm()
    {
        DeleteSaveConfirmation confirmation = new DeleteSaveConfirmation();
        confirmation.Request();

        Assert.That(confirmation.Request(), Is.True);
        Assert.That(confirmation.IsArmed, Is.False);
    }

    [Test]
    public void ConfirmationRequiresAFreshArmAfterItRuns()
    {
        DeleteSaveConfirmation confirmation = new DeleteSaveConfirmation();
        confirmation.Request();
        Assert.That(confirmation.Request(), Is.True);

        // The button is re-armed from scratch, it never confirms twice in a row.
        Assert.That(confirmation.Request(), Is.False);
        Assert.That(confirmation.IsArmed, Is.True);
    }

    [Test]
    public void ExpiredArmMustNotDelete()
    {
        DeleteSaveConfirmation confirmation = new DeleteSaveConfirmation();
        confirmation.Request();
        int armedVersion = confirmation.Version;

        Assert.That(confirmation.Expire(armedVersion), Is.True);
        Assert.That(confirmation.IsArmed, Is.False);
        Assert.That(confirmation.Request(), Is.False);
    }

    [Test]
    public void StaleTimeoutCannotDisarmARecentArm()
    {
        DeleteSaveConfirmation confirmation = new DeleteSaveConfirmation();
        confirmation.Request();
        int staleVersion = confirmation.Version;
        confirmation.Expire(staleVersion);

        // A later arm must survive the timeout scheduled by the previous one.
        confirmation.Request();
        Assert.That(confirmation.Expire(staleVersion), Is.False);
        Assert.That(confirmation.IsArmed, Is.True);
    }

    [Test]
    public void StaleTimeoutCannotCancelTheArmThatWasAlreadyConfirmed()
    {
        DeleteSaveConfirmation confirmation = new DeleteSaveConfirmation();
        confirmation.Request();
        int armedVersion = confirmation.Version;
        Assert.That(confirmation.Request(), Is.True);

        Assert.That(confirmation.Expire(armedVersion), Is.False);
        Assert.That(confirmation.IsArmed, Is.False);
    }

    [Test]
    public void TimeoutWindowIsTheDocumentedFourSeconds()
    {
        Assert.That(DeleteSaveConfirmation.ArmTimeoutMilliseconds, Is.EqualTo(4000));
    }
}
