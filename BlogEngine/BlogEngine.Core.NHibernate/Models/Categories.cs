using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Categories
    {
        public virtual int CategoryRowID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual Guid CategoryID { get; set; }

        public virtual string CategoryName { get; set; }

        public virtual string Description { get; set; }

        public virtual Guid? ParentID { get; set; }
    }
}
