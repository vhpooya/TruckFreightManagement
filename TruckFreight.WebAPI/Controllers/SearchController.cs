using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Search.Queries.GlobalSearch;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class SearchController : BaseController
    {
        /// <summary>
        /// Global search across all entities
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GlobalSearch([FromQuery] GlobalSearchQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Search suggestions
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<ActionResult> GetSearchSuggestions([FromQuery] string term)
        {
            var query = new GetSearchSuggestionsQuery { SearchTerm = term };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}

/