using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Documents.Commands.UploadDocument;
using TruckFreight.Application.Features.Documents.Queries.GetUserDocuments;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class DocumentsController : BaseController
    {
        /// <summary>
        /// Upload user document for verification
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult> UploadDocument([FromForm] UploadDocumentCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get user's documents
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetUserDocuments()
        {
            var query = new GetUserDocumentsQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Delete document
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(Guid id)
        {
            var command = new DeleteDocumentCommand { DocumentId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Download document
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadDocument(Guid id)
        {
            var query = new DownloadDocumentQuery { DocumentId = id };
            var result = await Mediator.Send(query);
            
            if (result.IsSuccess)
            {
                return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
            }
            
            return HandleResult(result);
        }
    }
}

/