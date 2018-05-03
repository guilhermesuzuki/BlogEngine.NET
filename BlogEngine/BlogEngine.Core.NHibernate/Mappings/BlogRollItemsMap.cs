using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class BlogRollItemsMap: ClassMap<Models.BlogRollItems>
    {
        public BlogRollItemsMap(): base()
        {
            DynamicUpdate();

            Table("be_BlogRollItems");
            Id(i => i.BlogRollRowId).GeneratedBy.Identity();
            Map(i => i.BlogId).Not.Nullable();
            Map(i => i.BlogRollId).Not.Nullable();
            Map(i => i.BlogUrl).Not.Nullable().Length(255);
            Map(i => i.Description);
            Map(i => i.FeedUrl).Length(255);
            Map(i => i.SortIndex).Not.Nullable();
            Map(i => i.Title).Not.Nullable().Length(255);
            Map(i => i.Xfn).Length(255);
        }
    }
}
