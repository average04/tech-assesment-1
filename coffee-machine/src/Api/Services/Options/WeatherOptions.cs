namespace CoffeeMachine.Api.Services.Options;

public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public string? ApiKey { get; set; }
    public string City { get; set; } = "London";
    public string Units { get; set; } = "metric";
}
