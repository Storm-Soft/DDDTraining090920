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
            var modelSelectedEvents = (await store.LoadEvents(userProfileId1)).OfType<ModelSelectedEvent>();
            Assert.Contains(modelSelectedEvents, @event => @event.Equals(testEvent));
        }
    }
}
