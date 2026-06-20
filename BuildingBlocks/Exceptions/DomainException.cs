namespace BuildingBlocks.Exceptions;

public class DomainException(string code) : Exception(code)
{
    public string Code { get; } = code;
}
