using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
   public class CargoOwner : BaseEntity
   {
       public Guid UserId { get; private set; }
       public string CompanyName { get; private set; }
       public string BusinessRegistrationNumber { get; private set; }
       public string TaxId { get; private set; }
       public Address BusinessAddress { get; private set; }
       public string Website { get; private set; }
       public double Rating { get; private set; }
       public int TotalOrders { get; private set; }
       public bool IsVerifiedBusiness { get; private set; }

       // Navigation Properties
       public virtual User User { get; private set; }
       public virtual ICollection<CargoRequest> CargoRequests { get; private set; }
       public virtual ICollection<CargoOwnerRating> Ratings { get; private set; }

       protected CargoOwner()
       {
           CargoRequests = new HashSet<CargoRequest>();
           Ratings = new HashSet<CargoOwnerRating>();
       }

       public CargoOwner(Guid userId, string companyName, string businessRegistrationNumber, 
                        Address businessAddress)
           : this()
       {
           UserId = userId;
           CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
           BusinessRegistrationNumber = businessRegistrationNumber;
           BusinessAddress = businessAddress ?? throw new ArgumentNullException(nameof(businessAddress));
           Rating = 5.0;
           IsVerifiedBusiness = false;
       }

       public void UpdateBusinessInfo(string companyName, string businessRegistrationNumber, 
                                    string taxId, Address businessAddress, string website)
       {
           CompanyName = companyName;
           BusinessRegistrationNumber = businessRegistrationNumber;
           TaxId = taxId;
           BusinessAddress = businessAddress;
           Website = website;
       }

       public void VerifyBusiness()
       {
           IsVerifiedBusiness = true;
       }

       public void CompleteOrder()
       {
           TotalOrders++;
       }

       public void UpdateRating(double newRating)
       {
           if (newRating >= 1 && newRating <= 5)
           {
               Rating = newRating;
           }
       }
   }
}
