using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Documents.Queries.GetPendingDocuments;

namespace TruckFreight.WebAdmin.Models.Documents
{
    public class DocumentListViewModel : BaseViewModel
    {
        public PagedResponse<DocumentDto> Documents { get; set; } = new PagedResponse<DocumentDto>(new List<DocumentDto>(), 1, 20, 0);
        public string SelectedStatus { get; set; } = string.Empty;
    }
}

/