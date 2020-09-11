using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DDDTraining.Tests
{
    public class EventStoreShould
    {
        private static readonly UserProfileId userProfileId1 = new UserProfileId(Guid.NewGuid());
        private static readonly Model model1 = new Model("1");
        private static readonly Model model2 = new Model("2");

        public void ResetStore()
        {
            if (File.Exists(EventStore.StorageFile))
                File.Delete(EventStore.StorageFile);
        }
        [Fact]
        public async Task Store_Any_Event()
        {
            ResetStore();

            var testEvent = new ModelSelectedEvent(userProfileId1, model1);
            var store = new EventStore();
            Assert.Empty(await store.LoadEvents(userProfileId1));
            await store.Persist(new IEvent[] { testEvent });

            var store2 = new EventStore();
            var modelSelectedEvents = (await store2.LoadEvents(userProfileId1)).OfType<ModelSelectedEvent>();
            Assert.Contains(modelSelectedEvents, @event => @event.Equals(testEvent));
        }

        [Fact]
        public async Task Keep_Event_Order_When_Store_Is_Access_Concurrently()
        {
            ResetStore();
            var testEvent = new ModelSelectedEvent(userProfileId1, model1);
            var testEvent2 = new ModelSelectedEvent(userProfileId1, model2);
            var testEvent3 = new ModelSelectedEvent(userProfileId1, model1);

            var store = new EventStore();
            await Task.WhenAll(Task.Run(() => store.Persist(new IEvent[] { testEvent, testEvent2 })),
                               Task.Run(() => store.Persist(new IEvent[] { testEvent3 })));

            var store2 = new EventStore();
            var events = await store2.LoadEvents(userProfileId1);            
        }
    }
}
