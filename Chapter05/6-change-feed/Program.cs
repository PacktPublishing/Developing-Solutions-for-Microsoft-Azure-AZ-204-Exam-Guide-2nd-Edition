using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.Threading;

namespace TheCloudShops_Processor
{

    /*
     * IMPORTANT NOTICE 
     * TO GET THE DOCUMENT APPEARS ON THE FEED THE DOCUMENT SHOULD BE MODIFIED. 
     *    YOU CAN MODIFY DOCUMENT BY RUNNING Selector PROJECT 
     *      OR USE AZURE PORTAL TO COMPLETE MODIFICATION.     
     */

    public class Program
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "";

        private CosmosClient cosmosClient;
        private Database database;

        private Container container;
        private Container leasecontainer;

        private string databaseId = "AZ204Demo";
        private string containerId = "TheCloudShops";
        private string containerLeaseId = "TheCloudShops-lease";

        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Beginning operations...\n");
                Program p = new Program();
                await p.GetStartedDemoAsync();

            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        public async Task GetStartedDemoAsync()
        {
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();

            await this.ProcessFeed();
        }


        private async Task CreateContainerAsync()
        {
            this.container = (Container)await this.database.CreateContainerIfNotExistsAsync(containerId, "/OrderAddress/City");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
    
            this.leasecontainer = await this.database.CreateContainerIfNotExistsAsync(containerLeaseId, "/id");
            Console.WriteLine("Created Container: {0}\n", this.leasecontainer.Id);
        }

        private async Task CreateDatabaseAsync()
        {
            this.database = (Database) await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }


        private async Task ProcessFeed()
        {
            static async Task HandleChangesAsync(
                ChangeFeedProcessorContext context,
                IReadOnlyCollection<Order> changes,
                CancellationToken cancellationToken)
            {
                Console.WriteLine($"Started handling changes for lease {context.LeaseToken}...");
                Console.WriteLine($"Change Feed request consumed {context.Headers.RequestCharge} RU.");
                Console.WriteLine($"SessionToken: {context.Headers.Session}");

                if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine($"Change Feed request took longer than expected. Diagnostics:" + context.Diagnostics.ToString());
                }

                foreach (Order item in changes)
                {
                    Console.WriteLine($"Detected operation for item with id {item.id}.");
                    // Simulate some asynchronous operation
                    await Task.Delay(10);
                }

                Console.WriteLine("Finished handling changes.");
            }

            Container sourceContainer = cosmosClient.GetContainer(databaseId, containerId);
            Container leaseContainer = cosmosClient.GetContainer(databaseId, containerLeaseId);

            var procBuilder = sourceContainer.GetChangeFeedProcessorBuilder<Order>(
                processorName: "orderItemProcessor",
                onChangesDelegate: HandleChangesAsync 
            );

             ChangeFeedProcessor processor = procBuilder
                .WithInstanceName("TheCloudShops")
                .WithLeaseContainer(leaseContainer)
                .Build();

            await processor.StartAsync();


            Console.ReadKey();
            await processor.StopAsync();
        }

    }
}
