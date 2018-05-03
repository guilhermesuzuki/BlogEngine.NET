using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class UsersMap: ClassMap<Models.Users>
    {
        public UsersMap(): base()
        {
            DynamicUpdate();

            Table("be_Users");
            Id(i => i.UserID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.UserName).Nullable().Length(100);
            Map(i => i.Password).Nullable().Length(255);
            Map(i => i.LastLoginTime).Nullable();
            Map(i => i.EmailAddress).Nullable().Length(100);
        }
    }
}
