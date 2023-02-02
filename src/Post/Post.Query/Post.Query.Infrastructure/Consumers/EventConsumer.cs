using System.Text.Json;
using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;

namespace Post.Query.Infrastructure.Consumers;

internal class EventConsumer : IEventConsumer
{
    private readonly IEventHandler _eventHandler;
    private readonly ConsumerConfig _consumerConfig;

    public EventConsumer(IOptions<ConsumerConfig> consumerConfig, IEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _consumerConfig = consumerConfig.Value;
    }
    public void Consume(string topic)
    {
        using var consumer = new ConsumerBuilder<string, string>(_consumerConfig)
            .SetKeyDeserializer(Deserializers.Utf8)
            .SetValueDeserializer(Deserializers.Utf8)
            .Build();

        consumer.Subscribe(topic);
        while (true)
        {
            var consumeResult = consumer.Consume();
            if (consumeResult.Message == null) continue;
            var serializerOptions = new JsonSerializerOptions
            {
                Converters = { new EventJsonConverter() }
            };
            var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, serializerOptions);
            var handlerMethod = _eventHandler.GetType().GetMethod("On", new Type[] { @event.GetType() });

            if (handlerMethod == null)
                throw new ArgumentNullException(nameof(handlerMethod), "Can not find handler method");

            handlerMethod.Invoke(_eventHandler, new object[] { @event });
            consumer.Commit(consumeResult);
        }
    }
}
