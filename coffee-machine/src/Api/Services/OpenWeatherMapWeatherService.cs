using System.Text.Json;
using CoffeeMachine.Api.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeeMachine.Api.Services;

public sealed class OpenWeatherMapWeatherService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly WeatherOptions _options;
    private readonly ILogger<OpenWeatherMapWeatherService> _logger;

    public OpenWeatherMapWeatherService(
        HttpClient http,
        IOptions<WeatherOptions> options,
        ILogger<OpenWeatherMapWeatherService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<double?> GetCurrentTemperatureCelsiusAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogDebug("Weather API key not configured; skipping lookup.");
            return null;
        }

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(_options.City)}&units={Uri.EscapeDataString(_options.Units)}&appid={Uri.EscapeDataString(_options.ApiKey)}";

        try
        {
            using var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Weather lookup returned {Status} for {City}.", (int)response.StatusCode, _options.City);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (doc.RootElement.TryGetProperty("main", out var main) &&
                main.TryGetProperty("temp", out var temp) &&
                temp.TryGetDouble(out var celsius))
            {
                return celsius;
            }

            _logger.LogWarning("Weather response missing main.temp for {City}.", _options.City);
            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Weather lookup failed for {City}.", _options.City);
            return null;
        }
    }
}
