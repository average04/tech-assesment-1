namespace CoffeeMachine.Api.Models;

public abstract record BrewOutcome
{
    public sealed record Ready(BrewResponse Response) : BrewOutcome;
    public sealed record OutOfCoffee : BrewOutcome
    {
        public static readonly OutOfCoffee Instance = new();
    }
    public sealed record Teapot : BrewOutcome
    {
        public static readonly Teapot Instance = new();
    }
}
