using NetTopologySuite.Geometries;

namespace TaviLi.Domain.ValueObjects
{
    public class Address
    {
        public required string FullAddress { get; set; }
        public required Point Location { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
    }
}
