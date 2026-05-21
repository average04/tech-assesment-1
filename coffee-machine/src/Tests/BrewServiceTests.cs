using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CoffeeMachine.Tests;

public class BrewServiceTests
{
    private const string HotMessage = "Your piping hot coffee is ready";

    private static FakeTimeProvider FakeTime(DateTimeOffset now)
    {
        var tp = new FakeTimeProvider(now);
        return tp;
    }

    private sealed class CountingCounter : IBrewCounter
    {
        private int _n;
        public int Next() => ++_n;
    }

    [Fact]
    public void First_call_returns_ready_with_hot_message_and_now()
    {
        var now = new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero);
        var sut = new BrewService(FakeTime(now), new CountingCounter());

        var outcome = sut.Brew();

        outcome.Should().BeOfType<BrewOutcome.Ready>()
            .Which.Response.Should().Be(new BrewResponse(HotMessage, now));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(9)]
    public void Non_fifth_calls_return_ready(int callNumber)
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());
        BrewOutcome? last = null;
        for (var i = 0; i < callNumber; i++) last = sut.Brew();
        last.Should().BeOfType<BrewOutcome.Ready>();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public void Every_fifth_call_returns_out_of_coffee(int callNumber)
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());
        BrewOutcome? last = null;
        for (var i = 0; i < callNumber; i++) last = sut.Brew();
        last.Should().BeOfType<BrewOutcome.OutOfCoffee>();
    }

    [Fact]
    public void April_first_returns_teapot_on_first_call()
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());

        var outcome = sut.Brew();

        outcome.Should().BeOfType<BrewOutcome.Teapot>();
    }

    [Fact]
    public void April_first_returns_teapot_even_on_fifth_call()
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());
        BrewOutcome? last = null;
        for (var i = 0; i < 5; i++) last = sut.Brew();
        last.Should().BeOfType<BrewOutcome.Teapot>();
    }

    [Fact]
    public void Counter_advances_on_every_call_even_on_april_first()
    {
        var counter = new CountingCounter();
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero)), counter);
        for (var i = 0; i < 3; i++) sut.Brew();
        counter.Next().Should().Be(4); // 3 calls done, next is the 4th
    }
}
