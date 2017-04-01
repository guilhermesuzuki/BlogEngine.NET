using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PagesMap: ClassMap<Models.Pages>
    {
        public PagesMap(): base()
        {
            DynamicUpdate();

            Table("be_Pages");
            Id(i => i.PageRowID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.DateCreated);
            Map(i => i.DateModified);
            Map(i => i.Description);
            Map(i => i.IsDeleted);
            Map(i => i.IsFrontPage);
            Map(i => i.IsPublished);
            Map(i => i.Keywords);
            Map(i => i.PageContent);
            Map(i => i.PageID);
            Map(i => i.Parent);
            Map(i => i.ShowInList);
            Map(i => i.Slug).Length(255);
            Map(i => i.SortOrder);
        }
    }
}
