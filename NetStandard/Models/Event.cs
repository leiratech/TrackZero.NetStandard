using TrackZero.NetStandard.Extensions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackZero.NetStandard.Models
{
    public class Event
    {
        public Event()
        {

        }

        public Event(string emitterType,
                     object emitterId,
                     string eventName,
                     object id = default,
                     DateTime? startTime = default,
                     Dictionary<string, object> customAttributes = default,
                     IEnumerable<EntityReference> targets = default,
                     DateTime? endTime = default)
                : this(new EntityReference(emitterType, emitterId),
                      eventName,
                      id,
                      startTime,
                      customAttributes,
                      targets,
                      endTime)
        {

        }

        public Event(string emitterType,
                     object emitterId,
                     string name)
            : this(emitterType, emitterId, name, default, DateTime.UtcNow, null, null, null)
        {

        }

        public Event(EntityReference emitter, string name, object id = default, DateTime? startTime = default, Dictionary<string, object> attributes = default, IEnumerable<EntityReference> targets = default, DateTime? endTime = default)
        {

            Emitter = emitter;

            Id = id ?? Guid.NewGuid();


            Name = name;

            StartTime = startTime;
            EndTime = endTime;

            CustomAttributes = attributes ?? new Dictionary<string, object>();

            Targets = targets?.ToList() ?? new List<EntityReference>();
            Validate();

        }

        public Event AddEntityReferencedAttribute(string attributeName, string type, object id)
        {
            id.ValidateTypeForPremitiveValue();
            CustomAttributes.Add(attributeName, new EntityReference(type, id));
            return this;
        }
        public Event AddAttribute(string attributeName, object value)
        {
            value.ValidateTypeForPremitiveValueOrReferenceType();
            CustomAttributes.Add(attributeName, value);
            return this;
        }

        public Event AddTarget(string type, object id)
        {
            id.ValidateTypeForPremitiveValue();
            Targets.Add(new EntityReference(type, id));
            return this;
        }


        public EntityReference Emitter { get; set; }

        public object Id { get; set; }
        public string Name { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Dictionary<string, object> CustomAttributes { get; }
        public List<EntityReference> Targets { get; }

        public bool SendSuccessful { get; set; }
        public DateTime StoreTime { get; set; }

        public void Validate()
        {
            Emitter.Validate();

            Id.ValidateTypeForPremitiveValue();
            if (Id == default)
            {
                throw new ArgumentNullException(nameof(Id));
            }

            if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentNullException(nameof(Name));
            }

            CustomAttributes?.Values.AsParallel().ForAll(o =>
            {
                o.ValidateTypeForPremitiveValueOrReferenceType();
            });

            CustomAttributes?.Keys.AsParallel().ForAll(o =>
            {
                if (string.IsNullOrEmpty(o) || string.IsNullOrWhiteSpace(o))
                {
                    new ArgumentNullException(nameof(CustomAttributes));
                }
            });

            Targets.AsParallel().ForAll(t => t.Validate());
        }
    }
}
