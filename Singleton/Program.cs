using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Singleton;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demonstrating Singleton Pattern with ConfigurationManager");
            
            // Access the singleton from multiple places
            var config1 = ConfigurationManager.Instance;
            var config2 = ConfigurationManager.Instance;
            
            // Verify both references point to the same instance
            Console.WriteLine($"Are both references the same instance? {ReferenceEquals(config1, config2)}");
            
            // Read some configuration
            Console.WriteLine($"Database connection: {config1.GetConnectionString("Database")}");
            Console.WriteLine($"API URL: {config1.GetSetting("ApiUrl")}");
            
            // Modify a setting through one reference
            config1.UpdateSetting("ApiUrl", "https://api.newdomain.com/v2");
            
            // Read the updated setting through the other reference
            Console.WriteLine($"Updated API URL (from config2): {config2.GetSetting("ApiUrl")}");
            
            // Demonstrate thread safety with parallel access
            Console.WriteLine("\nDemonstrating thread safety with parallel access:");
            
            Parallel.For(0, 5, i =>
            {
                Console.WriteLine($"Thread {i}: Using instance {ConfigurationManager.Instance.GetHashCode()}");
            });
        }
    }

    /// <summary>
    /// Thread-safe singleton implementation of a configuration manager
    /// that might load settings from a file, database, or environment
    /// </summary>
    public sealed class ConfigurationManager
    {
        // Lazy<T> handles thread-safety and lazy initialization
        private static readonly Lazy<ConfigurationManager> _lazyInstance = 
            new Lazy<ConfigurationManager>(() => new ConfigurationManager());
            
        // Dictionary to store configuration settings
        private readonly Dictionary<string, string> _settings;
        
        // Private constructor
        private ConfigurationManager() 
        {
            Console.WriteLine("ConfigurationManager instance created - loading settings...");
            // In a real app, might load from file, DB, or environment variables
            _settings = new Dictionary<string, string>
            {
                ["DatabaseHost"] = "localhost",
                ["DatabaseName"] = "ProductionDB",
                ["DatabaseUser"] = "admin",
                ["ApiUrl"] = "https://api.example.com/v1",
                ["LogLevel"] = "Info"
            };
        }
        
        // Public access to the singleton instance
        public static ConfigurationManager Instance => _lazyInstance.Value;
        
        // Application methods
        public string GetConnectionString(string name)
        {
            if (name == "Database")
            {
                return $"Server={_settings["DatabaseHost"]};Database={_settings["DatabaseName"]};User Id={_settings["DatabaseUser"]}";
            }
            return string.Empty;
        }
        
        public string GetSetting(string key)
        {
            return _settings.TryGetValue(key, out var value) ? value : string.Empty;
        }
        
        public void UpdateSetting(string key, string value)
        {
            _settings[key] = value;
            Console.WriteLine($"Setting '{key}' updated to '{value}'");
        }
    }
