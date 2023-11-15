using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Container = Microsoft.Azure.Cosmos.Container;

namespace TestApiService.Controllers;

public class AuthenticationController : ControllerBase
{

    private Container cosmosDbContainer;

    private string databaseId = "ecom-database";
    private string containerName = "authentication";

    public AuthenticationController(IConfiguration configuration)
    {
        var connectionString = "AccountEndpoint=https://bits-assignment-ecom.documents.azure.com:443/;AccountKey=OzdgSV7jqP3DltPXwMJcR0njEa93NHskqmCO5d9QgrAZElGcZUVecCTqoorJMMvimdmHz29K8lEzACDb1stieg==;";
        var client = new CosmosClient(connectionString);
        this.cosmosDbContainer = client.GetContainer(databaseId, containerName);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegistrationModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest("Username and password are required for registration.");
        }

        // Create a new User entity and set the properties
        var newUser = new User
        {
            id = model.Username,
            Username = model.Username,
            Password = model.Password,
            // Set other properties as needed
        };

        try
        {
            await this.cosmosDbContainer.CreateItemAsync<User>(newUser);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
        // Add the new user to the database
        return Ok("Registration successful");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest("Username and password are required for login.");
        }
        // Implement login logic here
        // Verify the credentials provided in the LoginModel
        // Check against your user database
        string id = model.Username;
        User user = await this.cosmosDbContainer.ReadItemAsync<User>(id, new PartitionKey(id));
        if (user.Password.Equals(model.Password))
        {
            Console.WriteLine("Authentication Token: " + user._etag);
            return Ok("Login successful");
        }
        return BadRequest("User Not found Please check username or password");
    }
}

public class RegistrationModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    // Add more registration fields as needed
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class User
{
    public string id { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    public string _etag { get; set; }
}


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}