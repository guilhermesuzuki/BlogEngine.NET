using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class PostTag
    {
        public virtual int PostTagID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual Guid PostID { get; set; }

        public virtual string Tag { get; set; }
    }
}
