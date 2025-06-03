using System;
using System.Collections.Generic;
using TruckFreight.Domain.Common;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string PlateNumber { get; private set; }
        public VehicleType Type { get; private set; }
        public string Brand { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string Color { get; private set; }
        public string VIN { get; private set; }
        public string EngineNumber { get; private set; }
        public decimal Capacity { get; private set; }
        public string CapacityUnit { get; private set; }
        public decimal Length { get; private set; }
        public decimal Width { get; private set; }
        public decimal Height { get; private set; }
        public string DimensionUnit { get; private set; }
        public string InsuranceNumber { get; private set; }
        public DateTime? InsuranceExpiryDate { get; private set; }
        public string RegistrationNumber { get; private set; }
        public DateTime? RegistrationExpiryDate { get; private set; }
        public string InspectionNumber { get; private set; }
        public DateTime? InspectionExpiryDate { get; private set; }
        public string FuelType { get; private set; }
        public decimal FuelCapacity { get; private set; }
        public string FuelCapacityUnit { get; private set; }
        public string Notes { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? LastMaintenanceDate { get; private set; }
        public int? LastMaintenanceOdometer { get; private set; }
        public int? CurrentOdometer { get; private set; }
        public string OdometerUnit { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual ICollection<Trip> Trips { get; private set; }
        public virtual ICollection<VehicleDocument> Documents { get; private set; }
        public virtual ICollection<VehicleMaintenance> MaintenanceHistory { get; private set; }

        protected Vehicle()
        {
            Trips = new HashSet<Trip>();
            Documents = new HashSet<VehicleDocument>();
            MaintenanceHistory = new HashSet<VehicleMaintenance>();
            IsActive = true;
        }

        public Vehicle(
            Guid userId,
            string plateNumber,
            VehicleType type,
            string brand,
            string model,
            int year,
            string color,
            string vin,
            string engineNumber,
            decimal capacity,
            string capacityUnit,
            decimal length,
            decimal width,
            decimal height,
            string dimensionUnit)
            : this()
        {
            UserId = userId;
            PlateNumber = plateNumber ?? throw new ArgumentNullException(nameof(plateNumber));
            Type = type;
            Brand = brand ?? throw new ArgumentNullException(nameof(brand));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Year = year;
            Color = color ?? throw new ArgumentNullException(nameof(color));
            VIN = vin ?? throw new ArgumentNullException(nameof(vin));
            EngineNumber = engineNumber ?? throw new ArgumentNullException(nameof(engineNumber));
            Capacity = capacity;
            CapacityUnit = capacityUnit ?? throw new ArgumentNullException(nameof(capacityUnit));
            Length = length;
            Width = width;
            Height = height;
            DimensionUnit = dimensionUnit ?? throw new ArgumentNullException(nameof(dimensionUnit));
        }

        public void UpdateInsuranceInfo(string insuranceNumber, DateTime expiryDate)
        {
            InsuranceNumber = insuranceNumber;
            InsuranceExpiryDate = expiryDate;
        }

        public void UpdateRegistrationInfo(string registrationNumber, DateTime expiryDate)
        {
            RegistrationNumber = registrationNumber;
            RegistrationExpiryDate = expiryDate;
        }

        public void UpdateInspectionInfo(string inspectionNumber, DateTime expiryDate)
        {
            InspectionNumber = inspectionNumber;
            InspectionExpiryDate = expiryDate;
        }

        public void UpdateFuelInfo(string fuelType, decimal fuelCapacity, string fuelCapacityUnit)
        {
            FuelType = fuelType;
            FuelCapacity = fuelCapacity;
            FuelCapacityUnit = fuelCapacityUnit;
        }

        public void UpdateOdometer(int currentOdometer, string odometerUnit)
        {
            CurrentOdometer = currentOdometer;
            OdometerUnit = odometerUnit;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
        }

        public void UpdateStatus(bool isActive)
        {
            IsActive = isActive;
        }

        public void RecordMaintenance(DateTime maintenanceDate, int odometer, string notes)
        {
            LastMaintenanceDate = maintenanceDate;
            LastMaintenanceOdometer = odometer;
            MaintenanceHistory.Add(new VehicleMaintenance(Id, maintenanceDate, odometer, notes));
        }

        public bool IsInsuranceValid => InsuranceExpiryDate.HasValue && InsuranceExpiryDate.Value > DateTime.UtcNow;
        public bool IsRegistrationValid => RegistrationExpiryDate.HasValue && RegistrationExpiryDate.Value > DateTime.UtcNow;
        public bool IsInspectionValid => InspectionExpiryDate.HasValue && InspectionExpiryDate.Value > DateTime.UtcNow;
        public bool IsFullyCompliant => IsInsuranceValid && IsRegistrationValid && IsInspectionValid;
    }
}
