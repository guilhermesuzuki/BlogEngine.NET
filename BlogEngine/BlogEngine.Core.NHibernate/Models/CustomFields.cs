using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class CustomFields
    {
        public virtual string CustomType { get; set; }

        public virtual string ObjectId { get; set; }

        public virtual Guid BlogId { get; set; }

        public virtual string Key { get; set; }

        public virtual string Value { get; set; }

        public virtual string Attribute { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CustomFields)
            {
                var customfield = (CustomFields)obj;

                return this.BlogId == customfield.BlogId
                    && this.ObjectId == customfield.ObjectId
                    && this.Key == customfield.Key
                    && this.CustomType == customfield.CustomType;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
