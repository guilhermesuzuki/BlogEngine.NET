using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class UserRoles
    {
        public virtual int UserRoleID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Role { get; set; }
    }
}
