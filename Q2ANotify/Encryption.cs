using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify
{
    public static class Encryption
    {
        public static string Encrypt(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var input = Encoding.UTF8.GetBytes(value);
            var output = ProtectedData.Protect(input, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(output);
        }

        public static string Decrypt(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var input = Convert.FromBase64String(value);
            var output = ProtectedData.Unprotect(input, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(output);
        }
    }
}
