using Campingapp_24.Data;
using Campingapp_24.Models;
using Microsoft.AspNetCore.Mvc;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampingSpotsController : ControllerBase
    {
        private readonly Database _database;

        public CampingSpotsController(Database database)
        {
            _database = database;
        }

        // GET: api/campingspots
        [HttpGet]
        public IActionResult GetCampingSpots()
        {
            try
            {
                string query = "SELECT SpotID, Spot_Name, Price_Per_Night, City, State, Zip_Code, Country FROM CampingSpots";
                List<CampingSpots> campingSpots = new List<CampingSpots>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CampingSpots spot = new CampingSpots
                        {
                            SpotID = reader.GetInt32("SpotID"),
                            Spot_Name = reader.GetString("Spot_Name"),
                            Price_Per_Night = reader.GetDecimal("Price_Per_Night"),
                            City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString("City"),
                            State = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString("State"),
                            Zip_Code = reader.IsDBNull(reader.GetOrdinal("Zip_Code")) ? null : reader.GetString("Zip_Code"),
                            Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString("Country")
                        };

                        campingSpots.Add(spot);
                    }

                    reader.Close();
                }

                return Ok(campingSpots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/campingspots/filter
        [HttpGet("filter")]
        public IActionResult FilterCampingSpots(string amenities)
        {
            try
            {
                // Split the amenities input into individual amenities for querying
                var amenitiesList = amenities.Split(',').Select(a => a.Trim()).ToList();

                // Build the dynamic query with parameter placeholders
                string query = @"
            SELECT cs.SpotID, cs.Spot_Name, cs.Price_Per_Night, cs.City, cs.State, cs.Zip_Code, cs.Country
            FROM CampingSpots cs
            INNER JOIN Amenities a ON cs.SpotID = a.SpotID
            WHERE a.Amenities_Name IN (" + string.Join(", ", amenitiesList.Select((_, index) => $"@amenity{index}")) + @")
            GROUP BY cs.SpotID
        ";

                List<CampingSpots> campingSpots = new List<CampingSpots>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;

                    // Add amenities parameters to the command
                    for (int i = 0; i < amenitiesList.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@amenity{i}", amenitiesList[i]);
                    }

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CampingSpots spot = new CampingSpots
                        {
                            SpotID = reader.GetInt32("SpotID"),
                            Spot_Name = reader.GetString("Spot_Name"),
                            Price_Per_Night = reader.GetDecimal("Price_Per_Night"),
                            City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString("City"),
                            State = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString("State"),
                            Zip_Code = reader.IsDBNull(reader.GetOrdinal("Zip_Code")) ? null : reader.GetString("Zip_Code"),
                            Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString("Country")
                        };

                        campingSpots.Add(spot);
                    }

                    reader.Close();
                }

                return Ok(campingSpots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // GET: api/CampingSpots/{id}
        [HttpGet("{id}")]
        public IActionResult GetCampingSpot(int id)
        {
            try
            {
                var query = @"
            SELECT cs.*, a.Amenities_Name 
            FROM campingspots cs
            LEFT JOIN amenities a ON cs.SpotID = a.SpotID
            WHERE cs.SpotID = @id";

                CampingSpots campingSpot = null;

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", id);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (campingSpot == null)
                        {
                            campingSpot = new CampingSpots
                            {
                                SpotID = reader.GetInt32("SpotID"),
                                Spot_Name = reader.GetString("Spot_Name"),
                                City = reader.GetString("City"),
                                State = reader.GetString("State"),
                                Zip_Code = reader.GetString("Zip_Code"),
                                Price_Per_Night = reader.GetDecimal("Price_Per_Night"),
                                Country = reader.GetString("Country"),
                                Amenities = new List<string>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Amenities_Name")))
                        {
                            campingSpot.Amenities.Add(reader.GetString("Amenities_Name"));
                        }
                    }
                }

                if (campingSpot == null)
                {
                    return NotFound();
                }

                return Ok(campingSpot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpPost]
        public IActionResult CreateCampingSpot([FromBody] CampingSpots spot)
        {
            try
            {
                string query = $@"
            INSERT INTO CampingSpots (Spot_Name, Price_Per_Night, City, State, Zip_Code, Country, OwnerId) 
            VALUES ('{spot.Spot_Name}', {spot.Price_Per_Night}, '{spot.City}', '{spot.State}', '{spot.Zip_Code}', '{spot.Country}', {spot.OwnerId})
        ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }

                return Ok("Camping spot created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // PUT: api/campingspots/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCampingSpot(int id, [FromBody] CampingSpots spot)
        {
            try
            {
                string query = $@"
            UPDATE CampingSpots 
            SET Spot_Name = '{spot.Spot_Name}', 
                Price_Per_Night = {spot.Price_Per_Night}, 
                City = '{spot.City}', 
                State = '{spot.State}', 
                Zip_Code = '{spot.Zip_Code}', 
                Country = '{spot.Country}'
            WHERE SpotID = {id}
        ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // DELETE: api/campingspots/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteCampingSpot(int id)
        {
            try
            {
                string query = $"DELETE FROM CampingSpots WHERE SpotID = {id}";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("owner/{ownerId}")]
        public IActionResult GetCampingSpotsByOwner(int ownerId)
        {
            try
            {
                // SQL query to get camping spots along with amenities and booking details for a specific owner
                string query = @"
        SELECT cs.SpotID, cs.Spot_Name, cs.Price_Per_Night, cs.City, cs.State, cs.Zip_Code, cs.Country,
               a.Amenities_Name, 
               b.BookingID, b.Check_In_Date, b.Check_Out_Date, COALESCE(b.BookingID IS NOT NULL, 0) AS IsBooked
        FROM CampingSpots cs
        LEFT JOIN Amenities a ON cs.SpotID = a.SpotID
        LEFT JOIN Booking b ON cs.SpotID = b.SpotID
        WHERE cs.OwnerId = @ownerId
        ORDER BY cs.SpotID";

                // Dictionary to store camping spots, keyed by SpotID
                Dictionary<int, CampingSpots> campingSpotsDict = new Dictionary<int, CampingSpots>();

                // Execute the query using the database connection
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@ownerId", ownerId); // Set the ownerId parameter
                    var reader = command.ExecuteReader();

                    // Read the results from the query
                    while (reader.Read())
                    {
                        int spotId = reader.GetInt32("SpotID"); // Get the SpotID

                        // If the spot is not already in the dictionary, add it
                        if (!campingSpotsDict.ContainsKey(spotId))
                        {

                            // Create a new CampingSpots object and populate its properties
                            CampingSpots spot = new CampingSpots
                            {
                                SpotID = spotId,
                                Spot_Name = reader.GetString("Spot_Name"),
                                Price_Per_Night = reader.GetDecimal("Price_Per_Night"),
                                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString("City"),
                                State = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString("State"),
                                Zip_Code = reader.IsDBNull(reader.GetOrdinal("Zip_Code")) ? null : reader.GetString("Zip_Code"),
                                Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString("Country"),
                                Amenities = new List<string>(),
                                IsBooked = !reader.IsDBNull(reader.GetOrdinal("IsBooked")) && reader.GetBoolean("IsBooked"),
                                Check_In_Date = reader.IsDBNull(reader.GetOrdinal("Check_In_Date")) ? (DateTime?)null : reader.GetDateTime("Check_In_Date"),
                                Check_Out_Date = reader.IsDBNull(reader.GetOrdinal("Check_Out_Date")) ? (DateTime?)null : reader.GetDateTime("Check_Out_Date")
                            };
                            campingSpotsDict.Add(spotId, spot); // Add the spot to the dictionary
                        }

                        // Get the amenity name, if available, and add it to the amenities list
                        string amenityName = reader.IsDBNull(reader.GetOrdinal("Amenities_Name")) ? null : reader.GetString("Amenities_Name");
                        if (!string.IsNullOrEmpty(amenityName))
                        {
                            campingSpotsDict[spotId].Amenities.Add(amenityName);
                        }
                    }

                    reader.Close();
                }

                // Convert the dictionary values to a list and return the list as the response
                List<CampingSpots> campingSpots = campingSpotsDict.Values.ToList();
                return Ok(campingSpots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
