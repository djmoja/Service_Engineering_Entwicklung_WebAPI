namespace Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using Api.Model;
using Xunit;

public class ApiIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiIntegrationTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCars_WithoutApiKey_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/cars");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCarById_WithoutApiKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/cars/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostCar_WithoutApiKey_ReturnsUnauthorized()
    {
        var newCar = new Car
        {
            Brand = "Tesla",
            Model = "Model 3",
            HorsePower = "350",   
            Doors = 4,
            Fuel = "Electric",
            Colors = new List<string> { "Red" }
        };
        var response = await _client.PostAsJsonAsync("/api/cars", newCar);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutCar_WithoutApiKey_ReturnsUnauthorized()
    {
        var car = new Car { Model = "Updated" };
        var response = await _client.PutAsJsonAsync($"/api/cars/{Guid.NewGuid()}", car);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCar_WithoutApiKey_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync($"/api/cars/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostCar_WithValidApiKey_ReturnsCreated()
    {
        var clientWithKey = _factory.CreateClient();
        clientWithKey.DefaultRequestHeaders.Add("X-API-Key", CustomWebApplicationFactory.TestApiKey);

        var newCar = new Car
        {
            Brand = "VW",
            Model = "ID.4",
            HorsePower = "200",   
            Doors = 5,
            Fuel = "Electric",
            Colors = new List<string> { "Blue" }
        };

        var response = await clientWithKey.PostAsJsonAsync("/api/cars", newCar);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdCar = await response.Content.ReadFromJsonAsync<Car>();
        Assert.NotNull(createdCar);
        Assert.NotEqual(Guid.Empty, createdCar.Id);
    }

    [Fact]
    public async Task PutCar_WithValidApiKey_ReturnsNoContent_WhenCarExists()
    {
        var clientWithKey = _factory.CreateClient();
        clientWithKey.DefaultRequestHeaders.Add("X-API-Key", CustomWebApplicationFactory.TestApiKey);

        var newCar = new Car
        {
            Brand = "BMW",
            Model = "i4",
            HorsePower = "340",   
            Doors = 4,
            Fuel = "Electric",
            Colors = new List<string> { "White" }
        };
        var postResponse = await clientWithKey.PostAsJsonAsync("/api/cars", newCar);
        var createdCar = await postResponse.Content.ReadFromJsonAsync<Car>();

        createdCar.Model = "i4 M50";
        var putResponse = await clientWithKey.PutAsJsonAsync($"/api/cars/{createdCar.Id}", createdCar);

        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCar_WithValidApiKey_ReturnsOk_WhenCarExists()
    {
        var clientWithKey = _factory.CreateClient();
        clientWithKey.DefaultRequestHeaders.Add("X-API-Key", CustomWebApplicationFactory.TestApiKey);

        var newCar = new Car
        {
            Brand = "Audi",
            Model = "e-tron",
            HorsePower = "400"    
        };
        var postResponse = await clientWithKey.PostAsJsonAsync("/api/cars", newCar);
        var createdCar = await postResponse.Content.ReadFromJsonAsync<Car>();

        var deleteResponse = await clientWithKey.DeleteAsync($"/api/cars/{createdCar.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }
}