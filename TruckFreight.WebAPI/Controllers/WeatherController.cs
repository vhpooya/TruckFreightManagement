using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Weather.Queries.GetRouteWeather;
using TruckFreight.Application.Features.Weather.Queries.GetCurrentWeather;

namespace TruckFreight.WebAPI.Controllers
{
   [Authorize]
   public class WeatherController : BaseController
   {
       /// <summary>
       /// Get weather information for a route
       /// </summary>
       [HttpGet("route")]
       public async Task<ActionResult> GetRouteWeather([FromQuery] GetRouteWeatherQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Get current weather at location
       /// </summary>
       [HttpGet("current")]
       public async Task<ActionResult> GetCurrentWeather([FromQuery] GetCurrentWeatherQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Get weather alerts for area
       /// </summary>
       [HttpGet("alerts")]
       public async Task<ActionResult> GetWeatherAlerts([FromQuery] GetWeatherAlertsQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }
   }
}

/