using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class ProfilesMap: ClassMap<Models.Profiles>
    {
        public ProfilesMap(): base()
        {
            DynamicUpdate();

            Table("be_Profiles");
            Id(i => i.ProfileID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.SettingName).Length(200);
            Map(i => i.SettingValue);
            Map(i => i.UserName).Length(100);
        }
    }
}
