using CQRS.Core.Domain;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Post.Cmd.Infrastructure.Config;

namespace Post.Cmd.Infrastructure.Repositories;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly IMongoCollection<EventModel> _eventsStoreCollection;

    public EventStoreRepository(IOptions<MongoDbConfig> mongoDbOptions)
    {
        var config = mongoDbOptions.Value;
        var mongoClient = new MongoClient(config.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(config.DatabaseName);
        _eventsStoreCollection = mongoDatabase.GetCollection<EventModel>(config.CollectionName);
    }

    public async Task SaveAsync(EventModel @event)
    {
        await _eventsStoreCollection.InsertOneAsync(@event);
    }

    public async Task<List<EventModel>> GetByAggregateIdAsync(Guid aggregateId)
    {
        return await _eventsStoreCollection
            .Find(x => x.AggregateIdentifier == aggregateId)
            .ToListAsync();
    }
}