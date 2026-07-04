using Kemono2Pawchive;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (!File.Exists("config.json"))
        {
            Console.WriteLine("No config Json found! Writing a default config.json file...");
            ConfigFile.WriteDefault();
            Console.WriteLine("Please close the program and fill out the config.json file with the required info");
            Console.ReadKey();
            return;
        }

        Kemono2PawchiveManager? manager;
        {
            ConfigFile? config = JsonSerializer.Deserialize<ConfigFile>(File.ReadAllText("config.json"), HelperFunctions.SerializerOptions);
            if(config is null)
            {
                Console.WriteLine("Config file couldn't be deserialized!");
                Console.WriteLine("Resetting File...");
                ConfigFile.WriteDefault();
                Console.WriteLine("Done! Please close the program and fill out the config.json file with the required info");
                Console.ReadKey();
                return;
            }

            if (config.Credentials is null)
            {
                Console.WriteLine("Failed deserializing credentials!");
                Console.WriteLine("Resetting File...");
                ConfigFile.WriteDefault();
                Console.WriteLine("Done! Please close the program and fill out the config.json file with the required info");
                Console.ReadKey();
                return;
            }

            bool hasMissingCredentials = false;
            if (!config.Credentials.Kemono.HasValidAuth())
            {
                hasMissingCredentials = true;
                Console.WriteLine("You have missing credentials for Kemono! Please edit the config.json file");
            }
            if (!config.Credentials.Pawchive.HasValidAuth())
            {
                hasMissingCredentials = true;
                Console.WriteLine("You have missing credentials for Pawchive! Please edit the config.json file");
            }
            if (hasMissingCredentials)
            {
                Console.ReadKey();
                return;
            }
            manager = await Kemono2PawchiveManager.Instantiate(config);
        }

        if(manager is null)
        {
            Console.WriteLine("Failed to login to one of the providers!");
            Console.ReadKey();
            return;
        }

        await MenuLoop(manager);

        manager.Dispose();
    }

    static async Task MenuLoop(Kemono2PawchiveManager manager)
    {
        bool validOptionMessage = false;
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1) Import Artists");
            Console.WriteLine("2) Import Posts");
            Console.WriteLine("3) Exit");
            if (validOptionMessage)
            {
                Console.WriteLine("Please Select a valid option!");
            }

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Console.Clear();
                    await manager.ProcessArtistsFromKemono2Pawchive();
                    Console.WriteLine("Done!");
                    Console.WriteLine("Press any key to Continue...");
                    Console.ReadKey();
                    validOptionMessage = false;
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Console.Clear();
                    await manager.ProcessPostsFromKemono2Pawchive();
                    Console.WriteLine("Done!");
                    Console.WriteLine("Press any key to Continue...");
                    Console.ReadKey();
                    validOptionMessage = false;
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    return;

                default:
                    validOptionMessage = true;
                    continue;
            }
        }
    }
}