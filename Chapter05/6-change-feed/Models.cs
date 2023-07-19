using Newtonsoft.Json;

namespace TheCloudShops_Processor
{

    public class Order 
    {
        public string id { get; set; }
        public string OrderNumber { get; set; }
        public Address OrderAddress { get; set; }
        public Customer OrderCustomer { get; set; }
        public OrderItem[] OrderItems { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Customer
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class Product 
    {
        public string ProductName { get; set; }
    }

    public class Address
    {
        public string State { get; set; }
        public string County { get; set; }
        public string City { get; set; }
    }
    public class OrderItem
    {
        public Product ProductItem { get; set; }
        public int Count { get; set; }
    }
}
