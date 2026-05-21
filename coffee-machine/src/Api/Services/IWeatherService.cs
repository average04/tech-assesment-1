namespace CoffeeMachine.Api.Services;

public interface IWeatherService
{
    Task<double?> GetCurrentTemperatureCelsiusAsync(CancellationToken ct);
}
