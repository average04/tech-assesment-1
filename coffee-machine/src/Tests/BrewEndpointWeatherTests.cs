using System.Net;
using System.Net.Http.Json;
using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CoffeeMachine.Tests;

public class BrewEndpointWeatherTests
{
    private sealed class StubWeatherService : IWeatherService
    {
        public double? NextTemperatureCelsius { get; set; }
        public Task<double?> GetCurrentTemperatureCelsiusAsync(CancellationToken ct) =>
            Task.FromResult(NextTemperatureCelsius);
    }

    private sealed class TestFactory : WebApplicationFactory<Program>
    {
        public FakeTimeProvider Time { get; } = new();
        public StubWeatherService Weather { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Time);

                services.RemoveAll<IWeatherService>();
                services.AddSingleton<IWeatherService>(Weather);
            });
        }
    }

    private static readonly DateTimeOffset NormalDay =
        new(2026, 5, 22, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Temperature_above_30_returns_iced_message()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(NormalDay);
        factory.Weather.NextTemperatureCelsius = 32.5;
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<BrewResponse>();
        body!.Message.Should().Be("Your refreshing iced coffee is ready");
    }

    [Fact]
    public async Task Temperature_at_30_exactly_returns_hot_message()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(NormalDay);
        factory.Weather.NextTemperatureCelsius = 30.0;
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<BrewResponse>();
        body!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Null_temperature_returns_hot_message()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(NormalDay);
        factory.Weather.NextTemperatureCelsius = null;
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<BrewResponse>();
        body!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Hot_temperature_does_not_override_503_on_fifth_call()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(NormalDay);
        factory.Weather.NextTemperatureCelsius = 35.0;
        var client = factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 5; i++) last = await client.GetAsync("/brew-coffee");

        last!.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await last.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Hot_temperature_does_not_override_418_on_april_first()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero));
        factory.Weather.NextTemperatureCelsius = 35.0;
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be((HttpStatusCode)418);
        (await res.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }
}
