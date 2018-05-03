using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class SettingsMap: ClassMap<Models.Settings>
    {
        public SettingsMap(): base()
        {
            DynamicUpdate();

            Table("be_Settings");
            Id(i => i.SettingRowId).GeneratedBy.Identity();
            Map(i => i.BlogId);
            Map(i => i.SettingName).Not.Nullable().Length(50);
            Map(i => i.SettingValue).Nullable();
        }
    }
}
