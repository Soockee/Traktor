using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Traktor
{
    class Util
    {
        private const int string_length = 12;
        public static string generateNewId()
        {
            /*
             * Algorithm Source: https://stackoverflow.com/a/1344365
             * by Luis Perez and Douglas
             */
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bit_count = (string_length * 6);
                var byte_count = ((bit_count + 7) / 8); // rounded up
                var bytes = new byte[byte_count];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
