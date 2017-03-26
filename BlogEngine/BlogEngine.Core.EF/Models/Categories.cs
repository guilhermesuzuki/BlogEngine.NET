using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogEngine.Core.EF.Models
{
    [Table("be_Categories")]
    internal class Categories
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryRowID { get; set; }

        [Required]
        public Guid BlogID { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid CategoryID { get; set; }

        [MaxLength(50)]
        public string CategoryName { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public Guid? ParentID { get; set; }
    }
}
