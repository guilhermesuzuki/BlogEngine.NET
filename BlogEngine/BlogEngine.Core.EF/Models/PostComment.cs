using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_PostComment")]
    internal class PostComment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostCommentRowID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid PostCommentID { get; set; }

        [Required]
        public Guid PostID { get; set; }

        [Required]
        public Guid ParentCommentID { get; set; }

        [Required]
        public DateTime CommentDate { get; set; }

        [MaxLength(255)]
        public string Author { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string Website { get; set; }

        public string Comment { get; set; }

        [MaxLength(255)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string Ip { get; set; }

        public bool? IsApproved { get; set; }

        [MaxLength(100)]
        public string ModeratedBy { get; set; }

        [MaxLength(255)]
        public string Avatar { get; set; }

        public bool IsSpam { get; set; }

        public bool IsDeleted { get; set; }
    }
}
