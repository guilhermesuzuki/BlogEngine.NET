using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PostTagMap: ClassMap<Models.PostTag>
    {
        public PostTagMap(): base()
        {
            DynamicUpdate();

            Table("be_PostTag");
            Id(i => i.PostTagID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.PostID);
            Map(i => i.Tag).Length(50);
        }
    }
}
