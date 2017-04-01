using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Settings
    {
        public virtual int SettingRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual string SettingName { get; set; }

        public virtual string SettingValue { get; set; }
    }
}
