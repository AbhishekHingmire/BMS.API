using System;
using System.Collections.Generic;

namespace BMS.API.Modules.Shared.Models
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<Locality> Localities { get; set; } = new List<Locality>();
    }
}
