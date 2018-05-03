using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PostCategoryMap: ClassMap<Models.PostCategory>
    {
        public PostCategoryMap(): base()
        {
            DynamicUpdate();

            Table("be_PostCategory");
            Id(i => i.PostCategoryID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.CategoryID);
            Map(i => i.PostID);
        }
    }
}
