using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Posts
    {
        public virtual int PostRowID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual Guid PostID { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual string PostContent { get; set; }

        public virtual DateTime? DateCreated { get; set; }

        public virtual DateTime? DateModified { get; set; }

        public virtual string Author { get; set; }

        public virtual bool? IsPublished { get; set; }

        public virtual bool? IsCommentEnabled { get; set; }

        public virtual int? Raters { get; set; }

        public virtual float? Rating { get; set; }

        public virtual string Slug { get; set; }

        public virtual bool IsDeleted { get; set; }
    }
}
