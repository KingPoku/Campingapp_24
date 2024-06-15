using Campingapp_24.Data;
using Campingapp_24.Models;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampingSpotCompositeController : ControllerBase
    {
        private readonly Database _database;

        public CampingSpotCompositeController(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCampingSpotWithDetails([FromForm] CampingSpotCreationRequest request)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    // Insert camping spot
                    int spotId = CreateCampingSpot(request);

                    // Define the path to the images directory
                    var uploadPath = Path.Combine("wwwroot", "camp-images");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }


                    // Insert images
                    foreach (var imageFile in request.ImageUrls)
                    {

                        Console.WriteLine($"Processing file: {imageFile.FileName}");

                        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Store the relative path in the database, not the physical file path
                        var relativePath = $"/camp-images/{fileName}";
                        CreateCampingSpotImage(spotId, relativePath);
                    }


                    // Insert amenities
                    foreach (var amenityName in request.Amenities)
                    {
                        CreateAmenity(spotId, amenityName);
                    }

                    transaction.Complete();
                    return Ok(new { SpotID = spotId, Spot_Name = request.Spot_Name, Price_Per_Night = request.Price_Per_Night });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }


        private int CreateCampingSpot(CampingSpotCreationRequest request)
        {
            string query = @"
        INSERT INTO CampingSpots (Spot_Name, Price_Per_Night, City, State, Zip_Code, Country, OwnerId) 
        VALUES (@Spot_Name, @Price_Per_Night, @City, @State, @Zip_Code, @Country, @OwnerId);
        SELECT LAST_INSERT_ID();
    ";

            using (var connection = _database.GetConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = query;
                command.Parameters.AddWithValue("@Spot_Name", request.Spot_Name);
                command.Parameters.AddWithValue("@Price_Per_Night", request.Price_Per_Night);
                command.Parameters.AddWithValue("@City", request.City);
                command.Parameters.AddWithValue("@State", request.State);
                command.Parameters.AddWithValue("@Zip_Code", request.Zip_Code);
                command.Parameters.AddWithValue("@Country", request.Country);
                command.Parameters.AddWithValue("@OwnerId", request.OwnerId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }


        private void CreateCampingSpotImage(int spotId, string imageUrl)
        {
            string query = @"
                INSERT INTO CampingSpotImage (SpotID, ImageUrl) 
                VALUES (@SpotID, @ImageUrl)
            ";

            using (var connection = _database.GetConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = query;
                command.Parameters.AddWithValue("@SpotID", spotId);
                command.Parameters.AddWithValue("@ImageUrl", imageUrl);
                command.ExecuteNonQuery();
            }
        }

        private void CreateAmenity(int spotId, string amenityName)
        {
            string query = @"
                INSERT INTO Amenities (SpotID, Amenities_Name) 
                VALUES (@SpotID, @AmenitiesName)
            ";

            using (var connection = _database.GetConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = query;
                command.Parameters.AddWithValue("@SpotID", spotId);
                command.Parameters.AddWithValue("@AmenitiesName", amenityName);
                command.ExecuteNonQuery();
            }
        }
    }
}
