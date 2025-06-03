namespace TruckFreight.Domain.Enums
{
    public enum CargoRequestStatus
    {
        Draft = 1,
        Published = 2,
        AssignedToDriver = 3,
        InProgress = 4,
        Loading = 5,
        InTransit = 6,
        Delivered = 7,
        Completed = 8,
        Cancelled = 9,
        Disputed = 10
    }
}
