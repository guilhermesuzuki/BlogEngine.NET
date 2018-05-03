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
using System.Web.Configuration;
using System.Web.Security;

namespace BlogEngine.Core.NHibernate.Providers
{
    public class NhMembershipProvider: DbMembershipProvider
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

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var oldPasswordCorrect = false;
            var success = false;

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    var actualPassword = session.Query<Models.Users>().FirstOrDefault(user => user.BlogID == Blog.CurrentInstance.BlogId && user.UserName.ToLower() == username.ToLower())?.Password;
                    if (string.IsNullOrWhiteSpace(actualPassword))
                    {
                        // This is a special case used for resetting.
                        if (oldPassword.ToLower() == "admin")
                        {
                            oldPasswordCorrect = true;
                        }
                    }
                    else
                    {
                        if (this.PasswordFormat == MembershipPasswordFormat.Hashed)
                        {
                            if (actualPassword == Utils.HashPassword(oldPassword))
                            {
                                oldPasswordCorrect = true;
                            }
                        }
                        else if (actualPassword == oldPassword)
                        {
                            oldPasswordCorrect = true;
                        }
                    }

                    // Update New Password
                    if (oldPasswordCorrect)
                    {
                        var toupdate = session.Query<Models.Users>().FirstOrDefault(user => user.BlogID == Blog.CurrentInstance.BlogId && user.UserName.ToLower() == username.ToLower());
                        if (toupdate != null)
                        {
                            toupdate.Password = this.PasswordFormat == MembershipPasswordFormat.Hashed ? Utils.HashPassword(newPassword) : newPassword;
                            session.SaveOrUpdate(toupdate);
                            session.Flush();
                        }

                        success = true;
                    }

                    ts.Complete();
                }
            }

            return success;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            return base.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool approved, object providerUserKey, out MembershipCreateStatus status)
        {
            var newuser = new Models.Users {
                BlogID = Blog.CurrentInstance.BlogId,
                UserName = username,
                Password = this.PasswordFormat == MembershipPasswordFormat.Hashed ? Utils.HashPassword(password) : password,
                EmailAddress = email,
                LastLoginTime = DateTime.Now,
            };

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    session.SaveOrUpdate(newuser);
                    session.Flush();
                }
                ts.Complete();
            }

            MembershipUser user = this.GetMembershipUser(username, email, DateTime.Now);
            status = MembershipCreateStatus.Success;

            return user;
        }

        protected override byte[] DecryptPassword(byte[] encodedPassword)
        {
            return base.DecryptPassword(encodedPassword);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                try
                {
                    using (var session = this.GetSession())
                    {
                        var todelete = session.Query<Models.Users>().FirstOrDefault(user => user.BlogID == Blog.CurrentInstance.BlogId && user.UserName.ToLower() == username.ToLower());
                        session.Delete(todelete);
                        session.Flush();
                        ts.Complete();
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        protected override byte[] EncryptPassword(byte[] password)
        {
            return base.EncryptPassword(password);
        }

        protected override byte[] EncryptPassword(byte[] password, MembershipPasswordCompatibilityMode legacyPasswordCompatibilityMode)
        {
            return base.EncryptPassword(password, legacyPasswordCompatibilityMode);
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return base.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var users = new MembershipUserCollection();

            using (var ts = new TransactionScope(TransactionScopeOption.Required) { })
            {
                using (var session = this.GetSession())
                {
                    if (Blog.CurrentInstance.IsSiteAggregation)
                    {
                        var allusers = session.Query<Models.Users>().ToList();
                        foreach (var user in allusers) users.Add(this.GetMembershipUser(user.UserName, user.EmailAddress, (DateTime)user.LastLoginTime));
                    }
                    else
                    {
                        var allusersfromblog = session.Query<Models.Users>().Where(user => user.BlogID == Blog.CurrentInstance.BlogId).ToList();
                        foreach (var user in allusersfromblog) users.Add(this.GetMembershipUser(user.UserName, user.EmailAddress, (DateTime)user.LastLoginTime));
                    }

                    ts.Complete();
                }
            }

            totalRecords = users.Count;
            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            return base.GetNumberOfUsersOnline();
        }

        public override string GetPassword(string username, string answer)
        {
            return base.GetPassword(username, answer);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return base.GetUser(providerUserKey, userIsOnline);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (var session = this.GetSession())
            {
                var user = session.Query<Models.Users>().FirstOrDefault(u => u.BlogID == Blog.CurrentInstance.BlogId && u.UserName.ToLower() == username.ToLower());
                return user != null ? this.GetMembershipUser(user.UserName, user.EmailAddress, (DateTime)user.LastLoginTime) : null;
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            using (var session = this.GetSession())
            {
                var user = session.Query<Models.Users>().FirstOrDefault(i => i.BlogID == Blog.CurrentInstance.BlogId && i.EmailAddress.ToLower() == email.ToLower());
                return user?.UserName;
            }
        }

        /// <summary>
        /// Gets membership user.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="lastLogin">
        /// The last login.
        /// </param>
        /// <returns>
        /// A MembershipUser.
        /// </returns>
        private MembershipUser GetMembershipUser(string userName, string email, DateTime lastLogin)
        {
            var user = new MembershipUser(
                this.Name, // Provider name
                userName, // Username
                userName, // providerUserKey
                email, // Email
                string.Empty, // passwordQuestion
                string.Empty, // Comment
                true, // approved
                false, // isLockedOut
                DateTime.Now, // creationDate
                lastLogin, // lastLoginDate
                DateTime.Now, // lastActivityDate
                DateTime.Now, // lastPasswordChangedDate
                new DateTime(1980, 1, 1)); // lastLockoutDate

            return user;
        }

    }
}
