using System;

namespace BMS.API.Modules.Shared.Models
{
    public class Locality
    {
        public Guid Id { get; set; }
        public Guid CityId { get; set; }
        public string Name { get; set; }

        public City City { get; set; }
    }
}
