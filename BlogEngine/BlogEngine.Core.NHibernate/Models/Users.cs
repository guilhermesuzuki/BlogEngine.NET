using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Users
    {
        public virtual int UserID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Password { get; set; }

        public virtual DateTime? LastLoginTime { get; set; }

        public virtual string EmailAddress { get; set; }
    }
}
