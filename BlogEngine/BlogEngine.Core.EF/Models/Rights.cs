using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Rights")]
    internal class Rights
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RightRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [Required, MaxLength(100)]
        public string RightName { get; set; }
    }
}
