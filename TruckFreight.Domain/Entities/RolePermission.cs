using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using TruckFreight.Domain.Common;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Domain.Entities
{

    public class RolePermission : BaseEntity
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }
        public DateTime GrantedAt { get; set; }
        public string GrantedBy { get; set; }
    }
}
