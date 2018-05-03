using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class DataStoreSettings
    {
        public virtual int DataStoreSettingRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual string ExtensionType { get; set; }

        public virtual string ExtensionId { get; set; }

        public virtual string Settings { get; set; }
    }
}
