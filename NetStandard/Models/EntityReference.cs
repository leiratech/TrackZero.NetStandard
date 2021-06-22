using TrackZero.NetStandard.Extensions;
using TrackZero.NetStandard.Interfaces;
using System;

namespace TrackZero.NetStandard.Models
{
    public class EntityReference : IEntityReference
    {
        /// <summary>
        /// Creates new reference to be used in events or entities.
        /// </summary>
        /// <param name="id">The id of the emitter entity, this Id is your own generated Id. It must be premitive type (ie. numeric, string, Guid)</param>
        /// <param name="type">The name of the entity type (ie. Car, Driver, User...etc)</param>
        public EntityReference(string type, object id)
        {
            Id = id;
            Type = type;
            Validate();
        }

        private EntityReference()
        {

        }

        public void Validate()
        {
            Id.ValidateTypeForPremitiveValue();
            if (Id == default)
            {
                throw new ArgumentNullException(nameof(Id));
            }

            if (string.IsNullOrEmpty(Type) || string.IsNullOrWhiteSpace(Type))
            {
                throw new ArgumentNullException(nameof(Type));
            }
        }

        public object Id { get; set; }
        public string Type { get; set; }
    }
}
