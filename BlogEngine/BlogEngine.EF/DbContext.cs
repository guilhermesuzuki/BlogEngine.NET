using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogEngine.Core;

namespace BlogEngine.EF
{
    internal class DbContext: System.Data.Entity.DbContext
    {
        /// <summary>
        /// Private constructor
        /// </summary>
        DbContext(): base("BlogEngine")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
               
        }

        public static DbContext Current { get; private set; } = new DbContext();
    }
}
