using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Unisave.Foundation
{
    /// <summary>
    /// Holds environment configuration for the framework execution
    /// </summary>
    public class EnvStore
    {
        /// <summary>
        /// Contains the environment values
        /// </summary>
        private readonly Dictionary<string, string> values
            = new Dictionary<string, string>();

        /// <summary>
        /// Indexer for getting and setting the values
        /// </summary>
        public string this[string key]
        {
            get => GetString(key);
            set => Set(key, value);
        }

        /// <summary>
        /// Returns true if a key is known
        /// </summary>
        public bool Has(string key)
        {
            return values.ContainsKey(key);
        }

        /// <summary>
        /// Gets value with a default specified
        /// </summary>
        public string GetString(string key, string defaultValue = null)
        {
            if (!Has(key))
                return defaultValue;

            return values[key];
        }
        
        /// <summary>
        /// Sets a string value
        /// </summary>
        public void Set(string key, string value)
        {
            values[key] = value;
        }

        /// <summary>
        /// Gets value, converted to integer with default specified
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            string s = GetString(key);

            if (s == null)
                return defaultValue;

            s = s.Trim();

            if (int.TryParse(s, out int i))
                return i;

            return defaultValue;
        }
        
        /// <summary>
        /// Sets an integer value
        /// </summary>
        public void Set(string key, int value)
        {
            values[key] = value.ToString();
        }
        
        /// <summary>
        /// Gets value, converted to bool with default specified
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            string s = GetString(key);

            if (s == null)
                return defaultValue;

            s = s.Trim().ToLowerInvariant();

            if (s == "true")
                return true;

            if (s == "1")
                return true;

            return false;
        }
        
        /// <summary>
        /// Sets a boolean value
        /// </summary>
        public void Set(string key, bool value)
        {
            values[key] = value.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Override current settings with settings from a given env
        /// (existing keys get replaced, new keys get created)
        /// </summary>
        public void OverrideWith(EnvStore overrideEnvStore)
        {
            foreach (var pair in overrideEnvStore.values)
                values[pair.Key] = pair.Value;
        }

        /// <summary>
        /// Parse out env config from a string
        /// </summary>
        public static EnvStore Parse(string source)
        {
            var env = new EnvStore();

            if (source == null)
                return env;
            
            string[] lines = Regex.Split(source, "\r\n|\r|\n");
            
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');

                if (parts.Length <= 1)
                    continue;

                string keyPart = parts[0];
                string valuePart = line.Substring(keyPart.Length + 1);
                string key = keyPart.Trim();
                string value = valuePart.Trim();

                if (key.StartsWith("#"))
                    continue;

                env[key] = value;
            }
            
            return env;
        }
    }
}