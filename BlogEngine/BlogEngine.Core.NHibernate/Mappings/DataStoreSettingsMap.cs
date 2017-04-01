using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class DataStoreSettingsMap: ClassMap<Models.DataStoreSettings>
    {
        public DataStoreSettingsMap(): base()
        {
            DynamicUpdate();

            Table("be_DataStoreSettings");
            Id(i => i.DataStoreSettingRowId).GeneratedBy.Identity();
            Map(i => i.BlogId).Not.Nullable();
            Map(i => i.ExtensionId).Not.Nullable().Length(100);
            Map(i => i.ExtensionType).Not.Nullable().Length(50);
            Map(i => i.Settings).Not.Nullable();
        }
    }
}
