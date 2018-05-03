using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class CustomFieldsMap: ClassMap<Models.CustomFields>
    {
        public CustomFieldsMap(): base()
        {
            DynamicUpdate();

            Table("be_CustomFields");

            CompositeId()
                .KeyProperty(i => i.CustomType)
                .KeyProperty(i => i.ObjectId)
                .KeyProperty(i => i.BlogId)
                .KeyProperty(i => i.Key);

            Map(i => i.BlogId);
            Map(i => i.Attribute);
            Map(i => i.CustomType).Not.Nullable().Length(25);
            Map(i => i.Key).Not.Nullable().Length(150);
            Map(i => i.ObjectId).Not.Nullable().Length(100);
            Map(i => i.Value).Not.Nullable();
        }
    }
}
