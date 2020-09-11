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
        public long Position { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    public class EventStore : IEventStore
    {
        public const string StorageFile = "storage.json";
        private readonly IList<IEvent> playedEvents = new List<IEvent>();

        private long currentPosition = 0;

        public EventStore()
        {
            playedEvents = LoadEvents();
        }

        private IList<IEvent> LoadEvents()
        {
            if (!File.Exists(StorageFile))
                return new List<IEvent>();
            var fileContent = File.ReadAllLines(StorageFile);
            var storedEvents = fileContent.Select(serializedDto => JsonConvert.DeserializeObject<StoredEvent>(serializedDto))
                               .OrderBy(serializedEvent => serializedEvent.Position)
                               .ToList();
            currentPosition = storedEvents.LastOrDefault()?.Position ?? 0;
            return storedEvents.Select(DeserializeEvent)
                               .ToList();
        }

        private IEvent DeserializeEvent(StoredEvent serializedEvent)
        {
            if (serializedEvent.Type == typeof(ModelSelectedEvent).Name)
                return JsonConvert.DeserializeObject<ModelSelectedEventDto>(serializedEvent.Payload).ToDomain();
            if (serializedEvent.Type == typeof(OptionSelectedEvent).Name)
                return JsonConvert.DeserializeObject<OptionSelectedEventDto>(serializedEvent.Payload).ToDomain();
            if (serializedEvent.Type == typeof(OptionAvailableEvent).Name)
                return JsonConvert.DeserializeObject<OptionAvailableEventDto>(serializedEvent.Payload).ToDomain();
            throw new NotSupportedException("Type d'evenement non supporté");
        }

        public Task<IEnumerable<IEvent>> LoadEvents(UserProfileId userProfileId)
         => Task.FromResult(playedEvents.Where(x => x.UserId.Equals(userProfileId)));

        public Task Persist(IEnumerable<IEvent> events)
            => File.AppendAllLinesAsync(StorageFile, events.Select(GetStoredEvent)
                                                           .Select(storedEvent => JsonConvert.SerializeObject(storedEvent)));

        private StoredEvent GetStoredEvent(IEvent @event)
             => new StoredEvent
             {
                 Position = currentPosition++,
                 Type = @event.GetType().Name,
                 Payload = JsonConvert.SerializeObject(@event)
             };
    }
}
