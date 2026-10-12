using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BoaSorte.Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Time.Testing;

namespace BoaSorte.Api.IntegrationTests.Draws;

/// <summary>HTTP contract: specs/001-sorteio-lista/contracts/api.md.</summary>
public sealed class DrawEndpointsTests(PostgresFixture postgres) : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 10, 12, 18, 0, 0, TimeSpan.Zero);
    private readonly FakeTimeProvider _clock = new(Start);
    private BoaSorteApiFactory _factory = null!;
    private HttpClient _client = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _factory = BoaSorteApiFactory.WithClock(postgres.NewDatabase(), _clock);
        _client = _factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private Task<HttpResponseMessage> Post(object body) => _client.PostAsJsonAsync("/api/v1/draws", body, Ct);

    private static async Task<JsonObject> Json(HttpResponseMessage response) =>
        JsonNode.Parse(await response.Content.ReadAsStringAsync(Ct))!.AsObject();

    private static object ListBody(int winnersCount = 1, bool showParticipants = true) => new
    {
        title = "Rifa do churrasco",
        kind = "list",
        participants = new[] { "Ana", "Bruno", "ana ", "", "Carla" },
        removeDuplicates = true,
        winnersCount,
        showParticipants,
    };

    [Fact]
    public async Task Post_list_draw_returns_201_with_location_and_contract_body()
    {
        var response = await Post(ListBody(winnersCount: 2));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await Json(response);
        var publicId = body["publicId"]!.GetValue<string>();
        Assert.Equal($"/api/v1/draws/{publicId}", response.Headers.Location?.OriginalString);
        Assert.Equal(12, publicId.Length);
        Assert.Equal("Rifa do churrasco", body["title"]!.GetValue<string>());
        Assert.Equal("list", body["kind"]!.GetValue<string>());
        Assert.Equal(Start, body["createdAt"]!.GetValue<DateTimeOffset>());
        Assert.Equal(Start.AddMonths(12), body["expiresAt"]!.GetValue<DateTimeOffset>());
        Assert.Equal(
            """{"removeDuplicates":true,"winnersCount":2,"showParticipants":true,"range":null}""",
            body["settings"]!.ToJsonString());
        Assert.Equal(5, body["submittedCount"]!.GetValue<long>());
        Assert.Equal(3, body["eligibleCount"]!.GetValue<long>());
        Assert.Equal("""["Ana","Bruno","Carla"]""", body["participants"]!.ToJsonString());

        var winners = body["winners"]!.AsArray();
        Assert.Equal([1, 2], winners.Select(w => w!["position"]!.GetValue<int>()));
        Assert.All(winners, w => Assert.Contains(w!["value"]!.GetValue<string>(), new[] { "Ana", "Bruno", "Carla" }));
    }

    [Fact]
    public async Task Post_range_draw_returns_numbers_and_no_participants()
    {
        var response = await Post(new { kind = "range", range = new { start = 1, end = 500 }, winnersCount = 3 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await Json(response);
        Assert.Equal("range", body["kind"]!.GetValue<string>());
        Assert.Null(body["title"]);
        Assert.Null(body["participants"]);
        Assert.Equal("""{"start":1,"end":500}""", body["settings"]!["range"]!.ToJsonString());
        Assert.False(body["settings"]!["removeDuplicates"]!.GetValue<bool>());
        Assert.All(body["winners"]!.AsArray(), w => Assert.InRange(int.Parse(w!["value"]!.GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture), 1, 500));
    }

    [Fact]
    public async Task Post_hides_participants_when_asked()
    {
        var body = await Json(await Post(ListBody(showParticipants: false)));

        Assert.Null(body["participants"]);
    }

    public static TheoryData<string, string> InvalidBodies() => new()
    {
        { """{"kind":"list","participants":["Ana"],"winnersCount":0}""", "winnersCount" },
        { """{"kind":"list","participants":["Ana"]}""", "winnersCount" },
        { """{"kind":"banana","participants":["Ana"],"winnersCount":1}""", "kind" },
        { """{"participants":["Ana"],"winnersCount":1}""", "kind" },
        { """{"kind":"list","winnersCount":1}""", "participants" },
        { """{"kind":"range","winnersCount":1}""", "range" },
        { """{"kind":"range","range":{"start":10,"end":1},"winnersCount":1}""", "range" },
        { $$"""{"kind":"list","title":"{{new string('x', 121)}}","participants":["Ana"],"winnersCount":1}""", "title" },
        { $$"""{"kind":"list","participants":["Ana","{{new string('x', 201)}}"],"winnersCount":1}""", "participants[1]" },
        { """{"kind":"list","participants":["Ana","ana"],"removeDuplicates":true,"winnersCount":2}""", "winnersCount" },
    };

    [Theory]
    [MemberData(nameof(InvalidBodies))]
    public async Task Post_invalid_input_returns_400_with_the_offending_field(string json, string field)
    {
        var response = await _client.PostAsync("/api/v1/draws", new StringContent(json, Encoding.UTF8, "application/json"), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var errors = (await Json(response))["errors"]!.AsObject();
        Assert.True(errors.ContainsKey(field), $"Expected an error for '{field}', got: {errors.ToJsonString()}");
    }

    [Fact]
    public async Task Post_not_enough_participants_tells_how_many_are_eligible()
    {
        var response = await Post(new { kind = "list", participants = new[] { "Ana", "Bruno" }, winnersCount = 3 });

        var message = (await Json(response))["errors"]!["winnersCount"]![0]!.GetValue<string>();
        Assert.Equal("Há apenas 2 participantes elegíveis.", message);
    }

    [Fact]
    public async Task Post_malformed_json_returns_400_problem()
    {
        var response = await _client.PostAsync("/api/v1/draws", new StringContent("{ nope", Encoding.UTF8, "application/json"), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_returns_the_same_draw_every_time()
    {
        var created = await Json(await Post(ListBody(winnersCount: 2)));
        var url = $"/api/v1/draws/{created["publicId"]}";

        var first = await _client.GetAsync(url, Ct);
        var second = await _client.GetAsync(url, Ct);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(created.ToJsonString(), (await Json(first)).ToJsonString());
        Assert.Equal(created.ToJsonString(), (await Json(second)).ToJsonString());
    }

    [Theory]
    [InlineData("aZ3k9QpL0xYt")]
    [InlineData("nao-existe")]
    public async Task Get_unknown_or_malformed_id_returns_404_problem(string publicId)
    {
        var response = await _client.GetAsync($"/api/v1/draws/{publicId}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_expired_draw_returns_410_problem()
    {
        var created = await Json(await Post(ListBody()));
        _clock.Advance(TimeSpan.FromDays(366));

        var response = await _client.GetAsync($"/api/v1/draws/{created["publicId"]}", Ct);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}

public class DrawEndpointsLimitsTests(PostgresFixture postgres)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static HttpRequestMessage CreateRequest(string clientIp) => new(HttpMethod.Post, "/api/v1/draws")
    {
        Content = JsonContent.Create(new { kind = "range", range = new { start = 1, end = 10 }, winnersCount = 1 }),
        Headers = { { "X-Forwarded-For", clientIp } },
    };

    [Fact]
    public async Task Post_is_limited_to_ten_draws_per_minute_per_client_ip()
    {
        await using var factory = new BoaSorteApiFactory(
            postgres.NewDatabase(),
            builder => builder.UseSetting("RateLimiting:CreateDraw:PermitLimit", "10"));
        using var client = factory.CreateClient();

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CreateRequest("203.0.113.7"), Ct)).StatusCode);
        }

        var limited = await client.SendAsync(CreateRequest("203.0.113.7"), Ct);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal("application/problem+json", limited.Content.Headers.ContentType?.MediaType);

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CreateRequest("203.0.113.8"), Ct)).StatusCode);
    }

    [Fact]
    public async Task Post_body_larger_than_3_mb_is_rejected_with_413()
    {
        await using var factory = new BoaSorteApiFactory(postgres.NewDatabase());
        factory.UseKestrel(0);
        factory.StartServer();
        using var client = factory.CreateClient();

        var participants = Enumerable.Repeat(new string('x', 200), 16_000); // ≈ 3.3 MB of JSON
        var json = JsonSerializer.Serialize(new { kind = "list", participants, winnersCount = 1 });
        var response = await client.PostAsync("/api/v1/draws", new StringContent(json, Encoding.UTF8, "application/json"), Ct);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Expired_draws_are_purged_in_the_background_after_the_grace_period()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 18, 0, 0, TimeSpan.Zero));
        await using var factory = BoaSorteApiFactory.WithClock(
            postgres.NewDatabase(),
            clock,
            builder => builder.UseSetting("Purge:Interval", "00:00:00.200"));
        using var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/v1/draws", new { kind = "range", range = new { start = 1, end = 10 }, winnersCount = 1 }, Ct);
        var url = created.Headers.Location!.OriginalString;
        clock.Advance(TimeSpan.FromDays(365 + 31 + 1)); // past expiry + grace period
        Assert.Equal(HttpStatusCode.Gone, (await client.GetAsync(url, Ct)).StatusCode);

        var status = HttpStatusCode.Gone;
        for (var attempt = 0; attempt < 50 && status != HttpStatusCode.NotFound; attempt++)
        {
            await Task.Delay(100, Ct);
            status = (await client.GetAsync(url, Ct)).StatusCode;
        }

        Assert.Equal(HttpStatusCode.NotFound, status);
    }
}
