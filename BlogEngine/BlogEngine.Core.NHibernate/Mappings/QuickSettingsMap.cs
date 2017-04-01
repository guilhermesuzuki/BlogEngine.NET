using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class QuickSettingsMap: ClassMap<Models.QuickSettings>
    {
        public QuickSettingsMap(): base()
        {
            DynamicUpdate();

            Table("be_QuickSettings");
            Id(i => i.QuickSettingID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.SettingName).Not.Nullable().Length(255);
            Map(i => i.SettingValue).Not.Nullable().Length(255);
            Map(i => i.UserName).Not.Nullable().Length(100);
        }
    }
}
