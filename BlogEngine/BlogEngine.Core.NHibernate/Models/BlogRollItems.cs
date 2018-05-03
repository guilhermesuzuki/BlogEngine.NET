using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class BlogRollItems
    {
        public virtual int BlogRollRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual Guid BlogRollId { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual string BlogUrl { get; set; }

        public virtual string FeedUrl { get; set; }

        public virtual string Xfn { get; set; }

        public virtual int SortIndex { get; set; }
    }
}
