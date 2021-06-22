using System;
using System.Text;

namespace TrackZero.NetStandard.Interfaces
{
    public interface IEntityReference
    {
        string Type { get; set; }
        object Id { get; }
        void Validate();
    }
}
