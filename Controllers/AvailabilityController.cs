using Campingapp_24.Data;
using Campingapp_24.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly Database _database;

        public AvailabilityController(Database database)
        {
            _database = database;
        }

        // GET: api/availability
        // Endpoint to get all availability records
        [HttpGet]
        public IActionResult GetAvailability()
        {
            try
            {
                // SQL query to select all records from the Availability table
                string query = "SELECT * FROM Availability";
                List<Availability> availabilities = new List<Availability>();

                // Execute the query using the database connection
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();  // Open the database connection
                    command.CommandText = query;
                    var reader = command.ExecuteReader(); // Execute the query and get a data reader

                    // Read the results from the query
                    while (reader.Read())
                    {
                        // Create a new Availability object and populate its properties
                        Availability availability = new Availability
                        {
                            AvailabilityID = reader.GetInt32("AvailabilityID"),
                            SpotID = reader.GetInt32("SpotID"),
                            Date = reader.GetDateTime("Date"),
                            Isbooked = reader.GetBoolean("Isbooked")
                        };

                        // Add the availability record to the list
                        availabilities.Add(availability);
                    }

                    reader.Close();
                }

                // Return the list of availability records as the response
                return Ok(availabilities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/availability/check
        // Endpoint to check the availability of a camping spot for a given date range
        [HttpGet("check")]
        public IActionResult CheckAvailability([FromQuery] int spotID, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate)
        {
            try
            {
                // SQL query to count booked dates for the specified spot and date range
                string query = @"
            SELECT COUNT(*) 
            FROM Availability 
            WHERE SpotID = @spotID AND Date >= @checkInDate AND Date < @checkOutDate AND Isbooked = 1";

                // Execute the query using the database connection
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();// Open the database connection
                    command.CommandText = query;

                    // Add parameters to the query to prevent SQL injection
                    command.Parameters.AddWithValue("@spotID", spotID);
                    command.Parameters.AddWithValue("@checkInDate", checkInDate);
                    command.Parameters.AddWithValue("@checkOutDate", checkOutDate);

                    // Execute the query and get the count of booked dates
                    var count = (long)command.ExecuteScalar();

                    // Determine if the spot is available based on the count
                    bool available = count == 0;

                    // Return the availability status as the response
                    return Ok(new { available });
                }
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error response if an exception occurs
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }







        // POST: api/availability
        [HttpPost]
        public IActionResult CreateAvailability([FromBody] Availability availability)
        {
            try
            {
                string query = $@"
                    INSERT INTO Availability (SpotID, Date, Isbooked) 
                    VALUES ({availability.SpotID}, '{availability.Date:yyyy-MM-dd}', Isbooked = {(availability.Isbooked ? 1 : 0)}
)
                ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }

                return Ok("Availability created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/availability/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAvailability(int id, [FromBody] Availability availability)
        {
            try
            {
                string query = $@"
                    UPDATE Availability 
                    SET SpotID = {availability.SpotID}, 
                        Date = '{availability.Date:yyyy-MM-dd}', 
                        Isbooked = {(availability.Isbooked ? 1 : 0)}

                    WHERE AvailabilityID = {id}
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

        // DELETE: api/availability/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAvailability(int id)
        {
            try
            {
                string query = $"DELETE FROM Availability WHERE AvailabilityID = {id}";

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

        // PUT: api/availability/toggle/{id}
        // Endpoint to toggle the availability status of a camping spot by its availability ID
        [HttpPut("toggle/{id}")]
        public IActionResult ToggleAvailability(int id)
        {
            try
            {

                // Establish a connection to the database and create a command
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();// Open the database connection

                    // SQL query to toggle the availability status
                    command.CommandText = "UPDATE Availability SET Isbooked = NOT Isbooked WHERE AvailabilityID = @id";
                    command.Parameters.AddWithValue("@id", id); // Parameter to prevent SQL injection

                    // Execute the query and get the number of affected rows
                    int rowsAffected = command.ExecuteNonQuery();

                    // If no rows were affected, return a 404 Not Found response
                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }

                // Return a 204 No Content response if the update was successful
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}

