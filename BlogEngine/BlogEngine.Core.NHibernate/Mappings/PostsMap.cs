using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PostsMap: ClassMap<Models.Posts>
    {
        public PostsMap() : base()
        {
            DynamicUpdate();

            Table("be_Posts");
            Id(i => i.PostRowID).GeneratedBy.Identity();
            Map(i => i.Author).Length(50);
            Map(i => i.PostID);
            Map(i => i.BlogID);
            Map(i => i.DateCreated);
            Map(i => i.DateModified);
            Map(i => i.Description);
            Map(i => i.IsCommentEnabled);
            Map(i => i.IsDeleted);
            Map(i => i.IsPublished);
            Map(i => i.PostContent);
            Map(i => i.Raters);
            Map(i => i.Rating);
            Map(i => i.Slug).Length(255);
            Map(i => i.Title).Length(255);
        }
    }
}
