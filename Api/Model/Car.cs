namespace Api.Model;

public class Car
{
    public string Id { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string HorsePower { get; set; } = string.Empty;
    
    public string ConstructionYear { get; set; } = string.Empty;
    public int Doors { get; set; }
    public string Fuel { get; set; } = string.Empty;
    public List<string> Colors { get; set; } = new();
}