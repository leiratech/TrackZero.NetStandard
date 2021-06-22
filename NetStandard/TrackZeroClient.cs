using TrackZero.NetStandard.Interfaces;
using TrackZero.NetStandard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TrackZero.NetStandard
{
    public class TrackZeroClient
    {
        private readonly HttpClient httpClient;
        private readonly ITrackZeroStorageProvider storageProvider;

        public string ApiKey { get; private set; }

        Task backgroundTask;
        public TrackZeroClient(string apiKey, ITrackZeroStorageProvider storageProvider = null)
        {
            ApiKey = apiKey;
            this.storageProvider = storageProvider;
            httpClient = new HttpClient();
            SetApiKey(apiKey);

            if (storageProvider != null)
            {
                backgroundTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await SendPending().ConfigureAwait(false);
                        }
                        catch
                        {
                            //super easy error handling
                        }
                        await Task.Delay(TimeSpan.FromSeconds(60)).ConfigureAwait(false);
                    }
                });
            }
        }

        private async Task SendPending()
        {
            Console.WriteLine("Timer Ticked");
            var entities = await storageProvider.GetPendingEntitiesAsync().ConfigureAwait(false);
            var events = await storageProvider.GetPendingEventsAsync().ConfigureAwait(false);

            var entitiesTask = UpsertEntityAsync(entities);
            var eventsTask = TrackEventAsync(events);

            Task.WaitAll(entitiesTask, eventsTask);

            Console.WriteLine($"Sent Entities {entities.Count()} | Events {events.Count()}");
        }

        public void SetApiKey(string apiKey)
        {
            ApiKey = apiKey;
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
            httpClient.DefaultRequestHeaders.Add("X-API-VERSION", "1.0");
            httpClient.BaseAddress = new Uri("https://api.trackzero.io");
        }

        /// <summary>
        /// Adds a new entity if it doesn't exist (based on Id and Type) or updates existing one if it exists.
        /// </summary>
        /// <param name="entity">Entity to create. Any EntityReference in CustomAttributes will automatically be created if do not exist.</param>
        /// <returns></returns>
        public async Task<Entity> UpsertEntityAsync(Entity entity)
        {
            try
            {

                if (storageProvider != null)
                {
                    await storageProvider.StoreAsync(entity).ConfigureAwait(false);
                }

                var response = await httpClient.PostAsync("tracking/entities", new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    if (storageProvider != null)
                    {
                        await storageProvider.CompleteAsync(entity).ConfigureAwait(false);
                    }
                    return entity;
                }

                throw new Exception("Unknown Error Occured");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int PageSize { get; set; } = 30;
        public async Task<IEnumerable<Entity>> UpsertEntityAsync(IEnumerable<Entity> entities)
        {
            try
            {
                if (storageProvider != null)
                {
                    await storageProvider.StoreAsync(entities).ConfigureAwait(false);
                }

                for (int i = 0; i < Math.Ceiling(entities.Count() / (double)PageSize); i++)
                {
                    var entitiesPage = entities.Skip(i * PageSize).Take(PageSize);
                    var response = await httpClient.PostAsync("tracking/entities/bulk", new StringContent(JsonConvert.SerializeObject(entitiesPage), Encoding.UTF8, "application/json")).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        if (storageProvider != null)
                        {
                            await storageProvider.CompleteAsync(entitiesPage).ConfigureAwait(false);
                        }
                    }
                }

                return entities;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds a new event.
        /// </summary>
        /// <param name="event">Event to create. Any EntityReference in CustomAttributes, Emitter and Targets will automatically be created if do not exist.</param>
        /// <returns></returns>
        public async Task<Event> TrackEventAsync(Event @event)
        {
            try
            {
                if (storageProvider != null)
                {
                    if (@event.Id == default)
                    {
                        @event.Id = Guid.NewGuid();
                    }
                    await storageProvider.StoreAsync(@event).ConfigureAwait(false);
                }


                var response = await httpClient.PostAsync("tracking/events", new StringContent(JsonConvert.SerializeObject(@event), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    if (storageProvider != null)
                    {
                        await storageProvider.CompleteAsync(@event).ConfigureAwait(false);
                    }

                    return @event;
                }

                throw new Exception("Unknown Error Occured");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Event>> TrackEventAsync(IEnumerable<Event> events)
        {
            try
            {
                if (storageProvider != null)
                {
                    events.Where(e => e.Id == default).AsParallel().ForAll(e => e.Id = Guid.NewGuid());
                    await storageProvider.StoreAsync(events).ConfigureAwait(false);
                }


                for (int i = 0; i < Math.Ceiling(events.Count() / (double)PageSize); i++)
                {
                    var eventsPage = events.Skip(i * PageSize).Take(PageSize);
                    var response = await httpClient.PostAsync("tracking/events/bulk", new StringContent(JsonConvert.SerializeObject(eventsPage), Encoding.UTF8, "application/json")).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        if (storageProvider != null)
                        {
                            await storageProvider.CompleteAsync(eventsPage).ConfigureAwait(false);
                        }
                    }
                }

                return events;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
