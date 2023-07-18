using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace TheCloudShops_Loader
{
    public class Program
    {
        // The Azure Cosmos DB endpoint to run this sample.
        private static readonly string EndpointUri = "https://cosmosdb-21198.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "NpKNVqa5f8z5qzNiB2iOmwfuvo9PMiKw7mJu6odrKyRaOPKwT9q9tLmilQN0TjzdAWN71qeTybnuACDb4PmKyw==";

        private CosmosClient cosmosClient;
        private Database database;
        private Container container;

        private string databaseId = "AZ204Demo";
        private string containerId = "TheCloudShops";

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

            await this.AddItemsToContainerAsync();
        }

        private async Task CreateContainerAsync()
        {
            this.container = (Container)await this.database.CreateContainerIfNotExistsAsync(containerId, "/OrderAddress/City");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        private async Task CreateDatabaseAsync()
        {
            this.database = (Database) await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }


        private async Task CreateDocumentsIfNotExists(Order order) 
        {
            try
            {
                ItemResponse<Order> readResponse = await this.container.ReadItemAsync<Order>(order.id, new PartitionKey(order.OrderAddress.City));
                Console.WriteLine("Item in database with id: {0} already exists\n", readResponse.Resource.id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                ItemResponse<Order> createResponse = await this.container.CreateItemAsync<Order>(order, new PartitionKey(order.OrderAddress.City));
                Console.WriteLine("Created item in the database with id: {0} Operation consumed {1} RUs.\n", createResponse.Resource.id, createResponse.RequestCharge);
            }
        }

        private async Task AddItemsToContainerAsync()
        {
            Customer customer1 = new Customer() { IsActive = true, Name= "Level4you" };
            Customer customer2 = new Customer() { IsActive = true, Name = "UpperLevel" };
            Customer customer3 = new Customer() { IsActive = false, Name = "Channel-9" };

            Product product1 = new Product() { ProductName= "Book" };
            Product product2 = new Product() { ProductName = "Food" };
            Product product3 = new Product() { ProductName = "Coffee" };

            Order order1 = new Order()
            {
                id = "o1",
                OrderNumber = "NL-21",
                OrderCustomer = customer1,
                OrderAddress = new Address { State = "WA", County = "King", City = "Seattle" },
                OrderItems = new[] {
                    new OrderItem() { ProductItem  = product1, Count = 7 },
                    new OrderItem() { ProductItem  = product3, Count = 1 } 
                }
            };
            Order order2 = new Order()
            {
                id = "o2",
                OrderNumber = "NL-22",
                OrderCustomer = customer2,
                OrderAddress = new Address { State = "WA", County = "King", City = "Redmond" },
                OrderItems = new[] {
                    new OrderItem() { ProductItem = product3, Count = 99 },
                    new OrderItem() { ProductItem = product2, Count = 5 },
                    new OrderItem() { ProductItem = product1, Count = 1 }
                }
            };
            Order order3 = new Order()
            {
                id = "o3",
                OrderNumber = "NL-23",
                OrderCustomer = customer2,
                OrderAddress = new Address { State = "WA", County = "King", City = "Redmond" },
                OrderItems = new[] {
                    new OrderItem() { ProductItem = product2, Count = 2 }
                }
            };

            await CreateDocumentsIfNotExists(order1);
            await CreateDocumentsIfNotExists(order2);
            await CreateDocumentsIfNotExists(order3);
        }
    }
}
