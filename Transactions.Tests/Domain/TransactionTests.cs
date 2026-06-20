using BuildingBlocks.Exceptions;
using FluentAssertions;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;
using Transactions.Domain.Events;

namespace Transactions.Tests.Domain;

public class TransactionTests
{
    // ── Constructor ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsDraftStatus()
    {
        var t = new Transaction("REF", "USD", "key");
        t.Status.Should().Be(TransactionStatus.Draft);
    }

    [Fact]
    public void Constructor_SetsReferenceAndCurrencyAndIdempotencyKey()
    {
        var t = new Transaction("REF-001", "EUR", "idem-1");
        t.Reference.Should().Be("REF-001");
        t.Currency.Should().Be("EUR");
        t.IdempotencyKey.Should().Be("idem-1");
    }

    [Fact]
    public void Constructor_AssignsNonEmptyId()
    {
        var t = new Transaction("REF", "USD", "key");
        t.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_EmptyItems_TotalAmountIsZero()
    {
        var t = new Transaction("REF", "USD", "key");
        t.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void Constructor_RaisesNoDomainEvents()
    {
        var t = new Transaction("REF", "USD", "key");
        t.DomainEvents.Should().BeEmpty();
    }

    // ── AddItem ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddItem_WhenDraft_AddsItemToCollection()
    {
        var t = new Transaction("REF", "USD", "key");
        t.AddItem("P1", 2, 10m);
        t.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_WhenDraft_UpdatesTotalAmount()
    {
        var t = new Transaction("REF", "USD", "key");
        t.AddItem("P1", 2, 10m);
        t.AddItem("P2", 1, 5m);
        t.TotalAmount.Should().Be(25m);
    }

    [Fact]
    public void AddItem_WhenSubmitted_ThrowsDomainException_CannotModifyAfterSubmission()
    {
        var t = CreateSubmitted();
        var act = () => t.AddItem("P2", 1, 5m);
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotModifyAfterSubmission");
    }

    [Fact]
    public void AddItem_WhenCompleted_ThrowsDomainException()
    {
        var t = CreateCompleted();
        var act = () => t.AddItem("P2", 1, 5m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WhenFailed_ThrowsDomainException()
    {
        var t = CreateFailed();
        var act = () => t.AddItem("P2", 1, 5m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WhenCancelled_ThrowsDomainException()
    {
        var t = CreateCancelled();
        var act = () => t.AddItem("P2", 1, 5m);
        act.Should().Throw<DomainException>();
    }

    // ── Submit ───────────────────────────────────────────────────────────────

    [Fact]
    public void Submit_WhenDraftWithItems_ChangesStatusToSubmitted()
    {
        var t = new Transaction("REF", "USD", "key");
        t.AddItem("P1", 1, 10m);
        t.Submit("corr-1");
        t.Status.Should().Be(TransactionStatus.Submitted);
    }

    [Fact]
    public void Submit_WhenDraftWithItems_RaisesTransactionSubmittedEvent()
    {
        var t = new Transaction("REF", "USD", "key");
        t.AddItem("P1", 1, 10m);
        t.Submit("corr-1");
        t.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TransactionSubmittedEvent>();
    }

    [Fact]
    public void Submit_SubmittedEvent_HasCorrectTransactionIdTotalAmountCurrencyCorrelationId()
    {
        var t = new Transaction("REF", "EUR", "key");
        t.AddItem("P1", 2, 15m);
        t.Submit("corr-xyz");

        var evt = t.DomainEvents.OfType<TransactionSubmittedEvent>().Single();
        evt.TransactionId.Should().Be(t.Id);
        evt.TotalAmount.Should().Be(30m);
        evt.Currency.Should().Be("EUR");
        evt.CorrelationId.Should().Be("corr-xyz");
    }

    [Fact]
    public void Submit_WhenNoItems_ThrowsDomainException_CannotSubmitWithoutItems()
    {
        var t = new Transaction("REF", "USD", "key");
        var act = () => t.Submit("corr");
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotSubmitWithoutItems");
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsDomainException_AlreadySubmitted()
    {
        var t = CreateSubmitted();
        var act = () => t.Submit("corr-2");
        act.Should().Throw<DomainException>().WithMessage("Transaction.AlreadySubmitted");
    }

    [Fact]
    public void Submit_WhenCompleted_ThrowsDomainException()
    {
        var t = CreateCompleted();
        var act = () => t.Submit("corr");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Submit_WhenCancelled_ThrowsDomainException()
    {
        var t = CreateCancelled();
        var act = () => t.Submit("corr");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Submit_WhenFailed_ThrowsDomainException()
    {
        var t = CreateFailed();
        var act = () => t.Submit("corr");
        act.Should().Throw<DomainException>();
    }

    // ── Cancel ───────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenDraft_ChangesStatusToCancelled()
    {
        var t = new Transaction("REF", "USD", "key");
        t.Cancel();
        t.Status.Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenDraft_RaisesTransactionCancelledEvent()
    {
        var t = new Transaction("REF", "USD", "key");
        t.Cancel();
        t.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TransactionCancelledEvent>();
    }

    [Fact]
    public void Cancel_WhenSubmitted_ChangesStatusToCancelled()
    {
        var t = CreateSubmitted();
        t.Cancel();
        t.Status.Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ThrowsDomainException_CannotCancelCompleted()
    {
        var t = CreateCompleted();
        var act = () => t.Cancel();
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotCancelCompleted");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException_AlreadyCancelled()
    {
        var t = CreateCancelled();
        var act = () => t.Cancel();
        act.Should().Throw<DomainException>().WithMessage("Transaction.AlreadyCancelled");
    }

    [Fact]
    public void Cancel_WhenFailed_ThrowsDomainException_CannotCancelFailed()
    {
        var t = CreateFailed();
        var act = () => t.Cancel();
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotCancelFailed");
    }

    // ── Complete ─────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_WhenSubmitted_ChangesStatusToCompleted()
    {
        var t = CreateSubmitted();
        t.Complete();
        t.Status.Should().Be(TransactionStatus.Completed);
    }

    [Fact]
    public void Complete_WhenSubmitted_SetsCompletedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var t = CreateSubmitted();
        t.Complete();
        t.CompletedAt.Should().NotBeNull().And.BeAfter(before);
    }

    [Fact]
    public void Complete_WhenSubmitted_RaisesTransactionCompletedEvent()
    {
        var t = CreateSubmitted();
        t.ClearDomainEvents();
        t.Complete();
        t.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TransactionCompletedEvent>();
    }

    [Fact]
    public void Complete_WhenDraft_ThrowsDomainException_CannotCompleteFromCurrentState()
    {
        var t = new Transaction("REF", "USD", "key");
        var act = () => t.Complete();
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotCompleteFromCurrentState");
    }

    [Fact]
    public void Complete_WhenCompleted_ThrowsDomainException()
    {
        var t = CreateCompleted();
        var act = () => t.Complete();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_WhenCancelled_ThrowsDomainException()
    {
        var t = CreateCancelled();
        var act = () => t.Complete();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_WhenFailed_ThrowsDomainException()
    {
        var t = CreateFailed();
        var act = () => t.Complete();
        act.Should().Throw<DomainException>();
    }

    // ── Fail ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Fail_WhenDraft_ChangesStatusToFailed()
    {
        var t = new Transaction("REF", "USD", "key");
        t.Fail();
        t.Status.Should().Be(TransactionStatus.Failed);
    }

    [Fact]
    public void Fail_WhenSubmitted_ChangesStatusToFailed()
    {
        var t = CreateSubmitted();
        t.ClearDomainEvents();
        t.Fail();
        t.Status.Should().Be(TransactionStatus.Failed);
    }

    [Fact]
    public void Fail_WhenFailed_ThrowsDomainException_AlreadyFailed()
    {
        var t = CreateFailed();
        var act = () => t.Fail();
        act.Should().Throw<DomainException>().WithMessage("Transaction.AlreadyFailed");
    }

    [Fact]
    public void Fail_WhenCompleted_ThrowsDomainException_CannotFailCompleted()
    {
        var t = CreateCompleted();
        var act = () => t.Fail();
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotFailCompleted");
    }

    [Fact]
    public void Fail_WhenCancelled_ThrowsDomainException_CannotFailCancelled()
    {
        var t = CreateCancelled();
        var act = () => t.Fail();
        act.Should().Throw<DomainException>().WithMessage("Transaction.CannotFailCancelled");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Transaction CreateSubmitted()
    {
        var t = new Transaction("REF", "USD", Guid.NewGuid().ToString());
        t.AddItem("P1", 1, 10m);
        t.Submit("corr");
        t.ClearDomainEvents();
        return t;
    }

    private static Transaction CreateCompleted()
    {
        var t = CreateSubmitted();
        t.Complete();
        t.ClearDomainEvents();
        return t;
    }

    private static Transaction CreateCancelled()
    {
        var t = new Transaction("REF", "USD", Guid.NewGuid().ToString());
        t.Cancel();
        t.ClearDomainEvents();
        return t;
    }

    private static Transaction CreateFailed()
    {
        var t = new Transaction("REF", "USD", Guid.NewGuid().ToString());
        t.Fail();
        t.ClearDomainEvents();
        return t;
    }
}
