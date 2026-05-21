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

public class BrewEndpointTests
{
    private sealed class TestFactory : WebApplicationFactory<Program>
    {
        // Start at an early date so SetUtcNow can move forward to any test date (FakeTimeProvider forbids going back in time).
        public FakeTimeProvider Time { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Time);
            });
        }
    }

    [Fact]
    public async Task First_call_returns_200_with_expected_shape()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<BrewResponse>();
        body.Should().NotBeNull();
        body!.Message.Should().Be("Your piping hot coffee is ready");
        body.Prepared.Should().Be(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Fifth_call_returns_503_with_empty_body()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 5; i++) last = await client.GetAsync("/brew-coffee");

        last!.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await last.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Sixth_call_returns_200_again()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        for (var i = 0; i < 5; i++) await client.GetAsync("/brew-coffee");
        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task April_first_returns_418_with_empty_body()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be((HttpStatusCode)418);
        (await res.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task April_first_returns_418_even_on_fifth_call()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 5; i++) last = await client.GetAsync("/brew-coffee");

        last!.StatusCode.Should().Be((HttpStatusCode)418);
    }
}
