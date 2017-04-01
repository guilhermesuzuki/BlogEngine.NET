using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class FileStoreDirectoryMap: ClassMap<Models.FileStoreDirectory>
    {
        public FileStoreDirectoryMap(): base()
        {
            DynamicUpdate();

            Table("be_FileStoreDirectory");
            Id(i => i.Id).GeneratedBy.Assigned();
            Map(i => i.ParentID).Not.Nullable();
            Map(i => i.BlogID).Not.Nullable();
            Map(i => i.Name).Not.Nullable().Length(255);
            Map(i => i.FullPath).Not.Nullable().Length(1000);
            Map(i => i.CreateDate).Not.Nullable();
            Map(i => i.LastAccess).Not.Nullable();
            Map(i => i.LastModify).Not.Nullable();
        }
    }
}
