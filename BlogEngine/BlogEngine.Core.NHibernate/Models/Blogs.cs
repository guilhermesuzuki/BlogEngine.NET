using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Blogs
    {
        public virtual int BlogRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual string BlogName { get; set; } = string.Empty;

        public virtual string Hostname { get; set; } = string.Empty;

        public virtual bool IsAnyTextBeforeHostnameAccepted { get; set; }

        public virtual string StorageContainerName { get; set; } = string.Empty;

        public virtual string VirtualPath { get; set; } = string.Empty;

        public virtual bool IsPrimary { get; set; }

        public virtual bool IsActive { get; set; }

        public virtual bool IsSiteAggregation { get; set; }
    }
}
