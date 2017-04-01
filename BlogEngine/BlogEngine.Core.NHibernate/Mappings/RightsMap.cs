using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class RightsMap: ClassMap<Models.Rights>
    {
        public RightsMap(): base()
        {
            DynamicUpdate();

            Table("be_Rights");
            Id(i => i.RightRowId).GeneratedBy.Identity();
            Map(i => i.BlogId);
            Map(i => i.RightName).Not.Nullable().Length(100);
        }
    }
}
