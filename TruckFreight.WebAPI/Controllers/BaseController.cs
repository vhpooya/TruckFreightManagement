using MediatR;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        private ISender _mediator = null!;
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected ActionResult HandleResult<T>(BaseResponse<T> result)
        {
            if (result == null)
                return NotFound();

            if (result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    200 => Ok(result),
                    201 => Created(string.Empty, result),
                    _ => Ok(result)
                };
            }

            return result.StatusCode switch
            {
                400 => BadRequest(result),
                401 => Unauthorized(result),
                403 => StatusCode(403, result),
                404 => NotFound(result),
                _ => StatusCode(500, result)
            };
        }

        protected ActionResult HandleResult(BaseResponse result)
        {
            if (result == null)
                return NotFound();

            if (result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    200 => Ok(result),
                    201 => Created(string.Empty, result),
                    _ => Ok(result)
                };
            }

            return result.StatusCode switch
            {
                400 => BadRequest(result),
                401 => Unauthorized(result),
                403 => StatusCode(403, result),
                404 => NotFound(result),
                _ => StatusCode(500, result)
            };
        }
    }
}

/