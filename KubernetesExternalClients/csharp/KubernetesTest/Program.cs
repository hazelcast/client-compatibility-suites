using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast;

namespace Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Started");
            var options = new HazelcastOptionsBuilder().Build();
            options.Networking.Addresses.Add("<EXTERNAL-IP>");
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, string>("mapForCsharp");
            await map.PutAsync("key", "value");
            var value = await map.GetAsync("key");
            if (value == "value")
            {
                Console.WriteLine("Successful connection!");
            }
            else
            {
                throw new Exception("Connection failed, check your configuration.");
            }
            Console.WriteLine("Starting to fill the map with random entries.");
            var random = new Random();
            var numberOfLoop = 0;
            while (numberOfLoop < 120)
            {
                var randomKey = random.Next(100_000);
                try
                {
                    await map.PutAsync("key" + randomKey, "value" + randomKey);
                    Console.WriteLine("Current map size: {0}", await map.GetSizeAsync());
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                numberOfLoop++;
            }
        }
    }
}
