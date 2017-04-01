using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PostNotifyMap: ClassMap<Models.PostNotify>
    {
        public PostNotifyMap(): base()
        {
            DynamicUpdate();

            Table("be_PostNotify");
            Id(i => i.PostNotifyID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.NotifyAddress).Length(255);
            Map(i => i.PostID);
        }
    }
}
