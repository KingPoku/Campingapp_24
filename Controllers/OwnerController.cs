using Campingapp_24.Data;
using Campingapp_24.Models;
using Campingapp_24.Utilities;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Text.Json;

namespace FoodRecipeApp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly Database _database;

        public OwnerController(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        // POST: api/owner/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] OwnerLoginModel login)
        {
            try
            {
                string query = "SELECT * FROM Owner WHERE Email = @Email";
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Email", login.Email);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var hashedPassword = reader.GetString(reader.GetOrdinal("Password"));
                        if (HashHelper.VerifyPassword(login.Password, hashedPassword))
                        {
                            var owner = new
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                FirstName = reader.GetString(reader.GetOrdinal("First_Name")),
                                LastName = reader.GetString(reader.GetOrdinal("Last_Name"))
                            };
                            return Ok(owner);
                        }
                    }
                }

                return Unauthorized("Invalid email or password");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CreateOwner([FromBody] Owner owner)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Hash the password
                owner.Password = HashHelper.HashPassword(owner.Password);

                // Insert owner into database
                string query = @"
        INSERT INTO Owner (Email, Password, First_Name, Last_Name) 
        VALUES (@Email, @Password, @FirstName, @LastName)";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Email", owner.Email);
                    command.Parameters.AddWithValue("@Password", owner.Password);
                    command.Parameters.AddWithValue("@FirstName", owner.First_Name);
                    command.Parameters.AddWithValue("@LastName", owner.Last_Name);

                    command.ExecuteNonQuery();

                    // Get the last inserted ID
                    command.CommandText = "SELECT LAST_INSERT_ID()";
                    var ownerId = (int)(ulong)command.ExecuteScalar();

                    // Create response object with all details
                    var response = new
                    {
                        ownerId,
                        owner.Email,
                        owner.Password,
                        owner.First_Name,
                        owner.Last_Name
                    };

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // GET: api/owner
        [HttpGet]
        public IActionResult GetOwners()
        {
            try
            {
                string query = "SELECT OwnerId, Email, Password, First_Name, Last_Name FROM Owner";
                List<Owner> owners = new List<Owner>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Password = reader.GetString(reader.GetOrdinal("Password")),
                            First_Name = reader.GetString(reader.GetOrdinal("First_Name")),
                            Last_Name = reader.GetString(reader.GetOrdinal("Last_Name"))
                        };

                        owners.Add(owner);
                    }

                    reader.Close();
                }

                return Ok(owners);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/owner/{id}
        [HttpGet("{id}")]
        public IActionResult GetOwner(int id)
        {
            try
            {
                string query = $"SELECT OwnerId, Email, Password, First_Name, Last_Name FROM Owner WHERE OwnerId = {id}";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open(); // Ensure the connection is open before executing the query

                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Password = reader.GetString(reader.GetOrdinal("Password")),
                            First_Name = reader.GetString(reader.GetOrdinal("First_Name")),
                            Last_Name = reader.GetString(reader.GetOrdinal("Last_Name"))
                        };

                        reader.Close();
                        return Ok(owner);
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


        // PUT: api/owner/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateOwner(int id, [FromBody] JsonElement updatedFields)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string query = "UPDATE Owner SET ";
                var parameters = new List<MySqlParameter>();

                if (updatedFields.TryGetProperty("Email", out JsonElement emailElement))
                {
                    query += "Email = @Email, ";
                    parameters.Add(new MySqlParameter("@Email", emailElement.GetString()));
                }

                if (updatedFields.TryGetProperty("Password", out JsonElement passwordElement))
                {
                    query += "Password = @Password, ";
                    parameters.Add(new MySqlParameter("@Password", HashHelper.HashPassword(passwordElement.GetString())));
                }

                if (updatedFields.TryGetProperty("First_Name", out JsonElement firstNameElement))
                {
                    query += "First_Name = @FirstName, ";
                    parameters.Add(new MySqlParameter("@FirstName", firstNameElement.GetString()));
                }

                if (updatedFields.TryGetProperty("Last_Name", out JsonElement lastNameElement))
                {
                    query += "Last_Name = @LastName, ";
                    parameters.Add(new MySqlParameter("@LastName", lastNameElement.GetString()));
                }

                query = query.TrimEnd(',', ' ') + " WHERE OwnerId = @OwnerId";
                parameters.Add(new MySqlParameter("@OwnerId", id));

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddRange(parameters.ToArray());

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



        // DELETE: api/owner/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteOwner(int id)
        {
            try
            {
                string query = $"DELETE FROM Owner WHERE OwnerId = {id}";

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
