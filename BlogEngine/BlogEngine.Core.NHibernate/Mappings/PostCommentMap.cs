using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class PostCommentMap: ClassMap<Models.PostComment>
    {
        public PostCommentMap(): base()
        {
            DynamicUpdate();

            Table("be_PostComment");
            Id(i => i.PostCommentRowID).GeneratedBy.Identity();
            Map(i => i.Author).Length(255);
            Map(i => i.Avatar).Length(255);
            Map(i => i.BlogID);
            Map(i => i.Comment);
            Map(i => i.CommentDate);
            Map(i => i.Country).Length(255);
            Map(i => i.Email).Length(255); 
            Map(i => i.Ip).Length(50); 
            Map(i => i.IsApproved);
            Map(i => i.IsDeleted);
            Map(i => i.IsSpam);
            Map(i => i.ModeratedBy).Length(100);
            Map(i => i.ParentCommentID);
            Map(i => i.PostCommentID);
            Map(i => i.PostID);
            Map(i => i.Website).Length(255); 
        }
    }
}
