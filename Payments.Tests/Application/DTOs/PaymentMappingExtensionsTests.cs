using FluentAssertions;
using Payments.Application.DTOs;
using Payments.Domain.Entities;

namespace Payments.Tests.Application.DTOs;

public class PaymentMappingExtensionsTests
{
    [Fact]
    public void ToDto_MapsAllScalarProperties()
    {
        var transactionId = Guid.NewGuid();
        var payment = new Payment(transactionId, 150m, "EUR", "corr-xyz");

        var dto = payment.ToDto();

        dto.Id.Should().Be(payment.Id);
        dto.TransactionId.Should().Be(transactionId);
        dto.Amount.Should().Be(150m);
        dto.Currency.Should().Be("EUR");
        dto.CorrelationId.Should().Be("corr-xyz");
    }

    [Fact]
    public void ToDto_StatusIsStringRepresentation()
    {
        var payment = new Payment(Guid.NewGuid(), 100m, "USD", "corr");

        var dto = payment.ToDto();

        dto.Status.Should().Be("Pending");
    }

    [Fact]
    public void ToDto_ConfirmedAt_IsNull_WhenPending()
    {
        var payment = new Payment(Guid.NewGuid(), 100m, "USD", "corr");

        var dto = payment.ToDto();

        dto.ConfirmedAt.Should().BeNull();
    }
}
