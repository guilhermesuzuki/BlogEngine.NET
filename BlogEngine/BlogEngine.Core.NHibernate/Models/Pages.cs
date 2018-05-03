using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Pages
    {
        public virtual int PageRowID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual Guid PageID { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual string PageContent { get; set; }

        public virtual string Keywords { get; set; }

        public virtual DateTime? DateCreated { get; set; }

        public virtual DateTime? DateModified { get; set; }

        public virtual bool? IsPublished { get; set; }

        public virtual bool? IsFrontPage { get; set; }

        public virtual Guid? Parent { get; set; } = null;

        public virtual bool? ShowInList { get; set; }

        public virtual string Slug { get; set; } = string.Empty;

        public virtual bool IsDeleted { get; set; }

        public virtual int? SortOrder { get; set; }
    }
}