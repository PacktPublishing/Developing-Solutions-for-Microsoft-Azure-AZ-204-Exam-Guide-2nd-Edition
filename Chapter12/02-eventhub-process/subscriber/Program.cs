using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs;
using System.Threading;
using Azure.Storage.Blobs;
using Microsoft.Azure.Amqp.Framing;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace subscriber
{ 
    class Program
    {
        private static EventHubProducerClient client;

        private const string EventHubConnectionString = "<you event hub connection string from previous script run>";
        private const string StorageConnectionString  = "<you storage account connection string from previous script run>";

        private static async Task Main(string[] args)
        {

            var storageClient = new BlobContainerClient(StorageConnectionString, "checkpoint");

            var processor = new SimpleEventProcessor(
                    storageClient, 1, "$Default",
                    EventHubConnectionString);

            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(60));

            try
            {
                await processor.StartProcessingAsync(cancellationSource.Token);
                await Task.Delay(Timeout.Infinite, cancellationSource.Token);

                Console.WriteLine("Receiving. Press key to stop worker.");
                Console.ReadKey();
            }
            catch (TaskCanceledException )
            {
                // This is expected if the cancellation token is
                // signaled.
                await processor.StopProcessingAsync();
            }
            finally
            {
                // Stopping may take up to the length of time defined
                // as the TryTimeout configured for the processor;
                // By default, this is 60 seconds.

                await processor.StopProcessingAsync();
            }

        }
    }

    public class SimpleEventProcessor : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
    {
        // This example uses a connection string, so only the single constructor
        // was implemented; applications will need to shadow each constructor of
        // the PluggableCheckpointStoreEventProcessor that they are using.

        public SimpleEventProcessor(
            BlobContainerClient storageClient,
            int eventBatchMaximumCount,
            string consumerGroup,
            string connectionString,
            EventProcessorOptions clientOptions = default)
                : base(
                    new BlobCheckpointStore(storageClient),
                    eventBatchMaximumCount,
                    consumerGroup,
                    connectionString,
                    clientOptions)
        {
        }

        protected async override Task OnProcessingEventBatchAsync(
            IEnumerable<EventData> messages,
            EventProcessorPartition partition,
            CancellationToken cancellationToken)
        {
            EventData lastEvent = null;
            try
            {    
                foreach (var currentEvent in messages)
                {
                    lastEvent = currentEvent;
                    var data = currentEvent.EventBody;
                    Console.WriteLine($"Event received. Partition: '{partition.PartitionId}', Data: '{data}'");

                }

                if (lastEvent != null)
                {
                    await UpdateCheckpointAsync(
                        partition.PartitionId,
                        CheckpointPosition.FromEvent(lastEvent),
                        cancellationToken)
                    .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while processing events: {ex}");
            }
        }

        protected override Task OnProcessingErrorAsync(
            Exception exception,
            EventProcessorPartition partition,
            string operationDescription,
            CancellationToken cancellationToken)
        {
            try
            {
                if (partition != null)
                {
                    Console.WriteLine($"Error on Partition: {partition.PartitionId}, Error: {operationDescription}: {exception}");
                }
                else
                {
                    Console.Error.WriteLine(
                        $"Exception while performing {operationDescription}: {exception}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while processing events: {ex}");
            }

            return Task.CompletedTask;
        }

        protected override Task OnInitializingPartitionAsync(
            EventProcessorPartition partition,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Initializing partition {partition.PartitionId}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception while initializing a partition: {ex}");
            }

            return Task.CompletedTask;
        }

        protected override Task OnPartitionProcessingStoppedAsync(
            EventProcessorPartition partition,
            ProcessingStoppedReason reason,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(
                    $"No longer processing partition {partition.PartitionId} because {reason}");
            }
            catch (Exception ex)
            {
 
                Console.WriteLine($"Exception while stopping processing for a partition: {ex}");
            }

            return Task.CompletedTask;
        }
    }


}
