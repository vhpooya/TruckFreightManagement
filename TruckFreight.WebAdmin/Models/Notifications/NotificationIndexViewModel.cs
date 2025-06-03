namespace TruckFreight.WebAdmin.Models.Notifications
{
    public class NotificationIndexViewModel : BaseViewModel
    {
    }

    public class SendBroadcastViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Domain.Enums.NotificationType Type { get; set; }
        public string TargetRole { get; set; } = string.Empty;
    }

    public class NotificationTemplatesViewModel : BaseViewModel
    {
        public List<NotificationTemplateDto> Templates { get; set; } = new List<NotificationTemplateDto>();
    }
}

/