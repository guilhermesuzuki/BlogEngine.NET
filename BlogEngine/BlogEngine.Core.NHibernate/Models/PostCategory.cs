using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class PostCategory
    {
        public virtual int PostCategoryID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual Guid PostID { get; set; }

        public virtual Guid CategoryID { get; set; }
    }
}
