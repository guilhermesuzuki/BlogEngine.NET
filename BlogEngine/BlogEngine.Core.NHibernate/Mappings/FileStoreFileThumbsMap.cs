using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class FileStoreFileThumbsMap : ClassMap<Models.FileStoreFileThumbs>
    {
        public FileStoreFileThumbsMap(): base()
        {
            DynamicUpdate();

            Table("be_FileStoreFileThumbs");
            Id(i => i.thumbnailId).GeneratedBy.Assigned();
            Map(i => i.FileId).Not.Nullable();
            Map(i => i.Size).Not.Nullable();
            Map(i => i.Contents).Not.Nullable();
        }
    }
}
