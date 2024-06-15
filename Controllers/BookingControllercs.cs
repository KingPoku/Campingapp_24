using Campingapp_24.Data;
using Campingapp_24.Models;
using Microsoft.AspNetCore.Mvc;

namespace Campingapp_24.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly Database _database;

        public BookingsController(Database database)
        {
            _database = database;
        }

        // GET: api/bookings
        [HttpGet]
        public IActionResult GetBookings()
        {
            try
            {
                string query = "SELECT * FROM Booking";
                List<Booking> bookings = new List<Booking>();

                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Booking booking = new Booking
                        {
                            BookingID = reader.GetInt32("BookingID"),
                            UserID = reader.GetInt32("UserID"),
                            SpotID = reader.GetInt32("SpotID"),
                            Check_In_Date = reader.GetDateTime("Check_In_Date"),
                            Check_Out_Date = reader.GetDateTime("Check_Out_Date"),
                            Total_Price = reader.GetDecimal("Total_Price")
                        };

                        bookings.Add(booking);
                    }

                    reader.Close();
                }

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/bookings
        [HttpPost]
        public IActionResult CreateBooking([FromBody] Booking booking)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    connection.Open();

                    // Check availability of the camping spot for the specified dates
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM Availability 
                WHERE SpotID = @SpotID AND Date >= @CheckInDate AND Date < @CheckOutDate AND Isbooked = 1";

                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = checkQuery;
                        checkCommand.Parameters.AddWithValue("@SpotID", booking.SpotID);
                        checkCommand.Parameters.AddWithValue("@CheckInDate", booking.Check_In_Date.ToString("yyyy-MM-dd"));
                        checkCommand.Parameters.AddWithValue("@CheckOutDate", booking.Check_Out_Date.ToString("yyyy-MM-dd"));

                        // Execute the query to count booked dates
                        var count = (long)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            // Return a conflict response if the spot is not available
                            return Conflict(new { message = "The selected spot is not available for the chosen dates." });
                        }
                    }

                    // Insert availability records if they don't exist
                    string insertAvailabilityQuery = @"
                INSERT IGNORE INTO Availability (SpotID, Date, Isbooked)
                VALUES (@SpotID, @Date, 0)";

                    DateTime currentDate = booking.Check_In_Date;
                    while (currentDate < booking.Check_Out_Date)
                    {
                        using (var insertAvailabilityCommand = connection.CreateCommand())
                        {
                            insertAvailabilityCommand.CommandText = insertAvailabilityQuery;
                            insertAvailabilityCommand.Parameters.AddWithValue("@SpotID", booking.SpotID);
                            insertAvailabilityCommand.Parameters.AddWithValue("@Date", currentDate.ToString("yyyy-MM-dd"));
                            insertAvailabilityCommand.ExecuteNonQuery();
                        }
                        currentDate = currentDate.AddDays(1); // Move to the next date
                    }

                    // Create the booking record in the Booking table
                    string bookingQuery = @"
                INSERT INTO Booking (UserID, SpotID, Check_In_Date, Check_Out_Date, Total_Price)
                VALUES (@UserID, @SpotID, @CheckInDate, @CheckOutDate, @TotalPrice)";

                    using (var bookingCommand = connection.CreateCommand())
                    {
                        bookingCommand.CommandText = bookingQuery;

                        // Add parameters to avoid SQL injection
                        bookingCommand.Parameters.AddWithValue("@UserID", booking.UserID);
                        bookingCommand.Parameters.AddWithValue("@SpotID", booking.SpotID);
                        bookingCommand.Parameters.AddWithValue("@CheckInDate", booking.Check_In_Date.ToString("yyyy-MM-dd"));
                        bookingCommand.Parameters.AddWithValue("@CheckOutDate", booking.Check_Out_Date.ToString("yyyy-MM-dd"));
                        bookingCommand.Parameters.AddWithValue("@TotalPrice", booking.Total_Price);

                        bookingCommand.ExecuteNonQuery(); // Execute the booking insertion
                    }

                    // Update availability to mark the dates as booked
                    string availabilityQuery = @"
                UPDATE Availability 
                SET Isbooked = 1 
                WHERE SpotID = @SpotID AND Date >= @CheckInDate AND Date < @CheckOutDate";

                    using (var availabilityCommand = connection.CreateCommand())
                    {
                        availabilityCommand.CommandText = availabilityQuery;
                        availabilityCommand.Parameters.AddWithValue("@SpotID", booking.SpotID);
                        availabilityCommand.Parameters.AddWithValue("@CheckInDate", booking.Check_In_Date.ToString("yyyy-MM-dd"));
                        availabilityCommand.Parameters.AddWithValue("@CheckOutDate", booking.Check_Out_Date.ToString("yyyy-MM-dd"));

                        availabilityCommand.ExecuteNonQuery();  // Execute the availability update
                    }
                }

                // Return a JSON response indicating success
                return Ok(new { message = "Booking created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpGet("user/{userId}")]
        public IActionResult GetUserBookings(int userId)
        {
            try
            {
                // SQL query to fetch bookings for the specified user
                string query = $"SELECT * FROM Booking WHERE UserID = {userId}";
                List<Booking> bookings = new List<Booking>();

                // Execute the query using the database connection
                using (var connection = _database.GetConnection())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = query;
                    var reader = command.ExecuteReader();

                    // Read the results from the query
                    while (reader.Read())
                    {
                        // Create a new Booking object and populate its properties
                        Booking booking = new Booking
                        {
                            BookingID = reader.GetInt32("BookingID"),
                            UserID = reader.GetInt32("UserID"),
                            SpotID = reader.GetInt32("SpotID"),
                            Check_In_Date = reader.GetDateTime("Check_In_Date"),
                            Check_Out_Date = reader.GetDateTime("Check_Out_Date"),
                            Total_Price = reader.GetDecimal("Total_Price")
                            // Fetch other booking fields if necessary
                        };

                        bookings.Add(booking);
                    }

                    reader.Close();
                }

                // Return the list of bookings as the response
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }







        // PUT: api/bookings/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateBooking(int id, [FromBody] Booking booking)
        {
            try
            {
                string query = $@"
            UPDATE Booking 
            SET UserID = {booking.UserID}, 
                SpotID = {booking.SpotID}, 
                Check_In_Date = '{booking.Check_In_Date:yyyy-MM-dd}', 
                Check_Out_Date = '{booking.Check_Out_Date:yyyy-MM-dd}', 
                Total_Price = {booking.Total_Price}
            WHERE BookingID = {id}
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


        // DELETE: api/bookings/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            try
            {
                string query = $"DELETE FROM Booking WHERE BookingID = {id}";

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
