using System;

namespace TruckFreight.Application.Features.Weather.DTOs
{
    public class WeatherForecastDto
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double TemperatureMin { get; set; }
        public double TemperatureMax { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public string WeatherCondition { get; set; }
        public string WeatherDescription { get; set; }
        public double Precipitation { get; set; }
        public double Visibility { get; set; }
        public double Pressure { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
    }
} 