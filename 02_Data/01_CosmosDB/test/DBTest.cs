using System.Text.Json;
using DotNetEnv;
using LiL.Data.Cosmos;

namespace LiL.DATA.Cosmos.Test;

public class DBTest
{
    string _dbAccountKey = "";
    string _dbAccountEndpoint = "";
    string _dbName = "";
    string _containerName = "";
    string _partitionKey = "";
    string _customerId = "";

    DB _db;
    
    public DBTest()
    {
        string configurationFile = "../../../../CreateEnv/Configuration.env";
        Env.Load(configurationFile);
    
        _dbAccountKey = Environment.GetEnvironmentVariable("LI4DEVS.COSMOS.DBACCOUNTKEY")??""; 
        _dbAccountEndpoint = Environment.GetEnvironmentVariable("LI4DEVS.COSMOS.DBACCOUNTENDPOINT")??"";

        _dbName = "lilDocument";
        _containerName = "orders";
        _partitionKey = "OrderRegion";

        _customerId = "2abd2f85-a077-4b09-a677-9d2a8fa25504";
        
        _db = new DB(_dbAccountKey, _dbAccountEndpoint);
    }

    [Fact]
    public async Task CreateDatabase_Dev()
    {
        (bool success, string? databaseId) result = await _db.CreateDatabase(_dbName, _containerName, _partitionKey);

        Assert.True(
            result.success && 
            result.databaseId == _dbName
        ); 
    }

    [Fact]
    public async Task AddItemToContainer_Dev()
    {
        Guid orderId = Guid.NewGuid();

        Order product = new Order(){
            id = orderId.ToString(),
            OrderId = orderId,
            OrderRegion = _partitionKey,
            CustomerId =  _customerId,
            SellerId = Guid.NewGuid().ToString(),
            Products = new List<string>(){
                JsonSerializer.Serialize ( new { ProductId = "345-543", ProductName = "Product 1"} ),
                JsonSerializer.Serialize ( new { ProductId = "345-544", ProductName = "Product 2"} ),
                JsonSerializer.Serialize ( new { ProductId = "345-545", ProductName = "Product 3"} )
            }
        };

        (bool success, string? id) result = await _db.AddItemToContainer(_dbName, _containerName, _partitionKey, product);
         
        Assert.True(
            result.success && 
            result.id == product.id
        );
    }

    [Fact]
    public async Task ReadItemFromContainer_Dev()
    {
        Guid orderId = Guid.NewGuid();

        // Create & store order
        Order product = new Order(){
            id = orderId.ToString(),
            OrderId = orderId,
            OrderRegion = _partitionKey,
            CustomerId =  _customerId,
            SellerId = Guid.NewGuid().ToString(),
            Products = new List<string>(){
                JsonSerializer.Serialize ( new { ProductId = "345-543", ProductName = "Product 1"} ),
                JsonSerializer.Serialize ( new { ProductId = "345-544", ProductName = "Product 2"} ),
                JsonSerializer.Serialize ( new { ProductId = "345-545", ProductName = "Product 3"} )
            }
        };
        await _db.AddItemToContainer(_dbName, _containerName, _partitionKey, product);

        // Read order
        (bool success, Order product) result = await _db.ReadItemFromContainer(
            _dbName, 
            _containerName,  
            _partitionKey,
            orderId.ToString()
        ); 
        
        Assert.True(
            result.success && 
            result.product.OrderId == orderId 
        );
    }

    [Fact]
    public async Task QueryItemsFromContainer_Dev()
    {

        string sellerId = Guid.NewGuid().ToString();

        for (int i = 0; i<5; i++) {
            Guid orderId = Guid.NewGuid();
            
            Order product = new Order(){
                id = orderId.ToString(),
                OrderId = orderId,
                OrderRegion = _partitionKey,
                CustomerId =  _customerId,
                SellerId = sellerId,
                Products = new List<string>(){
                    JsonSerializer.Serialize ( new { ProductId = "345-543", ProductName = "Product 1"} ),
                    JsonSerializer.Serialize ( new { ProductId = "345-544", ProductName = "Product 2"} ),
                    JsonSerializer.Serialize ( new { ProductId = "345-545", ProductName = "Product 3"} )
                }
            };
            await _db.AddItemToContainer(_dbName, _containerName, _partitionKey, product);
        }

        string query = $"SELECT * FROM c WHERE c.SellerId = '{sellerId}'";
        (bool success, List<Order> products) result = await _db.QueryItemsFromContainer(
            _dbName, 
            _containerName, 
            _partitionKey, 
            query
        );

        Assert.True(
            result.success && 
            result.products.Count == 5
        );
    }

}