using Campingapp_24.Data;
using Campingapp_24.Models;
using Campingapp_24.Utilities;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Text.Json;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly Database _database;

        public UsersController(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel login)
        {
            try
            {
                // Try to authenticate the user
                var user = AuthenticateUser(login.Email, login.Password);
                if (user != null)
                {
                    return Ok(user); // Return user details if authentication is successful
                }

                // If user authentication fails, try to authenticate the owner
                var owner = AuthenticateOwner(login.Email, login.Password);
                if (owner != null)
                {
                    return Ok(owner); // Return owner details if authentication is successful
                }

                return Unauthorized("Invalid email or password");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Method to authenticate a user based on email and password
        private object AuthenticateUser(string email, string password)
        {
            string query = "SELECT * FROM Users WHERE Email = @Email";
            using (var connection = _database.GetConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // Verify the password using the HashHelper
                    var hashedPassword = reader.GetString(reader.GetOrdinal("Password"));
                    if (HashHelper.VerifyPassword(password, hashedPassword))
                    {
                        // Return user details if the password is correct
                        return new
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            FirstName = reader.GetString(reader.GetOrdinal("First_Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Last_Name")),
                            Role = "User" // Indicate the role
                        };
                    }
                }
            }
            return null;
        }

        // Method to authenticate an owner based on email and password
        private object AuthenticateOwner(string email, string password)
        {
            string query = "SELECT * FROM Owner WHERE Email = @Email";
            using (var connection = _database.GetConnection())
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = query;
                command.Parameters.AddWithValue("@Email", email);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // Verify the password using the HashHelper
                    var hashedPassword = reader.GetString(reader.GetOrdinal("Password"));
                    if (HashHelper.VerifyPassword(password, hashedPassword))
                    {
                        // Return owner details if the password is correct
                        return new
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            FirstName = reader.GetString(reader.GetOrdinal("First_Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Last_Name")),
                            Role = "Owner" // Indicate the role
                        };
                    }
                }
            }
            return null;
        }





        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Hash the password
                user.Password = HashHelper.HashPassword(user.Password);

                // Insert user into database
                string query = @"
            INSERT INTO Users (Email, Password, First_Name, Last_Name) 
            VALUES (@Email, @Password, @FirstName, @LastName)
        ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@FirstName", user.First_Name);
                    command.Parameters.AddWithValue("@LastName", user.Last_Name);

                    command.ExecuteNonQuery();
                }

                return Ok("User created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet]
        [Route("api/checkUser")]
        public IActionResult CheckUserExistence(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            try
            {
                // Query the database to check if the user exists
                string query = @"
            SELECT COUNT(*) 
            FROM Users 
            WHERE Email = @Email
        ";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@Email", email);

                    // Convert the result to int
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    if (count > 0)
                    {
                        // User exists, return response indicating that
                        return Ok(new { exists = true });
                    }
                    else
                    {
                        // User does not exist, return response indicating that
                        return Ok(new { exists = false });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }






        // GET: api/users
        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                string query = "SELECT UserId, Email, Password, First_Name, Last_Name FROM Users";
                List<User> users = new List<User>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        User user = new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Password = reader.GetString(reader.GetOrdinal("Password")),
                            First_Name = reader.GetString(reader.GetOrdinal("First_Name")),
                            Last_Name = reader.GetString(reader.GetOrdinal("Last_Name"))
                        };

                        users.Add(user);
                    }

                    reader.Close();
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: api/users/{id}
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            try
            {
                string query = $"SELECT UserId, Email, Password, First_Name, Last_Name FROM Users WHERE UserId = {id}";

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open(); // Ensure the connection is open before executing the query

                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        User user = new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Password = reader.GetString(reader.GetOrdinal("Password")),
                            First_Name = reader.GetString(reader.GetOrdinal("First_Name")),
                            Last_Name = reader.GetString(reader.GetOrdinal("Last_Name"))
                        };

                        reader.Close();
                        return Ok(user);
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



        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] JsonElement updatedFields)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Start building the SQL update query
                string query = "UPDATE Users SET ";
                var parameters = new List<MySqlParameter>();

                // Check if the updated fields contain an email and add to the query and parameters list
                if (updatedFields.TryGetProperty("Email", out JsonElement emailElement))
                {
                    query += "Email = @Email, ";
                    parameters.Add(new MySqlParameter("@Email", emailElement.GetString()));
                }

                // Check if the updated fields contain a password, hash it, and add to the query and parameters list
                if (updatedFields.TryGetProperty("Password", out JsonElement passwordElement))
                {
                    query += "Password = @Password, ";
                    parameters.Add(new MySqlParameter("@Password", HashHelper.HashPassword(passwordElement.GetString())));
                }

                // Check if the updated fields contain a first name and add to the query and parameters list
                if (updatedFields.TryGetProperty("First_Name", out JsonElement firstNameElement))
                {
                    query += "First_Name = @FirstName, ";
                    parameters.Add(new MySqlParameter("@FirstName", firstNameElement.GetString()));
                }

                // Check if the updated fields contain a last name and add to the query and parameters list
                if (updatedFields.TryGetProperty("Last_Name", out JsonElement lastNameElement))
                {
                    query += "Last_Name = @LastName, ";
                    parameters.Add(new MySqlParameter("@LastName", lastNameElement.GetString()));
                }

                // Remove the trailing comma and space, and add the WHERE clause
                query = query.TrimEnd(',', ' ') + " WHERE UserId = @UserId";
                parameters.Add(new MySqlParameter("@UserId", id));

                // Execute the query using the database connection
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    command.Parameters.AddRange(parameters.ToArray());

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




        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                string query = $"DELETE FROM Users WHERE UserId = {id}";

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
