using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TruckFreight.WebAdmin.Models;

namespace TruckFreight.WebAdmin.Services
{
    public class NeshanMapService
    {
        private readonly HttpClient _httpClient;
        private readonly NeshanSettings _settings;
        private readonly ILogger<NeshanMapService> _logger;

        public NeshanMapService(
            HttpClient httpClient,
            IOptions<NeshanSettings> settings,
            ILogger<NeshanMapService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<LocationInfo> GetLocationInfoAsync(double latitude, double longitude)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_settings.BaseUrl}/v2/reverse?lat={latitude}&lng={longitude}&api-key={_settings.ApiKey}");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<NeshanResponse>(content);

                return new LocationInfo
                {
                    Address = result.address,
                    City = result.city,
                    District = result.district,
                    Province = result.province,
                    PostalCode = result.postal_code,
                    FormattedAddress = result.formatted_address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location info from Neshan API");
                throw;
            }
        }

        public async Task<RouteInfo> GetRouteInfoAsync(double originLat, double originLng, double destLat, double destLng)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_settings.BaseUrl}/v1/direction?origin={originLat},{originLng}&destination={destLat},{destLng}&api-key={_settings.ApiKey}");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<NeshanDirectionResponse>(content);

                return new RouteInfo
                {
                    Distance = result.routes[0].legs[0].distance.value,
                    Duration = result.routes[0].legs[0].duration.value,
                    Polyline = result.routes[0].overview_polyline.points
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route info from Neshan API");
                throw;
            }
        }

        public async Task<TrafficInfo> GetTrafficInfoAsync(double latitude, double longitude, double radius = 1000)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_settings.BaseUrl}/v1/traffic?lat={latitude}&lng={longitude}&radius={radius}&api-key={_settings.ApiKey}");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<NeshanTrafficResponse>(content);

                return new TrafficInfo
                {
                    TrafficLevel = result.traffic_level,
                    Speed = result.speed,
                    Delay = result.delay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting traffic info from Neshan API");
                throw;
            }
        }
    }

    public class NeshanSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }

    public class LocationInfo
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string FormattedAddress { get; set; }
    }

    public class RouteInfo
    {
        public int Distance { get; set; }
        public int Duration { get; set; }
        public string Polyline { get; set; }
    }

    public class TrafficInfo
    {
        public string TrafficLevel { get; set; }
        public int Speed { get; set; }
        public int Delay { get; set; }
    }

    public class NeshanResponse
    {
        public string address { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string province { get; set; }
        public string postal_code { get; set; }
        public string formatted_address { get; set; }
    }

    public class NeshanDirectionResponse
    {
        public Route[] routes { get; set; }
    }

    public class Route
    {
        public Leg[] legs { get; set; }
        public OverviewPolyline overview_polyline { get; set; }
    }

    public class Leg
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
    }

    public class Distance
    {
        public int value { get; set; }
        public string text { get; set; }
    }

    public class Duration
    {
        public int value { get; set; }
        public string text { get; set; }
    }

    public class OverviewPolyline
    {
        public string points { get; set; }
    }

    public class NeshanTrafficResponse
    {
        public string traffic_level { get; set; }
        public int speed { get; set; }
        public int delay { get; set; }
    }
} 