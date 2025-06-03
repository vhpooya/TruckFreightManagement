using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Infrastructure.Services.Maps
{
    public class NeshanMapService : IMapService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NeshanMapService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NeshanMapService(HttpClient httpClient, IConfiguration configuration, ILogger<NeshanMapService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Neshan:ApiKey"];
            _baseUrl = _configuration["Neshan:BaseUrl"] ?? "https://api.neshan.org";

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);
            }
        }

        public async Task<GeoLocation> GeocodeAddressAsync(Address address)
        {
            try
            {
                var query = Uri.EscapeDataString(address.GetFullAddress());
                var url = $"{_baseUrl}/v1/search?term={query}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<NeshanSearchResponse>(content);

                if (searchResult?.Items?.Any() == true)
                {
                    var firstResult = searchResult.Items.First();
                    return new GeoLocation(
                        firstResult.Location.Y, // Latitude
                        firstResult.Location.X, // Longitude
                        DateTime.UtcNow
                    );
                }

                // Fallback to coordinates from address if available
                return new GeoLocation(address.Latitude, address.Longitude, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geocoding address: {Address}", address.GetFullAddress());
                return new GeoLocation(address.Latitude, address.Longitude, DateTime.UtcNow);
            }
        }

        public async Task<Address> ReverseGeocodeAsync(GeoLocation location)
        {
            try
            {
                var url = $"{_baseUrl}/v1/reverse?lat={location.Latitude}&lng={location.Longitude}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var reverseResult = JsonSerializer.Deserialize<NeshanReverseResponse>(content);

                if (reverseResult != null)
                {
                    return new Address(
                        reverseResult.FormattedAddress ?? "",
                        reverseResult.City ?? "",
                        reverseResult.State ?? "",
                        "",
                        "Iran",
                        location.Latitude,
                        location.Longitude
                    );
                }

                return new Address("", "نامشخص", "نامشخص", "", "Iran", location.Latitude, location.Longitude);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reverse geocoding location: {Lat}, {Lng}", location.Latitude, location.Longitude);
                return new Address("", "نامشخص", "نامشخص", "", "Iran", location.Latitude, location.Longitude);
            }
        }

        public async Task<double> CalculateDistanceAsync(GeoLocation origin, GeoLocation destination)
        {
            try
            {
                var url = $"{_baseUrl}/v4/direction?" +
                         $"type=car&" +
                         $"origin={origin.Latitude},{origin.Longitude}&" +
                         $"destination={destination.Latitude},{destination.Longitude}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var routeResult = JsonSerializer.Deserialize<NeshanRouteResponse>(content);

                if (routeResult?.Routes?.Any() == true)
                {
                    var route = routeResult.Routes.First();
                    return route.Overview.Distance / 1000.0; // Convert meters to kilometers
                }

                // Fallback to Haversine formula
                return origin.CalculateDistanceTo(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance between {Origin} and {Destination}", 
                    $"{origin.Latitude},{origin.Longitude}", $"{destination.Latitude},{destination.Longitude}");
                
                // Fallback to Haversine formula
                return origin.CalculateDistanceTo(destination);
            }
        }

        public async Task<TimeSpan> CalculateDurationAsync(GeoLocation origin, GeoLocation destination)
        {
            try
            {
                var url = $"{_baseUrl}/v4/direction?" +
                         $"type=car&" +
                         $"origin={origin.Latitude},{origin.Longitude}&" +
                         $"destination={destination.Latitude},{destination.Longitude}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var routeResult = JsonSerializer.Deserialize<NeshanRouteResponse>(content);

                if (routeResult?.Routes?.Any() == true)
                {
                    var route = routeResult.Routes.First();
                    return TimeSpan.FromSeconds(route.Overview.Duration);
                }

                // Fallback estimation: 60 km/h average speed
                var distance = origin.CalculateDistanceTo(destination);
                return TimeSpan.FromHours(distance / 60);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating duration between {Origin} and {Destination}", 
                    $"{origin.Latitude},{origin.Longitude}", $"{destination.Latitude},{destination.Longitude}");
                
                // Fallback estimation
                var distance = origin.CalculateDistanceTo(destination);
                return TimeSpan.FromHours(distance / 60);
            }
        }

        public async Task<List<GeoLocation>> GetRouteAsync(GeoLocation origin, GeoLocation destination)
        {
            try
            {
                var url = $"{_baseUrl}/v4/direction?" +
                         $"type=car&" +
                         $"origin={origin.Latitude},{origin.Longitude}&" +
                         $"destination={destination.Latitude},{destination.Longitude}&" +
                         $"alternative=false";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var routeResult = JsonSerializer.Deserialize<NeshanRouteResponse>(content);

                if (routeResult?.Routes?.Any() == true)
                {
                    var route = routeResult.Routes.First();
                    var waypoints = new List<GeoLocation> { origin };

                    // Add intermediate waypoints from route geometry
                    if (route.Legs?.Any() == true)
                    {
                        foreach (var leg in route.Legs)
                        {
                            if (leg.Steps?.Any() == true)
                            {
                                foreach (var step in leg.Steps.Take(10)) // Limit waypoints
                                {
                                    waypoints.Add(new GeoLocation(
                                        step.StartLocation.Lat,
                                        step.StartLocation.Lng,
                                        DateTime.UtcNow
                                    ));
                                }
                            }
                        }
                    }

                    waypoints.Add(destination);
                    return waypoints;
                }

                return new List<GeoLocation> { origin, destination };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route between {Origin} and {Destination}", 
                    $"{origin.Latitude},{origin.Longitude}", $"{destination.Latitude},{destination.Longitude}");
                
                return new List<GeoLocation> { origin, destination };
            }
        }

        public async Task<List<string>> FindNearbyServicesAsync(GeoLocation location, string serviceType, double radiusKm = 10)
        {
            try
            {
                var searchTerm = GetSearchTermForService(serviceType);
                var url = $"{_baseUrl}/v1/search?" +
                         $"term={Uri.EscapeDataString(searchTerm)}&" +
                         $"lat={location.Latitude}&" +
                         $"lng={location.Longitude}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<NeshanSearchResponse>(content);

                if (searchResult?.Items?.Any() == true)
                {
                    return searchResult.Items
                        .Where(item => 
                        {
                            var itemLocation = new GeoLocation(item.Location.Y, item.Location.X, DateTime.UtcNow);
                            return location.CalculateDistanceTo(itemLocation) <= radiusKm;
                        })
                        .Select(item => $"{item.Title} - {item.Address}")
                        .Take(20)
                        .ToList();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby services of type {ServiceType}", serviceType);
                return new List<string>();
            }
        }

        private string GetSearchTermForService(string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "gas_station" or "fuel" => "پمپ بنزین",
                "restaurant" or "food" => "رستوران",
                "parking" => "پارکینگ",
                "repair" or "mechanic" => "تعمیرگاه",
                "mosque" => "مسجد",
                "hospital" => "بیمارستان",
                "pharmacy" => "داروخانه",
                "bank" => "بانک",
                "atm" => "خودپرداز",
                "rest_area" => "استراحتگاه",
                _ => serviceType
            };
        }
    }

    // DTOs for Neshan API responses
    public class NeshanSearchResponse
    {
        public List<NeshanSearchItem> Items { get; set; } = new();
    }

    public class NeshanSearchItem
    {
        public string Title { get; set; }
        public string Address { get; set; }
        public NeshanLocation Location { get; set; }
    }

    public class NeshanLocation
    {
        public double X { get; set; } // Longitude
        public double Y { get; set; } // Latitude
    }

    public class NeshanReverseResponse
    {
        public string FormattedAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public class NeshanRouteResponse
    {
        public List<NeshanRoute> Routes { get; set; } = new();
    }

    public class NeshanRoute
    {
        public NeshanRouteOverview Overview { get; set; }
        public List<NeshanRouteLeg> Legs { get; set; } = new();
    }

    public class NeshanRouteOverview
    {
        public int Distance { get; set; } // meters
        public int Duration { get; set; } // seconds
    }

    public class NeshanRouteLeg
    {
        public List<NeshanRouteStep> Steps { get; set; } = new();
    }

    public class NeshanRouteStep
    {
        public NeshanLatLng StartLocation { get; set; }
        public NeshanLatLng EndLocation { get; set; }
    }

    public class NeshanLatLng
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}

/