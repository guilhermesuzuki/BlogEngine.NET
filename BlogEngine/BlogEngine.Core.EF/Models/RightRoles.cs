using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_RightRoles")]
    internal class RightRoles
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RightRoleRowId { get; set; }

        [Required]
        public Guid BlogId { get; set; }

        [Required, MaxLength(100)]
        public string RightName { get; set; }

        [Required, MaxLength(100)]
        public string Role { get; set; }
    }
}
