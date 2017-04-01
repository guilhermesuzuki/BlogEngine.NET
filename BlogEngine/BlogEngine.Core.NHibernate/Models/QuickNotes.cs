using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Models
{
    internal class QuickNotes
    {
        public virtual int QuickNoteID { get; set; }

        public virtual Guid NoteID { get; set; }

        public virtual Guid BlogID { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Note { get; set; }

        public virtual DateTime? Updated { get; set; }
    }
}
