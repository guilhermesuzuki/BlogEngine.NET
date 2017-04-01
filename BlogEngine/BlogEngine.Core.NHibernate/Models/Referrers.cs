using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class Referrers
    {
        public virtual int ReferrerRowId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual Guid ReferrerId { get; set; }

        public virtual DateTime ReferralDay { get; set; }

        public virtual string ReferrerUrl { get; set; }

        public virtual int ReferralCount { get; set; }

        public virtual string Url { get; set; }

        public virtual bool? IsSpam { get; set; }
    }
}
