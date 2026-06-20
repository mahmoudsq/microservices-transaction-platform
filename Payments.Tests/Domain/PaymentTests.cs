using BuildingBlocks.Exceptions;
using FluentAssertions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Events;

namespace Payments.Tests.Domain;

public class PaymentTests
{
    // ── Constructor ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsPendingStatus()
    {
        var p = new Payment(Guid.NewGuid(), 100m, "USD", "corr-1");
        p.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Constructor_SetsTransactionIdAmountCurrencyCorrelationId()
    {
        var transactionId = Guid.NewGuid();
        var p = new Payment(transactionId, 250m, "EUR", "corr-abc");

        p.TransactionId.Should().Be(transactionId);
        p.Amount.Should().Be(250m);
        p.Currency.Should().Be("EUR");
        p.CorrelationId.Should().Be("corr-abc");
    }

    [Fact]
    public void Constructor_AssignsNonEmptyId()
    {
        var p = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        p.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_RaisesPaymentStartedEvent()
    {
        var p = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        p.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PaymentStartedEvent>();
    }

    [Fact]
    public void Constructor_PaymentStartedEvent_HasCorrectPaymentIdAndTransactionId()
    {
        var transactionId = Guid.NewGuid();
        var p = new Payment(transactionId, 100m, "USD", "corr");

        var evt = p.DomainEvents.OfType<PaymentStartedEvent>().Single();
        evt.PaymentId.Should().Be(p.Id);
        evt.TransactionId.Should().Be(transactionId);
    }

    // ── Confirm ──────────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_WhenPending_ChangesStatusToConfirmed()
    {
        var p = CreatePending();
        p.Confirm();
        p.Status.Should().Be(PaymentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenPending_SetsConfirmedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var p = CreatePending();
        p.Confirm();
        p.ConfirmedAt.Should().NotBeNull().And.BeAfter(before);
    }

    [Fact]
    public void Confirm_WhenPending_RaisesPaymentConfirmedEvent()
    {
        var p = CreatePending();
        p.Confirm();
        p.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PaymentConfirmedEvent>();
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ThrowsDomainException_AlreadyConfirmed()
    {
        var p = CreatePending();
        p.Confirm();
        var act = () => p.Confirm();
        act.Should().Throw<DomainException>().WithMessage("Payment.AlreadyConfirmed");
    }

    [Fact]
    public void Confirm_WhenFailed_ThrowsDomainException_CannotConfirmFailed()
    {
        var p = CreatePending();
        p.Fail();
        var act = () => p.Confirm();
        act.Should().Throw<DomainException>().WithMessage("Payment.CannotConfirmFailed");
    }

    // ── Fail ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Fail_WhenPending_ChangesStatusToFailed()
    {
        var p = CreatePending();
        p.Fail();
        p.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Fail_WhenPending_RaisesPaymentFailedEvent()
    {
        var p = CreatePending();
        p.Fail();
        p.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PaymentFailedEvent>();
    }

    [Fact]
    public void Fail_WhenConfirmed_ThrowsDomainException_CannotFailConfirmed()
    {
        var p = CreatePending();
        p.Confirm();
        var act = () => p.Fail();
        act.Should().Throw<DomainException>().WithMessage("Payment.CannotFailConfirmed");
    }

    [Fact]
    public void Fail_WhenAlreadyFailed_ThrowsDomainException_AlreadyFailed()
    {
        var p = CreatePending();
        p.Fail();
        var act = () => p.Fail();
        act.Should().Throw<DomainException>().WithMessage("Payment.AlreadyFailed");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Payment CreatePending()
    {
        var p = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        p.ClearDomainEvents();
        return p;
    }
}
