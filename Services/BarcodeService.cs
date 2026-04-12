using System.Net.Http.Json;
using System.Text.Json;
using MyMauiApp.Models;

namespace MyMauiApp.Services;

public class BarcodeService
{
    private readonly HttpClient _httpClient;

    public BarcodeService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://world.openfoodfacts.org/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FastTrack/1.0 (MAUI App)");
    }

    public async Task<FoodProduct?> LookupBarcodeAsync(string barcode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v2/product/{barcode}.json");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("status", out var status) || status.GetInt32() != 1)
                return null;

            if (!root.TryGetProperty("product", out var product))
                return null;

            var foodProduct = new FoodProduct { Barcode = barcode };

            foodProduct.Name = GetString(product, "product_name") ?? "Unknown Product";
            foodProduct.Brand = GetString(product, "brands") ?? "";

            if (product.TryGetProperty("nutriments", out var nutriments))
            {
                foodProduct.CaloriesPer100g = GetInt(nutriments, "energy-kcal_100g");
                foodProduct.ProteinPer100g = GetDouble(nutriments, "proteins_100g");
                foodProduct.CarbsPer100g = GetDouble(nutriments, "carbohydrates_100g");
                foodProduct.FatPer100g = GetDouble(nutriments, "fat_100g");

                foodProduct.CaloriesPerServing = GetInt(nutriments, "energy-kcal_serving");
                foodProduct.ProteinPerServing = GetDouble(nutriments, "proteins_serving");
                foodProduct.CarbsPerServing = GetDouble(nutriments, "carbohydrates_serving");
                foodProduct.FatPerServing = GetDouble(nutriments, "fat_serving");
            }

            foodProduct.ServingSize = GetString(product, "serving_size") ?? "";

            return foodProduct;
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var val) && val.ValueKind == JsonValueKind.String)
            return val.GetString();
        return null;
    }

    private static int GetInt(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var val))
        {
            if (val.ValueKind == JsonValueKind.Number)
                return val.TryGetInt32(out int i) ? i : (int)val.GetDouble();
        }
        return 0;
    }

    private static double GetDouble(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var val))
        {
            if (val.ValueKind == JsonValueKind.Number)
                return val.GetDouble();
        }
        return 0;
    }
}
