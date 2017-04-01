using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class StopWordsMap: ClassMap<Models.StopWords>
    {
        public StopWordsMap(): base()
        {
            DynamicUpdate();

            Table("be_StopWords");
            Id(i => i.StopWordRowId).GeneratedBy.Identity();
            Map(i => i.BlogId);
            Map(i => i.StopWord).Length(50);
        }
    }
}
