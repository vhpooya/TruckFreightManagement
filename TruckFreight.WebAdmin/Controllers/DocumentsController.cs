using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Documents.Queries.GetPendingDocuments;
using TruckFreight.Application.Features.Documents.Commands.ApproveDocument;
using TruckFreight.WebAdmin.Models.Documents;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Operator")]
    public class DocumentsController : BaseAdminController
    {
        public async Task<IActionResult> Index(int page = 1, string status = "")
        {
            var query = new GetPendingDocumentsQuery
            {
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<Domain.Enums.DocumentStatus>(status),
                PageNumber = page,
                PageSize = 20
            };

            var result = await Mediator.Send(query);

            var viewModel = new DocumentListViewModel
            {
                Documents = result.IsSuccess ? result.Data : new Application.Common.Models.PagedResponse<DocumentDto>(
                    new List<DocumentDto>(), page, 20, 0),
                SelectedStatus = status,
                PageTitle = "مدیریت مدارک"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id, string notes = "")
        {
            var command = new ApproveDocumentCommand 
            { 
                DocumentId = id, 
                ReviewNotes = notes 
            };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            var command = new RejectDocumentCommand 
            { 
                DocumentId = id, 
                ReviewNotes = reason 
            };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(Guid id)
        {
            var query = new DownloadDocumentQuery { DocumentId = id };
            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
            }

            SetErrorMessage(result.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}

/