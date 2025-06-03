namespace TruckFreight.Domain.Enums
{
    public enum VehicleType
    {
        // Light Trucks
        PickupTruck = 1,
        Van = 2,
        MiniTruck = 3,

        // Medium Trucks
        BoxTruck = 4,
        FlatbedTruck = 5,
        RefrigeratedTruck = 6,
        TankerTruck = 7,

        // Heavy Trucks
        SemiTruck = 8,
        DumpTruck = 9,
        ConcreteMixer = 10,
        CraneTruck = 11,

        // Specialized Trucks
        CarCarrier = 12,
        LivestockTruck = 13,
        ContainerTruck = 14,
        TowTruck = 15,

        // Other
        Other = 99
    }
}
