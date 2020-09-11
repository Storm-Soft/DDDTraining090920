using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DDDTraining.Tests
{
    public class StoredEvent
    {
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    public class EventStore : IEventStore
    {
        public const string StorageFile = "storage.json";
        private readonly IList<IEvent> playedEvents = new List<IEvent>();
        
        public EventStore()
        {
            this.playedEvents = LoadEvents();
        }
        
        private IList<IEvent> LoadEvents()
        {
            if (!File.Exists(StorageFile))
                return new List<IEvent>();
            var fileContent = File.ReadAllLines(StorageFile);
            return fileContent.Select(DeserializeEvent)
                              .ToList();
        }

        private IEvent DeserializeEvent(string serializedEvent)
        {
            var deserializedEvent = JsonConvert.DeserializeObject<StoredEvent>(serializedEvent);
            if (deserializedEvent.Type == typeof(ModelSelectedEvent).Name)
                return JsonConvert.DeserializeObject<ModelSelectedEvent>(deserializedEvent.Payload);
            if (deserializedEvent.Type == typeof(OptionSelectedEvent).Name)
                return JsonConvert.DeserializeObject<OptionSelectedEvent>(deserializedEvent.Payload);
            if (deserializedEvent.Type == typeof(OptionAvailableEvent).Name)
                return JsonConvert.DeserializeObject<OptionAvailableEvent>(deserializedEvent.Payload);
            throw new NotSupportedException("Type d'evenement non supporté");
        }

        public Task<IEnumerable<IEvent>> LoadEvents(UserProfileId userProfileId)
         => Task.FromResult(playedEvents.Where(x => x.UserId.Equals(userProfileId)));

        public Task Persist(IEnumerable<IEvent> events)
            => File.AppendAllLinesAsync(StorageFile, events.Select(GetStoredEvent)
                                                           .Select(storedEvent => JsonConvert.SerializeObject(storedEvent)));

        private static StoredEvent GetStoredEvent(IEvent @event)
             => new StoredEvent
             {
                 Type = @event.GetType().Name,
                 Payload = JsonConvert.SerializeObject(@event)
             };
    }
}
