using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;


namespace Campingapp_24.Utilities
{
    public static class HashHelper
    {
        public static string HashPassword(string password)
        {
            // Create a byte array to hold the salt (16 bytes)
            byte[] salt = new byte[128 / 8];

            // Generate a random salt using a cryptographic random number generator
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using PBKDF2 (Password-Based Key Derivation Function 2)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,  // The pseudo-random function to use (HMAC-SHA256)
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Convert the salt to a base64 string
            string saltString = Convert.ToBase64String(salt);

            // Return the salt and hashed password as a single string separated by a colon
            return $"{saltString}:{hashed}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Split the hashed password into its salt and hash components
            var parts = hashedPassword.Split(':');

            // Check if the hashed password is in the correct format
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            // Hash the input password with the same salt and parameters
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,  // The pseudo-random function to use (HMAC-SHA256)
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Return true if the hashed input password matches the stored hash, otherwise false
            return hash == hashed;
        }
    }
}
