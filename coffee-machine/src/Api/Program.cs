using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using CoffeeMachine.Api.Services.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBrewCounter, BrewCounter>();
builder.Services.AddSingleton<IBrewService, BrewService>();

builder.Services
    .AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName));

builder.Services.AddHttpClient<IWeatherService, OpenWeatherMapWeatherService>();

var app = builder.Build();

app.MapGet("/brew-coffee", async (IBrewService brewService, IWeatherService weather, CancellationToken ct) =>
{
    var temperatureC = await weather.GetCurrentTemperatureCelsiusAsync(ct);
    var outcome = brewService.Brew(temperatureC);
    return outcome switch
    {
        BrewOutcome.Ready ready => Results.Ok(ready.Response),
        BrewOutcome.OutOfCoffee => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
        BrewOutcome.Teapot => Results.StatusCode(StatusCodes.Status418ImATeapot),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
});

app.Run();

public partial class Program { }
