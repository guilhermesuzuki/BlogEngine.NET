using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PackageFilesMap: ClassMap<Models.PackageFiles>
    {
        public PackageFilesMap(): base()
        {
            DynamicUpdate();

            Table("be_PackageFiles");

            CompositeId()
                .KeyProperty(i => i.PackageId)
                .KeyProperty(i => i.FileOrder);

            Map(i => i.PackageId).Not.Nullable().Length(128);
            Map(i => i.FileOrder).Not.Nullable();
            Map(i => i.FilePath).Not.Nullable().Length(255);
            Map(i => i.IsDirectory).Not.Nullable();
        }
    }
}
