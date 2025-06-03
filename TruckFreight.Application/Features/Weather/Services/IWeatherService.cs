using System.Threading.Tasks;
using TruckFreight.Application.Features.Weather.DTOs;

namespace TruckFreight.Application.Features.Weather.Services
{
    public interface IWeatherService
    {
        Task<WeatherForecastDto> GetCurrentWeatherAsync(string location);
        Task<WeatherForecastDto[]> GetWeatherForecastAsync(string location, int days);
        Task<WeatherForecastDto> GetWeatherByCoordinatesAsync(double latitude, double longitude);
        Task<WeatherForecastDto[]> GetWeatherForecastByCoordinatesAsync(double latitude, double longitude, int days);
    }
} 