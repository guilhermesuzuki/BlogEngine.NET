using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class RolesMap: ClassMap<Models.Roles>
    {
        public RolesMap(): base()
        {
            DynamicUpdate();

            Table("be_Roles");
            Id(i => i.RoleID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.Role_).Column("Role").Not.Nullable().Length(100);
        }
    }
}
