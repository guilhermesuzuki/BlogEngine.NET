using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class FileStoreFilesMap: ClassMap<Models.FileStoreFiles>
    {
        public FileStoreFilesMap(): base()
        {
            DynamicUpdate();

            Table("be_FileStoreFiles");
            Id(i => i.FileID).GeneratedBy.Assigned();
            Map(i => i.ParentDirectoryID).Not.Nullable();
            Map(i => i.Name).Not.Nullable().Length(255);
            Map(i => i.FullPath).Not.Nullable().Length(255);
            Map(i => i.Contents).Not.Nullable();
            Map(i => i.Size).Not.Nullable();
            Map(i => i.CreateDate).Not.Nullable();
            Map(i => i.LastAccess).Not.Nullable();
            Map(i => i.LastModify).Not.Nullable();
        }
    }
}
