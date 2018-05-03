using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class BlogsMap: ClassMap<Models.Blogs>
    {
        public BlogsMap(): base()
        {
            DynamicUpdate();

            Table("be_Blogs");
            Id(i => i.BlogRowId).GeneratedBy.Identity();
            Map(i => i.BlogId).Not.Nullable();
            Map(i => i.BlogName).Not.Nullable().Length(255);
            Map(i => i.Hostname).Not.Nullable();
            Map(i => i.IsActive).Not.Nullable();
            Map(i => i.IsAnyTextBeforeHostnameAccepted).Not.Nullable();
            Map(i => i.IsPrimary).Not.Nullable();
            Map(i => i.IsSiteAggregation).Not.Nullable();
            Map(i => i.StorageContainerName).Not.Nullable().Length(255);
            Map(i => i.VirtualPath).Not.Nullable().Length(255);
        }
    }
}
