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
    public class ReviewsController : ControllerBase
    {
        private readonly Database _database;

        public ReviewsController(Database database)
        {
            _database = database;
        }

        // GET: api/reviews
        [HttpGet]
        public IActionResult GetReviews()
        {
            try
            {
                string query = "SELECT * FROM Reviews";
                List<Reviews> reviews = new List<Reviews>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Reviews review = new Reviews
                        {
                            ReviewID = reader.GetInt32("ReviewID"),
                            UserID = reader.GetInt32("UserID"),
                            SpotID = reader.GetInt32("SpotID"),
                            ReviewText = reader.GetString("ReviewText")
                        };

                        reviews.Add(review);
                    }

                    reader.Close();
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/reviews/{spotID}
        [HttpGet("{spotID}")]
        public IActionResult GetReviewsForSpot(int spotID)
        {
            try
            {
                string query = $"SELECT * FROM Reviews WHERE SpotID = {spotID}";
                List<Reviews> reviews = new List<Reviews>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Reviews review = new Reviews
                        {
                            ReviewID = reader.GetInt32("ReviewID"),
                            UserID = reader.GetInt32("UserID"),
                            SpotID = reader.GetInt32("SpotID"),
                            ReviewText = reader.GetString("ReviewText")
                        };

                        reviews.Add(review);
                    }

                    reader.Close();
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





        // POST: api/reviews
        [HttpPost]
        public IActionResult CreateReview([FromBody] Reviews review)
        {
            try
            {
                string query = $@"
                    INSERT INTO Reviews (UserID, SpotID, ReviewText) 
                    VALUES ({review.UserID}, {review.SpotID}, '{review.ReviewText}')
                ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }

                return Ok("Review created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/reviews/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateReview(int id, [FromBody] Reviews review)
        {
            try
            {
                string query = $@"
                    UPDATE Reviews 
                    SET UserID = {review.UserID}, 
                        SpotID = {review.SpotID}, 
                        ReviewText = '{review.ReviewText}'
                    WHERE ReviewID = {id}
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

        // DELETE: api/reviews/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteReview(int id)
        {
            try
            {
                string query = $"DELETE FROM Reviews WHERE ReviewID = {id}";

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

