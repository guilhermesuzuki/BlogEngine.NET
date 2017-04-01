using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class ReferrersMap: ClassMap<Models.Referrers>
    {
        public ReferrersMap(): base()
        {
            DynamicUpdate();

            Table("be_Referrers");
            Id(i => i.ReferrerRowId).GeneratedBy.Identity();
            Map(i => i.BlogId);
            Map(i => i.IsSpam);
            Map(i => i.ReferralCount);
            Map(i => i.ReferralDay);
            Map(i => i.ReferrerId);
            Map(i => i.ReferrerUrl).Length(255);
            Map(i => i.Url).Nullable().Length(255);
        }
    }
}
