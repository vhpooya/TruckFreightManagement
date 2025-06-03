using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TruckFreight.Domain.Common;

namespace TruckFreight.Domain.Entities { 
    public class Role : BaseEntity
    {
      
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
