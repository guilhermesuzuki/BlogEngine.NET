using BlogEngine.Core.Providers;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BlogEngine.Core.NHibernate.Providers
{
    public class NhRoleProvider : DbRoleProvider
    {
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

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
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
                                session.SaveOrUpdate(userrole);
                            }
                        }
                    }

                    //save changes
                    session.Flush();

                    //flushes the transaction
                    ts.Complete();
                }
            }

            // This needs to be called in order to keep the Right class in sync.
            Right.RefreshAllRights();
        }

        public override void CreateRole(string roleName)
        {
            var newrole = new Models.Roles { BlogID = Blog.CurrentInstance.BlogId, Role_ = roleName.Trim() };

            using (var session = this.GetSession())
            {
                session.SaveOrUpdate(newrole);
                session.Flush();
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
                using (var session = this.GetSession())
                {
                    var delRole = session.Query<Models.Roles>().FirstOrDefault(i => i.BlogID == Blog.CurrentInstance.BlogId && i.Role_.ToLower() == roleName.ToLower());
                    session.Delete(delRole);
                    session.Flush();
                }
            }

            // This needs to be called in order to keep the Right class in sync.
            Right.RefreshAllRights();

            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var session = this.GetSession())
            {
                var query = from ur in session.Query<Models.UserRoles>()
                            where ur.BlogID == Blog.CurrentInstance.BlogId && ur.Role.ToLower() == roleName.ToLower() && ur.UserName.ToLower() == usernameToMatch.ToLower()
                            select ur.UserName;

                return query.ToList().ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            using (var session = this.GetSession())
            {
                var query = from role in session.Query<Models.Roles>()
                            where role.BlogID == Blog.CurrentInstance.BlogId
                            select role.Role_;

                return query.ToList().ToArray();
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var session = this.GetSession())
            {
                var query = from ur in session.Query<Models.UserRoles>()
                            where ur.BlogID == Blog.CurrentInstance.BlogId && ur.UserName.ToLower() == username.ToLower()
                            select ur.Role;

                return query.ToList().ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var session = this.GetSession())
            {
                var query = from ur in session.Query<Models.UserRoles>()
                            where ur.BlogID == Blog.CurrentInstance.BlogId && ur.Role.ToLower() == roleName.ToLower()
                            select ur.UserName;

                return query.ToList().ToArray();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var session = this.GetSession())
            {
                var query = from ur in session.Query<Models.UserRoles>()
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
                using (var session = this.GetSession())
                {
                    foreach (var user in usernames)
                    {
                        foreach (var role in roleNames)
                        {
                            var userrole = session.Query<Models.UserRoles>().FirstOrDefault(
                                i => i.BlogID == Blog.CurrentInstance.BlogId && i.UserName.ToLower() == user.Trim().ToLower() && i.Role.ToLower() == role.Trim().ToLower()
                                );

                            session.Delete(userrole);

                        }
                    }

                    //save changes
                    session.Flush();

                    //flushes the transaction
                    ts.Complete();
                }
            }

            // This needs to be called in order to keep the Right class in sync.
            Right.RefreshAllRights();
        }

        public override bool RoleExists(string roleName)
        {
            using (var session = this.GetSession())
            {
                var query = session.Query<Models.Roles>().Where(i => i.BlogID == Blog.CurrentInstance.BlogId && i.Role_.ToLower() == roleName.ToLower());
                return query.Any();
            }
        }
    }
}
