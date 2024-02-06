using System.Net;
using Microsoft.Azure.Cosmos;

namespace LiL.Data.Cosmos;

public class Order{
    public string id { get; set;} = "";
    public Guid OrderId { get; set;} = Guid.NewGuid();
    public string SellerId { get; set;} = "";
    public string OrderRegion { get; set;} = "";
    public string CustomerId { get; set;} = "";
    public List<string> Products { get; set;} = new List<string>();
};

public class DB
{
   
    string _dbAccountKey = "";
    string _dbAccountEndpoint = "";

    CosmosClient _cosmosClient;

    public DB(string dbKey, string dbEndpoint)
    {
        _dbAccountKey = dbKey;
        _dbAccountEndpoint = dbEndpoint;

        //Create CosmosClient
        _cosmosClient = new CosmosClient(_dbAccountEndpoint, _dbAccountKey);
    }

    public async Task<(bool success, string? databaseId)> CreateDatabase(string databaseId, string containerId, string partitionKey)
    {
        //Create Database
        DatabaseResponse databaseResponse = 
            await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        
        //Create Container
        Container container = 
            await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                id: containerId,
                partitionKeyPath: $"/{partitionKey}",
                throughput: 400
        );

        return (
            databaseResponse.Database != null, 
            databaseResponse.Database?.Id
        );
    }

    public async Task<(bool success, string? id)> AddItemToContainer(string databaseId, string containerId, string partitionKey, Order order)
    {
        Container container = _cosmosClient.GetContainer(databaseId, containerId);
        
        ItemResponse<Order> itemResponse;
        try {
            //Read Item
            itemResponse = await container.ReadItemAsync<Order>(
                order.id, 
                new PartitionKey(partitionKey)
            );

            if (itemResponse.StatusCode == HttpStatusCode.OK) {
                //Upsert Item
                itemResponse = await container.UpsertItemAsync<Order>(
                    order, 
                    new PartitionKey(partitionKey)
                );
            }   
            return (
                itemResponse.StatusCode == HttpStatusCode.OK,
                itemResponse.Resource.id
            );
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            try {
                //Create Item
                itemResponse = await container.CreateItemAsync<Order>(
                    order, 
                    new PartitionKey(partitionKey)
                );

                if (itemResponse.StatusCode == HttpStatusCode.Created) {
                        return (
                        itemResponse.StatusCode == HttpStatusCode.Created,
                        itemResponse.Resource.id
                    );
                } else {
                    return (false, "");
                }
            } catch (Exception exE)
            {
                Console.WriteLine(exE.Message);
                return (false, "");
            }
        }    
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return (false, "");
        }
       
    }

    public async Task<(bool success, Order product)> ReadItemFromContainer(string databaseId, string containerId, string partitionKey, string customerId)
    {
        Container container = _cosmosClient.GetContainer(databaseId, containerId);

        try {
            ItemResponse<Order> response = await container.ReadItemAsync<Order>(
                customerId, 
                new PartitionKey(partitionKey)
            );

            return (
                response.StatusCode == HttpStatusCode.OK,
                response.Resource
            );
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return (false, new Order());
        }

    }

    public async Task<(bool success, List<Order> orders)> QueryItemsFromContainer(string databaseId, string containerId, string partitionKey, string query)
    {
        Container container = _cosmosClient.GetContainer(databaseId, containerId);
        
        QueryDefinition queryDefinition = new QueryDefinition(
            query
        );

        using FeedIterator<Order> feedIterator = container.GetItemQueryIterator<Order>(
            queryDefinition
        );

        List<Order> results = new List<Order>();
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<Order> feedResponse = await feedIterator.ReadNextAsync();
            results.AddRange(feedResponse.Resource.ToList());
        }
        return (true, results);
    }
}

