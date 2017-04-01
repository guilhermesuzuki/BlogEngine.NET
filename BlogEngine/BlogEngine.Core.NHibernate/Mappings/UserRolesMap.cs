using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class UserRolesMap: ClassMap<Models.UserRoles>
    {
        public UserRolesMap(): base()
        {
            DynamicUpdate();

            Table("be_UserRoles");
            Id(i => i.UserRoleID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.Role).Not.Nullable().Length(100);
            Map(i => i.UserName).Not.Nullable().Length(100);
        }
    }
}
