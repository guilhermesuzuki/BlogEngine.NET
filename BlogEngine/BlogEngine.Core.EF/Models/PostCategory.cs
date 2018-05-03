using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_PostCategory")]
    internal class PostCategory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostCategoryID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required]
        public Guid PostID { get; set; }

        [Required]
        public Guid CategoryID { get; set; }
    }
}
