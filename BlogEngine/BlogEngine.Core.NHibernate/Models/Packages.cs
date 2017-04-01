using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Packages
    {
        public virtual string PackageId { get; set; }

        public virtual string Version { get; set; }
    }
}
