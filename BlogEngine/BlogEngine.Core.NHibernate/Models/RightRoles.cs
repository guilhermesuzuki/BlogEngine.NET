using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class RightRoles
    {
        public virtual int RightRoleRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual string RightName { get; set; }

        public virtual string Role { get; set; }
    }
}
