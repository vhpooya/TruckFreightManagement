using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public enum CommissionType
    {
        Percentage = 1,
        FixedAmount = 2,
        TieredPercentage = 3,
        PerTransaction = 4
    }

    public enum CommissionApplicability
    {
        Driver = 1,
        CargoOwner = 2,
        Both = 3,
        Platform = 4
    }

    public class Commission : BaseEntity
    {
        //[Key]
        //public int CommissionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public CommissionType CommissionType { get; set; }

        [Required]
        public CommissionApplicability ApplicableTo { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,4)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinimumAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaximumAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ThresholdAmount { get; set; }

        public VehicleType? ApplicableVehicleType { get; set; }

        public CargoType? ApplicableCargoType { get; set; }

        [Required]
        public DateTime EffectiveFromDate { get; set; }

        public DateTime? EffectiveToDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(1000)]
        public string TierConfiguration { get; set; } // JSON for tiered rates

        public int CreatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? LastModifiedByUserId { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }

        [ForeignKey("LastModifiedByUserId")]
        public virtual User LastModifiedByUser { get; set; }

        public Commission()
        {
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
            EffectiveFromDate = DateTime.UtcNow;
        }
    }
}