using CoffeeMachine.Api.Models;

namespace CoffeeMachine.Api.Services;

public sealed class BrewService : IBrewService
{
    private const string HotMessage = "Your piping hot coffee is ready";

    private readonly TimeProvider _time;
    private readonly IBrewCounter _counter;

    public BrewService(TimeProvider time, IBrewCounter counter)
    {
        _time = time;
        _counter = counter;
    }

    public BrewOutcome Brew()
    {
        var callNumber = _counter.Next();
        var now = _time.GetLocalNow();

        if (now.Month == 4 && now.Day == 1)
        {
            return BrewOutcome.Teapot.Instance;
        }

        if (callNumber % 5 == 0)
        {
            return BrewOutcome.OutOfCoffee.Instance;
        }

        return new BrewOutcome.Ready(new BrewResponse(HotMessage, now));
    }
}
