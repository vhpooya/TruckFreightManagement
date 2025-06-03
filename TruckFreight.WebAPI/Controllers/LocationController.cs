using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Location.Queries.GetNearbyServices;
using TruckFreight.Application.Features.Location.Queries.GeocodeAddress;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class LocationController : BaseController
    {
        /// <summary>
        /// Find nearby services (gas stations, restaurants, etc.)
        /// </summary>
        [HttpGet("nearby-services")]
        public async Task<ActionResult> GetNearbyServices([FromQuery] GetNearbyServicesQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Geocode address to coordinates
        /// </summary>
        [HttpPost("geocode")]
        public async Task<ActionResult> GeocodeAddress([FromBody] GeocodeAddressQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Reverse geocode coordinates to address
        /// </summary>
        [HttpPost("reverse-geocode")]
        public async Task<ActionResult> ReverseGeocode([FromBody] ReverseGeocodeQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Calculate route between two points
        /// </summary>
        [HttpPost("calculate-route")]
        public async Task<ActionResult> CalculateRoute([FromBody] CalculateRouteQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}

/