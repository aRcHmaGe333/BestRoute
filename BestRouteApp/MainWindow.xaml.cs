using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using BestRouteApp.Models;
using BestRouteApp.Services;

namespace BestRouteApp;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly RouteService _routeService = new();
    private string _apiKey = string.Empty;
    private string _city = string.Empty;
    private string _baseStartingAddress = string.Empty;
    private string _newAddress = string.Empty;
    private string _routeLink = string.Empty;
    private string _statusMessage = string.Empty;
    private int _completionCounter;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<AddressItem> Addresses { get; } = new();

    public string ApiKey
    {
        get => _apiKey;
        set
        {
            if (_apiKey != value)
            {
                _apiKey = value;
                OnPropertyChanged();
            }
        }
    }

    public string City
    {
        get => _city;
        set
        {
            if (_city != value)
            {
                _city = value;
                OnPropertyChanged();
            }
        }
    }

    public string BaseStartingAddress
    {
        get => _baseStartingAddress;
        set
        {
            if (_baseStartingAddress != value)
            {
                _baseStartingAddress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentStartingAddress));
            }
        }
    }

    public string CurrentStartingAddress
    {
        get
        {
            var completed = Addresses
                .Where(item => item.IsCompleted && item.CompletionOrder.HasValue)
                .OrderBy(item => item.CompletionOrder)
                .LastOrDefault();
            return completed?.Address ?? BaseStartingAddress;
        }
    }

    public string NewAddress
    {
        get => _newAddress;
        set
        {
            if (_newAddress != value)
            {
                _newAddress = value;
                OnPropertyChanged();
            }
        }
    }

    public string RouteLink
    {
        get => _routeLink;
        set
        {
            if (_routeLink != value)
            {
                _routeLink = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private void OnAddAddress(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewAddress))
        {
            StatusMessage = "Enter an address to add.";
            return;
        }

        var added = 0;
        foreach (var address in ParseAddresses(NewAddress))
        {
            var normalized = NormalizeAddress(address);
            Addresses.Add(new AddressItem(normalized));
            added++;
        }

        if (added == 0)
        {
            StatusMessage = "Enter an address to add.";
            return;
        }

        NewAddress = string.Empty;
        StatusMessage = added == 1
            ? "Address added. Click Recalculate to update the route."
            : $"{added} addresses added. Click Recalculate to update the route.";
    }

    private static IEnumerable<string> ParseAddresses(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            yield break;
        }

        var lines = input.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        var sawEmpty = false;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                if (!sawEmpty)
                {
                    sawEmpty = true;
                }

                continue;
            }

            sawEmpty = false;
            yield return trimmed;
        }
    }

    private void OnAddressChecked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is AddressItem item)
        {
            if (item.IsCompleted)
            {
                item.CompletionOrder = ++_completionCounter;
            }
            else
            {
                item.CompletionOrder = null;
            }

            OnPropertyChanged(nameof(CurrentStartingAddress));
        }
    }

    private async void OnRecalculate(object sender, RoutedEventArgs e)
    {
        var origin = CurrentStartingAddress;
        if (string.IsNullOrWhiteSpace(origin))
        {
            StatusMessage = "Provide a base starting point or check off an address to set the starting point.";
            return;
        }

        var pendingStops = Addresses
            .Where(item => !item.IsCompleted)
            .Select(item => item.Address)
            .ToList();

        if (pendingStops.Count == 0)
        {
            StatusMessage = "All addresses are completed. Add a new stop to recalculate.";
            RouteLink = string.Empty;
            return;
        }

        StatusMessage = "Recalculating route...";

        try
        {
            var result = await _routeService.OptimizeRouteAsync(ApiKey, origin, pendingStops, optimize: true, CancellationToken.None);
            if (!string.Equals(BaseStartingAddress, result.Origin, StringComparison.OrdinalIgnoreCase))
            {
                BaseStartingAddress = result.Origin;
            }
            ReorderPendingStops(result.OrderedStops);
            RouteLink = BuildRouteLink(origin, result.OrderedStops);
            StatusMessage = result.Message;
        }
        catch (Exception ex)
        {
            RouteLink = BuildRouteLink(origin, pendingStops);
            StatusMessage = $"Unable to optimize route: {ex.Message}. Generated a basic link.";
        }
    }

    private void OnCopyLink(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RouteLink))
        {
            StatusMessage = "No route link available to copy.";
            return;
        }

        Clipboard.SetText(RouteLink);
        StatusMessage = "Route link copied to clipboard.";
    }

    private string NormalizeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(City))
        {
            return address;
        }

        var cityValue = City.Trim();
        if (address.Contains(cityValue, StringComparison.OrdinalIgnoreCase))
        {
            return address;
        }

        return $"{address}, {cityValue}";
    }

    private void ReorderPendingStops(IReadOnlyList<string> orderedStops)
    {
        var completed = Addresses
            .Where(item => item.IsCompleted)
            .OrderBy(item => item.CompletionOrder ?? int.MaxValue)
            .ToList();

        var pendingItems = Addresses
            .Where(item => !item.IsCompleted)
            .ToList();

        var orderedPending = new List<AddressItem>();
        foreach (var stop in orderedStops)
        {
            var match = pendingItems.FirstOrDefault(item =>
                string.Equals(item.Address, stop, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                continue;
            }

            orderedPending.Add(match);
            pendingItems.Remove(match);
        }

        orderedPending.AddRange(pendingItems);

        Addresses.Clear();
        foreach (var item in completed.Concat(orderedPending))
        {
            Addresses.Add(item);
        }
    }

    private static string BuildRouteLink(string origin, IReadOnlyList<string> stops)
    {
        if (stops.Count == 0)
        {
            return string.Empty;
        }

        var destination = stops[^1];
        var waypoints = stops.Take(stops.Count - 1).ToList();
        var queryParts = new List<string>
        {
            $"origin={Uri.EscapeDataString(origin)}",
            $"destination={Uri.EscapeDataString(destination)}",
            "travelmode=driving"
        };

        if (waypoints.Count > 0)
        {
            var waypointValue = string.Join('|', waypoints.Select(Uri.EscapeDataString));
            queryParts.Add($"waypoints={waypointValue}");
        }

        return $"https://www.google.com/maps/dir/?api=1&{string.Join("&", queryParts)}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
