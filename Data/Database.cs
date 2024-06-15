using System;
using MySql.Data.MySqlClient;

namespace Campingapp_24.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Method to get a new MySqlConnection using the provided connection string
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // Method to open a connection to the database
        public void OpenConnection()
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open(); // Attempt to open the connection
                    Console.WriteLine("Connection successful!"); // Display success message if connection is opened
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Display error message if connection fails
                }
            }
        }


        // Method to execute a query on the database
        public void ExecuteQuery(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open(); // Open the connection
                    MySqlCommand command = new MySqlCommand(query, connection); // Create a MySqlCommand object
                    MySqlDataReader reader = command.ExecuteReader(); // Execute the query and get a data reader

                    while (reader.Read())
                    {
                        // Access data using reader
                    }

                    reader.Close();  // Close the reader
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Display error message if query execution fails
                }
            }
        }

        
    }
}
