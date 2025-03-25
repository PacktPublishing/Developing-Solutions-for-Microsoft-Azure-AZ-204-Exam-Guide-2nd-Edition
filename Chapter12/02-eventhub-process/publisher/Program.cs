using System;
using Azure.Messaging.EventHubs;
using System.Threading.Tasks;
using System.Text;
using Azure.Messaging.EventHubs.Producer;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace publisher
{
 class Program
    {
        private const string EventHubConnectionString = "<you event hub connection string from previous script run>";
  
        private static async Task Main(string[] args)
        {

            var producer = new EventHubProducerClient(EventHubConnectionString, EventHubConnectionString.Split("EntityPath=")[1]);

            try
            {
                int batchNum = 1;
                while (true)
                {
                    await SendEventsToEventHubAsync(producer, batchNum++, 10);
                    Console.WriteLine($"Press any key to send more events");
                    Console.ReadKey();
                }
            }
            finally
            {
                await producer.CloseAsync();
            }

        }

        // Creates an Event Hub client and sends 10 messages to the event hub.
        private static async Task SendEventsToEventHubAsync(EventHubProducerClient producer, int batchNum, int numMsgToSend)
        {
            using EventDataBatch eventBatch = await producer.CreateBatchAsync();

            for (var i = 0; i < numMsgToSend; i = i + 2)
            {



                    //Create batch with 2 events
                    var eventBody = new BinaryData($"Event Number: {i} from batch {batchNum}");
                    var eventData = new EventData(eventBody);

                    if (!eventBatch.TryAdd(eventData))
                    {
                        // At this point, the batch is full but our last event was not
                        // accepted.  For our purposes, the event is unimportant so we
                        // will intentionally ignore it.  In a real-world scenario, a
                        // decision would have to be made as to whether the event should
                        // be dropped or published on its own.
                        break;
                    }

                }
                await producer.SendAsync(eventBatch);

           Console.WriteLine($"{numMsgToSend} events sent.");
        }
    }
}
