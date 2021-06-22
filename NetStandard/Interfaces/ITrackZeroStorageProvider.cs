using TrackZero.NetStandard.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrackZero.NetStandard.Interfaces
{
    public interface ITrackZeroStorageProvider
    {
        Task StoreAsync(Entity entity);
        Task StoreAsync(IEnumerable<Entity> entities);
        Task StoreAsync(Event @event);
        Task StoreAsync(IEnumerable<Event> events);
        Task<IEnumerable<Entity>> GetPendingEntitiesAsync();
        Task<IEnumerable<Event>> GetPendingEventsAsync();
        Task CompleteAsync(Entity entity);
        Task CompleteAsync(IEnumerable<Entity> entities);
        Task CompleteAsync(Event @event);
        Task CompleteAsync(IEnumerable<Event> events);


    }
}
