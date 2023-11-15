using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Container = Microsoft.Azure.Cosmos.Container;

namespace ProductService.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private Container cosmosDbContainer;

    private string databaseId = "ecom-database";
    private string containerName = "products";

    public ProductController(IConfiguration configuration)
    {
        var connectionString = "AccountEndpoint=https://bits-assignment-ecom.documents.azure.com:443/;AccountKey=OzdgSV7jqP3DltPXwMJcR0njEa93NHskqmCO5d9QgrAZElGcZUVecCTqoorJMMvimdmHz29K8lEzACDb1stieg==;";
        var client = new CosmosClient(connectionString);
        this.cosmosDbContainer = client.GetContainer(databaseId, containerName);
    }

    [HttpGet("GetAllBooks")]
    public async Task<IActionResult> GetAllBooksAsync()
    {
        // Create a list of sample books
        var books = new List<Book>
        {
            new Book { product_id = "1", Title = "Jab We Met", Author = "Author 1", Price = 19.99 },
            new Book { product_id = "2", Title = "Fern Goregaon", Author = "Author 2", Price = 24.99 },
            new Book { product_id = "3", Title = "The Alchemist", Author = "Paulo Coelho", Price = 14.99 },
            new Book { product_id = "4", Title = "To Kill a Mockingbird", Author = "Harper Lee", Price = 12.99 },
            new Book { product_id = "5", Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Price = 15.99 },
            new Book { product_id = "6", Title = "Pride and Prejudice", Author = "Jane Austen", Price = 11.99 },
            new Book { product_id = "7", Title = "1984", Author = "George Orwell", Price = 10.99 },
            new Book { product_id = "8", Title = "Brave New World", Author = "Aldous Huxley", Price = 13.99 },
            new Book { product_id = "9", Title = "The Catcher in the Rye", Author = "J.D. Salinger", Price = 9.99 },
            new Book { product_id = "10", Title = "The Hobbit", Author = "J.R.R. Tolkien", Price = 16.99 },
            new Book { product_id = "11", Title = "The Jitanshu Champ", Author = "Jitanshu Champ", Price = 0.0 }
        };

        try
        {
            foreach (var book in books)
            {
                book.Id = Guid.NewGuid().ToString();
                await this.cosmosDbContainer.CreateItemAsync(book);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return StatusCode(500, "Error saving books to Cosmos DB");
        }

        return Ok(books);
    }
}

public class Book
{
    [JsonProperty("id")] // Ensure the property is named 'id' in the JSON
    public string Id { get; set; }
    public string product_id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public double Price { get; set; }
}

