using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;

namespace CartManagementService.Controllers;

[ApiController]
[Route("[controller]")]
public class CartController : ControllerBase
{
    private Container cosmosDbContainer;

    private string databaseId = "cart-database";
    private string containerName = "cart";

    public CartController(IConfiguration configuration)
    {
        var connectionString = "AccountEndpoint=https://bits-assignment-ecom.documents.azure.com:443/;AccountKey=OzdgSV7jqP3DltPXwMJcR0njEa93NHskqmCO5d9QgrAZElGcZUVecCTqoorJMMvimdmHz29K8lEzACDb1stieg==;";
        var client = new CosmosClient(connectionString);
        this.cosmosDbContainer = client.GetContainer(databaseId, containerName);
    }

    [HttpPost("addToCart")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ProductId))
        {
            return BadRequest("User ID and Product ID are required for adding to the cart.");
        }

        // Set a unique 'id' for the item
        request.id = Guid.NewGuid().ToString();

        try
        {
            await this.cosmosDbContainer.CreateItemAsync(request);
            return Ok("Success");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest("Failed: " + e.Message);
        }
    }

    [HttpPost("getCartProducts")]
    public IActionResult GetCartProducts([FromBody] UserIdRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest("User ID is required to retrieve cart products.");
        }

        try
        {
            var query = new QueryDefinition($"SELECT * FROM cart WHERE cart.UserId = '{request.UserId}'");
            var iterator = this.cosmosDbContainer.GetItemQueryIterator<AddToCartRequest>(query);

            var cartProducts = new List<AddToCartRequest>();

            while (iterator.HasMoreResults)
            {
                var response = iterator.ReadNextAsync().Result;
                cartProducts.AddRange(response);
            }

            return Ok(cartProducts);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest("Failed to retrieve cart products: " + e.Message);
        }
    }

    public class AddToCartRequest
    {
        public string id { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
    }

    public class UserIdRequest
    {
        public string UserId { get; set; }
    }
}

