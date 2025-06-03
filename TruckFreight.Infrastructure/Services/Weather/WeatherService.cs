using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Infrastructure.Services.Weather
{
   public class WeatherService : IWeatherService
   {
       private readonly HttpClient _httpClient;
       private readonly IConfiguration _configuration;
       private readonly ILogger<WeatherService> _logger;
       private readonly string _apiKey;
       private readonly string _baseUrl;

       public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger)
       {
           _httpClient = httpClient;
           _configuration = configuration;
           _logger = logger;
           _apiKey = _configuration["Weather:ApiKey"];
           _baseUrl = _configuration["Weather:BaseUrl"] ?? "https://api.weatherapi.com/v1";
       }

       public async Task<WeatherInfo> GetCurrentWeatherAsync(GeoLocation location)
       {
           try
           {
               var url = $"{_baseUrl}/current.json?key={_apiKey}&q={location.Latitude},{location.Longitude}&lang=fa";

               var response = await _httpClient.GetAsync(url);
               response.EnsureSuccessStatusCode();

               var content = await response.Content.ReadAsStringAsync();
               var weatherData = JsonSerializer.Deserialize<WeatherApiResponse>(content, new JsonSerializerOptions
               {
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               });

               if (weatherData?.Current != null)
               {
                   return new WeatherInfo
                   {
                       Location = location,
                       Condition = MapToWeatherCondition(weatherData.Current.Condition.Code),
                       Temperature = weatherData.Current.TempC,
                       WindSpeed = weatherData.Current.WindKph,
                       Visibility = weatherData.Current.VisKm,
                       Description = weatherData.Current.Condition.Text,
                       Timestamp = DateTime.UtcNow
                   };
               }

               return new WeatherInfo
               {
                   Location = location,
                   Condition = WeatherCondition.Clear,
                   Temperature = 20,
                   WindSpeed = 0,
                   Visibility = 10,
                   Description = "اطلاعات آب و هوا در دسترس نیست",
                   Timestamp = DateTime.UtcNow
               };
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error getting current weather for location: {Lat}, {Lng}", location.Latitude, location.Longitude);
               return new WeatherInfo
               {
                   Location = location,
                   Condition = WeatherCondition.Clear,
                   Temperature = 20,
                   WindSpeed = 0,
                   Visibility = 10,
                   Description = "خطا در دریافت اطلاعات آب و هوا",
                   Timestamp = DateTime.UtcNow
               };
           }
       }

       public async Task<List<WeatherInfo>> GetRouteWeatherAsync(List<GeoLocation> route)
       {
           var weatherInfos = new List<WeatherInfo>();

           try
           {
               // Sample key points along the route (every 50km or major points)
               var keyPoints = SampleRoutePoints(route, 5);

               foreach (var point in keyPoints)
               {
                   var weather = await GetCurrentWeatherAsync(point);
                   weatherInfos.Add(weather);
                   
                   // Add small delay to avoid API rate limiting
                   await Task.Delay(100);
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error getting route weather");
           }

           return weatherInfos;
       }

       public async Task<List<WeatherAlert>> GetWeatherAlertsAsync(GeoLocation location, double radiusKm = 50)
       {
           try
           {
               // Note: This is a simplified implementation
               // In production, you might want to use a dedicated weather alerts API
               var currentWeather = await GetCurrentWeatherAsync(location);
               var alerts = new List<WeatherAlert>();

               // Generate alerts based on severe weather conditions
               if (currentWeather.WindSpeed > 50) // High wind
               {
                   alerts.Add(new WeatherAlert
                   {
                       Location = location,
                       Severity = WeatherSeverity.High,
                       Title = "هشدار باد شدید",
                       Description = $"سرعت باد {currentWeather.WindSpeed:F0} کیلومتر بر ساعت. رانندگی با احتیاط انجام شود.",
                       ValidFrom = DateTime.UtcNow,
                       ValidTo = DateTime.UtcNow.AddHours(6)
                   });
               }

               if (currentWeather.Visibility < 1) // Poor visibility
               {
                   alerts.Add(new WeatherAlert
                   {
                       Location = location,
                       Severity = WeatherSeverity.Severe,
                       Title = "هشدار دید کم",
                       Description = $"دید {currentWeather.Visibility:F1} کیلومتر. از رانندگی خودداری کنید.",
                       ValidFrom = DateTime.UtcNow,
                       ValidTo = DateTime.UtcNow.AddHours(3)
                   });
               }

               if (currentWeather.Condition == WeatherCondition.Stormy || currentWeather.Condition == WeatherCondition.Snowy)
               {
                   alerts.Add(new WeatherAlert
                   {
                       Location = location,
                       Severity = WeatherSeverity.High,
                       Title = "هشدار شرایط جوی نامناسب",
                       Description = $"شرایط جوی: {currentWeather.Description}. سفر را به تعویق بیندازید.",
                       ValidFrom = DateTime.UtcNow,
                       ValidTo = DateTime.UtcNow.AddHours(12)
                   });
               }

               return alerts;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error getting weather alerts");
               return new List<WeatherAlert>();
           }
       }

       public async Task<bool> IsSafeForTravelAsync(List<GeoLocation> route)
       {
           try
           {
               var routeWeather = await GetRouteWeatherAsync(route);
               
               foreach (var weather in routeWeather)
               {
                   // Check for dangerous conditions
                   if (weather.Visibility < 0.5 || // Very poor visibility
                       weather.WindSpeed > 70 ||    // Very high wind
                       weather.Condition == WeatherCondition.Stormy ||
                       weather.Condition == WeatherCondition.Extreme)
                   {
                       return false;
                   }
               }

               return true;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error checking travel safety");
               return true; // Default to safe if we can't check
           }
       }

       private List<GeoLocation> SampleRoutePoints(List<GeoLocation> route, int maxPoints)
       {
           if (route.Count <= maxPoints)
               return route;

           var sampledPoints = new List<GeoLocation>();
           var step = route.Count / maxPoints;

           for (int i = 0; i < route.Count; i += step)
           {
               sampledPoints.Add(route[i]);
           }

           // Always include the last point
           if (sampledPoints.Last() != route.Last())
           {
               sampledPoints.Add(route.Last());
           }

           return sampledPoints;
       }

       private WeatherCondition MapToWeatherCondition(int conditionCode)
       {
           // Weather API condition codes mapping
           return conditionCode switch
           {
               1000 => WeatherCondition.Clear,      // Sunny/Clear
               1003 or 1006 or 1009 => WeatherCondition.Cloudy, // Partly cloudy to Overcast
               1030 or 1135 or 1147 => WeatherCondition.Foggy,  // Mist/Fog
               >= 1063 and <= 1201 => WeatherCondition.Rainy,   // Rain conditions
               >= 1204 and <= 1237 => WeatherCondition.Snowy,   // Snow/Sleet conditions
               >= 1240 and <= 1264 => WeatherCondition.Rainy,   // Heavy rain
               >= 1273 and <= 1282 => WeatherCondition.Stormy,  // Thunderstorms
               _ => WeatherCondition.Clear
           };
       }
   }

   // DTOs for Weather API
   public class WeatherApiResponse
   {
       public WeatherApiCurrent Current { get; set; }
   }

   public class WeatherApiCurrent
   {
       public double TempC { get; set; }
       public double WindKph { get; set; }
       public double VisKm { get; set; }
       public WeatherApiCondition Condition { get; set; }
   }

   public class WeatherApiCondition
   {
       public string Text { get; set; }
       public int Code { get; set; }
   }
}

/