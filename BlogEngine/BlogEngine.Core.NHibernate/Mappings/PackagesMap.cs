using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PackagesMap: ClassMap<Models.Packages>
    {
        public PackagesMap(): base()
        {
            DynamicUpdate();

            Table("be_Packages");
            Id(i => i.PackageId).GeneratedBy.Assigned().Length(128);
            Map(i => i.Version).Not.Nullable().Length(128);
        }
    }
}
