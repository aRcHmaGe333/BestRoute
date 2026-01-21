using System.Net.Http;
using System.Text.Json;

namespace BestRouteApp.Services;

public sealed class RouteService
{
    private readonly HttpClient _httpClient = new();

    public async Task<RouteOptimizationResult> OptimizeRouteAsync(
        string apiKey,
        string origin,
        IReadOnlyList<string> stops,
        bool optimize,
        CancellationToken cancellationToken)
    {
        if (stops.Count == 0)
        {
            return new RouteOptimizationResult(origin, stops, false, "Add at least one stop to calculate a route.");
        }

        if (string.IsNullOrWhiteSpace(apiKey) || stops.Count == 1)
        {
            return new RouteOptimizationResult(origin, stops, false, string.IsNullOrWhiteSpace(apiKey)
                ? "API key missing. Generated a route link without optimization."
                : "Only one stop provided. Optimization is not required.");
        }

        var destination = stops[^1];
        var waypoints = stops.Take(stops.Count - 1).ToList();
        var waypointValue = (optimize ? "optimize:true|" : string.Empty) + string.Join('|', waypoints.Select(Uri.EscapeDataString));

        var requestUri = new UriBuilder("https://maps.googleapis.com/maps/api/directions/json")
        {
            Query = $"origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&waypoints={waypointValue}&key={Uri.EscapeDataString(apiKey)}"
        };

        using var response = await _httpClient.GetAsync(requestUri.Uri, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new RouteOptimizationResult(origin, stops, false, $"Directions API error: {response.StatusCode}. Using existing order.");
        }

        using var document = JsonDocument.Parse(content);
        var status = document.RootElement.GetProperty("status").GetString();
        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            var errorMessage = document.RootElement.TryGetProperty("error_message", out var error)
                ? error.GetString()
                : "Unknown error.";
            return new RouteOptimizationResult(origin, stops, false, $"Directions API status: {status}. {errorMessage}");
        }

        var orderElement = document.RootElement
            .GetProperty("routes")[0]
            .GetProperty("waypoint_order");

        var orderedWaypoints = orderElement.EnumerateArray()
            .Select(item => waypoints[item.GetInt32()])
            .ToList();

        var orderedStops = orderedWaypoints.Append(destination).ToList();
        return new RouteOptimizationResult(origin, orderedStops, true, "Optimized the pending stops order.");
    }
}

public sealed record RouteOptimizationResult(
    string Origin,
    IReadOnlyList<string> OrderedStops,
    bool UsedOptimization,
    string Message);
