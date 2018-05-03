using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Pages")]
    internal class Pages
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PageRowID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid PageID { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength]
        public string Description { get; set; }

        [MaxLength(255)]
        public string PageContent { get; set; }

        [MaxLength(255)]
        public string Keywords { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        public bool? IsPublished { get; set; }

        public bool? IsFrontPage { get; set; }

        public Guid? Parent { get; set; } = null;

        public bool? ShowInList { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
    }
}