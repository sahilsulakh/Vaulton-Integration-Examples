using System;
using System.Threading.Tasks;
using System.IO;
using VaultonSDK;

namespace VaultonExample
{
    public class VaultIntegration
    {
        private static VaultonClient _client;

        public static async Task Main(string[] args)
        {
            // Initialize your client with your API Key
            _client = new VaultonClient("YOUR_API_KEY_HERE");

            Console.WriteLine("Fetching vault file...");

            // Example: Retrieve a file named "motd.txt" from your Vault
            string filename = "motd.txt";
            string content = await _client.GetVaultFileContentAsync(filename);

            if (!string.IsNullOrEmpty(content))
            {
                Console.WriteLine("--- File Content Start ---");
                Console.WriteLine(content);
                Console.WriteLine("--- File Content End ---");

                // You can now parse this content as needed (e.g. JSON, config, etc.)
                // Example: Check for specific flags
                if (content.Contains("MAINTENANCE_MODE=TRUE"))
                {
                    Console.WriteLine("Application is in maintenance mode.");
                }
            }
            else
            {
                Console.WriteLine($"File '{filename}' not found or could not be read.");
            }

            Console.ReadKey();
        }
    }
}
