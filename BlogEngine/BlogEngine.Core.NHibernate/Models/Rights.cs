using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Rights
    {
        public virtual int RightRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual string RightName { get; set; }
    }
}
