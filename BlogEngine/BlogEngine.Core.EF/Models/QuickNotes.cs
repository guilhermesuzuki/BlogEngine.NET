using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_QuickNotes")]
    internal class QuickNotes
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuickNoteID { get; set; }

        [Required]
        public Guid NoteID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        public string Note { get; set; }

        public DateTime? Updated { get; set; }
    }
}
