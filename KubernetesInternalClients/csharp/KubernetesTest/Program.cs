using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast;
using Hazelcast.Networking;
using Hazelcast.Exceptions;

namespace Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Started");
            var options = new HazelcastOptionsBuilder().Build();
            Console.WriteLine("Reconnect mode is set");
            
            options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
            options.Networking.Addresses.Add("hz-hazelcast");
            
            // this must be consistent with what's in the GitHub action: if the action waits for 120s before testing
            // the log, then we must set an invocation retry timeout greater than 120s, else the invocations will
            // start and exceptions *will* be reported
            options.Messaging.RetryTimeoutSeconds = 180;
            
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, string>("map");
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
            var i = 0;
            while (true)
            {
                var randomKey = random.Next(100_000);
                try
                {
                    await map.PutAsync("key" + randomKey, "value" + randomKey);
                    if (i++ % 20 == 0) Console.WriteLine("Current map size: {0}", await map.GetSizeAsync());
                }
                 catch (ClientOfflineException e)
                {
                    if (i++ % 20 == 0) Console.WriteLine($"{e.GetType()} - State: {e.State}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.GetType()}: {e.Message} - State: {client.State}");
                }

                await Task.Delay(100);
           }
        }
    }
}