using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Campingapp_24.Data;
using Campingapp_24.Models;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmenitiesController : ControllerBase
    {
        private readonly Database _database;

        public AmenitiesController(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        // GET: api/amenities
        [HttpGet]
        public IActionResult GetAmenities()
        {
            try
            {
                string query = "SELECT * FROM Amenities";
                List<Amenities> amenities = new List<Amenities>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Amenities amenity = new Amenities
                        {
                            Amenities_ID = reader.GetInt32("Amenities_ID"),
                            Amenities_Name = reader.GetString("Amenities_Name")
                        };

                        amenities.Add(amenity);
                    }

                    reader.Close();
                }

                return Ok(amenities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/amenities/{id}
        [HttpGet("{id}")]
        public IActionResult GetAmenity(int id)
        {
            try
            {
                string query = $"SELECT * FROM Amenities WHERE Amenities_ID = {id}";
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        Amenities amenity = new Amenities
                        {
                            Amenities_ID = reader.GetInt32("Amenities_ID"),
                            Amenities_Name = reader.GetString("Amenities_Name")
                        };

                        reader.Close();
                        return Ok(amenity);
                    }
                    else
                    {
                        reader.Close();
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/amenities
        [HttpPost]
        public IActionResult CreateAmenity([FromBody] Amenities amenity)
        {
            try
            {
                string query = @"
            INSERT INTO Amenities (Amenities_Name) 
            VALUES (@AmenitiesName)
        ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@AmenitiesName", amenity.Amenities_Name);
                    command.ExecuteNonQuery();
                }

                return Ok("Amenity created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // PUT: api/amenities/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAmenity(int id, [FromBody] Amenities amenity)
        {
            try
            {
                string query = $@"
                    UPDATE Amenities 
                    SET Amenities_Name = @AmenitiesName
                    WHERE Amenities_ID = {id}
                ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@AmenitiesName", amenity.Amenities_Name);
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

        // DELETE: api/amenities/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAmenity(int id)
        {
            try
            {
                string query = $"DELETE FROM Amenities WHERE Amenities_ID = {id}";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
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
    }
}
