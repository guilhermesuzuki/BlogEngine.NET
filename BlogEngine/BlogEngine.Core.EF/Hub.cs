using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF
{
    static class Hub
    {
        #region Blog Conversion
        internal static Blog To(this Models.Blogs model)
        {
            var blog = new Blog
            {
                Id = model.BlogId,
                Name = model.BlogName,
                Hostname = model.Hostname,
                IsAnyTextBeforeHostnameAccepted = model.IsAnyTextBeforeHostnameAccepted,
                StorageContainerName = model.StorageContainerName,
                VirtualPath = model.VirtualPath,
                IsActive = model.IsActive,
                IsSiteAggregation = model.IsSiteAggregation
            };

            //sets an internal property
            blog.GetType()
                .GetProperty("IsPrimary")
                .SetValue(blog, model.IsPrimary);

            return blog;
        }

        internal static Models.Blogs To(this Blog model)
        {
            var blog = new Models.Blogs
            {
                BlogId = model.BlogId == Guid.Empty ? new Guid() : model.BlogId,
                BlogName = model.Name,
                Hostname = model.Hostname,
                IsActive = model.IsActive,
                IsAnyTextBeforeHostnameAccepted = model.IsAnyTextBeforeHostnameAccepted,
                IsPrimary = model.IsPrimary,
                IsSiteAggregation = model.IsSiteAggregation,
                StorageContainerName = model.StorageContainerName,
                VirtualPath = model.VirtualPath
            };

            return blog;
        }
        #endregion

        #region Post Conversion

        internal static Post To(this Models.Posts model)
        {
            var post = new Post
            {
                Author = model.Author,
                Content = model.PostContent,
                DateCreated = (DateTime)model.DateCreated,
                DateModified = (DateTime)model.DateModified,
                Description = model.Description,
                Id = model.PostID,
                Raters = (int)model.Raters,
                Rating = (int)model.Rating,
                Title = model.Title,
                Slug = model.Slug,
                HasCommentsEnabled = (bool)model.IsCommentEnabled,
                IsDeleted = model.IsDeleted,
                IsPublished = (bool)model.IsPublished,
            };

            post.AllComments.Clear();
            post.Tags.Clear();

            post.GetType()
                .GetProperty("BlogId")
                .SetValue(post, model.BlogID);

            return post;
        }

        internal static Models.Posts To(this Post model)
        {
            var post = new Models.Posts
            {
                Author = model.Author,
                BlogID = model.BlogId,
                DateCreated = model.DateCreated,
                DateModified = model.DateModified,
                Description = model.Description,
                IsCommentEnabled = model.HasCommentsEnabled,
                IsDeleted = model.IsDeleted,
                IsPublished = model.IsPublished,
                PostContent = model.Content,
                PostID = model.Id == Guid.Empty ? new Guid() : model.Id,
                Raters = model.Raters,
                Rating = model.Rating,
                Slug = model.Slug,
                Title = model.Title,
            };

            return post;
        }

        internal static Models.PostComment To(Comment model)
        {
            var comment = new Models.PostComment
            {
                Author = model.Author,
                Avatar = model.Avatar,
                BlogID = model.BlogId,
                Comment = model.Content,
                CommentDate = model.DateCreated,
                Country = model.Country,
                Email = model.Email,
                Ip = model.IP,
                IsApproved = model.IsApproved,
                IsDeleted = model.IsDeleted,
                IsSpam = model.IsSpam,
                ModeratedBy = model.ModeratedBy,
                ParentCommentID = model.ParentId,
                PostCommentID = model.Id == Guid.Empty ? new Guid(): model.Id,
                PostID = model.Parent.Id,
                Website = model.Website.AbsoluteUri,
            };

            return comment;
        }

        internal static Comment To(Models.PostComment model)
        {
            var comment = new Comment()
            {
                Author = model.Author,
                Avatar = model.Avatar,
                Content = model.Comment,
                DateCreated = model.CommentDate,
                Country = model.Country,
                Email = model.Email,
                IP = model.Ip,
                IsApproved = (bool)model.IsApproved,
                IsDeleted = model.IsDeleted,
                IsSpam = model.IsSpam,
                ModeratedBy = model.ModeratedBy,
                ParentId = model.ParentCommentID,
                Id = model.PostCommentID,
                Website = new Uri(model.Website),
            };

            comment.GetType()
                .GetProperty("BlogId")
                .SetValue(comment, model.BlogID);

            return comment;
        }

        #endregion

        #region Category Conversion

        internal static Models.Categories To(this Category model)
        {
            return new Models.Categories
            {
                BlogID = model.BlogId,
                CategoryID = model.Id == Guid.Empty ? new Guid() : model.Id,
                CategoryName = model.Title,
                Description = model.Description,
                ParentID = model.Parent,
            };
        }

        internal static Category To(this Models.Categories model)
        {
            var category = new Category
            {
                Description = model.Description,
                Title = model.CategoryName,
                Id = model.CategoryID,
                Parent = model.ParentID,
            };

            //blogid
            category.GetType()
                .GetProperty("BlogId")
                .SetValue(category, model.BlogID);

            return category;
        }

        #endregion

        #region Page Conversion

        internal static Models.Pages To(this Page model)
        {
            return new Models.Pages
            {
                BlogID = model.BlogId,
                DateCreated = model.DateCreated,
                DateModified = model.DateModified,
                Description = model.Description,
                IsDeleted = model.IsDeleted,
                IsFrontPage = model.IsFrontPage,
                IsPublished = model.IsPublished,
                Keywords = model.Keywords,
                PageContent = model.Content,
                PageID = model.Id == Guid.Empty ? new Guid() : model.Id,
                Parent = model.Parent,
                ShowInList = model.ShowInList,
                Slug = model.Slug,
                Title = model.Title,
            };
        }

        internal static Page To(this Models.Pages model)
        {
            var page = new Page
            {
                //BlogID = model.BlogId,
                DateCreated = (DateTime)model.DateCreated,
                DateModified = (DateTime)model.DateModified,
                Description = model.Description,
                IsDeleted = model.IsDeleted,
                IsFrontPage = (bool)model.IsFrontPage,
                IsPublished = (bool)model.IsPublished,
                Keywords = model.Keywords,
                Content = model.PageContent,
                Id = model.PageID,
                Parent = (Guid)model.Parent,
                ShowInList = (bool)model.ShowInList,
                Slug = model.Slug,
                Title = model.Title,
            };

            //blogid
            page.GetType()
                .GetProperty("BlogId")
                .SetValue(page, model.BlogID);

            return page;
        }

        #endregion

        #region BlogRollItem Conversion

        internal static Models.BlogRollItems To(this BlogRollItem model)
        {
            var blogrollitem = new Models.BlogRollItems
            {
                BlogId = Blog.CurrentInstance.BlogId,
                BlogRollId = model.Id,
                BlogUrl = model.BlogUrl.AbsoluteUri,
                Description = model.Description,
                FeedUrl = model.FeedUrl.AbsoluteUri,
                SortIndex = model.SortIndex,
                Title = model.Title,
                Xfn = model.Xfn,
            };

            return blogrollitem;
        }

        internal static BlogRollItem To(this Models.BlogRollItems model)
        {
            var blogrollitem = new BlogRollItem
            {
                Id = model.BlogRollId,
                BlogUrl = string.IsNullOrWhiteSpace(model.BlogUrl) ? new Uri(model.BlogUrl) : null,
                Description = model.Description,
                FeedUrl = string.IsNullOrWhiteSpace(model.FeedUrl) ? new Uri(model.FeedUrl) : null,
                SortIndex = model.SortIndex,
                Title = model.Title,
                Xfn = model.Xfn,
            };

            //blogid
            blogrollitem.GetType()
                .GetProperty("BlogId")
                .SetValue(blogrollitem, model.BlogId);

            return blogrollitem;
        }

        #endregion

        #region Referrers Conversion

        internal static Models.Referrers To(this Referrer model)
        {
            var referrers = new Models.Referrers
            {
                BlogId = model.BlogId,
                IsSpam = model.PossibleSpam,
                ReferralCount = model.Count,
                ReferralDay = model.Day,
                ReferrerId = model.Id,
                ReferrerUrl = model.ReferrerUrl?.AbsoluteUri,
                Url = model.Url?.AbsoluteUri,
            };

            return referrers;
        }

        internal static Referrer To(this Models.Referrers model)
        {
            var referrers = new Referrer
            {
                PossibleSpam = (bool)model.IsSpam,
                Count = model.ReferralCount,
                Day = model.ReferralDay,
                Id = model.ReferrerId,
                ReferrerUrl = string.IsNullOrWhiteSpace(model.ReferrerUrl) ? null : new Uri(model.ReferrerUrl),
                Url = string.IsNullOrWhiteSpace(model.Url) ? null : new Uri(model.Url),
            };

            //blogid
            referrers.GetType()
                .GetProperty("BlogId")
                .SetValue(referrers, model.BlogId);

            return referrers;
        }

        #endregion

        #region PackageFiles Conversion

        internal static Models.PackageFiles To(this Packaging.PackageFile model)
        {
            var packagefile = new Models.PackageFiles
            {
                FileOrder = model.FileOrder,
                FilePath = model.FilePath,
                IsDirectory = model.IsDirectory,
                PackageId = model.PackageId,
            };

            return packagefile;
        }

        internal static Packaging.PackageFile To(this Models.PackageFiles model)
        {
            var packagefile = new Packaging.PackageFile
            {
                FileOrder = model.FileOrder,
                FilePath = model.FilePath,
                IsDirectory = model.IsDirectory,
                PackageId = model.PackageId,
            };

            return packagefile;
        }

        internal static Models.Packages To(this Packaging.InstalledPackage model)
        {
            var package = new Models.Packages
            {
                PackageId = model.PackageId,
                Version = model.Version
            };

            return package;
        }

        internal static Packaging.InstalledPackage To(this Models.Packages model)
        {
            var package = new Packaging.InstalledPackage
            {
                PackageId = model.PackageId,
                Version = model.Version
            };

            return package;
        }

        #endregion

        #region CustomFields Conversion

        internal static Models.CustomFields To(this Data.Models.CustomField model)
        {
            var customfield = new Models.CustomFields
            {
                Attribute = model.Attribute,
                BlogId = model.BlogId,
                CustomType = model.CustomType,
                Key = model.Key,
                ObjectId = model.ObjectId,
                Value = model.Value
            };

            return customfield;
        }

        internal static Data.Models.CustomField To(this Models.CustomFields model)
        {
            var customfield = new Data.Models.CustomField
            {
                Attribute = model.Attribute,
                BlogId = model.BlogId,
                CustomType = model.CustomType,
                Key = model.Key,
                ObjectId = model.ObjectId,
                Value = model.Value
            };

            return customfield;
        } 

        #endregion

    }
}
