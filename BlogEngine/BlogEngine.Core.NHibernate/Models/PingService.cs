using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class PingService
    {
        public virtual int PingServiceID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual string Link { get; set; }
    }
}
