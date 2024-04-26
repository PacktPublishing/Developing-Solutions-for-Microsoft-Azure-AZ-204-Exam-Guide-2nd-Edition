using Microsoft.Extensions.Logging;

public static DemoMessage Run(string myQueueItem, ILogger log)
{
    return new DemoMessage() {
        PartitionKey = "Messages",
        RowKey = Guid.NewGuid().ToString(),
        Message = myQueueItem.ToString() };
}

public class DemoMessage
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string Message{ get; set; }
}
