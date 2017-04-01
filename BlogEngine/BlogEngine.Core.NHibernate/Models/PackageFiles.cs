using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class PackageFiles
    {
        public virtual string PackageId { get; set; }

        public virtual int FileOrder { get; set; }

        public virtual string FilePath { get; set; }

        public virtual bool IsDirectory { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PackageFiles)
            {
                var packagefiles = (PackageFiles)obj;

                return this.PackageId == packagefiles.PackageId
                    && this.FileOrder == packagefiles.FileOrder;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
