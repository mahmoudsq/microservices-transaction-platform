using BuildingBlocks.Abstractions;

namespace BuildingBlocks.SharedEntities;

public abstract class Entity : IHasDomainEvents
{
    public Guid Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() 
        => _domainEvents.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void SetId()
        => Id = Guid.CreateVersion7();
}
