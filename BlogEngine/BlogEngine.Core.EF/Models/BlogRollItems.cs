using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_BlogRollItems")]
    internal class BlogRollItems
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogRollRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [Required]
        public Guid BlogRollId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [MaxLength]
        public string Description { get; set; }

        [Required, MaxLength(255)]
        public string BlogUrl { get; set; }

        [MaxLength(255)]
        public string FeedUrl { get; set; }

        [MaxLength(255)]
        public string Xfn { get; set; }

        [Required]
        public int SortIndex { get; set; }
    }
}
