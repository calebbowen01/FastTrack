namespace MyMauiApp.Models;

public class WaterEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int AmountMl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
