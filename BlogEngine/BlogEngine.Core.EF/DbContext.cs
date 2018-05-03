using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogEngine.Core.EF
{
    internal class DbContext : System.Data.Entity.DbContext
    {
        public DbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {

        }

        public DbSet<Models.BlogRollItems> BlogRollItems { get; set; }

        public DbSet<Models.Blogs> Blogs { get; set; }

        public DbSet<Models.DataStoreSettings> DataStoreSettings { get; set; }

        public DbSet<Models.Posts> Posts { get; set; }

        public DbSet<Models.PostComment> PostComments { get; set; }

        public DbSet<Models.PostTag> PostTags { get; set; }

        public DbSet<Models.PostNotify> PostNotifications { get; set; }

        public DbSet<Models.PostCategory> PostCategories { get; set; }

        public DbSet<Models.Categories> Categories { get; set; }

        public DbSet<Models.CustomFields> CustomFields { get; set; }

        public DbSet<Models.Packages> Packages { get; set; }

        public DbSet<Models.PackageFiles> PackageFiles { get; set; }

        public DbSet<Models.Pages> Pages { get; set; }

        public DbSet<Models.PingService> PingServices { get; set; }

        public DbSet<Models.Profiles> Profiles { get; set; }

        public DbSet<Models.Referrers> Referrers { get; set; }

        public DbSet<Models.Rights> Rights { get; set; }

        public DbSet<Models.RightRoles> RightRoles { get; set; }

        public DbSet<Models.Role> Roles { get; set; }

        public DbSet<Models.Settings> Settings { get; set; }

        public DbSet<Models.StopWords> StopWords { get; set; }

        public DbSet<Models.UserRoles> UserRoles { get; set; }

        public DbSet<Models.Users> Users { get; set; }
    }
}
