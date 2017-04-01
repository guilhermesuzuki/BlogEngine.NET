using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class RightRolesMap: ClassMap<Models.RightRoles>
    {
        public RightRolesMap(): base()
        {
            DynamicUpdate();

            Table("be_RightRoles");
            Id(i => i.RightRoleRowId).GeneratedBy.Identity();
            Map(i => i.BlogId);
            Map(i => i.RightName).Not.Nullable().Length(100);
            Map(i => i.Role).Not.Nullable().Length(100);
        }
    }
}
