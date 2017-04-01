using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class FileStoreFileThumbs
    {
        public virtual Guid thumbnailId { get; set; }

        public virtual Guid FileId { get; set; }

        public virtual int Size { get; set; }

        public virtual Byte[] Contents { get; set; }
    }
}
