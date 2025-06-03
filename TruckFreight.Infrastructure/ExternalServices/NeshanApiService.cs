// In TruckFreightSystem.Infrastructure.ExternalServices/NeshanApiService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using TruckFreight.Infrastructure.Services;
using TruckFreight.Infrastructure.Services.Maps;


namespace TruckFreightSystem.Infrastructure.ExternalServices
{
    public class NeshanApiService : INeshanApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NeshanApiService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NeshanApiService(HttpClient httpClient, IConfiguration configuration, ILogger<NeshanApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["NeshanApi:ApiKey"] ?? throw new InvalidOperationException("Neshan API Key is not configured.");
            _baseUrl = configuration["NeshanApi:BaseUrl"] ?? "https://api.neshan.org"; // Default Neshan API base URL
            _httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<NeshanRouteResponse?> GetRouteDistanceAndDurationAsync(decimal originLat, decimal originLon, decimal destLat, decimal destLon)
        {
            var url = $"/v1/routing/no-traffic?type=car&points={originLat},{originLon},{destLat},{destLon}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<NeshanRoutingApiResponse>(url);
                if (response?.Routes != null && response.Routes.Any())
                {
                    var route = response.Routes.First();
                    return new NeshanRouteResponse
                    {
                        DistanceMeters = (decimal)route.Distance,
                        DurationSeconds = route.Duration,
                        Polyline = route.OverviewPolyline.Points // Assuming Neshan provides this
                    };
                }
                _logger.LogWarning("Neshan API routing response empty or invalid for {Origin},{Destination}.", $"{originLat},{originLon}", $"{destLat},{destLon}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for Neshan routing API.");
                throw new ExternalServiceException("Failed to connect to Neshan routing API.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Neshan routing API response.");
                throw new ExternalServiceException("Error processing Neshan routing API response.", ex);
            }
        }

        public async Task<NeshanReverseGeocodeResponse?> ReverseGeocodeAsync(decimal latitude, decimal longitude)
        {
            var url = $"/v5/reverse?lat={latitude}&lng={longitude}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<NeshanReverseGeocodeApiResponse>(url);
                if (response != null)
                {
                    return new NeshanReverseGeocodeResponse
                    {
                        FormattedAddress = response.FormattedAddress ?? "N/A",
                        City = response.City ?? "N/A",
                        Neighbourhood = response.Neighbourhood ?? "N/A"
                    };
                }
                _logger.LogWarning("Neshan API reverse geocode response empty for {Lat},{Lon}.", latitude, longitude);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for Neshan reverse geocode API.");
                throw new ExternalServiceException("Failed to connect to Neshan reverse geocode API.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Neshan reverse geocode API response.");
                throw new ExternalServiceException("Error processing Neshan reverse geocode API response.", ex);
            }
        }

        public async Task<NeshanSearchResponse?> SearchAddressAsync(string term, decimal? latitude = null, decimal? longitude = null)
        {
            var url = $"/v1/search?term={Uri.EscapeDataString(term)}";
            if (latitude.HasValue && longitude.HasValue)
            {
                url += $"&lat={latitude.Value}&lng={longitude.Value}";
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<NeshanSearchApiResponse>(url);
                if (response?.Items != null)
                {
                    return new NeshanSearchResponse
                    {
                        Results = response.Items.Select(item => new NeshanSearchResult
                        {
                            Title = item.Title ?? "N/A",
                            Address = item.Address ?? "N/A",
                            Latitude = item.Location?.Y ?? 0,
                            Longitude = item.Location?.X ?? 0
                        }).ToList()
                    };
                }
                return new NeshanSearchResponse { Results = new List<NeshanSearchResult>() };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for Neshan search API.");
                throw new ExternalServiceException("Failed to connect to Neshan search API.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Neshan search API response.");
                throw new ExternalServiceException("Error processing Neshan search API response.", ex);
            }
        }

        public async Task<NeshanWeatherResponse?> GetWeatherInfoAsync(decimal latitude, decimal longitude)
        {
            // Note: Neshan API doesn't directly offer weather. You might need to use a different Iranian weather API.
            // For the sake of demonstration, I'll simulate a response or assume a hypothetical Neshan weather endpoint.
            // If there's a specific Iranian weather API you prefer, I can integrate it.

            // Hypothetical Neshan weather endpoint or simulation
            _logger.LogWarning("Neshan API does not directly provide weather information. Simulating weather data.");

            // Simulate weather conditions based on time/location if no actual API is integrated
            var isRainy = DateTime.UtcNow.Minute % 2 == 0; // Just for demo
            var temperature = 20 + new Random().Next(-5, 5); // Random temperature

            return new NeshanWeatherResponse
            {
                GeneralStatus = isRainy ? "Rainy" : "Clear",
                DetailedDescription = isRainy ? "Light rain expected. Roads might be slippery." : "Clear sky, good visibility.",
                TemperatureCelsius = temperature,
                HumidityPercentage = isRainy ? 80 : 40,
                WindSpeedKmPerHour = isRainy ? 25 : 10,
                Advisory = isRainy ? "Caution: Wet roads ahead." : "No specific advisories.",
                IsSevereWarning = isRainy
            };
        }

        // Internal DTOs for Neshan API raw responses
        private class NeshanRoutingApiResponse
        {
            public List<Route>? Routes { get; set; }
        }

        private class Route
        {
            public double Distance { get; set; } // meters
            public int Duration { get; set; } // seconds
            public OverviewPolyline OverviewPolyline { get; set; } = new OverviewPolyline();
        }

        private class OverviewPolyline
        {
            public string Points { get; set; } = string.Empty; // Encoded polyline string
        }

        private class NeshanReverseGeocodeApiResponse
        {
            public string? FormattedAddress { get; set; }
            public string? City { get; set; }
            public string? Neighbourhood { get; set; }
            // Add other properties as per Neshan's actual response
        }

        private class NeshanSearchApiResponse
        {
            public List<SearchItem>? Items { get; set; }
        }

        private class SearchItem
        {
            public string? Title { get; set; }
            public string? Address { get; set; }
            public Location? Location { get; set; }
        }

        private class Location
        {
            public decimal X { get; set; } // Longitude
            public decimal Y { get; set; } // Latitude
        }
    }
}