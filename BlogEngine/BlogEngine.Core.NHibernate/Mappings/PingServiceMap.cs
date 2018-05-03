using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PingServiceMap: ClassMap<Models.PingService>
    {
        public PingServiceMap(): base()
        {
            DynamicUpdate();

            Table("be_PingService");
            Id(i => i.PingServiceID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.Link).Length(255);
        }
    }
}
