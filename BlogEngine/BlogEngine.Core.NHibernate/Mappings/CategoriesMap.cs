using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class CategoriesMap: ClassMap<Models.Categories>
    {
        public CategoriesMap(): base()
        {
            DynamicUpdate();

            Table("be_Categories");
            Id(i => i.CategoryRowID).GeneratedBy.Identity();
            Map(i => i.BlogID).Not.Nullable();
            Map(i => i.CategoryID).Not.Nullable();
            Map(i => i.CategoryName).Length(50);
            Map(i => i.Description).Length(200);
            Map(i => i.ParentID);
        }
    }
}
