using BlogEngine.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Configuration;
using BlogEngine.Core.EF;
using System.Transactions;
using BlogEngine.Core.Packaging;
using BlogEngine.Core.Data.Models;
using BlogEngine.Core.DataStore;
using System.Xml.Serialization;
using System.IO;

namespace BlogEngine.Core.EF.Providers
{
    public class EFBlogProvider : DbBlogProvider
    {
        public EFBlogProvider() : base()
        {

        }

        EF.DbContext GetDbContext()
        {
            return new EF.DbContext(this.CNName);
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Blogs.FirstOrDefault(i => i.BlogId == id);
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Blogs.First(i => i.BlogId == blog.BlogId);
                dbContext.Blogs.Remove(model);
                dbContext.SaveChanges();
            }
        }

        public override void InsertBlog(Blog blog)
        {
            using (var dbContext = this.GetDbContext())
            {
                Models.Blogs model = blog.To();
                dbContext.Blogs.Add(model);
                dbContext.SaveChanges();
            }
        }

        public override void UpdateBlog(Blog blog)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Blogs.First(i => i.BlogId == blog.BlogId);

                model.BlogName = blog.Name;
                model.Hostname = blog.Hostname;
                model.IsAnyTextBeforeHostnameAccepted = blog.IsAnyTextBeforeHostnameAccepted;
                model.StorageContainerName = blog.StorageContainerName;
                model.VirtualPath = blog.VirtualPath;
                model.IsPrimary = blog.IsPrimary;
                model.IsActive = blog.IsActive;
                model.IsSiteAggregation = blog.IsSiteAggregation;

