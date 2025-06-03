namespace TruckFreight.Domain.Enums
{
    public enum MaintenanceType
    {
        // Regular Maintenance
        OilChange = 1,
        TireRotation = 2,
        BrakeService = 3,
        AirFilterChange = 4,
        FuelFilterChange = 5,
        CabinFilterChange = 6,
        BatteryCheck = 7,
        CoolantCheck = 8,
        TransmissionFluidCheck = 9,
        DifferentialFluidCheck = 10,

        // Inspections
        GeneralInspection = 11,
        SafetyInspection = 12,
        EmissionsInspection = 13,
        WeightInspection = 14,
        LoadSecurementInspection = 15,

        // Repairs
        EngineRepair = 16,
        TransmissionRepair = 17,
        SuspensionRepair = 18,
        BrakeRepair = 19,
        ElectricalRepair = 20,
        BodyRepair = 21,
        GlassRepair = 22,
        ExhaustRepair = 23,
        FuelSystemRepair = 24,
        CoolingSystemRepair = 25,

        // Upgrades
        PerformanceUpgrade = 26,
        SafetyUpgrade = 27,
        ComfortUpgrade = 28,
        TechnologyUpgrade = 29,

        // Other
        Other = 99
    }
} 