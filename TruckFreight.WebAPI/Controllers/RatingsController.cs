using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrRetryPGContinueEditcsharpusing TruckFreight.Application.Features.Ratings.Commands.SubmitRating;
using TruckFreight.Application.Features.Ratings.Queries.GetUserRatings;
using TruckFreight.Application.Features.Ratings.Queries.GetTripRatings;

namespace TruckFreight.WebAPI.Controllers
{
   [Authorize]
   public class RatingsController : BaseController
   {
       /// <summary>
       /// Submit rating for a completed trip
       /// </summary>
       [HttpPost]
       public async Task<ActionResult> SubmitRating([FromBody] SubmitRatingCommand command)
       {
           var result = await Mediator.Send(command);
           return HandleResult(result);
       }

       /// <summary>
       /// Get user's received ratings
       /// </summary>
       [HttpGet("received")]
       public async Task<ActionResult> GetReceivedRatings([FromQuery] GetUserRatingsQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Get user's given ratings
       /// </summary>
       [HttpGet("given")]
       public async Task<ActionResult> GetGivenRatings([FromQuery] GetGivenRatingsQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Get ratings for a specific trip
       /// </summary>
       [HttpGet("trip/{tripId}")]
       public async Task<ActionResult> GetTripRatings(Guid tripId)
       {
           var query = new GetTripRatingsQuery { TripId = tripId };
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Update existing rating
       /// </summary>
       [HttpPut("{id}")]
       public async Task<ActionResult> UpdateRating(Guid id, [FromBody] UpdateRatingCommand command)
       {
           command.RatingId = id;
           var result = await Mediator.Send(command);
           return HandleResult(result);
       }
   }
}

/