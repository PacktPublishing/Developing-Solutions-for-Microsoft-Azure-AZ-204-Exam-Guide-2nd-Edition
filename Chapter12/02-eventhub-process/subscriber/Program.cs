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
using Azure.Messaging.EventHubs.Consumer;


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

            var processor = new EventProcessorClient(storageClient,
                EventHubConsumerClient.DefaultConsumerGroupName,
                EventHubConnectionString,
                EventHubConnectionString.Split("EntityPath=")[1]);

            var eventProcessor = new SimpleEventProcessor();

            try
            {
                using var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(TimeSpan.FromSeconds(60));
                Console.WriteLine($"The process will automatically shutdown in 60 seconds");

                processor.PartitionClosingAsync += eventProcessor.CloseAsync;
                processor.PartitionInitializingAsync += eventProcessor.InitializingAsync;
                processor.ProcessEventAsync += eventProcessor.ProcessEventsAsync;
                processor.ProcessErrorAsync += eventProcessor.ProcessErrorAsync;

                try
                {
                    // Once processing has started, the delay will
                    // block to allow processing until cancellation
                    // is requested.

                    await processor.StartProcessingAsync(cancellationSource.Token);
                    await Task.Delay(Timeout.Infinite, cancellationSource.Token);

                    Console.WriteLine("Receiving. Press key to stop worker.");
                    Console.ReadKey();
                }
                catch (TaskCanceledException)
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
            finally
            {
                processor.PartitionClosingAsync -= eventProcessor.CloseAsync;
                processor.PartitionInitializingAsync -= eventProcessor.InitializingAsync;
                processor.ProcessEventAsync -= eventProcessor.ProcessEventsAsync;
                processor.ProcessErrorAsync -= eventProcessor.ProcessErrorAsync;

            }
        }
    }

    public class SimpleEventProcessor
    {
        public void LogError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        public Task CloseAsync(PartitionClosingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                string description = args.Reason switch
                {
                    ProcessingStoppedReason.OwnershipLost =>
                        "Another processor claimed ownership",

                    ProcessingStoppedReason.Shutdown =>
                        "The processor is shutting down",

                    _ => args.Reason.ToString()
                };

                Console.WriteLine($"Processor Shutting Down. Partition '{args.PartitionId}', Reason: '{description}'.");

            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return Task.CompletedTask;
        }

        public Task InitializingAsync(PartitionInitializingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                Console.WriteLine($"Initialize partition: {args.PartitionId}");

                // If no checkpoint was found, start processing
                // events enqueued now or in the future.

                EventPosition startPositionWhenNoCheckpoint =
                    EventPosition.FromEnqueuedTime(DateTimeOffset.UtcNow);

                args.DefaultStartingPosition = startPositionWhenNoCheckpoint;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return Task.CompletedTask;

        }

        public Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            try
            {
                // Always log the exception.

                Console.WriteLine("Error in the EventProcessorClient");
                Console.WriteLine($"\tOperation: {args.Operation ?? "Unknown"}");
                Console.WriteLine($"\tPartition: {args.PartitionId ?? "None"}");
                Console.WriteLine($"\tException: {args.Exception}");
                Console.WriteLine("");

                // If cancellation was requested, assume that
                // it was in response to an application request
                // and take no action.

                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // Allow the application to handle the exception according to
                // its business logic.

                LogError(args.Exception);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(ProcessEventArgs eventArgs)
        {

            try
            {

                Console.WriteLine($"Partition: {eventArgs.Partition.PartitionId}, Received event: {eventArgs.Data.EventBody}");
                eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);

            }
            catch (TaskCanceledException ex)
            {
                // This is expected when the delay is canceled.
                LogError(ex);
            }

            return Task.CompletedTask;

        }
    }


}
