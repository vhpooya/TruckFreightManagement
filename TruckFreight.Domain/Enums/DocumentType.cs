namespace TruckFreight.Domain.Enums
{
    public enum DocumentType
    {
        // Registration Documents
        VehicleRegistration = 1,
        VehicleTitle = 2,
        VehiclePermit = 3,
        VehicleLicense = 4,

        // Insurance Documents
        InsurancePolicy = 5,
        InsuranceCertificate = 6,
        InsuranceClaim = 7,

        // Inspection Documents
        SafetyInspection = 8,
        EmissionsInspection = 9,
        WeightInspection = 10,
        LoadSecurementInspection = 11,

        // Maintenance Documents
        MaintenanceRecord = 12,
        RepairInvoice = 13,
        ServiceHistory = 14,
        WarrantyDocument = 15,

        // Driver Documents
        DriverLicense = 16,
        MedicalCertificate = 17,
        TrainingCertificate = 18,
        BackgroundCheck = 19,

        // Cargo Documents
        CargoInsurance = 20,
        CargoManifest = 21,
        LoadingCertificate = 22,
        UnloadingCertificate = 23,

        // Other
        Other = 99
    }
}
