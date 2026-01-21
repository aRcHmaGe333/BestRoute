# BestRoute

BestRoute is a Windows (WPF) app that generates an optimized Google Maps route link for a set of in-city stops. It uses the Google Maps Directions API to reorder pending stops and produces a shareable Google Maps directions link.

## Key features
- Track stops with checkboxes; the last checked stop becomes the new starting point.
- Add new addresses mid-route and recalculate to re-optimize the pending stops.
- Share a single Google Maps route link with the driver.

## Setup
1. Open `BestRoute/BestRoute.sln` in Visual Studio (Windows).
2. Add your Google Maps Directions API key in the **Google Maps API Key** field.
3. Optional: enter a city name to normalize new addresses.

## Usage
1. Enter a **Base starting point**.
2. Add one or more stops.
3. Click **Recalculate** to optimize the pending stops and produce a route link.
4. Check off completed stops. The last checked stop becomes the current starting point.
5. Add more stops and click **Recalculate** to re-optimize.

> If no API key is provided, the app will still generate a route link but will not optimize the order.
