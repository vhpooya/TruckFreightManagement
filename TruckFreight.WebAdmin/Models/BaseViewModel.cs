namespace TruckFreight.WebAdmin.Models
{
    public abstract class BaseViewModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public string PageDescription { get; set; } = string.Empty;
        public List<string> BreadcrumbItems { get; set; } = new List<string>();
    }
}

/