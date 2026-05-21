namespace CoffeeMachine.Api.Services;

public sealed class BrewCounter : IBrewCounter
{
    private int _count;

    public int Next() => Interlocked.Increment(ref _count);
}
