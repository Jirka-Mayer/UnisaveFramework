﻿using System;

namespace Unisave.Utils
{
    /// <summary>
    /// String utilities
    /// </summary>
    public static class Str
    {
        /// <summary>
        /// Generates a random string
        /// (not cryptographically strong though)
        /// </summary>
        /// <param name="length">Length of the generated string</param>
        public static string Random(int length, Random random = null)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var buffer = new char[length];
            
            if (random == null)
                random = new Random();

            for (int i = 0; i < length; i++)
                buffer[i] = chars[random.Next(chars.Length)];

            return new String(buffer);
        }
    }
}