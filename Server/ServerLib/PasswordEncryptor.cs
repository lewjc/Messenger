using System;
using Server.DatabaseLib;
using Encrypt = BCrypt.Net.BCrypt;

namespace Server.ServerLib
{
    public class PasswordEncryptor
    {
        // We have this extra salt as an extra layer of security for the encrypted password.
        private static String extraSalt = "TT-|>";

        public static string GenerateNewPassword(string userPassword)
        {
            String newPassword = Encrypt.HashPassword(userPassword + extraSalt);
            return newPassword;
        }

        public static bool CheckPassword(string passwordToCheck, string hashedPassword)
        {
            return Encrypt.Verify(passwordToCheck, hashedPassword);
        }

    }
}
