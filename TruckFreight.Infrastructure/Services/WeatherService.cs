using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TruckFreight.Application.Features.Weather.DTOs;
using TruckFreight.Application.Features.Weather.Services;
using TruckFreight.Infrastructure.Models;

namespace TruckFreight.Infrastructure.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherSettings _settings;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(
            HttpClient httpClient,
            IOptions<WeatherSettings> settings,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<WeatherForecastDto> GetCurrentWeatherAsync(string location)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/weather?q={location}&appid={_settings.ApiKey}&units=metric");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(content);

                return MapToWeatherForecastDto(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current weather for location: {Location}", location);
                throw;
            }
        }

        public async Task<WeatherForecastDto[]> GetWeatherForecastAsync(string location, int days)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/forecast?q={location}&appid={_settings.ApiKey}&units=metric&cnt={days * 8}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var forecastData = JsonSerializer.Deserialize<OpenWeatherMapForecastResponse>(content);

                return forecastData.List.Select(MapToWeatherForecastDto).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather forecast for location: {Location}", location);
                throw;
            }
        }

        public async Task<WeatherForecastDto> GetWeatherByCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_settings.ApiKey}&units=metric");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(content);

                return MapToWeatherForecastDto(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }

        public async Task<WeatherForecastDto[]> GetWeatherForecastByCoordinatesAsync(double latitude, double longitude, int days)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/forecast?lat={latitude}&lon={longitude}&appid={_settings.ApiKey}&units=metric&cnt={days * 8}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var forecastData = JsonSerializer.Deserialize<OpenWeatherMapForecastResponse>(content);

                return forecastData.List.Select(MapToWeatherForecastDto).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather forecast for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }

        private WeatherForecastDto MapToWeatherForecastDto(OpenWeatherMapResponse data)
        {
            return new WeatherForecastDto
            {
                Date = DateTimeOffset.FromUnixTimeSeconds(data.Dt).DateTime,
                Temperature = data.Main.Temp,
                TemperatureMin = data.Main.TempMin,
                TemperatureMax = data.Main.TempMax,
                Humidity = data.Main.Humidity,
                Pressure = data.Main.Pressure,
                WindSpeed = data.Wind.Speed,
                WindDirection = GetWindDirection(data.Wind.Deg),
                WeatherCondition = data.Weather[0].Main,
                WeatherDescription = data.Weather[0].Description,
                Precipitation = data.Rain?.ThreeHour ?? 0,
                Visibility = data.Visibility,
                Location = data.Name,
                City = data.Name,
                Province = data.Sys.Country
            };
        }

        private string GetWindDirection(double degrees)
        {
            string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int index = (int)((degrees + 22.5) % 360) / 45;
            return directions[index];
        }
    }

    public class OpenWeatherMapResponse
    {
        public long Dt { get; set; }
        public MainData Main { get; set; }
        public WindData Wind { get; set; }
        public WeatherData[] Weather { get; set; }
        public RainData Rain { get; set; }
        public double Visibility { get; set; }
        public string Name { get; set; }
        public SysData Sys { get; set; }
    }

    public class OpenWeatherMapForecastResponse
    {
        public List<OpenWeatherMapResponse> List { get; set; }
    }

    public class MainData
    {
        public double Temp { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
    }

    public class WindData
    {
        public double Speed { get; set; }
        public double Deg { get; set; }
    }

    public class WeatherData
    {
        public string Main { get; set; }
        public string Description { get; set; }
    }

    public class RainData
    {
        public double ThreeHour { get; set; }
    }

    public class SysData
    {
        public string Country { get; set; }
    }
} 