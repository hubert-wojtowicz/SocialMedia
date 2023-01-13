using CQRS.Core.Events;

namespace CQRS.Core.Domain;

public abstract class AggregateRoot
{
    protected Guid _id;
    private readonly List<BaseEvent> _changes = new();

    public Guid Id => _id;
    public int Version { get; set; } = -1; // -1  means not set

    public IEnumerable<BaseEvent> GetUncommittedChanges() => _changes;

    public void MarkChangesAsCommitted()
    {
        _changes.Clear();
    }

    public void ReplyEvents(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyChange(@event, false);
        }
    }

    protected void RaiseEvent(BaseEvent @event)
    {
        ApplyChange(@event, true);
    }

    private void ApplyChange(BaseEvent @event, bool isNew)
    {
        var method = GetType().GetMethod("Appply", new[] { @event.GetType() });
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method), $"Apply method was not found in aggregate for {@event.GetType().Name}.");
        }

        method.Invoke(this, new object[] { @event });

        if (isNew)
        {
            _changes.Add(@event);
        }
    }
}
