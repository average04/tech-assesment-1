using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CoffeeMachine.Tests;

public class BrewServiceTemperatureTests
{
    private const string HotMessage = "Your piping hot coffee is ready";
    private const string IcedMessage = "Your refreshing iced coffee is ready";

    private static FakeTimeProvider FakeTime(DateTimeOffset now) => new(now);

    private sealed class CountingCounter : IBrewCounter
    {
        private int _n;
        public int Next() => ++_n;
    }

    private static BrewService Sut(DateTimeOffset now) =>
        new(FakeTime(now), new CountingCounter());

    private static readonly DateTimeOffset NormalDay =
        new(2026, 5, 22, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Null_temperature_uses_hot_message()
    {
        var outcome = Sut(NormalDay).Brew(currentTemperatureCelsius: null);
        outcome.Should().BeOfType<BrewOutcome.Ready>()
            .Which.Response.Message.Should().Be(HotMessage);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(20.0)]
    [InlineData(29.99)]
    [InlineData(30.0)]   // strict greater-than: 30.0 exactly → still hot
    public void Temperature_at_or_below_30_uses_hot_message(double celsius)
    {
        var outcome = Sut(NormalDay).Brew(currentTemperatureCelsius: celsius);
        outcome.Should().BeOfType<BrewOutcome.Ready>()
            .Which.Response.Message.Should().Be(HotMessage);
    }

    [Theory]
    [InlineData(30.01)]
    [InlineData(31.0)]
    [InlineData(45.5)]
    public void Temperature_above_30_uses_iced_message(double celsius)
    {
        var outcome = Sut(NormalDay).Brew(currentTemperatureCelsius: celsius);
        outcome.Should().BeOfType<BrewOutcome.Ready>()
            .Which.Response.Message.Should().Be(IcedMessage);
    }

    [Fact]
    public void Fifth_call_returns_out_of_coffee_regardless_of_temperature()
    {
        var sut = Sut(NormalDay);
        BrewOutcome? last = null;
        for (var i = 0; i < 5; i++) last = sut.Brew(currentTemperatureCelsius: 35.0);
        last.Should().BeOfType<BrewOutcome.OutOfCoffee>();
    }

    [Fact]
    public void April_first_returns_teapot_regardless_of_temperature()
    {
        var aprilFirst = new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);
        var outcome = Sut(aprilFirst).Brew(currentTemperatureCelsius: 35.0);
        outcome.Should().BeOfType<BrewOutcome.Teapot>();
    }
}
