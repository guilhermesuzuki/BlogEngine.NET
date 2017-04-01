using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.NHibernate.Mappings
{
    class QuickNotesMap: ClassMap<Models.QuickNotes>
    {
        public QuickNotesMap(): base()
        {
            DynamicUpdate();

            Table("be_QuickNotes");
            Id(i => i.QuickNoteID).GeneratedBy.Identity();
            Map(i => i.BlogID);
            Map(i => i.NoteID);
            Map(i => i.Note);
            Map(i => i.Updated);
            Map(i => i.UserName).Not.Nullable().Length(100);
        }
    }
}
