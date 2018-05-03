using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class PostComment
    {
        public virtual int PostCommentRowID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual Guid PostCommentID { get; set; }

        public virtual Guid PostID { get; set; }

        public virtual Guid ParentCommentID { get; set; }

        public virtual DateTime CommentDate { get; set; }

        public virtual string Author { get; set; }

        public virtual string Email { get; set; }

        public virtual string Website { get; set; }

        public virtual string Comment { get; set; }

        public virtual string Country { get; set; }

        public virtual string Ip { get; set; }

        public virtual bool? IsApproved { get; set; }

        public virtual string ModeratedBy { get; set; }

        public virtual string Avatar { get; set; }

        public virtual bool IsSpam { get; set; }

        public virtual bool IsDeleted { get; set; }
    }
}
