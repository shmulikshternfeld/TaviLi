namespace TaviLi.Application.Common.Dtos
{
    public class AddressDto
    {
        public required string FullAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
    }
}
