using TrackZero.NetStandard.Extensions;
using TrackZero.NetStandard.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackZero.NetStandard.Models
{
    public class Entity : IEntityReference
    {
        public Entity() { }
        /// <summary>
        /// Creates a new Entity Object
        /// </summary>
        /// <param name="id">The id of the object, this Id is your own Id. It must be premitive type (ie. numeric, string, Guid)</param>
        /// <param name="type">The name of the entity type (ie. Car, Driver, User...etc)</param>
        /// <param name="attributes">Any custom attributes you would like to include, the value can be a premitive type or an EntityReference</param>
        /// <exception cref="ArgumentNullException">Thrown when id is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when id or any customAttribute are not premitive type.</exception>
        /// <returns>Returns the entity you created or throws exception on error</returns>
        public Entity(string type, object id, Dictionary<string, object> attributes = default)
        {
            Type = type;
            Id = id;
            CustomAttributes = attributes ?? new Dictionary<string, object>();
            Validate();
        }

        public Entity(string type, object id)
        {
            Type = type;
            Id = id;
            Validate();
        }

        public Entity AddEntityReferencedAttribute(string attributeName, string type, object id)
        {
            id.ValidateTypeForPremitiveValue();
            CustomAttributes.Add(attributeName, new EntityReference(type, id));
            return this;
        }

        public Entity AddAttribute(string attributeName, object value)
        {
            value.ValidateTypeForPremitiveValueOrReferenceType();
            CustomAttributes.Add(attributeName, value);
            return this;
        }

        public Dictionary<string, object> CustomAttributes { get; } = new Dictionary<string, object>();

        public string Type { get; set; }

        public object Id { get; }

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

            CustomAttributes?.Values.AsParallel().ForAll(o => {
                o.ValidateTypeForPremitiveValueOrReferenceType();
            });
        }

        public bool SendSuccessful { get; set; }
        public DateTime StoreTime { get; set; }
    }
}
