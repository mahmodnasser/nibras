using System.Net;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Nibras.BuildingBlocks.Web.Tests;

/// <summary>Reads the roster list as a client would, following cursors.</summary>
internal static class RosterClient
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static async Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(RosterFixture roster, string pathAndQuery, Guid? tenant = null)
    {
        using var client = roster.Client();
        if (tenant is { } other)
        {
            client.DefaultRequestHeaders.Remove("X-Nibras-Tenant-Id");
            client.DefaultRequestHeaders.Add("X-Nibras-Tenant-Id", other.ToString("D"));
        }

        using var response = await client.GetAsync(new Uri(pathAndQuery, UriKind.Relative), Ct);
        var text = await response.Content.ReadAsStringAsync(Ct);
        return (response.StatusCode, JsonDocument.Parse(text).RootElement.Clone());
    }

    public static string WithCursor(string query, string cursor) =>
        query + (query.Contains('?', StringComparison.Ordinal) ? "&" : "?") + "cursor=" + Uri.EscapeDataString(cursor);

    public static IEnumerable<Guid> Ids(JsonElement page) => page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid());

    /// <summary>Every page of a list, following <c>nextCursor</c>, with the envelope checks of TC-API-002 on each.</summary>
    public static async Task<List<JsonElement>> AllPagesAsync(RosterFixture roster, string query)
    {
        var pages = new List<JsonElement>();
        var url = query;
        while (true)
        {
            var (status, page) = await GetAsync(roster, url);
            status.ShouldBe(HttpStatusCode.OK, page.ToString());
            ShouldBeEnvelope(page);
            pages.Add(page);
            if (page.GetProperty("nextCursor").ValueKind == JsonValueKind.Null)
            {
                page.GetProperty("hasMore").GetBoolean().ShouldBeFalse();
                return pages;
            }

            page.GetProperty("hasMore").GetBoolean().ShouldBeTrue();
            url = WithCursor(query, page.GetProperty("nextCursor").GetString()!);
            pages.Count.ShouldBeLessThan(500, "a cursor walk that never ends");
        }
    }

    public static void ShouldBeEnvelope(JsonElement page)
    {
        page.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal)
            .ShouldBe(["filterHash", "hasMore", "items", "nextCursor", "pageSize", "previousCursor", "sort"]);
        page.GetProperty("items").ValueKind.ShouldBe(JsonValueKind.Array);
    }
}
