using CoffeeMachine.Api.Models;

namespace CoffeeMachine.Api.Services;

public interface IBrewService
{
    BrewOutcome Brew();
}
