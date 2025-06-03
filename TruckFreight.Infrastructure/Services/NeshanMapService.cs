using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Infrastructure.Models;

namespace TruckFreight.Infrastructure.Services
{
    public class NeshanMapService : IMapService
    {
        private readonly ILogger<NeshanMapService> _logger;
        private readonly NeshanMapSettings _settings;
        private readonly HttpClient _httpClient;

        public NeshanMapService(
            ILogger<NeshanMapService> logger,
            IOptions<NeshanMapSettings> settings,
            HttpClient httpClient)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Api-Key", _settings.ApiKey);
        }

        public async Task<Result<RouteInfo>> GetRouteAsync(Location origin, Location destination, RouteOptions options = null)
        {
            try
            {
                var request = new
                {
                    origin = new { lat = origin.Latitude, lng = origin.Longitude },
                    destination = new { lat = destination.Latitude, lng = destination.Longitude },
                    alternatives = options?.Alternatives ?? false,
                    avoid = options?.Avoid ?? new List<string>(),
                    optimize = options?.Optimize ?? false
                };

                var response = await _httpClient.PostAsJsonAsync("route", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var routeData = JsonSerializer.Deserialize<NeshanRouteResponse>(content);

                var routeInfo = new RouteInfo
                {
                    Distance = routeData.Distance,
                    Duration = routeData.Duration,
                    Polyline = routeData.Polyline,
                    Steps = routeData.Steps.Select(s => new RouteStep
                    {
                        Distance = s.Distance,
                        Duration = s.Duration,
                        Instruction = s.Instruction,
                        Polyline = s.Polyline
                    }).ToList()
                };

                return Result<RouteInfo>.Success(routeInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route from Neshan Map API");
                return Result<RouteInfo>.Failure("Failed to get route");
            }
        }

        public async Task<Result<List<Location>>> GeocodeAsync(string address)
        {
            try
            {
                var response = await _httpClient.GetAsync($"geocode?address={Uri.EscapeDataString(address)}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var geocodeData = JsonSerializer.Deserialize<NeshanGeocodeResponse>(content);

                var locations = geocodeData.Results.Select(r => new Location
                {
                    Latitude = r.Location.Lat,
                    Longitude = r.Location.Lng,
                    Address = r.FormattedAddress
                }).ToList();

                return Result<List<Location>>.Success(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geocoding address: {Address}", address);
                return Result<List<Location>>.Failure("Failed to geocode address");
            }
        }

        public async Task<Result<string>> ReverseGeocodeAsync(Location location)
        {
            try
            {
                var response = await _httpClient.GetAsync($"reverse-geocode?lat={location.Latitude}&lng={location.Longitude}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var reverseGeocodeData = JsonSerializer.Deserialize<NeshanReverseGeocodeResponse>(content);

                return Result<string>.Success(reverseGeocodeData.FormattedAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reverse geocoding location: {Lat}, {Lng}", location.Latitude, location.Longitude);
                return Result<string>.Failure("Failed to reverse geocode location");
            }
        }

        public async Task<Result<List<Location>>> GetNearbyPlacesAsync(Location location, string type, double radius)
        {
            try
            {
                var response = await _httpClient.GetAsync($"nearby?lat={location.Latitude}&lng={location.Longitude}&type={type}&radius={radius}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var nearbyData = JsonSerializer.Deserialize<NeshanNearbyResponse>(content);

                var places = nearbyData.Results.Select(r => new Location
                {
                    Latitude = r.Location.Lat,
                    Longitude = r.Location.Lng,
                    Address = r.FormattedAddress,
                    Name = r.Name
                }).ToList();

                return Result<List<Location>>.Success(places);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby places for location: {Lat}, {Lng}", location.Latitude, location.Longitude);
                return Result<List<Location>>.Failure("Failed to get nearby places");
            }
        }
    }

    public class NeshanMapSettings
    {
        public string ApiKey { get; set; }
        public string ApiBaseUrl { get; set; }
    }

    public class NeshanRouteResponse
    {
        public double Distance { get; set; }
        public double Duration { get; set; }
        public string Polyline { get; set; }
        public List<NeshanRouteStep> Steps { get; set; }
    }

    public class NeshanRouteStep
    {
        public double Distance { get; set; }
        public double Duration { get; set; }
        public string Instruction { get; set; }
        public string Polyline { get; set; }
    }

    public class NeshanGeocodeResponse
    {
        public List<NeshanGeocodeResult> Results { get; set; }
    }

    public class NeshanGeocodeResult
    {
        public NeshanLocation Location { get; set; }
        public string FormattedAddress { get; set; }
    }

    public class NeshanLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class NeshanReverseGeocodeResponse
    {
        public string FormattedAddress { get; set; }
    }

    public class NeshanNearbyResponse
    {
        public List<NeshanNearbyResult> Results { get; set; }
    }

    public class NeshanNearbyResult
    {
        public NeshanLocation Location { get; set; }
        public string FormattedAddress { get; set; }
        public string Name { get; set; }
    }
} 