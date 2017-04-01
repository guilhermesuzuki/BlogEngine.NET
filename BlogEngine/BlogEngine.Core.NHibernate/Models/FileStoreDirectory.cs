using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class FileStoreDirectory
    {
        public virtual Guid Id { get; set; }

        public virtual Guid? ParentID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual string Name { get; set; }

        public virtual string FullPath { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual DateTime LastAccess { get; set; }

        public virtual DateTime LastModify { get; set; }
    }
}
