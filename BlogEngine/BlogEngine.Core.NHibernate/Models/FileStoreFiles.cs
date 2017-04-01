using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class FileStoreFiles
    {
        public virtual Guid FileID { get; set; }

        public virtual Guid ParentDirectoryID { get; set; }

        public virtual string Name { get; set; }

        public virtual string FullPath { get; set; }

        public virtual byte[] Contents { get; set; }

        public virtual int Size { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual DateTime LastAccess { get; set; }

        public virtual DateTime LastModify { get; set; }
    }
}