                dbContext.SaveChanges();
            }
        }

        public override List<Blog> FillBlogs()
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from blog in dbContext.Blogs select blog;
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Posts.FirstOrDefault(i => i.PostID == id);
                if (model != null)
                {
                    var post = model.To();

                    //tags
                    dbContext.PostTags.Where(i => i.PostID == id)
                        .ToList()
                        .ForEach(tag => post.Tags.Add(tag.Tag));

                    //comments
                    dbContext.PostComments.Where(i => i.PostID == id)
                        .ToList()
                        .Select(i => Hub.To(i))
                        .ToList()
                        .ForEach(i => { i.Parent = post; post.AllComments.Add(i); });

                    //categories
                    dbContext.PostCategories.Where(i => i.PostID == id)
                        .ToList()
                        .Select(i => this.SelectCategory(i.CategoryID))
                        .ToList()
                        .ForEach(i => post.Categories.Add(i));

                    //email notifications
                    dbContext.PostNotifications.Where(i => i.PostID == id)
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Posts.First(i => i.PostID == post.Id);
                dbContext.Posts.Remove(model);
                dbContext.SaveChanges();
            }
        }

        public override void InsertPost(Post post)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                Models.Posts model = post.To();

                using (var dbContext = this.GetDbContext())
                {
                    dbContext.Posts.Add(model);

                    //new tags
                    var newTags = post.Tags.Select(i => new Models.PostTag { BlogID = model.BlogID, PostID = model.PostID, Tag = i });
                    dbContext.PostTags.AddRange(newTags);

                    //new comments
                    var newComments = post.AllComments.Select(i => Hub.To(i));
                    dbContext.PostComments.AddRange(newComments);

                    //new categories
                    var newCategories = post.Categories.Select(i => new Models.PostCategory { BlogID = model.BlogID, CategoryID = i.Id, PostID = model.PostID });
                    dbContext.PostCategories.AddRange(newCategories);

                    //new notifications
                    var newNotifications = post.NotificationEmails.Select(i => new Models.PostNotify { BlogID = model.BlogID, NotifyAddress = i, PostID = model.PostID });
                    dbContext.PostNotifications.AddRange(newNotifications);

                    //flushes the changes to the database
                    dbContext.SaveChanges();

                    //commits the transaction
                    ts.Complete();
                }
            }
        }

        public override void UpdatePost(Post post)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var dbContext = this.GetDbContext())
                {
                    var model = dbContext.Posts.First(i => i.PostID == post.Id);

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
                    dbContext.SaveChanges();

                    //tags
                    var delTags = dbContext.PostTags.Where(i => i.PostID == model.PostID);
                    dbContext.PostTags.RemoveRange(delTags);

                    //new tags
                    var newTags = post.Tags.Select(i => new Models.PostTag { BlogID = model.BlogID, PostID = model.PostID, Tag = i });
                    dbContext.PostTags.AddRange(newTags);

                    //comments
                    var delComments = dbContext.PostComments.Where(i => i.PostID == model.PostID);
                    dbContext.PostComments.RemoveRange(delComments);

                    //new comments
                    var newComments = post.AllComments.Select(i => Hub.To(i));
                    dbContext.PostComments.AddRange(newComments);

                    //categories
                    var delCategories = dbContext.PostCategories.Where(i => i.PostID == model.PostID);
                    dbContext.PostCategories.RemoveRange(delCategories);

                    //new categories
                    var newCategories = post.Categories.Select(i => new Models.PostCategory { BlogID = model.BlogID, CategoryID = i.Id, PostID = model.PostID });
                    dbContext.PostCategories.AddRange(newCategories);

                    //email notifications
                    var delNotifications = dbContext.PostNotifications.Where(i => i.PostID == model.PostID);
                    dbContext.PostNotifications.RemoveRange(delNotifications);

                    //new notifications
                    var newNotifications = post.NotificationEmails.Select(i => new Models.PostNotify { BlogID = model.BlogID, NotifyAddress = i, PostID = model.PostID });
                    dbContext.PostNotifications.AddRange(newNotifications);

                    //flushes the changes to the database
                    dbContext.SaveChanges();

                    //commits the transaction
                    ts.Complete();
                }
            }
        }

        public override List<Post> FillPosts()
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from p in dbContext.Posts
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Categories.FirstOrDefault(i => i.CategoryID == id);
                if (model != null) return model.To();
                return new Category { };
            }
        }

        public override void DeleteCategory(Category category)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Categories.First(i => i.CategoryID == category.Id);
                dbContext.Categories.Remove(model);
                dbContext.SaveChanges();
            }
        }

        public override void InsertCategory(Category category)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = category.To();
                dbContext.Categories.Add(model);
                dbContext.SaveChanges();
            }
        }

        public override void UpdateCategory(Category category)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Categories.FirstOrDefault(i => i.CategoryID == category.Id);

                model.CategoryName = category.Title;
                model.Description = category.Description;
                model.ParentID = category.Parent;

                dbContext.SaveChanges();
            }
        }

        public override List<Category> FillCategories(Blog blog)
        {
            using (var dbContext = this.GetDbContext())
            {
                return dbContext.Categories
                    .Where(i => i.BlogID == blog.BlogId)
                    .ToList()
                    .Select(i => i.To()).ToList();
            }
        }

        #endregion

        #region Page Methods

        public override void DeletePage(Page page)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Pages.FirstOrDefault(i => i.PageID == page.Id);
                dbContext.Pages.Remove(model);
                dbContext.SaveChanges();
            }
        }

        public override void InsertPage(Page page)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = page.To();
                dbContext.Pages.Add(model);
                dbContext.SaveChanges();
            }
        }

        public override Page SelectPage(Guid id)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Pages.FirstOrDefault(i => i.PageID == id);
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Pages.FirstOrDefault(i => i.PageID == page.Id);

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
                
                dbContext.SaveChanges();
            }
        }

        public override List<Page> FillPages()
        {
            using (var dbContext = this.GetDbContext())
                return dbContext.Pages
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
            using (var dbContext = this.GetDbContext())
            {
                var namedvalues = dbContext.Profiles.Where(i => i.BlogID == Blog.CurrentInstance.BlogId && i.UserName.ToLower() == id.ToLower());

                if (Blog.CurrentInstance.IsSiteAggregation)
                {
                    namedvalues = dbContext.Profiles.Where(i => i.UserName.ToLower() == id.ToLower());
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
            using (var dbContext = this.GetDbContext())
            {
                var namedvalues = dbContext.Profiles.Where(i => i.BlogID == Blog.CurrentInstance.BlogId && i.UserName == profile.UserName);
                dbContext.Profiles.RemoveRange(namedvalues);
                dbContext.SaveChanges();
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

                using (var dbContext = this.GetDbContext())
                {
                    dbContext.Profiles.AddRange(namedvalues);
                    dbContext.SaveChanges();

                    //flushes the transaction
                    ts.Complete();
                }
            }
        }

        public override List<AuthorProfile> FillProfiles()
        {
            var usernames = new List<string> { };

            using (var dbContext = this.GetDbContext())
            {
                if (Blog.CurrentInstance.IsSiteAggregation)
                {
                    var query = from p in dbContext.Profiles
                                group p by p.UserName into username
                                select username.Key;

                    usernames.AddRange(query);
                }
                else
                {
                    var query = from p in dbContext.Profiles
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
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.BlogRollItems.FirstOrDefault(i => i.BlogRollId == id);
                if (model != null) return model.To();
            }

            return new BlogRollItem { };
        }

        public override void DeleteBlogRollItem(BlogRollItem blogRollItem)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.BlogRollItems.FirstOrDefault(i => i.BlogRollId == blogRollItem.Id);
                dbContext.BlogRollItems.Remove(model);
                dbContext.SaveChanges();
            }
        }

        public override void InsertBlogRollItem(BlogRollItem blogRollItem)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = blogRollItem.To();
                dbContext.BlogRollItems.Add(model);
                dbContext.SaveChanges();
            }
        }

        public override void UpdateBlogRollItem(BlogRollItem blogRollItem)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.BlogRollItems.FirstOrDefault(i => i.BlogRollId == blogRollItem.Id);

                model.BlogUrl = blogRollItem.BlogUrl?.AbsoluteUri;
                model.Description = blogRollItem.Description;
                model.FeedUrl = blogRollItem.FeedUrl?.AbsoluteUri;
                model.SortIndex = blogRollItem.SortIndex;
                model.Title = blogRollItem.Title;
                model.Xfn = blogRollItem.Xfn;

                dbContext.SaveChanges();
            }
        }

        public override List<BlogRollItem> FillBlogRoll()
        {
            using (var dbContext = this.GetDbContext())
            {
                return dbContext.BlogRollItems
                    .ToList()
                    .Select(i => i.To())
                    .ToList();
            }
        }

        #endregion

        #region Right Methods

        public override IDictionary<string, IEnumerable<string>> FillRights()
        {
            using (var dbContext = this.GetDbContext())
            {
                var rights = dbContext
                    .Rights
                    .Where(i => i.BlogId == Blog.CurrentInstance.BlogId)
                    .ToList();

                var rightRoles = new Dictionary<string, IEnumerable<string>>();

                //iterate through allrights
                foreach (var right in rights)
                {
                    var roles = from role in dbContext.RightRoles
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
                using (var dbContext = this.GetDbContext())
                {
                    //delete right roles
                    var delRightRoles = dbContext.RightRoles.Where(i => i.BlogId == Blog.CurrentInstance.BlogId).ToList();
                    var delRights = dbContext.Rights.Where(i => i.BlogId == Blog.CurrentInstance.BlogId).ToList();

                    dbContext.RightRoles.RemoveRange(delRightRoles);
                    dbContext.Rights.RemoveRange(delRights);

                    //adds them back together
                    foreach (var right in rights)
                    {
                        var newRight = new Models.Rights { BlogId = Blog.CurrentInstance.BlogId, RightName = right.DisplayName };
                        var newRoles = right.Roles.Select(role => { return new Models.RightRoles { BlogId = newRight.BlogId, RightName = newRight.RightName, Role = role }; });

                        dbContext.Rights.Add(newRight);
                        dbContext.RightRoles.AddRange(newRoles);
                    }

                    dbContext.SaveChanges();

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
                using (var dbContext = this.GetDbContext())
                {
                    var packages = dbContext.Packages.Where(i => i.PackageId == packageId);
                    var packageFiles = dbContext.PackageFiles.Where(i => i.PackageId == packageId);

                    dbContext.PackageFiles.RemoveRange(packageFiles);
                    dbContext.Packages.RemoveRange(packages);
                    dbContext.SaveChanges();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        public override List<InstalledPackage> FillPackages()
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = dbContext.Packages.ToList();
                return query.Select(i => i.To()).ToList();
            }
        }

        public override List<PackageFile> FillPackageFiles(string packageId)
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from pf in dbContext.PackageFiles
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

                using (var dbContext = this.GetDbContext())
                {
                    dbContext.PackageFiles.AddRange(newFiles);
                    dbContext.SaveChanges();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        public override void SavePackage(InstalledPackage package)
        {
            var newpackage = new Models.Packages { PackageId = package.PackageId, Version = package.Version };
            using (var dbContext = this.GetDbContext())
            {
                dbContext.Packages.Add(newpackage);
                dbContext.SaveChanges();
            }
        }

        #endregion

        #region Referrer Methods

        public override void InsertReferrer(Referrer referrer)
        {
            var referrers = Referrer.Referrers;
            referrers.Add(referrer);

            using (var dbContext = this.GetDbContext())
            {
                var model = referrer.To();
                dbContext.Referrers.Add(model);
                dbContext.SaveChanges();
            }
        }

        public override Referrer SelectReferrer(Guid id)
        {
            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Referrers.FirstOrDefault(i => i.ReferrerId == id);
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

                using (var dbContext = this.GetDbContext())
                {
                    var delReferrers = dbContext.Referrers.Where(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ReferralDay < cutoff);
                    dbContext.Referrers.RemoveRange(delReferrers);
                    dbContext.SaveChanges();

                    referrers.AddRange(dbContext.Referrers.Where(i => i.BlogId == Blog.CurrentInstance.BlogId).Select(i => i.To()));
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

            using (var dbContext = this.GetDbContext())
            {
                var model = dbContext.Referrers.FirstOrDefault(i => i.ReferrerId == referrer.Id);

                model.IsSpam = referrer.PossibleSpam;
                model.ReferralCount = referrer.Count;
                model.ReferralDay = referrer.Day;
                model.ReferrerUrl = referrer.ReferrerUrl?.AbsoluteUri;
                model.Url = referrer.Url?.AbsoluteUri;

                dbContext.SaveChanges();
            }
        }

        #endregion

        #region PingServices Methods

        public override StringCollection LoadPingServices()
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from ps in dbContext.PingServices
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
                using (var dbContext = this.GetDbContext())
                {
                    var delPingServices = dbContext.PingServices.Where(i => i.BlogID == Blog.CurrentInstance.BlogId);
                    dbContext.PingServices.RemoveRange(delPingServices);

                    foreach (var link in services)
                    {
                        var newPingService = new Models.PingService { BlogID = Blog.CurrentInstance.BlogId, Link = link };
                        dbContext.PingServices.Add(newPingService);
                    }

                    dbContext.SaveChanges();

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

            using (var dbContext = this.GetDbContext())
            {
                var query = from s in dbContext.Settings
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
                using (var dbContext = this.GetDbContext())
                {
                    var deletethese = dbContext.Settings.Where(i => i.BlogId == Blog.CurrentInstance.BlogId);
                    if (deletethese != null) dbContext.Settings.RemoveRange(deletethese);

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

                    dbContext.Settings.AddRange(newsettings);
                    dbContext.SaveChanges();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        #endregion

        #region CustomFields Methods

        public override void ClearCustomFields(string blogId, string customType, string objectType)
        {
            using (var dbContext = this.GetDbContext())
            {
                var customfields = from item in dbContext.CustomFields
                                   where item.BlogId.ToString() == blogId && item.CustomType == customType && item.ObjectId == objectType
                                   select item;

                dbContext.CustomFields.RemoveRange(customfields);
                dbContext.SaveChanges();
            }
        }

        public override void DeleteCustomField(CustomField field)
        {
            using (var dbContext = this.GetDbContext())
            {
                var customfield = dbContext
                    .CustomFields
                    .FirstOrDefault(i => i.CustomType == field.CustomType && i.BlogId == Blog.CurrentInstance.BlogId && i.ObjectId == field.ObjectId && i.Key == field.Key);

                dbContext.CustomFields.Remove(customfield);
                dbContext.SaveChanges();
            }
        }

        public override List<CustomField> FillCustomFields()
        {
            using (var dbContext = this.GetDbContext())
            {
                var models = dbContext.CustomFields.Where(item => item.BlogId == Blog.CurrentInstance.BlogId);
                return models
                    .ToList()
                    .Select(item => item.To())
                    .ToList();
            }
        }

        public override void SaveCustomField(CustomField field)
        {
            using (var dbContext = this.GetDbContext())
            {
                var toupdate = dbContext.CustomFields.FirstOrDefault(i => i.BlogId == field.BlogId && i.CustomType == field.CustomType && i.Key == field.Key && i.ObjectId == field.ObjectId);
                if (toupdate != null)
                {
                    toupdate.Attribute = field.Attribute;
                    toupdate.Value = field.Value;
                    dbContext.CustomFields.Attach(toupdate);
                }
                else
                {
                    var toadd = field.To();
                    dbContext.CustomFields.Add(toadd);
                }

                dbContext.SaveChanges();
            }
        }

        #endregion

        #region DataStoreSetting Methods

        public override object LoadFromDataStore(ExtensionType extensionType, string extensionId)
        {
            var type = extensionType.ToString();

            using (var dbContext = this.GetDbContext())
            {
                var datastoresetting = dbContext
                    .DataStoreSettings
                    .FirstOrDefault(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ExtensionType == type && i.ExtensionId == extensionId);

                return datastoresetting?.Settings;
            }
        }

        public override void RemoveFromDataStore(ExtensionType extensionType, string extensionId)
        {
            using (var dbContext = this.GetDbContext())
            {
                var todelete = dbContext
                    .DataStoreSettings
                    .Where(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ExtensionType == extensionType.ToString() && i.ExtensionId == extensionId);

                dbContext.DataStoreSettings.RemoveRange(todelete);
                dbContext.SaveChanges();
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

                using (var dbContext = this.GetDbContext())
                {
                    var todelete = dbContext.DataStoreSettings.Where(i => i.BlogId == Blog.CurrentInstance.BlogId && i.ExtensionType == type && i.ExtensionId == extensionId).ToList();

                    dbContext.DataStoreSettings.RemoveRange(todelete);

                    var newdatastoresetting = new Models.DataStoreSettings
                    {
                        BlogId = Blog.CurrentInstance.BlogId,
                        ExtensionId = extensionId,
                        ExtensionType = extensionType.ToString(),
                        Settings = objectXml
                    };

                    dbContext.DataStoreSettings.Add(newdatastoresetting);
                    dbContext.SaveChanges();

                    //completes the transaction
                    ts.Complete();
                }
            }
        }

        #endregion

        #region StopWords Methods

        public override StringCollection LoadStopWords()
        {
            using (var dbContext = this.GetDbContext())
            {
                var stopwords = dbContext.StopWords
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
