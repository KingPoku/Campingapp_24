using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Campingapp_24.Data;
using Campingapp_24.Models;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampingSpotImageController : ControllerBase
    {
        private readonly Database _database;

        public CampingSpotImageController(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        // GET: api/campingspotimage
        [HttpGet]
        public IActionResult GetCampingSpotImages()
        {
            try
            {
                string query = "SELECT * FROM CampingSpotImage";
                List<CampingSpotImage> campingSpotImages = new List<CampingSpotImage>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CampingSpotImage campingSpotImage = new CampingSpotImage
                        {
                            CampingSpotImage_ID = reader.GetInt32("CampingSpotImage_ID"),
                            SpotID = reader.GetInt32("SpotID"),
                            ImageUrl = reader.GetString("ImageUrl")
                        };

                        campingSpotImages.Add(campingSpotImage);
                    }

                    reader.Close();
                }

                return Ok(campingSpotImages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // Getting the images by SpotId
        [HttpGet("spot/{spotId}")]
        public IActionResult GetImagesBySpotId(int spotId)
        {
            try
            {
                string query = $"SELECT * FROM CampingSpotImage WHERE SpotID = {spotId}";
                List<CampingSpotImage> campingSpotImages = new List<CampingSpotImage>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CampingSpotImage campingSpotImage = new CampingSpotImage
                        {
                            CampingSpotImage_ID = reader.GetInt32("CampingSpotImage_ID"),
                            SpotID = reader.GetInt32("SpotID"),
                            ImageUrl = reader.GetString("ImageUrl")
                        };

                        campingSpotImages.Add(campingSpotImage);
                    }

                    reader.Close();
                }

                return Ok(campingSpotImages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }






        // GET: api/campingspotimage/{id}
        [HttpGet("{id}")]
        public IActionResult GetCampingSpotImage(int id)
        {
            try
            {
                string query = $"SELECT * FROM CampingSpotImage WHERE CampingSpotImage_ID = {id}";
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        CampingSpotImage campingSpotImage = new CampingSpotImage
                        {
                            CampingSpotImage_ID = reader.GetInt32("CampingSpotImage_ID"),
                            SpotID = reader.GetInt32("SpotID"),
                            ImageUrl = reader.GetString("ImageUrl")
                        };

                        reader.Close();
                        return Ok(campingSpotImage);
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

        // POST: api/campingspotimage
        [HttpPost]
        public IActionResult CreateCampingSpotImage([FromBody] CampingSpotImage campingSpotImage)
        {
            try
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
                    command.Parameters.AddWithValue("@SpotID", campingSpotImage.SpotID);
                    command.Parameters.AddWithValue("@ImageUrl", campingSpotImage.ImageUrl);
                    command.ExecuteNonQuery();
                }

                return Ok("Camping spot image created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // PUT: api/campingspotimage/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCampingSpotImage(int id, [FromBody] CampingSpotImage campingSpotImage)
        {
            try
            {
                string query = $@"
                    UPDATE CampingSpotImage 
                    SET SpotID = @SpotID, 
                        ImageUrl = @ImageUrl
                    WHERE CampingSpotImage_ID = {id}
                ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@SpotID", campingSpotImage.SpotID);
                    command.Parameters.AddWithValue("@ImageUrl", campingSpotImage.ImageUrl);
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

        // DELETE: api/campingspotimage/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteCampingSpotImage(int id)
        {
            try
            {
                string query = $"DELETE FROM CampingSpotImage WHERE CampingSpotImage_ID = {id}";

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
    }
}
