using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Posts")]
    internal class Posts
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostRowID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid PostID { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public string PostContent { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        [MaxLength(50)]
        public string Author { get; set; }

        public bool? IsPublished { get; set; }

        public bool? IsCommentEnabled { get; set; }

        public int? Raters { get; set; }

        public float? Rating { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        public bool IsDeleted { get; set; }
    }
}
