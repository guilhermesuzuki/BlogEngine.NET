using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_PostTag")]
    internal class PostTag
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostTagID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required]
        public Guid PostID { get; set; }

        [MaxLength(50)]
        public string Tag { get; set; }
    }
}
