using BlogEngine.Core.Data.Models;
using BlogEngine.Core.DataStore;
using BlogEngine.Core.NHibernate;
using BlogEngine.Core.Packaging;
using BlogEngine.Core.Providers;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;

namespace BlogEngine.Core.NHibernate.Providers
{
    public class NhBlogProvider : DbBlogProvider
    {
        public NhBlogProvider() : base()
        {

        }

        ISession GetSession()
        {
            return Hub.GetSessionFactory(this.CNName).OpenSession();
        }

        /// <summary>
        /// Connectionstring Name (for Data Provider purposes)
        /// </summary>
        string CNName { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (string.IsNullOrEmpty(name)) name = this.GetType().Name;

            if (config != null)
            {
                this.CNName = config["connectionStringName"] ?? "BlogEngine";
            }

            base.Initialize(name, config);
        }

        #region Blog Methods

        public override Blog SelectBlog(Guid id)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Blogs>().FirstOrDefault(i => i.BlogId == id);
                if (model != null)
                {
                    var blog = model.To();
                    blog.MarkOld();
                    return blog;
                }
            }

            return new Blog();
        }

        public override void DeleteBlog(Blog blog)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Blogs>().First(i => i.BlogId == blog.BlogId);
                session.Delete(model);
                session.Flush();
            }
        }

        public override void InsertBlog(Blog blog)
        {
            using (var session = this.GetSession())
            {
                Models.Blogs model = blog.To();
                session.SaveOrUpdate(model);
                session.Flush();
            }
        }

        public override void UpdateBlog(Blog blog)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Blogs>().First(i => i.BlogId == blog.BlogId);

                model.BlogName = blog.Name;
                model.Hostname = blog.Hostname;
                model.IsAnyTextBeforeHostnameAccepted = blog.IsAnyTextBeforeHostnameAccepted;
                model.StorageContainerName = blog.StorageContainerName;
                model.VirtualPath = blog.VirtualPath;
                model.IsPrimary = blog.IsPrimary;
                model.IsActive = blog.IsActive;
                model.IsSiteAggregation = blog.IsSiteAggregation;

                session.SaveOrUpdate(model);
                session.Flush();
            }
        }

        public override List<Blog> FillBlogs()
        {
            using (var session = this.GetSession())
            {
                var query = from blog in session.Query<Models.Blogs>() select blog;
                var blogs = query.ToList().Select(i => i.To());

                //marks as old
                foreach (var blog in blogs) { blog.MarkOld(); }
                return blogs.ToList();
            }
        }

        public override bool SetupBlogFromExistingBlog(Blog existingBlog, Blog newBlog)
        {
            return base.SetupBlogFromExistingBlog(existingBlog, newBlog);
        }

        public override bool SetupNewBlog(Blog newBlog, string userName, string email, string password)
        {
            return base.SetupNewBlog(newBlog, userName, email, password);
        }

        public override bool DeleteBlogStorageContainer(Blog blog)
        {
            return base.DeleteBlogStorageContainer(blog);
        }

        #endregion

        #region Post Methods

        public override Post SelectPost(Guid id)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Posts>().FirstOrDefault(i => i.PostID == id);
                if (model != null)
                {
                    var post = model.To();

                    //tags
                    session.Query<Models.PostTag>()
                        .Where(i => i.PostID == id)
                        .ToList()
                        .ForEach(tag => post.Tags.Add(tag.Tag));

                    //comments
                    session.Query<Models.PostComment>()
                        .Where(i => i.PostID == id)
                        .ToList()
                        .Select(i => Hub.To(i))
                        .ToList()
                        .ForEach(i => { i.Parent = post; post.AllComments.Add(i); });

                    //categories
                    session.Query<Models.PostCategory>()
                        .Where(i => i.PostID == id)
                        .ToList()
                        .Select(i => this.SelectCategory(i.CategoryID))
                        .ToList()
                        .ForEach(i => post.Categories.Add(i));

                    //email notifications
                    session.Query<Models.PostNotify>()
                        .Where(i => i.PostID == id)
                        .ToList()
                        .ForEach(i => post.NotificationEmails.Add(i.NotifyAddress));

                    post.MarkOld();

                    return post;
                }

                return new Post { };
            }
        }

        public override void DeletePost(Post post)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Posts>().First(i => i.PostID == post.Id);
                session.Delete(model);
                session.Flush();
            }
        }

        public override void InsertPost(Post post)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                Models.Posts model = post.To();

                using (var session = this.GetSession())
                {
                    session.Save(model);

                    //new tags
                    var newTags = post.Tags.Select(i => new Models.PostTag { BlogID = model.BlogID, PostID = model.PostID, Tag = i });
                    session.Save(newTags);

                    //new comments
                    var newComments = post.AllComments.Select(i => Hub.To(i));
                    session.Save(newComments);

                    //new categories
                    var newCategories = post.Categories.Select(i => new Models.PostCategory { BlogID = model.BlogID, CategoryID = i.Id, PostID = model.PostID });
                    session.Save(newCategories);

                    //new notifications
                    var newNotifications = post.NotificationEmails.Select(i => new Models.PostNotify { BlogID = model.BlogID, NotifyAddress = i, PostID = model.PostID });
                    session.Save(newNotifications);

                    //flushes the changes to the database
                    session.Flush();

                    //commits the transaction
                    ts.Complete();
                }
            }
        }

        public override void UpdatePost(Post post)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    var model = session.Query<Models.Posts>().First(i => i.PostID == post.Id);

                    model.Author = post.Author;
                    model.BlogID = post.BlogId;
                    model.DateCreated = post.DateCreated;
                    model.DateModified = post.DateModified;
                    model.Description = post.Description;
                    model.IsCommentEnabled = post.HasCommentsEnabled;
                    model.IsDeleted = post.IsDeleted;
                    model.IsPublished = post.IsPublished;
                    model.PostContent = post.Content;
                    model.Raters = post.Raters;
                    model.Rating = post.Rating;
                    model.Slug = post.Slug;
                    model.Title = post.Title;

                    //updates the model
                    session.SaveOrUpdate(model);

                    //tags
                    var delTags = session.Query<Models.PostTag>().Where(i => i.PostID == model.PostID);
                    session.Delete(delTags);

                    //new tags
                    var newTags = post.Tags.Select(i => new Models.PostTag { BlogID = model.BlogID, PostID = model.PostID, Tag = i });
                    session.Delete(newTags);

                    //comments
                    var delComments = session.Query<Models.PostComment>().Where(i => i.PostID == model.PostID);
                    session.Delete(delComments);

                    //new comments
                    var newComments = post.AllComments.Select(i => Hub.To(i));
                    session.SaveOrUpdate(newComments);

                    //categories
                    var delCategories = session.Query<Models.PostCategory>().Where(i => i.PostID == model.PostID);
                    session.Delete(delCategories);

                    //new categories
                    var newCategories = post.Categories.Select(i => new Models.PostCategory { BlogID = model.BlogID, CategoryID = i.Id, PostID = model.PostID });
                    session.SaveOrUpdate(newCategories);

                    //email notifications
                    var delNotifications = session.Query<Models.PostNotify>().Where(i => i.PostID == model.PostID);
                    session.Delete(delNotifications);

                    //new notifications
                    var newNotifications = post.NotificationEmails.Select(i => new Models.PostNotify { BlogID = model.BlogID, NotifyAddress = i, PostID = model.PostID });
                    session.SaveOrUpdate(newNotifications);

                    //flushes the changes to the database
                    session.Flush();

                    //commits the transaction
                    ts.Complete();
                }
            }
        }

        public override List<Post> FillPosts()
        {
            using (var Session = this.GetSession())
            {
                var query = from p in Session.Query<Models.Posts>()
                            select p.PostID;

                var posts = query
                    .ToList()
                    .Select(id => this.SelectPost(id))
                    .ToList();

                //sorts using the default comparer
                posts.Sort();

                return posts;
            }
        }

        #endregion

        #region Category Methods

        public override Category SelectCategory(Guid id)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Categories>().FirstOrDefault(i => i.CategoryID == id);
                if (model != null) return model.To();
                return new Category { };
            }
        }

        public override void DeleteCategory(Category category)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Categories>().First(i => i.CategoryID == category.Id);
                session.Delete(model);
                session.Flush();
            }
        }

        public override void InsertCategory(Category category)
        {
            using (var session = this.GetSession())
            {
                var model = category.To();
                session.SaveOrUpdate(model);
                session.Flush();
            }
        }

        public override void UpdateCategory(Category category)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Categories>().FirstOrDefault(i => i.CategoryID == category.Id);

                model.CategoryName = category.Title;
                model.Description = category.Description;
                model.ParentID = category.Parent;

                session.Flush();
            }
        }

        public override List<Category> FillCategories(Blog blog)
        {
            using (var session = this.GetSession())
            {
                return session.Query<Models.Categories>()
                    .Where(i => i.BlogID == blog.BlogId)
                    .ToList()
                    .Select(i => i.To()).ToList();
            }
        }

        #endregion

        #region Page Methods

        public override void DeletePage(Page page)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Pages>().FirstOrDefault(i => i.PageID == page.Id);
                session.Delete(model);
                session.Flush();
            }
        }

        public override void InsertPage(Page page)
        {
            using (var session = this.GetSession())
            {
                var model = page.To();
                session.SaveOrUpdate(model);
                session.Flush();
            }
        }

        public override Page SelectPage(Guid id)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Pages>().FirstOrDefault(i => i.PageID == id);
                if (model != null)
                {
                    var page = model.To();
                    return page;
                }
            }

            return new Page { };
        }

        public override void UpdatePage(Page page)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Pages>().FirstOrDefault(i => i.PageID == page.Id);

                model.DateCreated = page.DateCreated;
                model.DateModified = page.DateModified;
                model.Description = page.Description;
                model.IsDeleted = page.IsDeleted;
                model.IsFrontPage = page.IsFrontPage;
                model.IsPublished = page.IsPublished;
                model.Keywords = page.Keywords;
                model.PageContent = page.Content;
                model.Parent = page.Parent;
                model.ShowInList = page.ShowInList;
                model.Slug = page.Slug;
                model.Title = page.Title;
                
                session.Flush();
            }
        }

        public override List<Page> FillPages()
        {
            using (var session = this.GetSession())
                return session.Query<Models.Pages>()
                .ToList()
                .Select(i => i.To())
                .ToList();
        }

        #endregion

        #region Profile Methods

        public override void InsertProfile(AuthorProfile profile)
        {
            base.InsertProfile(profile);
        }

        public override AuthorProfile SelectProfile(string id)
        {
            using (var session = this.GetSession())
            {
                var namedvalues = session.Query<Models.Profiles>().Where(i => i.BlogID == Blog.CurrentInstance.BlogId && i.UserName.ToLower() == id.ToLower());

                if (Blog.CurrentInstance.IsSiteAggregation)
                {
                    namedvalues = session.Query<Models.Profiles>().Where(i => i.UserName.ToLower() == id.ToLower());
                }

                if (namedvalues != null)
                {
                    var dic = new StringDictionary();
                    foreach (var namedvalue in namedvalues)
                    {
                        dic[namedvalue.SettingName] = namedvalue.SettingValue;
                    }

                    var profile = new AuthorProfile(id)
                    {
                        DisplayName = dic.ContainsKey("DisplayName") ? dic["DisplayName"] : string.Empty,
                        FirstName = dic.ContainsKey("FirstName") ? dic["FirstName"] : string.Empty,
                        MiddleName = dic.ContainsKey("MiddleName") ? dic["MiddleName"] : string.Empty,
                        LastName = dic.ContainsKey("LastName") ? dic["LastName"] : string.Empty,
                        CityTown = dic.ContainsKey("CityTown") ? dic["CityTown"] : string.Empty,
                        RegionState = dic.ContainsKey("RegionState") ? dic["RegionState"] : string.Empty,
                        Country = dic.ContainsKey("Country") ? dic["Country"] : string.Empty,
                        AboutMe = dic.ContainsKey("AboutMe") ? dic["AboutMe"] : string.Empty,
                        PhotoUrl = dic.ContainsKey("PhotoURL") ? dic["PhotoURL"] : string.Empty,
                        Company = dic.ContainsKey("Company") ? dic["Company"] : string.Empty,
                        EmailAddress = dic.ContainsKey("EmailAddress") ? dic["EmailAddress"] : string.Empty,
                        PhoneMain = dic.ContainsKey("PhoneMain") ? dic["PhoneMain"] : string.Empty,
                        PhoneMobile = dic.ContainsKey("PhoneMobile") ? dic["PhoneMobile"] : string.Empty,
                        PhoneFax = dic.ContainsKey("PhoneFax") ? dic["PhoneFax"] : string.Empty,
                        Private = dic.ContainsKey("IsPrivate") ? dic["IsPrivate"] == "true" : false,
                    };

                    if (dic.ContainsKey("Birthday"))
                    {
                        DateTime date;
                        if (DateTime.TryParse(dic["Birthday"], out date))
                        {
                            profile.Birthday = date;
                        }
                    }

                    return profile;
                }
            }

            return base.SelectProfile(id);
        }

        public override void DeleteProfile(AuthorProfile profile)
        {
            using (var session = this.GetSession())
            {
                var namedvalues = session.Query<Models.Profiles>().Where(i => i.BlogID == Blog.CurrentInstance.BlogId && i.UserName == profile.UserName);
                session.Delete(namedvalues);
                session.Flush();
            }
        }

        public override void UpdateProfile(AuthorProfile profile)
        {
            var namedvalues = new List<Models.Profiles>();

            if (!String.IsNullOrEmpty(profile.DisplayName))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "DisplayName",
                    SettingValue = profile.DisplayName,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.FirstName))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "FirstName",
                    SettingValue = profile.FirstName,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.MiddleName))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "MiddleName",
                    SettingValue = profile.MiddleName,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.LastName))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "LastName",
                    SettingValue = profile.LastName,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.CityTown))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "CityTown",
                    SettingValue = profile.CityTown,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.RegionState))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "RegionState",
                    SettingValue = profile.RegionState,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.Country))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "Country",
                    SettingValue = profile.Country,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.AboutMe))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "AboutMe",
                    SettingValue = profile.AboutMe,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.PhotoUrl))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "PhotoURL",
                    SettingValue = profile.PhotoUrl,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.Company))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "Company",
                    SettingValue = profile.Company,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.EmailAddress))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "EmailAddress",
                    SettingValue = profile.EmailAddress,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.PhoneMain))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "PhoneMain",
                    SettingValue = profile.PhoneMain,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.PhoneMobile))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "PhoneMobile",
                    SettingValue = profile.PhoneMobile,
                    UserName = profile.UserName
                });
            }

            if (!String.IsNullOrEmpty(profile.PhoneFax))
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "PhoneFax",
                    SettingValue = profile.PhoneFax,
                    UserName = profile.UserName
                });
            }

            if (profile.Birthday != DateTime.MinValue)
            {
                namedvalues.Add(new Models.Profiles
                {
                    BlogID = profile.BlogId,
                    SettingName = "Birthday",
                    SettingValue = profile.Birthday.ToString("yyyy-MM-dd"),
                    UserName = profile.UserName
                });
            }

            namedvalues.Add(new Models.Profiles
            {
                BlogID = profile.BlogId,
                SettingName = "IsPrivate",
                SettingValue = profile.Private.ToString(),
                UserName = profile.UserName
            });

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                // Remove Profile
                this.DeleteProfile(profile);

                using (var session = this.GetSession())
                {
                    session.SaveOrUpdate(namedvalues);
                    session.Flush();

                    //flushes the transaction
                    ts.Complete();
                }
            }
        }

        public override List<AuthorProfile> FillProfiles()
        {
            var usernames = new List<string> { };

            using (var session = this.GetSession())
            {
                if (Blog.CurrentInstance.IsSiteAggregation)
                {
                    var query = from p in session.Query<Models.Profiles>()
                                group p by p.UserName into username
                                select username.Key;

                    usernames.AddRange(query);
                }
                else
                {
                    var query = from p in session.Query<Models.Profiles>()
                                where p.BlogID == Blog.CurrentInstance.BlogId
                                group p by p.UserName into username
                                select username.Key;

                    usernames.AddRange(query);
                }
            }

            return usernames
                .ToList()
                .Select(i => { return this.SelectProfile(i); })
                .ToList();
        }

        #endregion

        #region BlogRollItem Methods

        public override BlogRollItem SelectBlogRollItem(Guid id)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.BlogRollItems>().FirstOrDefault(i => i.BlogRollId == id);
                if (model != null) return model.To();
            }

            return new BlogRollItem { };
        }

        public override void DeleteBlogRollItem(BlogRollItem blogRollItem)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.BlogRollItems>().FirstOrDefault(i => i.BlogRollId == blogRollItem.Id);
                session.Delete(model);
                session.Flush();
            }
        }

        public override void InsertBlogRollItem(BlogRollItem blogRollItem)
        {
            using (var session = this.GetSession())
            {
                var model = blogRollItem.To();
                session.SaveOrUpdate(model);
                session.Flush();
            }
        }

        public override void UpdateBlogRollItem(BlogRollItem blogRollItem)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.BlogRollItems>().FirstOrDefault(i => i.BlogRollId == blogRollItem.Id);

                model.BlogUrl = blogRollItem.BlogUrl?.AbsoluteUri;
                model.Description = blogRollItem.Description;
                model.FeedUrl = blogRollItem.FeedUrl?.AbsoluteUri;
                model.SortIndex = blogRollItem.SortIndex;
                model.Title = blogRollItem.Title;
                model.Xfn = blogRollItem.Xfn;

                session.Flush();
            }
        }

        public override List<BlogRollItem> FillBlogRoll()
        {
            using (var session = this.GetSession())
            {
                return session.Query<Models.BlogRollItems>()
                    .ToList()
                    .Select(i => i.To())
                    .ToList();
            }
        }

        #endregion

        #region Right Methods

        public override IDictionary<string, IEnumerable<string>> FillRights()
        {
            using (var session = this.GetSession())
            {
                var rights = session.Query<Models.Rights>().Where(i => i.BlogId == Blog.CurrentInstance.BlogId);

                var rightRoles = new Dictionary<string, IEnumerable<string>>();

                //iterate through allrights
                foreach (var right in rights)
                {
                    var roles = from role in session.Query<Models.RightRoles>()
                                where role.BlogId == right.BlogId && role.RightName == right.RightName
                                select role.Role;

                    rightRoles[right.RightName] = roles.Distinct().ToArray();
                }

                return rightRoles;
            }
        }

        public override void SaveRights(IEnumerable<Right> rights)
        {
            if (rights == null)
            {
                throw new ArgumentNullException("rights");
            }

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    //delete right roles
                    var delRightRoles = session.Query<Models.RightRoles>().Where(i => i.BlogId == Blog.CurrentInstance.BlogId);
                    var delRights = session.Query<Models.Rights>().Where(i => i.BlogId == Blog.CurrentInstance.BlogId);

                    session.Delete(delRightRoles);
                    session.Delete(delRights);

                    //adds them back together
                    foreach (var right in rights)
                    {
                        var newRight = new Models.Rights { BlogId = Blog.CurrentInstance.BlogId, RightName = right.DisplayName };
                        var newRoles = right.Roles.Select(role => { return new Models.RightRoles { BlogId = newRight.BlogId, RightName = newRight.RightName, Role = role }; });

                        session.SaveOrUpdate(newRight);
                        session.SaveOrUpdate(newRoles);
                    }

                    session.Flush();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        #endregion

        #region Packages Methods

        public override void DeletePackage(string packageId)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    var packages = session.Query<Models.Packages>().Where(i => i.PackageId == packageId);
                    var packageFiles = session.Query<Models.PackageFiles>().Where(i => i.PackageId == packageId);

                    session.Delete(packageFiles);
                    session.Delete(packages);
                    session.Flush();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        public override List<InstalledPackage> FillPackages()
        {
            using (var session = this.GetSession())
            {
                var query = session.Query<Models.Packages>().ToList();
                return query.Select(i => i.To()).ToList();
            }
        }

        public override List<PackageFile> FillPackageFiles(string packageId)
        {
            using (var session = this.GetSession())
            {
                var query = from pf in session.Query<Models.PackageFiles>()
                            where pf.PackageId == packageId
                            select pf;

                return query
                    .ToList()
                    .Select(i => i.To())
                    .ToList();
            }
        }

        public override void SavePackageFiles(List<PackageFile> packageFiles)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                var newFiles = packageFiles.Select(i => i.To());

                using (var session = this.GetSession())
                {
                    session.SaveOrUpdate(newFiles);
                    session.Flush();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        public override void SavePackage(InstalledPackage package)
        {
            var newpackage = new Models.Packages { PackageId = package.PackageId, Version = package.Version };
            using (var session = this.GetSession())
            {
                session.SaveOrUpdate(newpackage);
                session.Flush();
            }
        }

        #endregion

        #region Referrer Methods

        public override void InsertReferrer(Referrer referrer)
        {
            var referrers = Referrer.Referrers;
            referrers.Add(referrer);

            using (var session = this.GetSession())
            {
                var model = referrer.To();
                session.SaveOrUpdate(model);
                session.Flush();
            }
        }

        public override Referrer SelectReferrer(Guid id)
        {
            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Referrers>().FirstOrDefault(i => i.ReferrerId == id);
                if (model != null)
                {
                    var referrer = model.To();
                    referrer.MarkOld();
                    return referrer;
                }
            }

            return new Referrer { };
        }

        public override List<Referrer> FillReferrers()
        {
            var referrers = new List<Referrer>();

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                //delete old referrers
                var cutoff = DateTime.Today.AddDays(-BlogSettings.Instance.NumberOfReferrerDays);

                using (var session = this.GetSession())
                {
                    var delReferrers = session.Query<Models.Referrers>().Where(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ReferralDay < cutoff);
                    session.Delete(delReferrers);
                    session.Flush();

                    referrers.AddRange(session.Query<Models.Referrers>().Where(i => i.BlogId == Blog.CurrentInstance.BlogId).Select(i => i.To()));
                    referrers.ForEach(i => i.MarkOld());

                    //completes the transaction
                    ts.Complete();
                }
            }

            return referrers;
        }

        public override void UpdateReferrer(Referrer referrer)
        {
            var referrers = Referrer.Referrers;
            referrers.Remove(referrer);
            referrers.Add(referrer);

            using (var session = this.GetSession())
            {
                var model = session.Query<Models.Referrers>().FirstOrDefault(i => i.ReferrerId == referrer.Id);

                model.IsSpam = referrer.PossibleSpam;
                model.ReferralCount = referrer.Count;
                model.ReferralDay = referrer.Day;
                model.ReferrerUrl = referrer.ReferrerUrl?.AbsoluteUri;
                model.Url = referrer.Url?.AbsoluteUri;

                session.Flush();
            }
        }

        #endregion

        #region PingServices Methods

        public override StringCollection LoadPingServices()
        {
            using (var session = this.GetSession())
            {
                var query = from ps in session.Query<Models.PingService>()
                            where ps.BlogID == Blog.CurrentInstance.BlogId
                            select ps.Link;

                var pingservices = new StringCollection() { };
                pingservices.AddRange(query.ToArray());
                return pingservices;
            }
        }

        public override void SavePingServices(StringCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    var delPingServices = session.Query<Models.PingService>().Where(i => i.BlogID == Blog.CurrentInstance.BlogId);
                    session.Delete(delPingServices);

                    foreach (var link in services)
                    {
                        var newPingService = new Models.PingService { BlogID = Blog.CurrentInstance.BlogId, Link = link };
                        session.SaveOrUpdate(newPingService);
                    }

                    session.Flush();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        #endregion

        #region Settings Methods

        public override StringDictionary LoadSettings(Blog blog)
        {
            var dic = new StringDictionary() { };

            using (var session = this.GetSession())
            {
                var query = from s in session.Query<Models.Settings>()
                            where s.BlogId == blog.BlogId
                            select new { s.SettingName, s.SettingValue };

                query.ToList().ForEach(i => dic.Add(i.SettingName, i.SettingValue));
            }

            return dic;
        }

        public override void SaveSettings(StringDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    var deletethese = session.Query<Models.Settings>().Where(i => i.BlogId == Blog.CurrentInstance.BlogId);
                    if (deletethese != null) session.Delete(deletethese);

                    var newsettings = new List<Models.Settings> { };

                    foreach (var key in settings.Keys)
                    {
                        var newsetting = new Models.Settings
                        {
                            BlogId = Blog.CurrentInstance.BlogId,
                            SettingName = key.ToString(),
                            SettingValue = settings[key.ToString()]
                        };

                        newsettings.Add(newsetting);
                    }

                    session.SaveOrUpdate(newsettings);
                    session.Flush();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        #endregion

        #region CustomFields Methods

        public override void ClearCustomFields(string blogId, string customType, string objectType)
        {
            using (var session = this.GetSession())
            {
                var customfields = from item in session.Query<Models.CustomFields>()
                                   where item.BlogId.ToString() == blogId && item.CustomType == customType && item.ObjectId == objectType
                                   select item;

                session.Delete(customfields);
                session.Flush();
            }
        }

        public override void DeleteCustomField(CustomField field)
        {
            using (var session = this.GetSession())
            {
                var customfield = session
                    .Query<Models.CustomFields>()
                    .FirstOrDefault(i => i.CustomType == field.CustomType && i.BlogId == Blog.CurrentInstance.BlogId && i.ObjectId == field.ObjectId && i.Key == field.Key);

                session.Delete(customfield);
                session.Flush();
            }
        }

        public override List<CustomField> FillCustomFields()
        {
            using (var session = this.GetSession())
            {
                var models = session.Query<Models.CustomFields>().Where(item => item.BlogId == Blog.CurrentInstance.BlogId);
                return models
                    .ToList()
                    .Select(item => item.To())
                    .ToList();
            }
        }

        public override void SaveCustomField(CustomField field)
        {
            using (var session = this.GetSession())
            {
                var toupdate = session.Query<Models.CustomFields>().FirstOrDefault(i => i.BlogId == field.BlogId && i.CustomType == field.CustomType && i.Key == field.Key && i.ObjectId == field.ObjectId);
                if (toupdate != null)
                {
                    toupdate.Attribute = field.Attribute;
                    toupdate.Value = field.Value;
                    session.SaveOrUpdate(toupdate);
                }
                else
                {
                    var toadd = field.To();
                    session.Save(toadd);
                }

                session.Flush();
            }
        }

        #endregion

        #region DataStoreSetting Methods

        public override object LoadFromDataStore(ExtensionType extensionType, string extensionId)
        {
            var type = extensionType.ToString();

            using (var session = this.GetSession())
            {
                var datastoresetting = session
                    .Query<Models.DataStoreSettings>()
                    .FirstOrDefault(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ExtensionType == type && i.ExtensionId == extensionId);

                return datastoresetting?.Settings;
            }
        }

        public override void RemoveFromDataStore(ExtensionType extensionType, string extensionId)
        {
            using (var session = this.GetSession())
            {
                var todelete = session
                    .Query<Models.DataStoreSettings>()
                    .Where(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ExtensionType == extensionType.ToString() && i.ExtensionId == extensionId);

                session.Delete(todelete);
                session.Flush();
            }
        }

        public override void SaveToDataStore(ExtensionType extensionType, string extensionId, object settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            // Save
            var xs = new XmlSerializer(settings.GetType());
            string objectXml = string.Empty;
            using (var sw = new StringWriter() { })
            {
                xs.Serialize(sw, settings);
                objectXml = sw.ToString();
            }

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                var type = extensionType.ToString();

                using (var session = this.GetSession())
                {
                    var todelete = session
                        .Query<Models.DataStoreSettings>()
                        .Where(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ExtensionType == type && i.ExtensionId == extensionId);

                    session.Delete(todelete);
                    session.Flush();

                    var newdatastoresetting = new Models.DataStoreSettings
                    {
                        BlogId = Blog.CurrentInstance.BlogId,
                        ExtensionId = extensionId,
                        ExtensionType = extensionType.ToString(),
                        Settings = objectXml
                    };

                    session.SaveOrUpdate(newdatastoresetting);
                    session.Flush();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        #endregion

        #region StopWords Methods

        public override StringCollection LoadStopWords()
        {
            using (var session = this.GetSession())
            {
                var stopwords = session.Query<Models.StopWords>()
                    .Where(i => i.BlogId == Blog.CurrentInstance.BlogId)
                    .Select(i => i.StopWord)
                    .ToArray();

                var list = new StringCollection() { };
                list.AddRange(stopwords);
                return list;
            }
        }

        #endregion

    }
}
