using BuildingBlocks.SharedEntities;

namespace BuildingBlocks.Idempotency;

public class InboxMessage : Entity
{
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;

    public string ServiceName { get; set; } = default!;
    public string Consumer { get; set; } = default!;
    public DateTime ReceivedAt { get; set; }
    public DateTime ProcessedAt { get; set; }

    public InboxMessage() => SetId();

    public InboxMessage(Guid id)
    {
        Id = id;
    }
}
