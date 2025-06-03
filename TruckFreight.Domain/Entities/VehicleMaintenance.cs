using System;
using TruckFreight.Domain.Common;

namespace TruckFreight.Domain.Entities
{
    public class VehicleMaintenance : BaseEntity
    {
        public Guid VehicleId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string MaintenanceType { get; private set; }
        public decimal Cost { get; private set; }
        public string Currency { get; private set; }
        public DateTime MaintenanceDate { get; private set; }
        public decimal OdometerReading { get; private set; }
        public string OdometerUnit { get; private set; }
        public string ServiceProvider { get; private set; }
        public string ServiceLocation { get; private set; }
        public string TechnicianName { get; private set; }
        public string TechnicianContact { get; private set; }
        public string InvoiceNumber { get; private set; }
        public string InvoiceUrl { get; private set; }
        public string Notes { get; private set; }
        public bool IsScheduled { get; private set; }
        public DateTime? ScheduledDate { get; private set; }
        public string ScheduledBy { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string CompletedBy { get; private set; }

        // Navigation Properties
        public virtual Vehicle Vehicle { get; private set; }

        protected VehicleMaintenance() { }

        public VehicleMaintenance(
            Guid vehicleId,
            string title,
            string maintenanceType,
            decimal cost,
            string currency,
            DateTime maintenanceDate,
            decimal odometerReading,
            string odometerUnit,
            string serviceProvider,
            string serviceLocation)
        {
            VehicleId = vehicleId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            MaintenanceType = maintenanceType ?? throw new ArgumentNullException(nameof(maintenanceType));
            Cost = cost;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            MaintenanceDate = maintenanceDate;
            OdometerReading = odometerReading;
            OdometerUnit = odometerUnit ?? throw new ArgumentNullException(nameof(odometerUnit));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            ServiceLocation = serviceLocation ?? throw new ArgumentNullException(nameof(serviceLocation));
            IsScheduled = false;
            IsCompleted = false;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public void UpdateTechnicianInfo(string name, string contact)
        {
            TechnicianName = name;
            TechnicianContact = contact;
        }

        public void UpdateInvoiceInfo(string invoiceNumber, string invoiceUrl)
        {
            InvoiceNumber = invoiceNumber;
            InvoiceUrl = invoiceUrl;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
        }

        public void Schedule(DateTime scheduledDate, string scheduledBy)
        {
            IsScheduled = true;
            ScheduledDate = scheduledDate;
            ScheduledBy = scheduledBy;
        }

        public void Complete(string completedBy)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            CompletedBy = completedBy;
        }

        public void Reschedule(DateTime newScheduledDate)
        {
            if (!IsScheduled)
                throw new InvalidOperationException("Cannot reschedule an unscheduled maintenance.");

            ScheduledDate = newScheduledDate;
        }

        public void Cancel()
        {
            IsScheduled = false;
            ScheduledDate = null;
            ScheduledBy = null;
        }
    }
} 