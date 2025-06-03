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
    public class MapIrService : IMapService
    {
        private readonly ILogger<MapIrService> _logger;
        private readonly MapIrSettings _settings;
        private readonly HttpClient _httpClient;
        
        public MapIrService(
            ILogger<MapIrService> logger,
            IOptions<MapIrSettings> settings,
            HttpClient httpClient)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        }

        public async Task<Result<RouteInfo>> GetRouteAsync(Location origin, Location destination, RouteOptions options = null)
        {
            
            try
            {
                var request = new
                {
                    origin = $"{origin.Latitude},{origin.Longitude}",
                    destination = $"{destination.Latitude},{destination.Longitude}",
                    alternatives = options?.Alternatives ?? false,
                    avoid = options?.Avoid ?? new List<string>(),
                    optimize = options?.Optimize ?? false
                };

                var response = await _httpClient.PostAsJsonAsync("route", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var routeData = JsonSerializer.Deserialize<MapIrRouteResponse>(content);

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
                _logger.LogError(ex, "Error getting route from Map.ir API");
                return Result<RouteInfo>.Failure("Failed to get route");
            }
        }

        public async Task<Result<List<Location>>> GeocodeAsync(string address)
        {
            try
            {
                var response = await _httpClient.GetAsync($"geocode?text={Uri.EscapeDataString(address)}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var geocodeData = JsonSerializer.Deserialize<MapIrGeocodeResponse>(content);

                var locations = geocodeData.Value.Select(r => new Location
                {
                    Latitude = r.Coordinates[1],
                    Longitude = r.Coordinates[0],
                    Address = r.Address
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
                var response = await _httpClient.GetAsync($"reverse?lat={location.Latitude}&lon={location.Longitude}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var reverseGeocodeData = JsonSerializer.Deserialize<MapIrReverseGeocodeResponse>(content);

                return Result<string>.Success(reverseGeocodeData.Address);
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
                var response = await _httpClient.GetAsync($"search?lat={location.Latitude}&lon={location.Longitude}&type={type}&radius={radius}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var nearbyData = JsonSerializer.Deserialize<MapIrNearbyResponse>(content);

                var places = nearbyData.Value.Select(r => new Location
                {
                    Latitude = r.Coordinates[1],
                    Longitude = r.Coordinates[0],
                    Address = r.Address,
                    Name = r.Title
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

    public class MapIrSettings
    {
        public string ApiKey { get; set; }
        public string ApiBaseUrl { get; set; }
    }

    public class MapIrRouteResponse
    {
        public double Distance { get; set; }
        public double Duration { get; set; }
        public string Polyline { get; set; }
        public List<MapIrRouteStep> Steps { get; set; }
    }

    public class MapIrRouteStep
    {
        public double Distance { get; set; }
        public double Duration { get; set; }
        public string Instruction { get; set; }
        public string Polyline { get; set; }
    }

    public class MapIrGeocodeResponse
    {
        public List<MapIrGeocodeResult> Value { get; set; }
    }

    public class MapIrGeocodeResult
    {
        public double[] Coordinates { get; set; }
        public string Address { get; set; }
    }

    public class MapIrReverseGeocodeResponse
    {
        public string Address { get; set; }
    }

    public class MapIrNearbyResponse
    {
        public List<MapIrNearbyResult> Value { get; set; }
    }

    public class MapIrNearbyResult
    {
        public double[] Coordinates { get; set; }
        public string Address { get; set; }
        public string Title { get; set; }
    }
} 