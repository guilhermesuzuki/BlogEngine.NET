using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_UserRoles")]
    internal class UserRoles
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserRoleID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; }

        [MaxLength(100)]
        public string Role { get; set; }
    }
}
