using System;
using Azure.Messaging.EventHubs;
using System.Threading.Tasks;
using System.Text;
using Azure.Messaging.EventHubs.Producer;
using System.Diagnostics.Metrics;

namespace publisher
{
 class Program
    {
        private static EventHubProducerClient client;
        private const string EventHubConnectionString = "<you event hub connection string from previous script run>";
  
        private static async Task Main(string[] args)
        {
            client = new EventHubProducerClient(EventHubConnectionString);

            await SendEventsToEventHubAsync(10);

            await client.CloseAsync();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        // Creates an Event Hub client and sends 10 messages to the event hub.
        private static async Task SendEventsToEventHubAsync(int numMsgToSend)
        {
            for (var i = 0; i < numMsgToSend; i=i+2)
            {
                //Create batch with 2 events
                using EventDataBatch eventBatch = await client.CreateBatchAsync();
                {
                    for (var j = 0; j < 2; j++)
                    {

                        try
                        {
                            var message = $"Event #{i+j}";

                            var eventData = new EventData(new BinaryData(message));
                            Console.WriteLine($"Sending event: {message}");
                            eventBatch.TryAdd(eventData);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                        }
                    }

                }

                await Task.Delay(10);
                await client.SendAsync(eventBatch);

            }   

            Console.WriteLine($"{numMsgToSend} events sent.");
        }
    }
}
