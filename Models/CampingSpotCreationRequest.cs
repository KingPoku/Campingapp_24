using Microsoft.AspNetCore.Mvc;

namespace Campingapp_24.Models
{
    public class CampingSpotCreationRequest
    {
        public string Spot_Name { get; set; }
        public double Price_Per_Night { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip_Code { get; set; }
        public string Country { get; set; }
        [FromForm]
        public IEnumerable<IFormFile> ImageUrls { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();

        public int OwnerId { get; set; }
    }
}
