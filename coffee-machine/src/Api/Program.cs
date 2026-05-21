using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBrewCounter, BrewCounter>();
builder.Services.AddSingleton<IBrewService, BrewService>();

var app = builder.Build();

app.MapGet("/brew-coffee", (IBrewService brewService) =>
{
    var outcome = brewService.Brew();
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
