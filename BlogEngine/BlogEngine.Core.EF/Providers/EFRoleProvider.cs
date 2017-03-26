using BlogEngine.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Transactions;
using System.Configuration;

namespace BlogEngine.Core.EF.Providers
{
    public class EFRoleProvider : DbRoleProvider
    {
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

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var dbContext = this.GetDbContext())
                {
                    foreach (var username in usernames)
                    {
                        foreach (var rolename in roleNames)
                        {
                            if (!rolename.Equals(BlogConfig.AnonymousRole))
                            {
                                var userrole = new Models.UserRoles
                                {
                                    BlogID = Blog.CurrentInstance.BlogId,
                                    Role = rolename.Trim(),
                                    UserName = username.Trim(),
                                };

                                //saves changes to database
                                dbContext.UserRoles.Add(userrole);
                            }
                        }
                    }

                    //save changes
                    dbContext.SaveChanges();

                    //flushes the transaction
                    ts.Complete();
                }
            }

            // This needs to be called in order to keep the Right class in sync.
            Right.RefreshAllRights();
        }

        public override void CreateRole(string roleName)
        {
            var newrole = new Models.Role { BlogID = Blog.CurrentInstance.BlogId, Role_ = roleName.Trim() };

            using (var dbContext = this.GetDbContext())
            {
                dbContext.Roles.Add(newrole);
                dbContext.SaveChanges();
            }

            // This needs to be called in order to keep the Right class in sync.
            // SQL Server on slow connections need few seconds to complete query
            System.Threading.Thread.Sleep(5000);
            Right.RefreshAllRights();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!Security.IsSystemRole(roleName))
            {
                using (var dbContext = this.GetDbContext())
                {
                    var delRole = dbContext.Roles.FirstOrDefault(i => i.BlogID == Blog.CurrentInstance.BlogId && i.Role_.ToLower() == roleName.ToLower());
                    dbContext.Roles.Remove(delRole);
                    dbContext.SaveChanges();
                }
            }

            // This needs to be called in order to keep the Right class in sync.
            Right.RefreshAllRights();

            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from ur in dbContext.UserRoles
                            where ur.BlogID == Blog.CurrentInstance.BlogId && ur.Role.ToLower() == roleName.ToLower() && ur.UserName.ToLower() == usernameToMatch.ToLower()
                            select ur.UserName;

                return query.ToList().ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from role in dbContext.Roles
                            where role.BlogID == Blog.CurrentInstance.BlogId
                            select role.Role_;

                return query.ToList().ToArray();
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from ur in dbContext.UserRoles
                            where ur.BlogID == Blog.CurrentInstance.BlogId && ur.UserName.ToLower() == username.ToLower()
                            select ur.Role;

                return query.ToList().ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from ur in dbContext.UserRoles
                            where ur.BlogID == Blog.CurrentInstance.BlogId && ur.Role.ToLower() == roleName.ToLower()
                            select ur.UserName;

                return query.ToList().ToArray();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = from ur in dbContext.UserRoles
                            where ur.BlogID == Blog.CurrentInstance.BlogId
                                && ur.Role.ToLower() == roleName.ToLower()
                                && ur.UserName.ToLower() == username.ToLower()
                            select ur;

                return query.Any();
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var dbContext = this.GetDbContext())
                {
                    foreach (var user in usernames)
                    {
                        foreach (var role in roleNames)
                        {
                            var userrole = dbContext.UserRoles.FirstOrDefault(
                                i => i.BlogID == Blog.CurrentInstance.BlogId && i.UserName.ToLower() == user.Trim().ToLower() && i.Role.ToLower() == role.Trim().ToLower()
                                );

                            dbContext.UserRoles.Remove(userrole);

                        }
                    }

                    //save changes
                    dbContext.SaveChanges();

                    //flushes the transaction
                    ts.Complete();
                }
            }

            // This needs to be called in order to keep the Right class in sync.
            Right.RefreshAllRights();
        }

        public override bool RoleExists(string roleName)
        {
            using (var dbContext = this.GetDbContext())
            {
                var query = dbContext.Roles.Where(i => i.BlogID == Blog.CurrentInstance.BlogId && i.Role_.ToLower() == roleName.ToLower());
                return query.Any();
            }
        }
    }
}
