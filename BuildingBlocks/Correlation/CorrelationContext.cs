namespace BuildingBlocks.Correlation;

public class CorrelationContext(string correlationId) 
    : ICorrelationContext
{
    public string CorrelationId { get; } = correlationId;
}
